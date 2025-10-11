import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.Health)
export class HealthComponent extends Component {
  // Match C# struct layout
  // changedTick is inherited from Component base class
  @serverField(1, "f32") Health: number;
  @serverField(2, "f32") MaxHealth: number;

  constructor(entityId: string, health?: number, maxHealth?: number) {
    super(entityId);

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
