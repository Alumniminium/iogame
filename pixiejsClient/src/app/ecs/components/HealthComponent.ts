import { Component } from "../core/Component";

export class HealthComponent extends Component {
  current: number;
  max: number;
  regenRate: number;
  isDead: boolean;

  constructor(
    entityId: string,
    max: number,
    current: number,
    regenRate: number,
  ) {
    super(entityId);

    this.max = max;
    this.current = current;
    this.regenRate = regenRate;
    this.isDead = this.current <= 0;
  }
}
