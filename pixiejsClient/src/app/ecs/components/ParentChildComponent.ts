import { Component } from "../core/Component";

export interface ParentChildConfig {
  parentId: string;
  gridX?: number;
  gridY?: number;
  shape?: number;
  rotation?: number;
}

export class ParentChildComponent extends Component {
  parentId: string;
  gridX: number;
  gridY: number;
  shape: number;
  rotation: number;

  constructor(entityId: string, config: ParentChildConfig | string) {
    super(entityId);

    if (typeof config === "string") {
      this.parentId = config;
      this.gridX = 0;
      this.gridY = 0;
      this.shape = 0;
      this.rotation = 0;
    } else {
      this.parentId = config.parentId;
      this.gridX = config.gridX || 0;
      this.gridY = config.gridY || 0;
      this.shape = config.shape || 0;
      this.rotation = config.rotation || 0;
    }
  }
}
