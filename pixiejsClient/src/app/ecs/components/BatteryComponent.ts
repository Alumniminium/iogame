import { Component } from "../core/Component";

export interface BatteryConfig {
  capacity?: number;
  currentCharge?: number;
  chargeRate?: number;
  dischargeRate?: number;
  enginePowerDraw?: number;
  shieldPowerDraw?: number;
  weaponPowerDraw?: number;
}

export class BatteryComponent extends Component {
  capacity: number;
  currentCharge: number;
  chargeRate: number;
  dischargeRate: number;
  enginePowerDraw: number;
  shieldPowerDraw: number;
  weaponPowerDraw: number;

  constructor(entityId: string, config: BatteryConfig = {}) {
    super(entityId);

    this.capacity = config.capacity || 100;
    this.currentCharge =
      config.currentCharge !== undefined ? config.currentCharge : this.capacity;
    this.chargeRate = config.chargeRate || 0;
    this.dischargeRate = config.dischargeRate || 0;
    this.enginePowerDraw = config.enginePowerDraw || 0;
    this.shieldPowerDraw = config.shieldPowerDraw || 0;
    this.weaponPowerDraw = config.weaponPowerDraw || 0;
  }
}
