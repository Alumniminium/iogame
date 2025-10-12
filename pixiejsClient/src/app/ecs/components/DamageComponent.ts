import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.Damage)
export class DamageComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "guid") attacker: string;
  @serverField(2, "f32") damage: number;

  constructor(ntt: NTT, attacker: string = "", damage: number = 0) {
    super(ntt);
    this.attacker = attacker;
    this.damage = damage;
  }
}
