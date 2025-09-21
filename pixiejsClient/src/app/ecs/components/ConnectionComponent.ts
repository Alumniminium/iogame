import { Component } from "../core/Component";

export interface ConnectionPoint {
  x: number; // Local offset from center
  y: number; // Local offset from center
  direction: "top" | "bottom" | "left" | "right";
}

export interface ConnectionConfig {
  connectionPoints?: ConnectionPoint[];
  connectedEntities?: string[];
}

export class ConnectionComponent extends Component {
  connectionPoints: ConnectionPoint[];
  connectedEntities: string[];

  constructor(entityId: string, config: ConnectionConfig = {}) {
    super(entityId);

    this.connectionPoints = config.connectionPoints || [];
    this.connectedEntities = config.connectedEntities || [];
  }

  addConnection(entityId: string): void {
    if (!this.connectedEntities.includes(entityId)) {
      this.connectedEntities.push(entityId);
      this.markChanged();
    }
  }

  removeConnection(entityId: string): void {
    const index = this.connectedEntities.indexOf(entityId);
    if (index !== -1) {
      this.connectedEntities.splice(index, 1);
      this.markChanged();
    }
  }

  hasConnectionAt(direction: string): boolean {
    return this.connectionPoints.some((point) => point.direction === direction);
  }
}
