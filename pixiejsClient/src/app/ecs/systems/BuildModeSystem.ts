import { ShipTemplate } from "../../shipbuilder/ShipTemplate";
import { BuildGrid, GridPart } from "../../ui/shipbuilder/BuildGrid";

export interface BuildState {
  isActive: boolean;
  selectedPartType: "hull" | "shield" | "engine" | null;
  selectedShape: "triangle" | "square" | null;
}

export class BuildModeSystem {
  private buildState: BuildState;
  private buildGrid: BuildGrid | null = null;

  constructor() {
    this.buildState = {
      isActive: false,
      selectedPartType: null,
      selectedShape: null,
    };
  }

  setBuildGrid(buildGrid: BuildGrid): void {
    this.buildGrid = buildGrid;
  }

  enterBuildMode(): void {
    this.buildState.isActive = true;
  }

  exitBuildMode(): void {
    this.buildState.isActive = false;
    this.buildGrid?.hideGhost();
  }

  selectPart(partType: "hull" | "shield" | "engine", shape: "triangle" | "square"): void {
    this.buildState.selectedPartType = partType;
    this.buildState.selectedShape = shape;
  }

  updateGhostPosition(gridX: number, gridY: number): void {
    if (!this.buildState.isActive || !this.buildGrid || !this.buildState.selectedPartType || !this.buildState.selectedShape) {
      return;
    }

    if (this.isValidPlacement(gridX, gridY)) {
      this.buildGrid.showGhost(gridX, gridY, this.buildState.selectedPartType, this.buildState.selectedShape);
    } else {
      this.buildGrid.hideGhost();
    }
  }

  hideGhost(): void {
    this.buildGrid?.hideGhost();
  }

  placePart(gridX: number, gridY: number): boolean {
    if (!this.buildState.isActive || !this.buildGrid || !this.buildState.selectedPartType || !this.buildState.selectedShape) {
      return false;
    }

    if (!this.isValidPlacement(gridX, gridY)) {
      return false;
    }

    const part: GridPart = {
      gridX,
      gridY,
      type: this.buildState.selectedPartType,
      shape: this.buildState.selectedShape,
      color: this.getPartColor(this.buildState.selectedPartType),
    };

    this.buildGrid.addPart(part);
    return true;
  }

  removePart(gridX: number, gridY: number): boolean {
    if (!this.buildGrid) return false;
    return this.buildGrid.removePart(gridX, gridY);
  }

  private isValidPlacement(gridX: number, gridY: number): boolean {
    if (!this.buildGrid) return false;

    // Check grid bounds
    const dims = this.buildGrid.getGridDimensions();
    if (gridX < 0 || gridX >= dims.width || gridY < 0 || gridY >= dims.height) {
      return false;
    }

    // Check if position is already occupied
    return this.buildGrid.getPartAt(gridX, gridY) === null;
  }

  getPartAt(gridX: number, gridY: number): GridPart | null {
    return this.buildGrid?.getPartAt(gridX, gridY) || null;
  }

  clearAllParts(): void {
    this.buildGrid?.clearAllParts();
  }

  isInBuildMode(): boolean {
    return this.buildState.isActive;
  }

  getSelectedPart(): { type: string | null; shape: string | null } {
    return {
      type: this.buildState.selectedPartType,
      shape: this.buildState.selectedShape,
    };
  }

  loadTemplate(template: ShipTemplate, offsetX: number = 0, offsetY: number = 0): void {
    if (!this.buildGrid) return;

    // Clear existing parts first
    this.clearAllParts();

    // Place each part from the template
    template.parts.forEach(part => {
      const gridX = part.gridX + offsetX;
      const gridY = part.gridY + offsetY;

      // Check if position is valid
      if (this.isValidPlacement(gridX, gridY)) {
        const gridPart: GridPart = {
          gridX,
          gridY,
          type: part.type,
          shape: part.shape,
          color: this.getPartColor(part.type),
        };
        this.buildGrid?.addPart(gridPart);
      }
    });
  }

  private getPartColor(type: string): number {
    switch (type) {
      case "hull": return 0x808080;
      case "shield": return 0x0080ff;
      case "engine": return 0xff8000;
      default: return 0xffffff;
    }
  }
}