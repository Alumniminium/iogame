import { Component } from '../core/Component';

export interface EnergyConfig {
  max: number;
  current?: number;
  regenRate?: number;
  consumptionRate?: number;
}

export class EnergyComponent extends Component {
  current: number;
  max: number;
  regenRate: number;
  consumptionRate: number;
  isRecharging: boolean;

  constructor(entityId: number, config: EnergyConfig) {
    super(entityId);

    this.max = config.max;
    this.current = config.current !== undefined ? config.current : config.max;
    this.regenRate = config.regenRate || 0;
    this.consumptionRate = config.consumptionRate || 0;
    this.isRecharging = true;
  }
}