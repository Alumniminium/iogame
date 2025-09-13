import { Component } from '../core/Component';

export class HealthComponent extends Component {
  health: number;
  readonly maxHealth: number;

  constructor(entityId: number, health: number, maxHealth: number) {
    super(entityId);
    this.health = health;
    this.maxHealth = maxHealth;
  }

  takeDamage(amount: number): void {
    this.health = Math.max(0, this.health - amount);
    this.markChanged();
  }

  heal(amount: number): void {
    this.health = Math.min(this.maxHealth, this.health + amount);
    this.markChanged();
  }

  get healthPercentage(): number {
    return (this.health / this.maxHealth) * 100;
  }
}