import { Component } from "../core/Component";
import { ComponentType } from "../../enums/ComponentIds";

export class LifetimeComponent extends Component {
  public lifetimeSeconds: number;

  constructor(entityId: string, lifetimeSeconds: number) {
    super(entityId);
    this.lifetimeSeconds = lifetimeSeconds;
  }
}
