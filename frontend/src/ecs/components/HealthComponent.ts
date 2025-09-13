import { Component } from '../core/Component';

export interface HealthConfig {
  max: number;
  current?: number;
  regenRate?: number;
}

export class HealthComponent extends Component {
  current: number;
  max: number;
  regenRate: number;
  isDead: boolean;

  constructor(entityId: number, config: HealthConfig) {
    super(entityId);

    this.max = config.max;
    this.current = config.current !== undefined ? config.current : config.max;
    this.regenRate = config.regenRate || 0;
    this.isDead = this.current <= 0;
  }
}