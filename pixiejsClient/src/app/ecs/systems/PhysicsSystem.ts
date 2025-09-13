import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { AABBComponent } from "../components/AABBComponent";
import { NetworkComponent } from "../components/NetworkComponent";
import { Vector2 } from "../core/types";

// Server physics constants
const SPEED_LIMIT = 400; // units/second
const MAP_SIZE_X = 1500;
const MAP_SIZE_Y = 100000;
const GRAVITY_THRESHOLD = MAP_SIZE_Y - 1500;
const GRAVITY_FORCE = 9.81;
const VELOCITY_THRESHOLD = 0.1;
const ANGULAR_VELOCITY_THRESHOLD = 0.1;
const PHYSICS_SUBSTEPS = 8;

export class PhysicsSystem extends System {
  readonly componentTypes = [PhysicsComponent];

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const physics = entity.get(PhysicsComponent)!;
    const network = entity.get(NetworkComponent);

    // Skip physics updates for static entities (like server structures)
    // Static entities are non-locally-controlled entities that should not move
    const isStaticEntity = network && !network.isLocallyControlled &&
      Math.abs(physics.linearVelocity.x) < 0.01 && Math.abs(physics.linearVelocity.y) < 0.01;

    // Skip most physics processing for static entities (matching server's exclusion of EntityType.Static)
    if (isStaticEntity) {
      return;
    }

    // Get size for boundaries (server line 42)
    const sizeX = physics.shapeType === 0 ? physics.radius : physics.width;
    const sizeY = physics.shapeType === 0 ? physics.radius : physics.height;
    const substepDt = deltaTime / PHYSICS_SUBSTEPS;

    // Store last state for interpolation (server line 44-45)
    physics.lastPosition = { ...physics.position };
    physics.lastRotation = physics.rotationRadians;

    // Run physics in 8 substeps for better accuracy (matching server)
    for (let step = 0; step < PHYSICS_SUBSTEPS; step++) {
      // Apply gravity when below threshold (matching server line 47-48)
      if (physics.position.y > GRAVITY_THRESHOLD) {
        physics.acceleration.y += GRAVITY_FORCE * substepDt;
      }

      // Check for NaN in acceleration (server line 50-51)
      if (isNaN(physics.acceleration.x) || isNaN(physics.acceleration.y)) {
        physics.acceleration.x = 0;
        physics.acceleration.y = 0;
      }

      // Angular integration (server line 54)
      physics.rotationRadians += physics.angularVelocity * substepDt;

      // Angular drag (server line 55)
      physics.angularVelocity *= 1 - (physics.drag * substepDt);

      // Normalize rotation (server line 57-60)
      if (physics.rotationRadians > Math.PI * 2)
        physics.rotationRadians -= Math.PI * 2;
      if (physics.rotationRadians < 0)
        physics.rotationRadians += Math.PI * 2;

      // Angular threshold (server line 62-63)
      if (Math.abs(physics.angularVelocity) < ANGULAR_VELOCITY_THRESHOLD)
        physics.angularVelocity = 0;

      // Velocity integration (server line 66)
      physics.linearVelocity.x += physics.acceleration.x;
      physics.linearVelocity.y += physics.acceleration.y;

      // Speed limit (server line 67)
      const speed = Math.sqrt(physics.linearVelocity.x * physics.linearVelocity.x + physics.linearVelocity.y * physics.linearVelocity.y);
      if (speed > SPEED_LIMIT) {
        const scale = SPEED_LIMIT / speed;
        physics.linearVelocity.x *= scale;
        physics.linearVelocity.y *= scale;
      }

      // Linear drag (server line 68)
      physics.linearVelocity.x *= 1 - (physics.drag * substepDt);
      physics.linearVelocity.y *= 1 - (physics.drag * substepDt);

      // Reset acceleration (server line 71)
      physics.acceleration.x = 0;
      physics.acceleration.y = 0;

      // Check for NaN in velocity (server line 73-74)
      if (isNaN(physics.linearVelocity.x) || isNaN(physics.linearVelocity.y)) {
        physics.linearVelocity.x = 0;
        physics.linearVelocity.y = 0;
      }

      // Linear threshold (server line 76-77)
      if (Math.abs(physics.linearVelocity.x) < VELOCITY_THRESHOLD)
        physics.linearVelocity.x = 0;
      if (Math.abs(physics.linearVelocity.y) < VELOCITY_THRESHOLD)
        physics.linearVelocity.y = 0;

      // Position integration (server line 80-82)
      const newX = physics.position.x + physics.linearVelocity.x * substepDt;
      const newY = physics.position.y + physics.linearVelocity.y * substepDt;
      physics.position.x = Math.max(sizeX, Math.min(newX, MAP_SIZE_X - sizeX));
      physics.position.y = Math.max(sizeY, Math.min(newY, MAP_SIZE_Y - sizeY));

      // Boundary reflection (server line 85-86)
      if (physics.position.x === sizeX || physics.position.x === MAP_SIZE_X - sizeX) {
        physics.linearVelocity.x = -physics.linearVelocity.x * physics.elasticity;
      }
      if (physics.position.y === sizeY || physics.position.y === MAP_SIZE_Y - sizeY) {
        physics.linearVelocity.y = -physics.linearVelocity.y * physics.elasticity;
      }
    }

    // Update flags (server line 88-92)
    if (physics.position.x !== physics.lastPosition.x || physics.position.y !== physics.lastPosition.y ||
      physics.rotationRadians !== physics.lastRotation) {
      physics.transformUpdateRequired = true;
      physics.aabbUpdateRequired = true;
      physics.changedTick = Date.now(); // In real implementation, use game tick
      physics.markChanged();
    }

    // Update AABB if required (matching server AABB update logic exactly)
    if (physics.aabbUpdateRequired) {
      const aabb = entity.get(AABBComponent);
      if (aabb) {
        if (physics.shapeType !== 0) { // Non-circle (matching server ShapeType.Circle check)
          // For non-circles, calculate AABB from transformed vertices (matching server AABBSystem.cs:21-45)
          // TODO: Implement actual vertex transformation when rotation is supported
          // For now, use center-based calculation as approximation
          if (physics.shapeType === 2 || physics.shapeType === 4) { // Rectangle/Box (both type 2 and 4)
            aabb.updateAABBRect(physics.position.x, physics.position.y, physics.width, physics.height);
          } else {
            const size = Math.max(physics.width, physics.height);
            aabb.updateAABB(physics.position.x, physics.position.y, size / 2);
          }
        } else {
          // For circles, match server logic exactly (AABBSystem.cs:49-52)
          const radius = physics.size / 2;
          aabb.aabb.x = physics.position.x - radius;
          aabb.aabb.y = physics.position.y - radius;
          aabb.aabb.width = radius * 2;
          aabb.aabb.height = radius * 2;
          aabb.markChanged();
        }
      }
      physics.aabbUpdateRequired = false;
    }
  }



  // Utility methods for other systems to use (matching server)
  static applyImpulse(physics: PhysicsComponent, impulse: Vector2): void {
    // Server doesn't use mass - impulses are added directly to velocity
    physics.linearVelocity.x += impulse.x;
    physics.linearVelocity.y += impulse.y;
    physics.markChanged();
  }

  static applyForceAtPoint(
    physics: PhysicsComponent,
    force: Vector2,
    point: Vector2,
  ): void {
    // Apply linear force
    physics.addForce(force);

    // Calculate torque from force at point
    const relativePoint = {
      x: point.x - physics.position.x,
      y: point.y - physics.position.y,
    };
    const torque = relativePoint.x * force.y - relativePoint.y * force.x;
    physics.addTorque(torque);
  }
}
