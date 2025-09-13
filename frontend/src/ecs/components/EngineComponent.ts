import { Component } from '../core/Component';

export class EngineComponent extends Component {
  throttle: number = 0; // 0-1 (0-100%)
  maxThrottle: number = 1;
  powerDraw: number = 0; // kW
  rcsActive: boolean = false;

  constructor(entityId: number) {
    super(entityId);
  }

  setThrottle(value: number): void {
    this.throttle = Math.max(0, Math.min(this.maxThrottle, value));
    this.markChanged();
  }

  get throttlePercentage(): number {
    return Math.round(this.throttle * 100);
  }

  setPowerDraw(kw: number): void {
    this.powerDraw = kw;
    this.markChanged();
  }

  setRcs(active: boolean): void {
    this.rcsActive = active;
    this.markChanged();
  }
}