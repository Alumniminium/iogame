import { Component } from "../core/Component";

export class HealthComponent extends Component {
  Health: number;
  MaxHealth: number;

  constructor(entityId: string, health: number, maxHealth: number) {
    super(entityId);

    this.Health = health;
    this.MaxHealth = maxHealth;
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
