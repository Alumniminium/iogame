import { Vector2 as Vector2Interface } from '../ecs/core/types';

export class Vector2 implements Vector2Interface {
  x: number;
  y: number;

  constructor(x: number = 0, y: number = 0) {
    this.x = x;
    this.y = y;
  }

  static from(obj: Vector2Interface): Vector2 {
    return new Vector2(obj.x, obj.y);
  }

  add(other: Vector2Interface): Vector2 {
    return new Vector2(this.x + other.x, this.y + other.y);
  }

  subtract(other: Vector2Interface): Vector2 {
    return new Vector2(this.x - other.x, this.y - other.y);
  }

  multiply(scalar: number): Vector2 {
    return new Vector2(this.x * scalar, this.y * scalar);
  }

  divide(scalar: number): Vector2 {
    return new Vector2(this.x / scalar, this.y / scalar);
  }

  magnitude(): number {
    return Math.sqrt(this.x * this.x + this.y * this.y);
  }

  normalize(): Vector2 {
    const mag = this.magnitude();
    return mag > 0 ? this.divide(mag) : new Vector2(0, 0);
  }

  distance(other: Vector2Interface): number {
    const dx = this.x - other.x;
    const dy = this.y - other.y;
    return Math.sqrt(dx * dx + dy * dy);
  }

  clone(): Vector2 {
    return new Vector2(this.x, this.y);
  }

  toString(): string {
    return `(${this.x}, ${this.y})`;
  }
}