import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.Lifetime)
export class LifeTimeComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "f32") public lifetimeSeconds: number;

  constructor(entityId: string, lifetimeSeconds?: number) {
    super(entityId);

    if (lifetimeSeconds !== undefined) {
      this.lifetimeSeconds = lifetimeSeconds;
    } else {
      // Defaults for deserialization
      this.lifetimeSeconds = 0;
    }
  }
}
