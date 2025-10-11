import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { World } from "../core/World";

export interface EnergyConfig {
  batteryCapacity: number;
  availableCharge?: number;
  chargeRate?: number;
  dischargeRate?: number;
}

@component(ServerComponentType.Energy)
export class EnergyComponent extends Component {
  // Match C# struct layout
  // changedTick is inherited from Component base class
  @serverField(1, "f32") dischargeRateAcc: number; // Accumulated discharge for this frame
  @serverField(2, "f32") dischargeRate: number; // Current discharge rate
  @serverField(3, "f32") chargeRate: number; // Charging rate per second
  @serverField(4, "f32") availableCharge: number; // Current charge
  @serverField(5, "f32") batteryCapacity: number; // Maximum charge

  constructor(entityId: string, config?: EnergyConfig) {
    super(entityId);

    if (config) {
      this.batteryCapacity = config.batteryCapacity;
      this.availableCharge =
        config.availableCharge !== undefined
          ? config.availableCharge
          : config.batteryCapacity;
      this.chargeRate = config.chargeRate || 0;
      this.dischargeRate = config.dischargeRate || 0;
      this.dischargeRateAcc = 0;
    } else {
      // Defaults for deserialization
      this.batteryCapacity = 100;
      this.availableCharge = 100;
      this.chargeRate = 0;
      this.dischargeRate = 0;
      this.dischargeRateAcc = 0;
    }
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
