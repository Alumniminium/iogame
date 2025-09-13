import { Component } from '../core/Component';
import { Vector2 } from '../core/types';

export interface PhysicsConfig {
  position: Vector2;
  size: number;
  mass?: number;
  drag?: number;
  elasticity?: number;
  velocity?: Vector2;
  acceleration?: Vector2;
  rotation?: number;
  angularVelocity?: number;
}

export class PhysicsComponent extends Component {
  position: Vector2;
  velocity: Vector2;
  acceleration: Vector2;
  rotation: number;
  angularVelocity: number;
  size: number;
  readonly mass: number;
  drag: number;
  elasticity: number;

  // State for interpolation/rendering
  lastPosition: Vector2;
  lastRotation: number;

  // Physics flags
  isStatic: boolean;
  isKinematic: boolean;

  constructor(entityId: number, config: PhysicsConfig) {
    super(entityId);

    this.position = { ...config.position };
    this.velocity = config.velocity ? { ...config.velocity } : { x: 0, y: 0 };
    this.acceleration = config.acceleration ? { ...config.acceleration } : { x: 0, y: 0 };
    this.rotation = config.rotation || 0;
    this.angularVelocity = config.angularVelocity || 0;
    this.size = config.size;
    this.mass = config.mass || 1;
    this.drag = config.drag || 0.02;
    this.elasticity = config.elasticity || 0.8;

    this.lastPosition = { ...this.position };
    this.lastRotation = this.rotation;

    this.isStatic = false;
    this.isKinematic = false;
  }

  // Utility methods for common physics operations
  setPosition(position: Vector2): void {
    this.position = { ...position };
    this.markChanged();
  }

  setVelocity(velocity: Vector2): void {
    this.velocity = { ...velocity };
    this.markChanged();
  }

  addForce(force: Vector2): void {
    this.acceleration.x += force.x / this.mass;
    this.acceleration.y += force.y / this.mass;
    this.markChanged();
  }

  setRotation(rotation: number): void {
    this.rotation = rotation;
    this.markChanged();
  }

  addTorque(torque: number): void {
    this.angularVelocity += torque / this.mass;
    this.markChanged();
  }

  getSpeed(): number {
    return Math.sqrt(this.velocity.x * this.velocity.x + this.velocity.y * this.velocity.y);
  }

  getDirection(): Vector2 {
    const speed = this.getSpeed();
    if (speed === 0) return { x: 0, y: 0 };
    return { x: this.velocity.x / speed, y: this.velocity.y / speed };
  }

  serialize(): Record<string, any> {
    return {
      ...super.serialize(),
      position: this.position,
      velocity: this.velocity,
      acceleration: this.acceleration,
      rotation: this.rotation,
      angularVelocity: this.angularVelocity,
      size: this.size,
      mass: this.mass,
      drag: this.drag,
      elasticity: this.elasticity,
      isStatic: this.isStatic,
      isKinematic: this.isKinematic
    };
  }
}