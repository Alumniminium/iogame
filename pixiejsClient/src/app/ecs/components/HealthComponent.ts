import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.Health)
export class HealthComponent extends Component {
  // Match C# struct layout
  // changedTick is inherited from Component base class
  @serverField(1, "f32") Health: number;
  @serverField(2, "f32") MaxHealth: number;

  constructor(ntt: NTT, health?: number, maxHealth?: number) {
    super(ntt);

    this.Health = health ?? 100;
    this.MaxHealth = maxHealth ?? 100;
  }

  get current(): number {
    return this.Health;
  }

  get max(): number {
    return this.MaxHealth;
  }

  get isDead(): boolean {
    return this.Health <= 0;
  }
}
