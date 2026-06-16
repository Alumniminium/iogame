import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.BodyDamage)
export class BodyDamageComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "f32") damage: number;

  constructor(ntt: NTT, damage: number = 1) {
    super(ntt);
    this.damage = damage;
  }
}
