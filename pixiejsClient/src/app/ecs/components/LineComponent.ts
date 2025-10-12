import { Component } from "../core/Component";

export interface Vector2 {
  x: number;
  y: number;
}

export class LineComponent extends Component {
  origin: Vector2;
  hit: Vector2;
  color: number;
  duration: number;
  createdAt?: number;

  constructor(entityId: string, origin: Vector2 = { x: 0, y: 0 }, hit: Vector2 = { x: 0, y: 0 }, color: number = 0xff0000, duration: number = 1000) {
    super(entityId);
    this.origin = origin;
    this.hit = hit;
    this.color = color;
    this.duration = duration;
  }
}
