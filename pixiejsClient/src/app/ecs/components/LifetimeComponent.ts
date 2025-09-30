import { Component } from "../core/Component";

export class LifetimeComponent extends Component {
  public lifetimeSeconds: number;

  constructor(entityId: string, lifetimeSeconds: number) {
    super(entityId);
    this.lifetimeSeconds = lifetimeSeconds;
  }
}
