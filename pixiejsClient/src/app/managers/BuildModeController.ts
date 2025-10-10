import { Container, Text } from "pixi.js";
import { World } from "../ecs/core/World";
import { Box2DBodyComponent } from "../ecs/components/Box2DBodyComponent";
import { BuildModeSystem } from "../ecs/systems/BuildModeSystem";
import { BuildGrid } from "../ui/shipbuilder/BuildGrid";
import {
  ShapeSelector,
  type ShapeType,
} from "../ui/shipbuilder/ShapeSelector";
import {
  ComponentDialog,
  type ComponentConfig,
} from "../ui/shipbuilder/ComponentDialog";
import { ShipPartManager } from "./ShipPartManager";

interface BuildModeCallbacks {
  onEnter?: () => void;
  onExit?: () => void;
}

/**
 * Manages build mode state and UI for ship construction.
 * Handles build grid, shape/component selection, and part placement/removal.
 */
export class BuildModeController {
  private buildModeSystem: BuildModeSystem;
  private shipPartManager: ShipPartManager;
  private worldBuildGrid: BuildGrid;
  private buildControlsText: Text;
  private shapeSelector: ShapeSelector;
  private componentDialog: ComponentDialog;

  private inBuildMode = false;
  private isDragging = false;
  private dragMode: "place" | "remove" | null = null;
  private pendingShapeType: ShapeType | null = null;

  private callbacks: BuildModeCallbacks = {};
  private getLocalPlayerId: () => string | null;

  constructor(
    buildModeSystem: BuildModeSystem,
    shipPartManager: ShipPartManager,
    gameWorldContainer: Container,
    uiContainer: Container,
    getLocalPlayerId: () => string | null,
  ) {
    this.buildModeSystem = buildModeSystem;
    this.shipPartManager = shipPartManager;
    this.getLocalPlayerId = getLocalPlayerId;

    // Create build grid in world space
    this.worldBuildGrid = new BuildGrid({
      cellSize: 1, // 1x1 world units per cell
      gridWidth: 39, // Odd number so there's a true center cell
      gridHeight: 39,
      lineColor: 0x00ff00,
      lineAlpha: 0.8,
      backgroundColor: 0x000000,
      backgroundAlpha: 0.2,
    });
    this.worldBuildGrid.visible = false;

    const gridPixelWidth =
      this.worldBuildGrid.getGridDimensions().width *
      this.worldBuildGrid.getCellSize();
    const gridPixelHeight =
      this.worldBuildGrid.getGridDimensions().height *
      this.worldBuildGrid.getCellSize();
    this.worldBuildGrid.pivot.set(gridPixelWidth / 2, gridPixelHeight / 2);

    gameWorldContainer.addChild(this.worldBuildGrid);
    this.buildModeSystem.setBuildGrid(this.worldBuildGrid);

    // Create build controls text in UI space
    this.buildControlsText = new Text({
      text: "",
      style: {
        fontFamily: "Arial",
        fontSize: 16,
        fill: 0x00ff00,
        align: "left",
      },
    });
    this.buildControlsText.visible = false;
    uiContainer.addChild(this.buildControlsText);

    // Create shape selector in UI space
    this.shapeSelector = new ShapeSelector();
    this.shapeSelector.setOnShapeSelected((shape) =>
      this.onShapeSelected(shape),
    );
    uiContainer.addChild(this.shapeSelector);

    // Create component dialog in UI space
    this.componentDialog = new ComponentDialog();
    this.componentDialog.setOnConfirm((config) =>
      this.onComponentSelected(config),
    );
    uiContainer.addChild(this.componentDialog);

    this.setupWorldGridEvents();
    this.setupBuildModeKeyboard();
  }

  /**
   * Set callbacks for build mode state changes
   */
  public setCallbacks(callbacks: BuildModeCallbacks): void {
    this.callbacks = callbacks;
  }

  /**
   * Check if currently in build mode
   */
  public isInBuildMode(): boolean {
    return this.inBuildMode;
  }

  /**
   * Toggle build mode on/off
   */
  public toggle(): void {
    if (this.inBuildMode) {
      this.exit();
    } else {
      this.enter();
    }
  }

  /**
   * Enter build mode
   */
  public enter(): void {
    this.inBuildMode = true;
    this.buildModeSystem.enterBuildMode();

    this.positionBuildGridAroundPlayer();

    this.worldBuildGrid.visible = true;

    this.showBuildModeControls();
    this.shapeSelector.show();

    this.callbacks.onEnter?.();
  }

  /**
   * Exit build mode
   */
  public exit(): void {
    this.inBuildMode = false;
    this.buildModeSystem.exitBuildMode();

    this.worldBuildGrid.visible = false;

    this.hideBuildModeControls();
    this.shapeSelector.hide();
    this.componentDialog.hide();

    this.callbacks.onExit?.();
  }

  /**
   * Update build grid position and ghost preview
   * Should be called every frame when in build mode
   */
  public update(): void {
    if (this.worldBuildGrid.visible && this.buildModeSystem.isInBuildMode()) {
      this.positionBuildGridAroundPlayer();
    }
  }

  /**
   * Handle resize event
   */
  public resize(width: number, height: number): void {
    if (this.buildControlsText) {
      this.buildControlsText.x = width / 2 - this.buildControlsText.width / 2;
      this.buildControlsText.y = 20;
    }

    if (this.shapeSelector) {
      this.shapeSelector.x = width / 2 - 150;
      this.shapeSelector.y = height - 80;
    }

    if (this.componentDialog) {
      this.componentDialog.x = width / 2 - 200;
      this.componentDialog.y = height / 2 - 175;
    }
  }

  private positionBuildGridAroundPlayer(): void {
    const localPlayerId = this.getLocalPlayerId();
    if (!localPlayerId) return;

    const playerEntity = World.getEntity(localPlayerId);
    if (!playerEntity) return;

    const physics = playerEntity.get(Box2DBodyComponent);
    if (!physics) return;

    this.worldBuildGrid.x = physics.position.x;
    this.worldBuildGrid.y = physics.position.y;

    this.worldBuildGrid.rotation = physics.rotationRadians;

    this.worldBuildGrid.position.set(
      this.worldBuildGrid.x,
      this.worldBuildGrid.y,
    );
  }

  private showBuildModeControls(): void {
    const selected = this.buildModeSystem.getSelectedPart();
    this.buildControlsText.text = `BUILD MODE - ${selected.type?.toUpperCase() || "HULL"} (${selected.shape?.toUpperCase() || "SQUARE"})
1: Hull □   2: Shield   3: Engine   4: Hull △   T: Toggle Shape   R: Rotate   Right-Click: Remove   ESC: Exit`;
    this.buildControlsText.visible = true;
  }

  private hideBuildModeControls(): void {
    this.buildControlsText.visible = false;
  }

  private updateBuildModeControls(): void {
    if (this.buildControlsText.visible) {
      this.showBuildModeControls();
    }
  }

  private onShapeSelected(shape: ShapeType): void {
    this.pendingShapeType = shape;
    this.componentDialog.show();
  }

  private onComponentSelected(config: ComponentConfig): void {
    if (!this.pendingShapeType) return;

    const shape = this.pendingShapeType === "box" ? "square" : "triangle";
    this.buildModeSystem.setPendingBlock(shape, config);

    this.shapeSelector.hide();
    this.pendingShapeType = null;
  }

  private setupWorldGridEvents(): void {
    (this.worldBuildGrid as any).eventMode = "static";
    // Enable right-click events
    (this.worldBuildGrid as any).cursor = "pointer";

    this.worldBuildGrid.on(
      "pointermove",
      this.onWorldGridPointerMove.bind(this),
    );
    this.worldBuildGrid.on(
      "pointerdown",
      this.onWorldGridPointerDown.bind(this),
    );
    this.worldBuildGrid.on("pointerup", this.onWorldGridPointerUp.bind(this));
    this.worldBuildGrid.on(
      "pointerupoutside",
      this.onWorldGridPointerUp.bind(this),
    );
    this.worldBuildGrid.on("rightclick", this.onWorldGridRightClick.bind(this));
  }

  private onWorldGridPointerMove(event: any): void {
    if (!this.buildModeSystem.isInBuildMode()) return;

    const gridPos = this.worldBuildGrid.worldToGrid(
      event.global.x,
      event.global.y,
    );

    if (this.worldBuildGrid.isValidGridPosition(gridPos.gridX, gridPos.gridY)) {
      if (this.isDragging && this.dragMode) {
        const existingPart = this.buildModeSystem.getPartAt(
          gridPos.gridX,
          gridPos.gridY,
        );

        if (this.dragMode === "place" && !existingPart) {
          const placed = this.buildModeSystem.placePart(
            gridPos.gridX,
            gridPos.gridY,
          );
          if (placed) {
            const part = this.buildModeSystem.getPartAt(
              gridPos.gridX,
              gridPos.gridY,
            );
            if (part) {
              this.shipPartManager.createShipPart(part.gridX, part.gridY, {
                type: part.type,
                shape: part.shape,
                rotation: part.rotation,
                attachedComponents: part.attachedComponents,
              });
            }
          }
        } else if (this.dragMode === "remove" && existingPart) {
          const removed = this.worldBuildGrid.removePart(
            gridPos.gridX,
            gridPos.gridY,
          );
          if (removed) {
            this.shipPartManager.removeShipPart(gridPos.gridX, gridPos.gridY);
          }
        }
      }

      this.buildModeSystem.updateGhostPosition(gridPos.gridX, gridPos.gridY);

      const existingPart = this.buildModeSystem.getPartAt(
        gridPos.gridX,
        gridPos.gridY,
      );
      const highlightColor = existingPart ? 0xff0000 : 0x00ff00; // Red if occupied, green if free
      this.worldBuildGrid.highlightCell(
        gridPos.gridX,
        gridPos.gridY,
        highlightColor,
      );
    } else {
      this.worldBuildGrid.clearHighlight();
      this.buildModeSystem.hideGhost();
    }
  }

  private onWorldGridPointerDown(event: any): void {
    if (!this.buildModeSystem.isInBuildMode()) return;

    const gridPos = this.worldBuildGrid.worldToGrid(
      event.global.x,
      event.global.y,
    );

    if (this.worldBuildGrid.isValidGridPosition(gridPos.gridX, gridPos.gridY)) {
      const existingPart = this.buildModeSystem.getPartAt(
        gridPos.gridX,
        gridPos.gridY,
      );

      this.isDragging = true;

      // PixiJS FederatedPointerEvent: button 0=left, 1=middle, 2=right
      // Also check pointerType to ensure it's a right-click from mouse
      const isRightClick =
        event.button === 2 || event.nativeEvent?.button === 2;
      if (event.shiftKey || isRightClick) {
        this.dragMode = "remove";
        if (existingPart) {
          const removed = this.worldBuildGrid.removePart(
            gridPos.gridX,
            gridPos.gridY,
          );
          if (removed) {
            this.shipPartManager.removeShipPart(gridPos.gridX, gridPos.gridY);
          }
        }
      } else {
        this.dragMode = "place";
        if (!existingPart) {
          const placed = this.buildModeSystem.placePart(
            gridPos.gridX,
            gridPos.gridY,
          );
          if (placed) {
            const part = this.buildModeSystem.getPartAt(
              gridPos.gridX,
              gridPos.gridY,
            );
            if (part) {
              this.shipPartManager.createShipPart(part.gridX, part.gridY, {
                type: part.type,
                shape: part.shape,
                rotation: part.rotation,
                attachedComponents: part.attachedComponents,
              });
            }
          }
        }
      }
    }
  }

  private onWorldGridPointerUp(): void {
    this.isDragging = false;
    this.dragMode = null;
  }

  private onWorldGridRightClick(event: any): void {
    if (!this.buildModeSystem.isInBuildMode()) return;

    const gridPos = this.worldBuildGrid.worldToGrid(
      event.global.x,
      event.global.y,
    );

    if (this.worldBuildGrid.isValidGridPosition(gridPos.gridX, gridPos.gridY)) {
      const existingPart = this.buildModeSystem.getPartAt(
        gridPos.gridX,
        gridPos.gridY,
      );

      if (existingPart) {
        const removed = this.worldBuildGrid.removePart(
          gridPos.gridX,
          gridPos.gridY,
        );
        if (removed) {
          this.shipPartManager.removeShipPart(gridPos.gridX, gridPos.gridY);
        }
      }
    }
  }

  private setupBuildModeKeyboard(): void {
    document.addEventListener("keydown", (event) => {
      if (!this.buildModeSystem.isInBuildMode()) return;

      switch (event.code) {
        case "Digit1":
          this.buildModeSystem.selectPart(
            "hull",
            (this.buildModeSystem.getSelectedPart().shape as
              | "triangle"
              | "square") || "square",
          );
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        case "Digit2":
          this.buildModeSystem.selectPart(
            "shield",
            (this.buildModeSystem.getSelectedPart().shape as
              | "triangle"
              | "square") || "square",
          );
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        case "Digit3":
          this.buildModeSystem.selectPart(
            "engine",
            (this.buildModeSystem.getSelectedPart().shape as
              | "triangle"
              | "square") || "square",
          );
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        case "Digit4":
          this.buildModeSystem.selectPart("hull", "triangle");
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        case "KeyT": {
          const current = this.buildModeSystem.getSelectedPart();
          const newShape = current.shape === "square" ? "triangle" : "square";
          this.buildModeSystem.selectPart(
            (current.type as "hull" | "shield" | "engine") || "hull",
            newShape,
          );
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        }
        case "KeyR":
          this.buildModeSystem.rotatePart();
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        case "Escape":
          this.exit();
          event.preventDefault();
          break;
      }
    });
  }
}
