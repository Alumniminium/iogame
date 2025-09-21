import { Container } from "pixi.js";
import { BuildGrid } from "./BuildGrid";
import { PartPalette, type PartDefinition } from "./PartPalette";
import { TemplateSelector } from "./TemplateSelector";
import { Button } from "../Button";
import { BuildModeSystem } from "../../ecs/systems/BuildModeSystem";
import { ShipTemplate } from "../../shipbuilder/ShipTemplate";

export interface ShipBuilderConfig {
  canvasWidth?: number;
  canvasHeight?: number;
}

export class ShipBuilderUI extends Container {
  private buildGrid!: BuildGrid;
  private partPalette!: PartPalette;
  private templateSelector!: TemplateSelector;
  private saveButton!: Button;
  private cancelButton!: Button;
  private clearButton!: Button;
  private buildModeSystem: BuildModeSystem;

  private lastGridPosition = { gridX: -1, gridY: -1 };

  constructor(buildModeSystem: BuildModeSystem, config: ShipBuilderConfig = {}) {
    super();

    this.buildModeSystem = buildModeSystem;

    const canvasWidth = config.canvasWidth || 800;
    const canvasHeight = config.canvasHeight || 600;

    this.initializeComponents(canvasWidth, canvasHeight);
    this.setupEventHandlers();
    this.layoutComponents(canvasWidth, canvasHeight);
  }

  private initializeComponents(_canvasWidth: number, _canvasHeight: number): void {
    // Create build grid in the center
    this.buildGrid = new BuildGrid({
      cellSize: 32,
      gridWidth: 20,
      gridHeight: 15,
      lineColor: 0x444444,
      lineAlpha: 0.7,
      backgroundColor: 0x1a1a1a,
      backgroundAlpha: 0.8,
    });

    // Create part palette on the right side
    this.partPalette = new PartPalette({
      width: 200,
      height: 400,
    });

    // Create template selector on the left side
    this.templateSelector = new TemplateSelector({
      width: 200,
      height: 300,
    });

    // Create action buttons
    this.saveButton = new Button({
      text: "Save Ship",
      width: 120,
      height: 40,
    });

    this.cancelButton = new Button({
      text: "Cancel",
      width: 120,
      height: 40,
    });

    this.clearButton = new Button({
      text: "Clear All",
      width: 120,
      height: 40,
    });

    // Add all components
    this.addChild(this.buildGrid);
    this.addChild(this.partPalette);
    this.addChild(this.templateSelector);
    this.addChild(this.saveButton);
    this.addChild(this.cancelButton);
    this.addChild(this.clearButton);

    // Connect build mode system to build grid
    this.buildModeSystem.setBuildGrid(this.buildGrid);
  }

  private setupEventHandlers(): void {
    // Part selection from palette
    this.partPalette.setPartSelectedCallback((part: PartDefinition) => {
      this.buildModeSystem.selectPart(part.type, part.shape);
    });

    // Template selection
    this.templateSelector.setTemplateSelectedCallback((template: ShipTemplate) => {
      // Load template at grid center
      const gridDims = this.buildGrid.getGridDimensions();
      const offsetX = Math.floor(gridDims.width / 2) - template.centerX;
      const offsetY = Math.floor(gridDims.height / 2) - template.centerY;
      this.buildModeSystem.loadTemplate(template, offsetX, offsetY);
    });

    // Grid interaction
    this.buildGrid.eventMode = 'static';
    this.buildGrid.on('pointermove', this.onGridPointerMove.bind(this));
    this.buildGrid.on('pointerdown', this.onGridPointerDown.bind(this));
    this.buildGrid.on('pointerup', this.onGridPointerUp.bind(this));
    this.buildGrid.on('pointerout', this.onGridPointerOut.bind(this));

    // Button handlers
    this.saveButton.onPress.connect(() => {
      this.onSaveShip();
    });

    this.cancelButton.onPress.connect(() => {
      this.onCancel();
    });

    this.clearButton.onPress.connect(() => {
      this.onClearAll();
    });

    // Keyboard shortcuts
    window.addEventListener('keydown', this.onKeyDown.bind(this));
  }

  private onGridPointerMove(event: any): void {
    const gridPos = this.buildGrid.worldToGrid(event.global.x, event.global.y);

    // Only update if position changed
    if (gridPos.gridX !== this.lastGridPosition.gridX || gridPos.gridY !== this.lastGridPosition.gridY) {
      this.lastGridPosition = gridPos;

      if (this.buildGrid.isValidGridPosition(gridPos.gridX, gridPos.gridY)) {
        // Update ghost position
        this.buildModeSystem.updateGhostPosition(gridPos.gridX, gridPos.gridY);

        // Highlight current cell
        const existingPart = this.buildModeSystem.getPartAt(gridPos.gridX, gridPos.gridY);
        const highlightColor = existingPart ? 0xff0000 : 0x00ff00; // Red if occupied, green if free
        this.buildGrid.highlightCell(gridPos.gridX, gridPos.gridY, highlightColor);
      } else {
        this.buildGrid.clearHighlight();
        this.buildModeSystem.hideGhost();
      }
    }
  }

  private onGridPointerDown(event: any): void {
    const gridPos = this.buildGrid.worldToGrid(event.global.x, event.global.y);

    if (this.buildGrid.isValidGridPosition(gridPos.gridX, gridPos.gridY)) {
      // Check if we're removing or placing
      const existingPart = this.buildModeSystem.getPartAt(gridPos.gridX, gridPos.gridY);

      if (event.shiftKey || event.button === 2) { // Right click or shift+click to remove
        if (existingPart) {
          this.buildModeSystem.removePart(gridPos.gridX, gridPos.gridY);
        }
      } else { // Left click to place
        if (!existingPart) {
          this.buildModeSystem.placePart(gridPos.gridX, gridPos.gridY);
        }
      }
    }
  }

  private onGridPointerUp(): void {
    // Handle pointer up
  }

  private onGridPointerOut(): void {
    this.buildGrid.clearHighlight();
    this.buildModeSystem.hideGhost();
    this.lastGridPosition = { gridX: -1, gridY: -1 };
  }

  private onKeyDown(event: KeyboardEvent): void {
    if (!this.buildModeSystem.isInBuildMode()) return;

    switch (event.code) {
      case 'KeyR':
        // TODO: Implement rotation
        event.preventDefault();
        break;
      case 'Escape':
        this.onCancel();
        event.preventDefault();
        break;
      case 'KeyS':
        if (event.ctrlKey) {
          this.onSaveShip();
          event.preventDefault();
        }
        break;
    }
  }

  private onSaveShip(): void {
    console.log("Saving ship design...");
    // TODO: Implement ship saving logic
    this.exitBuildMode();
  }

  private onCancel(): void {
    console.log("Canceling ship build...");
    this.buildModeSystem.clearAllParts();
    this.exitBuildMode();
  }

  private onClearAll(): void {
    console.log("Clearing all parts...");
    this.buildModeSystem.clearAllParts();
    this.partPalette.clearSelection();
  }

  private layoutComponents(canvasWidth: number, canvasHeight: number): void {
    // Center the grid
    const gridPixelWidth = this.buildGrid.getGridDimensions().width * this.buildGrid.getCellSize();
    const gridPixelHeight = this.buildGrid.getGridDimensions().height * this.buildGrid.getCellSize();

    this.buildGrid.x = (canvasWidth - gridPixelWidth) / 2; // Center horizontally
    this.buildGrid.y = (canvasHeight - gridPixelHeight) / 2;

    // Position palette on the right
    this.partPalette.x = canvasWidth - 220;
    this.partPalette.y = (canvasHeight - 400) / 2;

    // Position template selector on the left
    this.templateSelector.x = 20;
    this.templateSelector.y = (canvasHeight - 300) / 2;

    // Position buttons at the bottom
    const buttonY = canvasHeight - 60;
    const buttonSpacing = 130;
    const startX = (canvasWidth - (3 * buttonSpacing)) / 2;

    this.saveButton.x = startX;
    this.saveButton.y = buttonY;

    this.cancelButton.x = startX + buttonSpacing;
    this.cancelButton.y = buttonY;

    this.clearButton.x = startX + (2 * buttonSpacing);
    this.clearButton.y = buttonY;
  }

  public show(): void {
    this.visible = true;
    this.buildModeSystem.enterBuildMode();
    this.partPalette.show();
    this.templateSelector.show();

    console.log("Ship builder UI shown");
  }

  public hide(): void {
    this.visible = false;
    this.partPalette.hide();
    this.templateSelector.hide();
  }

  private exitBuildMode(): void {
    this.hide();
    this.buildModeSystem.exitBuildMode();

    // Notify parent that build mode is exiting
    this.emit('buildModeExit');
  }

  public resize(width: number, height: number): void {
    this.layoutComponents(width, height);
  }

  public destroy(): void {
    window.removeEventListener('keydown', this.onKeyDown.bind(this));
    super.destroy();
  }
}