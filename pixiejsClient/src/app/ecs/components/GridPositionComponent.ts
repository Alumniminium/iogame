import { Component } from "../core/Component";

export interface GridPositionConfig {
  gridX?: number;
  gridY?: number;
  rotation?: number;
}

export class GridPositionComponent extends Component {
  gridX: number;
  gridY: number;
  rotation: number; // 0, 90, 180, 270 degrees

  constructor(entityId: string, config: GridPositionConfig = {}) {
    super(entityId);

    this.gridX = config.gridX || 0;
    this.gridY = config.gridY || 0;
    this.rotation = config.rotation || 0;
  }

  setPosition(gridX: number, gridY: number): void {
    this.gridX = gridX;
    this.gridY = gridY;
    this.markChanged();
  }

  setRotation(rotation: number): void {
    this.rotation = rotation % 360;
    this.markChanged();
  }
}
