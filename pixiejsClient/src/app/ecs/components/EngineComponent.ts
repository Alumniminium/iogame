import { Component } from "../core/Component";

export interface EngineConfig {
  maxPropulsion: number;
  powerUse?: number;
  throttle?: number;
  rcs?: boolean;
  rotation?: number;
}

export class EngineComponent extends Component {
  powerUse: number; // MaxPropulsion * 2
  throttle: number; // 0.0 to 1.0
  maxPropulsion: number; // Maximum thrust force
  rcs: boolean; // Reaction Control System
  rotation: number; // -1 to 1 for turning
  changedTick: number;

  constructor(entityId: string, config: EngineConfig) {
    super(entityId);

    this.maxPropulsion = config.maxPropulsion;
    this.powerUse =
      config.powerUse !== undefined ? config.powerUse : this.maxPropulsion * 2;
    this.throttle = config.throttle || 0;
    this.rcs = config.rcs || false;
    this.rotation = config.rotation || 0;
    this.changedTick = 0;
  }
}
