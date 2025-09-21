export interface PartPlacement {
  gridX: number;
  gridY: number;
  type: "hull" | "shield" | "engine";
  shape: "triangle" | "square";
  rotation?: number;
}

export interface ShipTemplate {
  name: string;
  description: string;
  parts: PartPlacement[];
  centerX: number; // Center of mass for positioning
  centerY: number;
}

export class ShipTemplates {
  static readonly FIGHTER: ShipTemplate = {
    name: "Fighter",
    description: "Fast attack ship with basic hull and engine",
    centerX: 2,
    centerY: 2,
    parts: [
      // Core hull
      { gridX: 2, gridY: 2, type: "hull", shape: "square" },
      { gridX: 1, gridY: 2, type: "hull", shape: "square" },
      { gridX: 3, gridY: 2, type: "hull", shape: "square" },

      // Front nose
      { gridX: 2, gridY: 1, type: "hull", shape: "triangle" },

      // Engine at back
      { gridX: 2, gridY: 3, type: "engine", shape: "square" },

      // Wing tips
      { gridX: 0, gridY: 2, type: "hull", shape: "triangle" },
      { gridX: 4, gridY: 2, type: "hull", shape: "triangle" },
    ],
  };

  static readonly CRUISER: ShipTemplate = {
    name: "Cruiser",
    description: "Balanced ship with shields and multiple engines",
    centerX: 2,
    centerY: 3,
    parts: [
      // Main hull core
      { gridX: 2, gridY: 3, type: "hull", shape: "square" },
      { gridX: 1, gridY: 3, type: "hull", shape: "square" },
      { gridX: 3, gridY: 3, type: "hull", shape: "square" },
      { gridX: 2, gridY: 2, type: "hull", shape: "square" },
      { gridX: 2, gridY: 4, type: "hull", shape: "square" },

      // Shield generator in center
      { gridX: 2, gridY: 3, type: "shield", shape: "square" },

      // Multiple engines
      { gridX: 1, gridY: 5, type: "engine", shape: "square" },
      { gridX: 3, gridY: 5, type: "engine", shape: "square" },

      // Front sections
      { gridX: 1, gridY: 1, type: "hull", shape: "triangle" },
      { gridX: 3, gridY: 1, type: "hull", shape: "triangle" },
      { gridX: 2, gridY: 0, type: "hull", shape: "triangle" },
    ],
  };

  static readonly INTERCEPTOR: ShipTemplate = {
    name: "Interceptor",
    description: "Minimal fast ship for hit-and-run tactics",
    centerX: 1,
    centerY: 2,
    parts: [
      // Minimal hull
      { gridX: 1, gridY: 2, type: "hull", shape: "square" },
      { gridX: 1, gridY: 1, type: "hull", shape: "triangle" },

      // Single powerful engine
      { gridX: 1, gridY: 3, type: "engine", shape: "square" },

      // Wing stubs
      { gridX: 0, gridY: 2, type: "hull", shape: "triangle" },
      { gridX: 2, gridY: 2, type: "hull", shape: "triangle" },
    ],
  };

  static readonly DEFENDER: ShipTemplate = {
    name: "Defender",
    description: "Heavy armor and shields for defensive roles",
    centerX: 2,
    centerY: 2,
    parts: [
      // Heavy hull core
      { gridX: 2, gridY: 2, type: "hull", shape: "square" },
      { gridX: 1, gridY: 1, type: "hull", shape: "square" },
      { gridX: 3, gridY: 1, type: "hull", shape: "square" },
      { gridX: 1, gridY: 2, type: "hull", shape: "square" },
      { gridX: 3, gridY: 2, type: "hull", shape: "square" },
      { gridX: 1, gridY: 3, type: "hull", shape: "square" },
      { gridX: 3, gridY: 3, type: "hull", shape: "square" },

      // Multiple shield generators
      { gridX: 1, gridY: 1, type: "shield", shape: "square" },
      { gridX: 3, gridY: 1, type: "shield", shape: "square" },

      // Engine at back
      { gridX: 2, gridY: 4, type: "engine", shape: "square" },

      // Armor points
      { gridX: 2, gridY: 0, type: "hull", shape: "triangle" },
      { gridX: 0, gridY: 2, type: "hull", shape: "triangle" },
      { gridX: 4, gridY: 2, type: "hull", shape: "triangle" },
    ],
  };

  static getAllTemplates(): ShipTemplate[] {
    return [this.FIGHTER, this.CRUISER, this.INTERCEPTOR, this.DEFENDER];
  }

  static getTemplate(name: string): ShipTemplate | null {
    return (
      this.getAllTemplates().find((template) => template.name === name) || null
    );
  }
}
