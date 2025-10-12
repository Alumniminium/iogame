import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.HealthRegen)
export class HealthRegenComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "f32") passiveHealPerSec: number;

  constructor(ntt: NTT, passiveHealPerSec: number = 0) {
    super(ntt);
    this.passiveHealPerSec = passiveHealPerSec;
  }
}
