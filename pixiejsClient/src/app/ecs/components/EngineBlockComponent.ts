import { Component } from "../core/Component";

export interface EngineBlockConfig {
  maxThrust?: number;
  currentThrust?: number;
  fuelConsumption?: number;
  efficiency?: number;
  direction?: number; // 0-360 degrees, direction of thrust relative to block
  thrustVector?: { x: number; y: number };
}

export class EngineBlockComponent extends Component {
  maxThrust: number;
  currentThrust: number;
  fuelConsumption: number; // Fuel per second at max thrust
  efficiency: number; // 0-1, affects fuel consumption
  direction: number; // Thrust direction in degrees
  thrustVector: { x: number; y: number };

  constructor(entityId: string, config: EngineBlockConfig = {}) {
    super(entityId);

    this.maxThrust = config.maxThrust || 50;
    this.currentThrust = config.currentThrust || 0;
    this.fuelConsumption = config.fuelConsumption || 5;
    this.efficiency = config.efficiency || 1.0;
    this.direction = config.direction || 0;
    this.thrustVector = config.thrustVector || { x: 0, y: 0 };

    this.updateThrustVector();
  }

  setThrust(percentage: number): void {
    this.currentThrust = Math.max(0, Math.min(1, percentage)) * this.maxThrust;
    this.updateThrustVector();
    this.markChanged();
  }

  setDirection(degrees: number): void {
    this.direction = degrees % 360;
    this.updateThrustVector();
    this.markChanged();
  }

  private updateThrustVector(): void {
    const radians = (this.direction * Math.PI) / 180;
    this.thrustVector.x = Math.cos(radians) * this.currentThrust;
    this.thrustVector.y = Math.sin(radians) * this.currentThrust;
  }

  getFuelConsumptionRate(): number {
    const thrustPercentage = this.currentThrust / this.maxThrust;
    return this.fuelConsumption * thrustPercentage * this.efficiency;
  }

  getThrustPercentage(): number {
    return this.currentThrust / this.maxThrust;
  }
}