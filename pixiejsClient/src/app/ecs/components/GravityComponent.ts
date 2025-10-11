import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.Gravity)
export class GravityComponent extends Component {
  // Match C# struct layout
  // changedTick is inherited from Component base class
  @serverField(1, "f32") strength: number;
  @serverField(2, "f32") radius: number;

  constructor(entityId: string, strength?: number, radius?: number) {
    super(entityId);
    this.strength = strength ?? 100;
    this.radius = radius ?? 500;
  }
}
