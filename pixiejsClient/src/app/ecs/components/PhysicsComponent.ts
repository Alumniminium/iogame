import { Component } from "../core/Component";
import { Vector2 } from "../core/types";

export enum ShapeType {
  Circle = 0,
  Triangle = 1,
  Box = 2,
  Rectangle = 4,
}

export interface PhysicsConfig {
  position: Vector2;
  size: number;
  density?: number;
  drag?: number;
  elasticity?: number;
  velocity?: Vector2;
  acceleration?: Vector2;
  rotation?: number;
  angularVelocity?: number;
  shapeType?: ShapeType;
  width?: number;
  height?: number;
  color?: number;
  sides?: number;
}

export class PhysicsComponent extends Component {
  // Transform Data
  position: Vector2;
  lastPosition: Vector2;
  rotationRadians: number;
  lastRotation: number;

  // Motion Data
  linearVelocity: Vector2;
  acceleration: Vector2;
  angularVelocity: number;

  // Physics Properties
  readonly density: number;
  readonly elasticity: number;
  drag: number;
  inertia: number;

  // Shape Data
  readonly shapeType: ShapeType;
  size: number; // Diameter for circles
  width: number; // For boxes/triangles
  height: number; // For boxes/triangles
  readonly sides: number;
  readonly color: number;

  // Update Flags
  transformUpdateRequired: boolean;
  aabbUpdateRequired: boolean;
  changedTick: number;

  constructor(entityId: number, config: PhysicsConfig) {
    super(entityId);

    // Transform Data
    this.position = { ...config.position };
    this.lastPosition = { ...this.position };
    this.rotationRadians = config.rotation || 0;
    this.lastRotation = this.rotationRadians;

    // Motion Data
    this.linearVelocity = config.velocity
      ? { ...config.velocity }
      : { x: 0, y: 0 };
    this.acceleration = config.acceleration
      ? { ...config.acceleration }
      : { x: 0, y: 0 };
    this.angularVelocity = config.angularVelocity || 0;

    // Physics Properties
    this.density = config.density || 1;
    this.elasticity = config.elasticity || 0.8;
    this.drag = config.drag || 0.002; // Server default
    this.inertia = 0; // Will be calculated based on shape

    // Shape Data
    this.shapeType = config.shapeType || ShapeType.Circle;
    this.size = config.size;
    this.width = config.width || config.size;
    this.height = config.height || config.size;
    this.sides = config.sides || 0;
    this.color = config.color || 0xffffff;

    // Update Flags
    this.transformUpdateRequired = true;
    this.aabbUpdateRequired = true;
    this.changedTick = 0;

    // Calculate inertia based on shape
    this.calculateInertia();
  }

  private calculateInertia(): void {
    const area = this.getArea();
    const mass = area * this.density;

    switch (this.shapeType) {
      case ShapeType.Circle:
        this.inertia = 0.5 * mass * (this.radius * this.radius);
        break;
      case ShapeType.Box:
        this.inertia =
          (mass * (this.width * this.width + this.height * this.height)) / 12;
        break;
      default:
        this.inertia = mass * (this.radius * this.radius);
        break;
    }
  }

  // Computed Properties (matching server)
  get radius(): number {
    return this.size / 2;
  }

  get mass(): number {
    return this.getArea() * this.density;
  }

  get invMass(): number {
    return 1.0 / this.mass;
  }

  get invInertia(): number {
    return this.inertia > 0 ? 1.0 / this.inertia : 0;
  }

  get forward(): Vector2 {
    return {
      x: Math.cos(this.rotationRadians),
      y: Math.sin(this.rotationRadians),
    };
  }

  // Utility methods matching server behavior
  getArea(): number {
    switch (this.shapeType) {
      case ShapeType.Circle:
        return Math.PI * this.radius * this.radius;
      case ShapeType.Box:
        return this.width * this.height;
      case ShapeType.Triangle:
        return (this.width * this.height) / 2;
      default:
        return Math.PI * this.radius * this.radius;
    }
  }

  setPosition(position: Vector2): void {
    this.position = { ...position };
    this.transformUpdateRequired = true;
    this.aabbUpdateRequired = true;
    this.markChanged();
  }

  setVelocity(velocity: Vector2): void {
    this.linearVelocity = { ...velocity };
    this.markChanged();
  }

  addForce(force: Vector2): void {
    // Server doesn't use mass in physics calculations - forces are added directly as acceleration
    this.acceleration.x += force.x;
    this.acceleration.y += force.y;
    this.markChanged();
  }

  setRotation(rotation: number): void {
    this.rotationRadians = rotation;
    this.transformUpdateRequired = true;
    this.markChanged();
  }

  addTorque(torque: number): void {
    this.angularVelocity += torque * this.invInertia;
    this.markChanged();
  }

  getSpeed(): number {
    return Math.sqrt(
      this.linearVelocity.x * this.linearVelocity.x +
        this.linearVelocity.y * this.linearVelocity.y,
    );
  }

  getDirection(): Vector2 {
    const speed = this.getSpeed();
    if (speed === 0) return { x: 0, y: 0 };
    return {
      x: this.linearVelocity.x / speed,
      y: this.linearVelocity.y / speed,
    };
  }
}
