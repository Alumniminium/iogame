import { Entity } from "./core/Entity";
import { PhysicsComponent } from "./components/PhysicsComponent";
import { GravityComponent } from "./components/GravityComponent";
import { World } from "./core/World";

/**
 * Camera state for rendering transformations
 */
export interface CameraState {
  x: number;
  y: number;
  zoom: number;
  rotation: number;
}

/**
 * Clean, minimal camera system with entity following, smooth interpolation, and gravity-based rotation.
 *
 * Features:
 * - Follows any entity with PhysicsComponent via SetTarget()
 * - Time-based position interpolation (faster when farther away)
 * - Automatic rotation based on nearest gravity source (2s linear animation)
 * - Gravity source always appears at bottom of viewport
 */
export class Camera {
  private x = 0;
  private y = 0;
  private zoom = 1;
  private rotation = 0;

  private targetEntity: Entity | null = null;
  private targetRotation = 0;
  private currentGravitySourceId: string | null = null;
  private rotationAnimationTime = 0;
  private rotationAnimationDuration = 2.0; // 2 seconds
  private isRotating = false;
  private rotationStart = 0;

  private readonly positionReachTime = 0.2; // seconds to reach target position

  constructor(initialZoom: number = 1) {
    this.zoom = initialZoom;
  }

  /**
   * Set the entity for the camera to follow
   */
  setTarget(entity: Entity | null): void {
    this.targetEntity = entity;
  }

  /**
   * Set camera zoom level
   */
  setZoom(zoom: number): void {
    this.zoom = zoom;
  }

  /**
   * Get current camera state for rendering
   */
  getState(): CameraState {
    return {
      x: this.x,
      y: this.y,
      zoom: this.zoom,
      rotation: this.rotation,
    };
  }

  /**
   * Update camera position and rotation based on target entity and gravity.
   * This is the main camera logic in one clean method.
   */
  update(deltaTime: number): void {
    if (!this.targetEntity || !this.targetEntity.has(PhysicsComponent)) return;

    const physics = this.targetEntity.get(PhysicsComponent)!;
    const targetPos = physics.position;

    // Find closest gravity source within radius
    const closestGravity = this.findClosestGravitySource(targetPos);

    // Handle gravity-based rotation
    this.updateGravityRotation(closestGravity, targetPos);

    // Interpolate position (time-based, reaches target quickly)
    this.interpolatePosition(targetPos, deltaTime);

    // Interpolate rotation if animating
    if (this.isRotating) this.interpolateRotation(deltaTime);
  }

  /**
   * Find the closest gravity source within its effective radius of the target position
   */
  private findClosestGravitySource(targetPos: { x: number; y: number }): { entity: Entity; gravity: GravityComponent; distance: number } | null {
    let closest: {
      entity: Entity;
      gravity: GravityComponent;
      distance: number;
    } | null = null;
    let minDistance = Infinity;

    const entities = World.getAllEntities();
    for (const entity of entities) {
      if (!entity.has(GravityComponent)) continue;
      if (!entity.has(PhysicsComponent)) continue;

      const gravity = entity.get(GravityComponent)!;
      const physics = entity.get(PhysicsComponent)!;

      const dx = physics.position.x - targetPos.x;
      const dy = physics.position.y - targetPos.y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      // Check if within gravity radius
      if (distance > gravity.radius) continue;

      if (distance < minDistance) {
        minDistance = distance;
        closest = { entity, gravity, distance };
      }
    }

    return closest;
  }

  /**
   * Update target rotation based on gravity source and start animation if gravity changed
   */
  private updateGravityRotation(closestGravity: { entity: Entity; gravity: GravityComponent } | null, targetPos: { x: number; y: number }): void {
    const newGravityId = closestGravity?.entity.id ?? null;
    const gravitySourceChanged = newGravityId !== this.currentGravitySourceId;

    // Update current gravity source ID
    this.currentGravitySourceId = newGravityId;

    // Calculate target rotation (only when gravity source changes)
    if (closestGravity && gravitySourceChanged) {
      const gravityPhysics = closestGravity.entity.get(PhysicsComponent)!;
      const gravityPos = gravityPhysics.position;

      // Direction from target to gravity (the pull direction)
      const dx = gravityPos.x - targetPos.x;
      const dy = gravityPos.y - targetPos.y;
      const angle = Math.atan2(dy, dx);

      // Rotate so this direction points "down" on screen (90° in world space)
      // Subtract Math.PI/2 because 0° = right, 90° = down
      let newTargetRotation = angle - Math.PI / 2;

      // Snap to nearest 90° increment (0°, 90°, 180°, 270°)
      newTargetRotation = Math.round(newTargetRotation / (Math.PI / 2)) * (Math.PI / 2);

      // Normalize to [-π, π]
      this.targetRotation = this.normalizeAngle(newTargetRotation);

      // Start rotation animation
      this.startRotationAnimation();
    }
    // When exiting gravity field, maintain current rotation (do nothing)
  }

  /**
   * Start a new rotation animation to the target rotation
   */
  private startRotationAnimation(): void {
    // Normalize current rotation to [-π, π] before starting animation
    this.rotation = this.normalizeAngle(this.rotation);
    this.rotationStart = this.rotation;

    // Choose shortest rotation path
    const diff = this.targetRotation - this.rotation;
    if (diff > Math.PI) {
      this.targetRotation -= 2 * Math.PI;
    } else if (diff < -Math.PI) {
      this.targetRotation += 2 * Math.PI;
    }

    this.isRotating = true;
    this.rotationAnimationTime = 0;
  }

  /**
   * Interpolate camera position toward target (time-based)
   */
  private interpolatePosition(targetPos: { x: number; y: number }, deltaTime: number): void {
    // Time-based interpolation: reach target in positionReachTime seconds
    const t = Math.min(deltaTime / this.positionReachTime, 1);

    this.x += (targetPos.x - this.x) * t;
    this.y += (targetPos.y - this.y) * t;
  }

  /**
   * Interpolate camera rotation toward target rotation (linear over 2 seconds)
   */
  private interpolateRotation(deltaTime: number): void {
    this.rotationAnimationTime += deltaTime;

    // Linear interpolation over rotationAnimationDuration seconds
    const t = Math.min(this.rotationAnimationTime / this.rotationAnimationDuration, 1);

    this.rotation = this.rotationStart + (this.targetRotation - this.rotationStart) * t;

    // Animation complete
    if (t >= 1) {
      this.isRotating = false;
      this.rotation = this.targetRotation;
    }
  }

  /**
   * Normalize angle to [-π, π] range
   */
  private normalizeAngle(angle: number): number {
    while (angle > Math.PI) angle -= 2 * Math.PI;
    while (angle < -Math.PI) angle += 2 * Math.PI;
    return angle;
  }
}
