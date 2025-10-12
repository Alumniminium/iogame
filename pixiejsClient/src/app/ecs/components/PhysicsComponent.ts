import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import type { Vector2 } from "../core/types";

/**
 * Physics shape types for collision detection
 */
export enum ShapeType {
  Circle = 0,
  Triangle = 1,
  Box = 2,
  Rectangle = 4,
}

/**
 * Configuration for creating a PhysicsComponent
 */
export interface Box2DBodyConfig {
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
  isStatic?: boolean;
}

/**
 * Box2D physics body component for entities.
 * Handles position, velocity, rotation, forces, and collision shape.
 * Mirrors server-side PhysicsComponent.
 */
@component(ServerComponentType.Physics)
export class PhysicsComponent extends Component {
  // Fields that match C# struct layout for serialization
  // changedTick is inherited from Component base class
  @serverField(1, "i64", { skip: true }) bodyId: bigint = 0n; // B2BodyId - not used in JS
  @serverField(2, "bool") isStatic: boolean = false;
  @serverField(3, "u32") color: number;
  @serverField(4, "f32") density: number;
  @serverField(5, "i32") sides: number;
  @serverField(6, "vector2") lastPosition: Vector2;
  @serverField(7, "f32") lastRotation: number;

  // Client-side properties not serialized
  position: Vector2;
  rotationRadians: number;
  linearVelocity: Vector2;
  acceleration: Vector2;
  angularVelocity: number;
  readonly elasticity: number;
  drag: number;
  inertia: number;
  shapeType: ShapeType; // Made non-readonly for fromBuffer
  size: number;
  width: number;
  height: number;

  private vertices: Vector2[] | null = null;
  private transformedVertices: Vector2[] | null = null;

  transformUpdateRequired: boolean;
  aabbUpdateRequired: boolean;

  constructor(entityId: string, config?: Box2DBodyConfig) {
    super(entityId);

    if (config) {
      this.position = { ...config.position };
      this.lastPosition = { ...this.position };
      this.rotationRadians = config.rotation || 0;
      this.lastRotation = this.rotationRadians;

      this.linearVelocity = config.velocity ? { ...config.velocity } : { x: 0, y: 0 };
      this.acceleration = config.acceleration ? { ...config.acceleration } : { x: 0, y: 0 };
      this.angularVelocity = config.angularVelocity || 0;

      this.density = config.density || 1;
      this.elasticity = config.elasticity || 0.8;
      this.drag = config.drag || 0.002;
      this.inertia = 0;

      this.shapeType = config.shapeType || ShapeType.Circle;
      this.size = config.size;
      this.width = config.width || config.size;
      this.height = config.height || config.size;
      this.sides = config.sides || 0;
      this.color = config.color || 0xffffff;
      this.isStatic = config.isStatic || false;
    } else {
      // Default values for deserialization
      this.position = { x: 0, y: 0 };
      this.lastPosition = { x: 0, y: 0 };
      this.rotationRadians = 0;
      this.lastRotation = 0;
      this.linearVelocity = { x: 0, y: 0 };
      this.acceleration = { x: 0, y: 0 };
      this.angularVelocity = 0;
      this.density = 1;
      this.elasticity = 0.8;
      this.drag = 0.002;
      this.inertia = 0;
      this.shapeType = ShapeType.Circle;
      this.size = 1;
      this.width = 1;
      this.height = 1;
      this.sides = 0;
      this.color = 0xffffff;
      this.isStatic = false;
    }

    this.transformUpdateRequired = true;
    this.aabbUpdateRequired = true;

    this.calculateInertia();

    if (this.shapeType !== ShapeType.Circle) {
      if (this.sides === 4 || this.shapeType === ShapeType.Box) {
        this.vertices = this.createBoxVertices(this.width, this.height);
      } else if (this.sides === 3) {
        this.vertices = this.createTriangleVertices(this.width, this.height);
      }

      if (this.vertices) this.transformedVertices = new Array(this.vertices.length);
    }
  }

  /**
   * Override fromBuffer to apply deserialized data to client-side properties
   */
  fromBuffer(reader: any): void {
    super.fromBuffer(reader);
    // After deserialization, sync client-side properties with server data
    this.position = { ...this.lastPosition };
    this.rotationRadians = this.lastRotation;
    // Derive shapeType from sides
    this.shapeType = this.sides === 3 ? ShapeType.Triangle : this.sides === 4 ? ShapeType.Box : ShapeType.Circle;
  }

  /**
   * Calculate moment of inertia based on shape and mass
   */
  private calculateInertia(): void {
    const area = this.getArea();
    const mass = area * this.density;

    switch (this.shapeType) {
      case ShapeType.Circle:
        this.inertia = 0.5 * mass * (this.radius * this.radius);
        break;
      case ShapeType.Box:
        this.inertia = (mass * (this.width * this.width + this.height * this.height)) / 12;
        break;
      default:
        this.inertia = mass * (this.radius * this.radius);
        break;
    }
  }

  /**
   * Get radius for circular shapes
   */
  get radius(): number {
    return this.size / 2;
  }

  /**
   * Calculate mass from area and density
   */
  get mass(): number {
    return this.getArea() * this.density;
  }

  /**
   * Inverse mass for physics calculations
   */
  get invMass(): number {
    return 1.0 / this.mass;
  }

  /**
   * Inverse moment of inertia for rotation calculations
   */
  get invInertia(): number {
    return this.inertia > 0 ? 1.0 / this.inertia : 0;
  }

  /**
   * Get forward direction vector based on current rotation
   */
  get forward(): Vector2 {
    return {
      x: Math.cos(this.rotationRadians),
      y: Math.sin(this.rotationRadians),
    };
  }

  /**
   * Calculate area based on shape type
   */
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

  /**
   * Set position and mark transforms for update
   */
  setPosition(position: Vector2): void {
    this.position = { ...position };
    this.transformUpdateRequired = true;
    this.aabbUpdateRequired = true;
  }

  /**
   * Set linear velocity
   */
  setVelocity(velocity: Vector2): void {
    this.linearVelocity = { ...velocity };
  }

  /**
   * Apply force to acceleration
   */
  addForce(force: Vector2): void {
    this.acceleration.x += force.x;
    this.acceleration.y += force.y;
  }

  /**
   * Set rotation and mark transforms for update
   */
  setRotation(rotation: number): void {
    this.rotationRadians = rotation;
    this.transformUpdateRequired = true;
  }

  /**
   * Apply torque to angular velocity
   */
  addTorque(torque: number): void {
    this.angularVelocity += torque * this.invInertia;
  }

  /**
   * Get magnitude of linear velocity
   */
  getSpeed(): number {
    return Math.sqrt(this.linearVelocity.x * this.linearVelocity.x + this.linearVelocity.y * this.linearVelocity.y);
  }

  /**
   * Get normalized direction of movement
   */
  getDirection(): Vector2 {
    const speed = this.getSpeed();
    if (speed === 0) return { x: 0, y: 0 };
    return {
      x: this.linearVelocity.x / speed,
      y: this.linearVelocity.y / speed,
    };
  }

  private createBoxVertices(width: number, height: number): Vector2[] {
    const halfWidth = width / 2;
    const halfHeight = height / 2;

    return [
      { x: -halfWidth, y: -halfHeight },
      { x: halfWidth, y: -halfHeight },
      { x: halfWidth, y: halfHeight },
      { x: -halfWidth, y: halfHeight },
    ];
  }

  private createTriangleVertices(width: number, height: number): Vector2[] {
    const halfWidth = width / 2;
    const halfHeight = height / 2;

    return [
      { x: 0, y: -halfHeight },
      { x: halfWidth, y: halfHeight },
      { x: -halfWidth, y: halfHeight },
    ];
  }

  /**
   * Get world-space vertices with rotation applied
   */
  getTransformedVertices(): Vector2[] | null {
    if (!this.vertices || !this.transformedVertices) return null;

    if (this.transformUpdateRequired) {
      const cos = Math.cos(this.rotationRadians);
      const sin = Math.sin(this.rotationRadians);

      for (let i = 0; i < this.vertices.length; i++) {
        const v = this.vertices[i];
        this.transformedVertices[i] = {
          x: v.x * cos - v.y * sin + this.position.x,
          y: v.x * sin + v.y * cos + this.position.y,
        };
      }
      this.transformUpdateRequired = false;
    }

    return this.transformedVertices;
  }
}
