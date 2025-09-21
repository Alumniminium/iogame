import { Component } from "../core/Component";

export interface HullConfig {
  maxHealth?: number;
  currentHealth?: number;
  mass?: number;
  armor?: number;
  shape?: "triangle" | "square";
}

export class HullComponent extends Component {
  maxHealth: number;
  currentHealth: number;
  mass: number;
  armor: number;
  shape: "triangle" | "square";

  constructor(entityId: string, config: HullConfig = {}) {
    super(entityId);

    this.maxHealth = config.maxHealth || 100;
    this.currentHealth = config.currentHealth || this.maxHealth;
    this.mass = config.mass || 10;
    this.armor = config.armor || 5;
    this.shape = config.shape || "square";
  }

  takeDamage(amount: number): boolean {
    this.currentHealth = Math.max(
      0,
      this.currentHealth - Math.max(0, amount - this.armor),
    );
    this.markChanged();
    return this.currentHealth <= 0;
  }

  repair(amount: number): void {
    this.currentHealth = Math.min(this.maxHealth, this.currentHealth + amount);
    this.markChanged();
  }

  getHealthPercentage(): number {
    return this.currentHealth / this.maxHealth;
  }
}
