import { ShipTemplate, ShipTemplatePart } from "../../shipbuilder/ShipTemplate";
import { BuildGrid, GridPart } from "../../ui/shipbuilder/BuildGrid";
import { ColorComponent } from "../components/ColorComponent";

export interface BuildState {
  isActive: boolean;
  selectedPartType: "hull" | "shield" | "engine" | null;
  selectedShape: "triangle" | "square" | null;
  selectedRotation: number; // 0=0°, 1=90°, 2=180°, 3=270°
}

export class BuildModeSystem {
  private buildState: BuildState;
  private buildGrid: BuildGrid | null = null;

  constructor() {
    this.buildState = {
      isActive: false,
      selectedPartType: null,
      selectedShape: null,
      selectedRotation: 0,
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

  selectPart(
    partType: "hull" | "shield" | "engine",
    shape: "triangle" | "square",
  ): void {
    this.buildState.selectedPartType = partType;
    this.buildState.selectedShape = shape;
  }

  updateGhostPosition(gridX: number, gridY: number): void {
    if (
      !this.buildState.isActive ||
      !this.buildGrid ||
      !this.buildState.selectedPartType ||
      !this.buildState.selectedShape
    ) {
      return;
    }

    if (this.isValidPlacement(gridX, gridY)) {
      this.buildGrid.showGhost(
        gridX,
        gridY,
        this.buildState.selectedPartType,
        this.buildState.selectedShape,
        this.buildState.selectedRotation,
      );
    } else {
      this.buildGrid.hideGhost();
    }
  }

  hideGhost(): void {
    this.buildGrid?.hideGhost();
  }

  placePart(gridX: number, gridY: number): boolean {
    if (
      !this.buildState.isActive ||
      !this.buildGrid ||
      !this.buildState.selectedPartType ||
      !this.buildState.selectedShape
    ) {
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
      color: ColorComponent.getPartColor(this.buildState.selectedPartType),
      rotation: this.buildState.selectedRotation,
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

    // Use the grid's own validation which supports negative coordinates
    if (!this.buildGrid.isValidGridPosition(gridX, gridY)) {
      return false;
    }

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

  loadTemplate(
    template: ShipTemplate,
    offsetX: number = 0,
    offsetY: number = 0,
  ): void {
    if (!this.buildGrid) return;

    this.clearAllParts();

    template.parts.forEach((part: ShipTemplatePart) => {
      const gridX = part.gridX + offsetX;
      const gridY = part.gridY + offsetY;

      if (this.isValidPlacement(gridX, gridY)) {
        const gridPart: GridPart = {
          gridX,
          gridY,
          type: part.type,
          shape: part.shape,
          color: ColorComponent.getPartColor(part.type),
          rotation: 0, // Templates start with 0 rotation
        };
        this.buildGrid?.addPart(gridPart);
      }
    });
  }

  rotatePart(): void {
    this.buildState.selectedRotation =
      (this.buildState.selectedRotation + 1) % 4;
  }

  getSelectedRotation(): number {
    return this.buildState.selectedRotation;
  }
}
