import { Component } from '../core/Component';

export class HealthRegenComponent extends Component {
  passiveHealPerSec: number;

  constructor(entityId: number, passiveHealPerSec: number = 5) {
    super(entityId);
    this.passiveHealPerSec = passiveHealPerSec;
  }
}