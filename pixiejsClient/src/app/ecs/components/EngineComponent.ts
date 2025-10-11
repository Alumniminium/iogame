import { Component, component, serverField } from "../core/Component";
import { ServerComponentType as ServerComponentType } from "../../enums/ComponentIds";

export interface EngineConfig {
  maxPropulsion: number;
  powerUse?: number;
  throttle?: number;
  rcs?: boolean;
}

@component(ServerComponentType.Engine)
export class EngineComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "f32") powerUse: number; // MaxPropulsion * 0.01
  @serverField(2, "f32") throttle: number; // 0.0 to 1.0
  @serverField(3, "f32") maxPropulsion: number; // Maximum thrust force in Newtons
  @serverField(4, "bool") rcs: boolean; // Reaction Control System

  constructor(entityId: string, config?: EngineConfig) {
    super(entityId);

    if (config) {
      this.maxPropulsion = config.maxPropulsion;
      this.powerUse = config.powerUse !== undefined ? config.powerUse : this.maxPropulsion * 0.01;
      this.throttle = config.throttle || 0;
      this.rcs = config.rcs !== undefined ? config.rcs : true;
    } else {
      // Defaults for deserialization
      this.maxPropulsion = 0;
      this.powerUse = 0;
      this.throttle = 0;
      this.rcs = true;
    }
  }
}
