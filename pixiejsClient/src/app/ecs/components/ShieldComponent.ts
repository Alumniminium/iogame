import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { World } from "../core/World";

export interface ShieldConfig {
  maxCharge: number;
  charge?: number;
  powerUse?: number;
  powerUseRecharge?: number;
  radius?: number;
  minRadius?: number;
  targetRadius?: number;
  rechargeRate?: number;
  rechargeDelayMs?: number;
}

@component(ServerComponentType.Shield)
export class ShieldComponent extends Component {
  // Match C# struct layout exactly
  // changedTick is inherited from Component base class
  @serverField(1, "bool") powerOn: boolean = true;
  @serverField(2, "bool") lastPowerOn: boolean = true;
  @serverField(3, "f32") charge: number;
  @serverField(4, "f32") maxCharge: number;
  @serverField(5, "f32") powerUse: number;
  @serverField(6, "f32") powerUseRecharge: number;
  @serverField(7, "f32") radius: number;
  @serverField(8, "f32") minRadius: number;
  @serverField(9, "f32") targetRadius: number;
  @serverField(10, "f32") rechargeRate: number;
  @serverField(11, "i64") rechargeDelayTicks: bigint = 0n;
  @serverField(12, "i64") lastDamageTimeTicks: bigint = 0n;

  // Non-serialized client-side properties
  rechargeDelayMs: number;
  lastDamageTime: number;

  constructor(entityId: string, config?: ShieldConfig) {
    super(entityId);

    if (config) {
      this.maxCharge = config.maxCharge;
      this.charge = config.charge !== undefined ? config.charge : config.maxCharge;
      this.powerUse = config.powerUse || 10;
      this.powerUseRecharge = config.powerUseRecharge || this.powerUse * 2.5;
      this.radius = config.radius || 50;
      this.minRadius = config.minRadius || 20;
      this.targetRadius = config.targetRadius || 50;
      this.rechargeRate = config.rechargeRate || 10;
      this.rechargeDelayMs = config.rechargeDelayMs || 5000;

      // Convert milliseconds to ticks for server compatibility
      this.rechargeDelayTicks = BigInt(Math.floor(((config.rechargeDelayMs || 5000) / 1000) * 60));
    } else {
      // Default values for deserialization
      this.maxCharge = 100;
      this.charge = 100;
      this.powerUse = 10;
      this.powerUseRecharge = 25;
      this.radius = 50;
      this.minRadius = 20;
      this.targetRadius = 50;
      this.rechargeRate = 10;
      this.rechargeDelayMs = 5000;
    }

    this.powerOn = false;
    this.lastPowerOn = false;
    this.radius = this.calculateRadius();
    this.lastDamageTime = 0;
    this.lastDamageTimeTicks = 0n;
  }

  private calculateRadius(): number {
    const chargePercent = this.charge / this.maxCharge;
    return Math.max(this.minRadius, this.targetRadius * chargePercent);
  }

  updateRadius(): void {
    const newRadius = this.calculateRadius();
    if (Math.abs(this.radius - newRadius) > 0.1) {
      this.radius = newRadius;
      this.changedTick = World.currentTick;
    }
  }

  takeDamage(damage: number): number {
    const absorbedDamage = Math.min(damage, this.charge);
    this.charge -= absorbedDamage;
    this.lastDamageTime = Date.now();
    this.updateRadius();

    if (absorbedDamage > 0) {
      this.changedTick = World.currentTick;
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
      this.changedTick = World.currentTick;
    }
  }

  getChargePercentage(): number {
    return this.charge / this.maxCharge;
  }
}
