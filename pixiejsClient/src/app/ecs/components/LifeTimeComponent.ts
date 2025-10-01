import { Component } from "../core/Component";

export class LifeTimeComponent extends Component {
  public lifetimeSeconds: number;

  constructor(entityId: string, lifetimeSeconds: number) {
    super(entityId);
    this.lifetimeSeconds = lifetimeSeconds;
  }
}
