import { BuildGrid, GridPart, type AttachedComponent } from "../../ui/shipbuilder/BuildGrid";
import { ColorComponent } from "../components/ColorComponent";
import type { ComponentConfig } from "../../ui/shipbuilder/ComponentDialog";

export interface BuildState {
  isActive: boolean;
  selectedPartType: "hull" | "shield" | "engine" | null;
  selectedShape: "triangle" | "square" | null;
  selectedRotation: number; // 0=0°, 1=90°, 2=180°, 3=270°
  pendingShape: "triangle" | "square" | null;
  pendingConfig: ComponentConfig | null;
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
      pendingShape: null,
      pendingConfig: null,
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

  setPendingBlock(shape: "triangle" | "square", config: ComponentConfig): void {
    this.buildState.pendingShape = shape;
    this.buildState.pendingConfig = config;
    this.buildState.selectedShape = shape;

    // Set type based on component (or default to hull)
    if (config.type === "engine") {
      this.buildState.selectedPartType = "engine";
    } else if (config.type === "shield") {
      this.buildState.selectedPartType = "shield";
    } else {
      this.buildState.selectedPartType = "hull";
    }
  }

  updateGhostPosition(gridX: number, gridY: number): void {
    if (!this.buildState.isActive || !this.buildGrid || !this.buildState.selectedPartType || !this.buildState.selectedShape) {
      return;
    }

    if (this.isValidPlacement(gridX, gridY)) {
      this.buildGrid.showGhost(gridX, gridY, this.buildState.selectedPartType, this.buildState.selectedShape, this.buildState.selectedRotation);
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

    const attachedComponents: AttachedComponent[] = [];

    // Add attached component if specified
    if (this.buildState.pendingConfig && this.buildState.pendingConfig.type !== "empty") {
      const config = this.buildState.pendingConfig;

      // Create properly typed component based on discriminated union
      if (config.type === "engine" && config.engineThrust) {
        attachedComponents.push({
          type: "engine",
          engineThrust: config.engineThrust,
        });
      } else if (config.type === "shield" && config.shieldCharge && config.shieldRadius) {
        attachedComponents.push({
          type: "shield",
          shieldCharge: config.shieldCharge,
          shieldRadius: config.shieldRadius,
        });
      } else if (config.type === "weapon" && config.weaponDamage && config.weaponRateOfFire) {
        attachedComponents.push({
          type: "weapon",
          weaponDamage: config.weaponDamage,
          weaponRateOfFire: config.weaponRateOfFire,
        });
      }
    }

    const part: GridPart = {
      gridX,
      gridY,
      type: this.buildState.selectedPartType,
      shape: this.buildState.selectedShape,
      color: ColorComponent.getPartColor(this.buildState.selectedPartType),
      rotation: this.buildState.selectedRotation,
      attachedComponents: attachedComponents.length > 0 ? attachedComponents : undefined,
    };

    this.buildGrid.addPart(part);

    // Don't clear pendingConfig - allow repeated placements with same configuration
    // Only clear pendingShape to allow shape changes between placements
    this.buildState.pendingShape = null;

    return true;
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

  isInBuildMode(): boolean {
    return this.buildState.isActive;
  }

  getSelectedPart(): { type: string | null; shape: string | null } {
    return {
      type: this.buildState.selectedPartType,
      shape: this.buildState.selectedShape,
    };
  }

  rotatePart(): void {
    this.buildState.selectedRotation = (this.buildState.selectedRotation + 1) % 4;
  }
}
