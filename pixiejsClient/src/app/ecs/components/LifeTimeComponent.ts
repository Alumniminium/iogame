import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.Lifetime)
export class LifeTimeComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "f32") public lifetimeSeconds: number;

  constructor(ntt: NTT, lifetimeSeconds?: number) {
    super(ntt);

    if (lifetimeSeconds !== undefined) {
      this.lifetimeSeconds = lifetimeSeconds;
    } else {
      // Defaults for deserialization
      this.lifetimeSeconds = 0;
    }
  }
}
