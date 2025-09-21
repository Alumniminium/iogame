import { Component } from "../core/Component";

export interface ShieldGeneratorConfig {
  maxPower?: number;
  currentPower?: number;
  rechargeRate?: number;
  powerConsumption?: number;
  radius?: number;
  efficiency?: number;
  active?: boolean;
}

export class ShieldGeneratorComponent extends Component {
  maxPower: number;
  currentPower: number;
  rechargeRate: number; // Power per second
  powerConsumption: number; // Power per second when active
  radius: number; // Shield coverage radius
  efficiency: number; // 0-1, affects power consumption
  active: boolean;

  constructor(entityId: string, config: ShieldGeneratorConfig = {}) {
    super(entityId);

    this.maxPower = config.maxPower || 200;
    this.currentPower = config.currentPower || this.maxPower;
    this.rechargeRate = config.rechargeRate || 10;
    this.powerConsumption = config.powerConsumption || 15;
    this.radius = config.radius || 50;
    this.efficiency = config.efficiency || 1.0;
    this.active = config.active || false;
  }

  toggleActive(): void {
    this.active = !this.active;
    this.markChanged();
  }

  update(deltaTime: number, hasAvailablePower: boolean): void {
    if (this.active && hasAvailablePower && this.currentPower > 0) {
      // Consume power when active
      const consumption = this.powerConsumption * this.efficiency * deltaTime;
      this.currentPower = Math.max(0, this.currentPower - consumption);
    } else if (!this.active || !hasAvailablePower) {
      // Recharge when inactive or no power available
      this.currentPower = Math.min(this.maxPower, this.currentPower + this.rechargeRate * deltaTime);
    }
    this.markChanged();
  }

  getPowerPercentage(): number {
    return this.currentPower / this.maxPower;
  }

  canOperate(): boolean {
    return this.currentPower > 0;
  }
}