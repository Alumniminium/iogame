import { Component } from "../core/Component";

export class GravityComponent extends Component {
  strength: number;
  radius: number;

  constructor(entityId: string, strength: number, radius: number) {
    super(entityId);
    this.strength = strength;
    this.radius = radius;
  }
}
