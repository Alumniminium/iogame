import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { PhysicsComponent } from '../components/PhysicsComponent';
import { Vector2 } from '../core/types';

export class PhysicsSystem extends System {
  readonly componentTypes = [PhysicsComponent];

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const physics = entity.getComponent(PhysicsComponent)!;

    if (physics.isStatic) {
      return; // Static bodies don't move
    }

    // Store last state for interpolation
    physics.lastPosition = { ...physics.position };
    physics.lastRotation = physics.rotation;

    if (!physics.isKinematic) {
      // Apply physics integration for dynamic bodies
      this.integrateVelocity(physics, deltaTime);
      this.integratePosition(physics, deltaTime);
      this.applyDrag(physics, deltaTime);
    }
    // Kinematic bodies can have their position/rotation set directly by other systems
    // but we still update the interpolation data

    physics.markChanged();
  }

  private integrateVelocity(physics: PhysicsComponent, deltaTime: number): void {
    // Update velocity based on acceleration (F = ma, a = F/m)
    physics.velocity.x += physics.acceleration.x * deltaTime;
    physics.velocity.y += physics.acceleration.y * deltaTime;
    physics.angularVelocity += 0; // TODO: Add torque support if needed

    // Reset acceleration for next frame (forces are applied for one frame only)
    physics.acceleration.x = 0;
    physics.acceleration.y = 0;
  }

  private integratePosition(physics: PhysicsComponent, deltaTime: number): void {
    // Update position based on velocity (Verlet integration for better stability)
    physics.position.x += physics.velocity.x * deltaTime;
    physics.position.y += physics.velocity.y * deltaTime;

    // Update rotation based on angular velocity
    physics.rotation += physics.angularVelocity * deltaTime;

    // Normalize rotation to [-π, π]
    while (physics.rotation > Math.PI) physics.rotation -= 2 * Math.PI;
    while (physics.rotation < -Math.PI) physics.rotation += 2 * Math.PI;
  }

  private applyDrag(physics: PhysicsComponent, deltaTime: number): void {
    // Apply linear drag
    const dragFactor = Math.pow(1 - physics.drag, deltaTime);
    physics.velocity.x *= dragFactor;
    physics.velocity.y *= dragFactor;

    // Apply angular drag
    physics.angularVelocity *= dragFactor;

    // Stop very small velocities to prevent jitter
    if (Math.abs(physics.velocity.x) < 0.01) physics.velocity.x = 0;
    if (Math.abs(physics.velocity.y) < 0.01) physics.velocity.y = 0;
    if (Math.abs(physics.angularVelocity) < 0.01) physics.angularVelocity = 0;
  }

  // Utility methods for other systems to use
  static applyImpulse(physics: PhysicsComponent, impulse: Vector2): void {
    if (physics.isStatic) return;

    physics.velocity.x += impulse.x / physics.mass;
    physics.velocity.y += impulse.y / physics.mass;
    physics.markChanged();
  }

  static applyForceAtPoint(physics: PhysicsComponent, force: Vector2, point: Vector2): void {
    if (physics.isStatic) return;

    // Apply linear force
    physics.addForce(force);

    // Calculate torque from force at point
    const relativePoint = {
      x: point.x - physics.position.x,
      y: point.y - physics.position.y
    };
    const torque = relativePoint.x * force.y - relativePoint.y * force.x;
    physics.addTorque(torque);
  }

  static setStatic(physics: PhysicsComponent, isStatic: boolean): void {
    physics.isStatic = isStatic;
    if (isStatic) {
      // Clear velocities for static bodies
      physics.velocity = { x: 0, y: 0 };
      physics.angularVelocity = 0;
      physics.acceleration = { x: 0, y: 0 };
    }
    physics.markChanged();
  }

  static setKinematic(physics: PhysicsComponent, isKinematic: boolean): void {
    physics.isKinematic = isKinematic;
    if (isKinematic) {
      // Kinematic bodies don't respond to forces
      physics.acceleration = { x: 0, y: 0 };
    }
    physics.markChanged();
  }
}