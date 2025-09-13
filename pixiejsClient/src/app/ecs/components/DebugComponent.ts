import { Component } from "../core/Component";

export interface DebugConfig {
  debugType: string;
  targetEntityId?: number;
}

export class DebugComponent extends Component {
  debugType: string;
  targetEntityId?: number;

  constructor(entityId: number, config: DebugConfig) {
    super(entityId);

    this.debugType = config.debugType;
    this.targetEntityId = config.targetEntityId;
  }
}