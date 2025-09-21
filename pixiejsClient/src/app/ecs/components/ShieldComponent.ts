import { Component } from "../core/Component";

export interface ShieldConfig {
  maxCharge: number;
  charge?: number;
  powerUse?: number;
  powerUseRecharge?: number;
  minRadius?: number;
  targetRadius?: number;
  rechargeRate?: number;
  rechargeDelayMs?: number;
}

export class ShieldComponent extends Component {
  powerOn: boolean;
  lastPowerOn: boolean;
  charge: number;
  maxCharge: number;
  powerUse: number; // Idle power consumption
  powerUseRecharge: number; // Recharge power (2.5x idle)
  radius: number;
  minRadius: number;
  targetRadius: number;
  rechargeRate: number;
  rechargeDelayMs: number; // Delay in milliseconds
  lastDamageTime: number; // Timestamp of last damage
  changedTick: number;

  constructor(entityId: string, config: ShieldConfig) {
    super(entityId);

    this.maxCharge = config.maxCharge;
    this.charge =
      config.charge !== undefined ? config.charge : config.maxCharge;
    this.powerUse = config.powerUse || 10;
    this.powerUseRecharge = config.powerUseRecharge || this.powerUse * 2.5;
    this.minRadius = config.minRadius || 20;
    this.targetRadius = config.targetRadius || 50;
    this.rechargeRate = config.rechargeRate || 10;
    this.rechargeDelayMs = config.rechargeDelayMs || 5000;

    this.powerOn = false;
    this.lastPowerOn = false;
    this.radius = this.calculateRadius();
    this.lastDamageTime = 0;
    this.changedTick = 0;
  }

  private calculateRadius(): number {
    const chargePercent = this.charge / this.maxCharge;
    return Math.max(this.minRadius, this.targetRadius * chargePercent);
  }

  updateRadius(): void {
    const newRadius = this.calculateRadius();
    if (Math.abs(this.radius - newRadius) > 0.1) {
      this.radius = newRadius;
      this.changedTick = Date.now(); // In real implementation, use game tick
      this.markChanged();
    }
  }

  takeDamage(damage: number): number {
    const absorbedDamage = Math.min(damage, this.charge);
    this.charge -= absorbedDamage;
    this.lastDamageTime = Date.now();
    this.updateRadius();

    if (absorbedDamage > 0) {
      this.changedTick = Date.now(); // In real implementation, use game tick
      this.markChanged();
    }

    return damage - absorbedDamage; // Return remaining damage
  }

  canRecharge(currentTime: number): boolean {
    return currentTime - this.lastDamageTime >= this.rechargeDelayMs;
  }

  recharge(amount: number): void {
    if (this.charge < this.maxCharge) {
      this.charge = Math.min(this.maxCharge, this.charge + amount);
      this.updateRadius();
      this.changedTick = Date.now(); // In real implementation, use game tick
      this.markChanged();
    }
  }

  getChargePercentage(): number {
    return this.charge / this.maxCharge;
  }
}
