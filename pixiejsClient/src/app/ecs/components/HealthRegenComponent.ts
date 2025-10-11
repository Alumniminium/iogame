import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.HealthRegen)
export class HealthRegenComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "f32") passiveHealPerSec: number;

  constructor(entityId: string, passiveHealPerSec: number = 0) {
    super(entityId);
    this.passiveHealPerSec = passiveHealPerSec;
  }
}
