import { Component } from '../core/Component';

export class ShieldComponent extends Component {
  charge: number;
  maxCharge: number;
  powerConsumption: number; // kW when active
  active: boolean = false;
  radius: number;

  constructor(entityId: number, maxCharge: number = 100, radius: number = 25) {
    super(entityId);
    this.charge = maxCharge;
    this.maxCharge = maxCharge;
    this.powerConsumption = 0;
    this.radius = radius;
  }

  activate(): void {
    this.active = true;
    this.markChanged();
  }

  deactivate(): void {
    this.active = false;
    this.markChanged();
  }

  toggle(): void {
    this.active = !this.active;
    this.markChanged();
  }

  consumeCharge(amount: number): boolean {
    if (this.charge >= amount) {
      this.charge = Math.max(0, this.charge - amount);
      this.markChanged();
      return true;
    }
    return false;
  }

  recharge(amount: number): void {
    this.charge = Math.min(this.maxCharge, this.charge + amount);
    this.markChanged();
  }

  update(deltaTime: number): void {
    // Auto-recharge when not active
    if (!this.active && this.charge < this.maxCharge) {
      this.recharge(20 * deltaTime); // 20 charge per second
    }

    // Consume power when active
    if (this.active && this.charge > 0) {
      this.consumeCharge(10 * deltaTime); // 10 charge per second when active
      this.powerConsumption = 25; // 25kW when active
    } else {
      this.powerConsumption = 0;
    }

    // Auto-deactivate if no charge
    if (this.active && this.charge <= 0) {
      this.deactivate();
    }
  }

  get chargePercentage(): number {
    return (this.charge / this.maxCharge) * 100;
  }

  get isEffective(): boolean {
    return this.active && this.charge > 0;
  }
}