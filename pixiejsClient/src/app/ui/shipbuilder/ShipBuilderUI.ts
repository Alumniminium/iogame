import { Container, FederatedPointerEvent } from "pixi.js";
import { BuildGrid } from "./BuildGrid";
import { PartPalette, type PartDefinition } from "./PartPalette";
import { TemplateSelector } from "./TemplateSelector";
import { Button } from "../Button";
import { BuildModeSystem } from "../../ecs/systems/BuildModeSystem";
import { ShipTemplate } from "../../shipbuilder/ShipTemplate";
import {
  ShipConfigurationPacket,
  type ShipPart,
} from "../../network/packets/ShipConfigurationPacket";
import { NetworkManager } from "../../network/NetworkManager";

export interface ShipBuilderConfig {
  canvasWidth?: number;
  canvasHeight?: number;
}

export class ShipBuilderUI extends Container {
  private buildGrid!: BuildGrid;
  private partPalette!: PartPalette;
  private templateSelector!: TemplateSelector;
  private cancelButton!: Button;
  private clearButton!: Button;
  private buildModeSystem: BuildModeSystem;

  private lastGridPosition = { gridX: -1, gridY: -1 };

  constructor(
    buildModeSystem: BuildModeSystem,
    config: ShipBuilderConfig = {},
  ) {
    super();

    this.buildModeSystem = buildModeSystem;

    const canvasWidth = config.canvasWidth || 800;
    const canvasHeight = config.canvasHeight || 600;

    this.initializeComponents();
    this.setupEventHandlers();
    this.layoutComponents(canvasWidth, canvasHeight);
  }

  private initializeComponents(): void {
    this.buildGrid = new BuildGrid({
      cellSize: 32,
      gridWidth: 20,
      gridHeight: 15,
      lineColor: 0x444444,
      lineAlpha: 0.7,
      backgroundColor: 0x1a1a1a,
      backgroundAlpha: 0.8,
    });

    this.partPalette = new PartPalette({
      width: 200,
      height: 400,
    });

    this.templateSelector = new TemplateSelector({
      width: 200,
      height: 300,
    });

    this.cancelButton = new Button({
      text: "Exit Build Mode",
      width: 150,
      height: 40,
    });

    this.clearButton = new Button({
      text: "Clear All",
      width: 120,
      height: 40,
    });

    this.addChild(this.buildGrid);
    this.addChild(this.partPalette);
    this.addChild(this.templateSelector);
    this.addChild(this.cancelButton);
    this.addChild(this.clearButton);

    this.buildModeSystem.setBuildGrid(this.buildGrid);
  }

  private setupEventHandlers(): void {
    this.partPalette.setPartSelectedCallback((part: PartDefinition) => {
      this.buildModeSystem.selectPart(part.type, part.shape);
    });

    this.templateSelector.setTemplateSelectedCallback(
      (template: ShipTemplate) => {
        const gridDims = this.buildGrid.getGridDimensions();
        const offsetX = Math.floor(gridDims.width / 2) - template.centerX;
        const offsetY = Math.floor(gridDims.height / 2) - template.centerY;
        this.buildModeSystem.loadTemplate(template, offsetX, offsetY);
      },
    );

    this.buildGrid.eventMode = "static";
    this.buildGrid.on("pointermove", this.onGridPointerMove.bind(this));
    this.buildGrid.on("pointerdown", this.onGridPointerDown.bind(this));
    this.buildGrid.on("pointerup", this.onGridPointerUp.bind(this));
    this.buildGrid.on("pointerout", this.onGridPointerOut.bind(this));

    this.cancelButton.onPress.connect(() => {
      this.onCancel();
    });

    this.clearButton.onPress.connect(() => {
      this.onClearAll();
    });

    window.addEventListener("keydown", this.onKeyDown.bind(this));
  }

  private onGridPointerMove(event: FederatedPointerEvent): void {
    const gridPos = this.buildGrid.worldToGrid(event.global.x, event.global.y);

    if (
      gridPos.gridX !== this.lastGridPosition.gridX ||
      gridPos.gridY !== this.lastGridPosition.gridY
    ) {
      this.lastGridPosition = gridPos;

      if (this.buildGrid.isValidGridPosition(gridPos.gridX, gridPos.gridY)) {
        this.buildModeSystem.updateGhostPosition(gridPos.gridX, gridPos.gridY);

        const existingPart = this.buildModeSystem.getPartAt(
          gridPos.gridX,
          gridPos.gridY,
        );
        const highlightColor = existingPart ? 0xff0000 : 0x00ff00; // Red if occupied, green if free
        this.buildGrid.highlightCell(
          gridPos.gridX,
          gridPos.gridY,
          highlightColor,
        );
      } else {
        this.buildGrid.clearHighlight();
        this.buildModeSystem.hideGhost();
      }
    }
  }

  private onGridPointerDown(event: FederatedPointerEvent): void {
    const gridPos = this.buildGrid.worldToGrid(event.global.x, event.global.y);

    if (this.buildGrid.isValidGridPosition(gridPos.gridX, gridPos.gridY)) {
      const existingPart = this.buildModeSystem.getPartAt(
        gridPos.gridX,
        gridPos.gridY,
      );

      if (event.shiftKey || event.button === 2) {
        if (existingPart) {
          this.buildModeSystem.removePart(gridPos.gridX, gridPos.gridY);
          this.sendShipConfiguration(); // Send update after removal
        }
      } else {
        if (!existingPart) {
          const placed = this.buildModeSystem.placePart(
            gridPos.gridX,
            gridPos.gridY,
          );
          if (placed) {
            this.sendShipConfiguration(); // Send update after placement
          }
        }
      }
    }
  }

  private onGridPointerUp(): void {}

  private onGridPointerOut(): void {
    this.buildGrid.clearHighlight();
    this.buildModeSystem.hideGhost();
    this.lastGridPosition = { gridX: -1, gridY: -1 };
  }

  private onKeyDown(event: KeyboardEvent): void {
    if (!this.buildModeSystem.isInBuildMode()) return;

    switch (event.code) {
      case "KeyR":
        this.buildModeSystem.rotatePart();
        event.preventDefault();
        break;
      case "Escape":
        this.onCancel();
        event.preventDefault();
        break;
    }
  }

  private sendShipConfiguration(): void {
    const parts = this.buildGrid.getAllParts();

    const shipParts: ShipPart[] = parts.map((part) => ({
      gridX: part.gridX,
      gridY: part.gridY,
      type: part.type === "hull" ? 0 : part.type === "shield" ? 1 : 2, // hull=0, shield=1, engine=2
      shape: part.shape === "triangle" ? 0 : 1, // triangle=0, square=1
      rotation: part.rotation, // 0=0°, 1=90°, 2=180°, 3=270°
    }));

    const playerId = (window as unknown as Record<string, unknown>)
      .localPlayerId as string; // Get local player ID
    if (playerId) {
      const packet = ShipConfigurationPacket.create(playerId, shipParts);
      NetworkManager.send(packet);
    } else {
    }
  }

  private onCancel(): void {
    this.buildModeSystem.clearAllParts();
    this.exitBuildMode();
  }

  private onClearAll(): void {
    this.buildModeSystem.clearAllParts();
    this.partPalette.clearSelection();
    this.sendShipConfiguration(); // Send empty configuration to server
  }

  private layoutComponents(canvasWidth: number, canvasHeight: number): void {
    const gridPixelWidth =
      this.buildGrid.getGridDimensions().width * this.buildGrid.getCellSize();
    const gridPixelHeight =
      this.buildGrid.getGridDimensions().height * this.buildGrid.getCellSize();

    this.buildGrid.x = (canvasWidth - gridPixelWidth) / 2; // Center horizontally
    this.buildGrid.y = (canvasHeight - gridPixelHeight) / 2;

    this.partPalette.x = canvasWidth - 220;
    this.partPalette.y = (canvasHeight - 400) / 2;

    this.templateSelector.x = 20;
    this.templateSelector.y = (canvasHeight - 300) / 2;

    const buttonY = canvasHeight - 60;
    const buttonSpacing = 160;
    const startX = (canvasWidth - 2 * buttonSpacing) / 2;

    this.cancelButton.x = startX;
    this.cancelButton.y = buttonY;

    this.clearButton.x = startX + buttonSpacing;
    this.clearButton.y = buttonY;
  }

  public show(): void {
    this.visible = true;
    this.buildModeSystem.enterBuildMode();
    this.partPalette.show();
    this.templateSelector.show();
  }

  public hide(): void {
    this.visible = false;
    this.partPalette.hide();
    this.templateSelector.hide();
  }

  private exitBuildMode(): void {
    this.hide();
    this.buildModeSystem.exitBuildMode();

    this.emit("buildModeExit");
  }

  public resize(width: number, height: number): void {
    this.layoutComponents(width, height);
  }

  public destroy(): void {
    window.removeEventListener("keydown", this.onKeyDown.bind(this));
    super.destroy();
  }
}
