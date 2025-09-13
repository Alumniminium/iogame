import { Component } from '../core/Component';

export class EnergyComponent extends Component {
  availableCharge: number;
  readonly batteryCapacity: number;
  chargeRate: number;
  dischargeRate: number;

  constructor(entityId: number, batteryCapacity: number, chargeRate: number = 10) {
    super(entityId);
    this.availableCharge = batteryCapacity;
    this.batteryCapacity = batteryCapacity;
    this.chargeRate = chargeRate;
    this.dischargeRate = 0;
  }

  consumeEnergy(amount: number): boolean {
    if (this.availableCharge >= amount) {
      this.availableCharge -= amount;
      this.markChanged();
      return true;
    }
    return false;
  }

  charge(amount: number): void {
    this.availableCharge = Math.min(this.batteryCapacity, this.availableCharge + amount);
    this.markChanged();
  }

  update(deltaTime: number): void {
    // Auto-charge over time
    if (this.availableCharge < this.batteryCapacity) {
      this.charge(this.chargeRate * deltaTime);
    }

    // Apply discharge rate
    if (this.dischargeRate > 0) {
      this.consumeEnergy(this.dischargeRate * deltaTime);
    }
  }

  get energyPercentage(): number {
    return (this.availableCharge / this.batteryCapacity) * 100;
  }
}