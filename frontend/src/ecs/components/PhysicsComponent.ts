import { Component } from '../core/Component';
import { Vector2 } from '../core/types';

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
  lastPosition: Vector2;
  lastRotation: number;

  constructor(entityId: number, position: Vector2, size: number, mass: number = 1) {
    super(entityId);
    this.position = { ...position };
    this.velocity = { x: 0, y: 0 };
    this.acceleration = { x: 0, y: 0 };
    this.rotation = 0;
    this.angularVelocity = 0;
    this.size = size;
    this.mass = mass;
    this.drag = 0.02;
    this.elasticity = 0.8;
    this.lastPosition = { ...position };
    this.lastRotation = 0;
  }

  update(deltaTime: number): void {
    // Store last state for interpolation
    this.lastPosition = { ...this.position };
    this.lastRotation = this.rotation;
    
    // Update velocity based on acceleration
    this.velocity.x += this.acceleration.x * deltaTime;
    this.velocity.y += this.acceleration.y * deltaTime;

    // Update position based on velocity
    this.position.x += this.velocity.x * deltaTime;
    this.position.y += this.velocity.y * deltaTime;

    // Update rotation
    this.rotation += this.angularVelocity * deltaTime;

    // Apply drag
    const dragFactor = Math.pow(1 - this.drag, deltaTime);
    this.velocity.x *= dragFactor;
    this.velocity.y *= dragFactor;
    this.angularVelocity *= dragFactor;

    this.markChanged();
  }
}