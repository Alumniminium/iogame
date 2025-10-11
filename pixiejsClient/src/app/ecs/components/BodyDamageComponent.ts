import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.BodyDamage)
export class BodyDamageComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "f32") damage: number;

  constructor(entityId: string, damage: number = 1) {
    super(entityId);
    this.damage = damage;
  }
}
