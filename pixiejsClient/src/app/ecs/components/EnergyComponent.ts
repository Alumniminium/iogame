import { Component } from "../core/Component";
import { World } from "../core/World";

export interface EnergyConfig {
  batteryCapacity: number;
  availableCharge?: number;
  chargeRate?: number;
  dischargeRate?: number;
}

export class EnergyComponent extends Component {
  dischargeRateAcc: number; // Accumulated discharge for this frame
  dischargeRate: number; // Current discharge rate
  chargeRate: number; // Charging rate per second
  availableCharge: number; // Current charge
  batteryCapacity: number; // Maximum charge

  constructor(entityId: string, config: EnergyConfig) {
    super(entityId);

    this.batteryCapacity = config.batteryCapacity;
    this.availableCharge =
      config.availableCharge !== undefined
        ? config.availableCharge
        : config.batteryCapacity;
    this.chargeRate = config.chargeRate || 0;
    this.dischargeRate = config.dischargeRate || 0;
    this.dischargeRateAcc = 0;
  }

  canConsume(amount: number): boolean {
    return this.availableCharge >= amount;
  }

  consume(amount: number): boolean {
    if (this.availableCharge >= amount) {
      this.availableCharge -= amount;
      this.dischargeRateAcc += amount;
      this.changedTick = World.currentTick;
      return true;
    }
    return false;
  }

  charge(amount: number): void {
    this.availableCharge = Math.min(
      this.batteryCapacity,
      this.availableCharge + amount,
    );
    this.changedTick = World.currentTick;
  }

  getChargePercentage(): number {
    return this.availableCharge / this.batteryCapacity;
  }
}
