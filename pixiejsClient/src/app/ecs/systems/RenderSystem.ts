import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { World } from "../core/World";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { RenderComponent } from "../components/RenderComponent";
import { ShieldComponent } from "../components/ShieldComponent";
import { Container, Graphics } from "pixi.js";
import { Vector2 } from "../core/types";

export interface Camera {
  x: number;
  y: number;
  zoom: number;
}

export class RenderSystem extends System {
  readonly componentTypes = [PhysicsComponent, RenderComponent];

  private gameContainer: Container;
  private camera: Camera = { x: 0, y: 0, zoom: 1 };
  private previousTargetPosition: Vector2 = { x: 0, y: 0 };
  private viewDistance: number = 300; // Default view distance
  private entityGraphics = new Map<string, Graphics>();
  private shieldGraphics = new Map<string, Graphics>();
  private lineGraphics: Graphics[] = [];
  private followTarget: Entity | null = null;
  private renderLineListener: (event: Event) => void;
  private canvasWidth = 800;
  private canvasHeight = 600;
  private mapWidth = 150;
  private mapHeight = 1000;
  private backgroundRect: Graphics;
  private backgroundGrid: Graphics;
  private backgroundDrawn = false; // Track if background has been drawn
  private hoveredEntityId: string | null = null;

  constructor(gameContainer: Container) {
    super();
    this.gameContainer = gameContainer;

    // Create background rectangle (dark background)
    this.backgroundRect = new Graphics();
    this.gameContainer.addChildAt(this.backgroundRect, 0);

    // Create grid (on top of background)
    this.backgroundGrid = new Graphics();
    this.gameContainer.addChildAt(this.backgroundGrid, 1);

    // Bind and listen for line render events
    this.renderLineListener = this.handleRenderLine.bind(this);
    window.addEventListener("render-line", this.renderLineListener);
  }

  initialize(): void {
    console.log("RenderSystem initialized");
    this.updateGrid(); // Draw initial grid
  }

  cleanup(): void {
    // Clean up event listener
    window.removeEventListener("render-line", this.renderLineListener);

    // Clean up all graphics objects
    this.entityGraphics.forEach((graphic) => graphic.destroy());
    this.entityGraphics.clear();

    // Clean up shield graphics
    this.shieldGraphics.forEach((graphic) => graphic.destroy());
    this.shieldGraphics.clear();

    // Clean up line graphics
    this.lineGraphics.forEach((graphic) => {
      if (this.gameContainer.children.includes(graphic)) {
        this.gameContainer.removeChild(graphic);
      }
      graphic.destroy();
    });
    this.lineGraphics = [];

    // Clean up background elements
    this.backgroundRect.destroy();
    this.backgroundGrid.destroy();
    this.backgroundDrawn = false;
  }

  resize(width: number, height: number): void {
    this.canvasWidth = width;
    this.canvasHeight = height;
    this.updateZoomFromViewDistance();
    this.updateGrid();
  }

  setViewDistance(viewDistance: number): void {
    this.viewDistance = viewDistance;
    this.updateZoomFromViewDistance();
    this.updateGrid();
  }

  setMapSize(width: number, height: number): void {
    this.mapWidth = width;
    this.mapHeight = height;
    this.backgroundDrawn = false; // Force redraw of background
    this.updateGrid();
  }

  private updateZoomFromViewDistance(): void {
    // Calculate zoom using field of view approach similar to original camera
    const fieldOfView = Math.PI / 4; // 45 degrees
    const distance = this.viewDistance;

    // Calculate viewport dimensions using trigonometry
    // const aspectRatio = this.canvasWidth / this.canvasHeight; // Currently unused, might be needed for future viewport calculations
    const viewportWidth = distance * Math.tan(fieldOfView);
    // const viewportHeight = viewportWidth / aspectRatio; // Currently unused, might be needed for future viewport calculations

    // Calculate scale based on screen size to viewport size ratio
    this.camera.zoom = this.canvasWidth / viewportWidth;
  }

  private updateGrid(): void {
    // Draw background only once (it doesn't change)
    if (!this.backgroundDrawn) {
      this.backgroundRect.rect(0, 0, this.mapWidth, this.mapHeight).fill(0x0a0a0a); // Very dark gray background
      this.backgroundDrawn = true;
    }

    // Clear and redraw grid
    this.backgroundGrid.clear();

    const gridSize = 100;

    // Draw some test lines to see if anything shows up
    this.backgroundGrid
      .moveTo(0, 0)
      .lineTo(100, 100)
      .stroke({ width: 5, color: 0xff0000 }); // Bright red test line

    // Draw vertical lines with explicit stroke calls
    for (let x = 0; x <= this.mapWidth; x += gridSize) {
      this.backgroundGrid
        .moveTo(x, 0)
        .lineTo(x, this.mapHeight)
        .stroke({ width: 1, color: 0xffffff, alpha: 0.8 }); // Bright white for testing
    }

    // Draw horizontal lines with explicit stroke calls
    for (let y = 0; y <= this.mapHeight; y += gridSize) {
      this.backgroundGrid
        .moveTo(0, y)
        .lineTo(this.mapWidth, y)
        .stroke({ width: 1, color: 0xffffff, alpha: 0.8 }); // Bright white for testing
    }
  }

  followEntity(entity: Entity): void {
    const newTarget = entity;

    // If we're switching targets, smoothly initialize the camera position
    if (this.followTarget !== newTarget) {
      const physics = newTarget.get(PhysicsComponent);
      if (physics) {
        if (!this.followTarget) {
          // First time setting a follow target - snap to position to avoid big jump
          this.camera.x = physics.position.x;
          this.camera.y = physics.position.y;
        }
        // Initialize previous position for smooth velocity calculation
        this.previousTargetPosition = {
          x: physics.position.x,
          y: physics.position.y,
        };
      }
      this.followTarget = newTarget;
    }
  }

  getCamera(): Camera {
    return { ...this.camera };
  }

  update(deltaTime: number): void {
    // First call the parent System.update to process entities
    super.update(deltaTime);

    // Update camera to follow target
    if (this.followTarget) {
      this.updateCameraFollow(deltaTime);
    }

    // Apply camera transform to game container
    this.applyCamera();

    // Grid is static now, so we don't need to update it on camera movement
    // Only update grid when zoom changes (handled in setViewDistance and resize)
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const physics = entity.get(PhysicsComponent)!;
    const render = entity.get(RenderComponent)!;
    const shield = entity.get(ShieldComponent); // Optional shield component

    if (!physics || !render) {
      console.warn(`Entity ${entity.id} missing components:`, {
        physics: !!physics,
        render: !!render,
      });
      return;
    }

    // Create or get existing graphics object
    let graphic = this.entityGraphics.get(entity.id);
    if (!graphic) {
      graphic = this.createEntityGraphic(render);
      this.setupEntityHoverEvents(graphic, entity.id);
      this.entityGraphics.set(entity.id, graphic);
      this.gameContainer.addChild(graphic);
    }

    // Redraw the shape with the correct size from physics component
    this.drawPolygon(graphic, render, physics.size, physics);

    // Handle shield rendering
    if (shield && shield.powerOn && shield.charge > 0) {
      this.drawShield(entity, shield, physics);
    } else {
      // Remove shield graphic if shield is off or has no charge
      this.removeShieldGraphic(entity.id);
    }

    // Update visual properties
    graphic.alpha = render.alpha;
    graphic.visible = render.visible;

    this.updateGraphicTransform(graphic, physics, deltaTime);
  }

  render(): void {
    // All rendering is handled by PixiJS automatically
    // This method can be used for any custom rendering logic if needed
  }

  private createEntityGraphic(render: RenderComponent): Graphics {
    const graphics = new Graphics();

    // Make graphics interactive for hover detection
    graphics.interactive = true;
    graphics.cursor = 'pointer';

    this.drawPolygon(graphics, render, 16); // Default size, will be updated in updateEntity
    return graphics;
  }

  private setupEntityHoverEvents(graphic: Graphics, entityId: string): void {
    graphic.on('pointerenter', () => {
      this.hoveredEntityId = entityId;
    });

    graphic.on('pointerleave', () => {
      if (this.hoveredEntityId === entityId) {
        this.hoveredEntityId = null;
      }
    });
  }

  getHoveredEntityId(): string | null {
    return this.hoveredEntityId;
  }

  private updateGraphicTransform(
    graphic: Graphics,
    physics: PhysicsComponent,
    deltaTime: number,
  ): void {
    // Use interpolation for smooth rendering between physics updates
    const lerpFactor = Math.min(deltaTime * 60, 1); // 60 FPS interpolation

    // Interpolate position
    const targetX = physics.position.x;
    const targetY = physics.position.y;
    const targetRotation = physics.rotationRadians;

    graphic.position.x += (targetX - graphic.position.x) * lerpFactor;
    graphic.position.y += (targetY - graphic.position.y) * lerpFactor;
    graphic.rotation +=
      this.normalizeRotationDiff(targetRotation - graphic.rotation) *
      lerpFactor;
  }

  private normalizeRotationDiff(diff: number): number {
    while (diff > Math.PI) diff -= 2 * Math.PI;
    while (diff < -Math.PI) diff += 2 * Math.PI;
    return diff;
  }

  private drawPolygon(
    graphics: Graphics,
    render: RenderComponent,
    size: number = 16,
    physics?: PhysicsComponent,
  ): void {
    graphics.clear();

    const sides = render.sides !== undefined ? render.sides : 3;

    if (sides < 3) {
      // Draw circle for 0-2 sides
      const radius = physics ? Math.max(physics.width, physics.height) / 2 : size / 2;
      graphics
        .circle(0, 0, radius)
        .fill(render.color);
    } else if (
      sides === 4 &&
      physics &&
      (render.shapeType === 2 || render.shapeType === 4)
    ) {
      // Special case for rectangles - match server coordinate system exactly
      const halfWidth = physics.width / 2;
      const halfHeight = physics.height / 2;

      // Draw rectangle using server's original coordinate system
      // Since we're flipping position/rotation in physics, keep vertices as server expects
      const points = [
        -halfWidth,
        halfHeight, // top-left
        halfWidth,
        halfHeight, // top-right
        halfWidth,
        -halfHeight, // bottom-right
        -halfWidth,
        -halfHeight, // bottom-left
      ];

      graphics
        .poly(points)
        .fill(render.color);
    } else if (sides === 3 && physics) {
      // Special case for triangles - match server Box2D vertices exactly
      const halfWidth = physics.width / 2;
      const halfHeight = physics.height / 2;

      const points = [
        0, -halfHeight,        // Top
        -halfWidth, halfHeight, // Bottom left
        halfWidth, halfHeight   // Bottom right
      ];

      graphics
        .poly(points)
        .fill(render.color);
    } else {
      // Draw regular polygon based on sides
      const radius = physics ? Math.max(physics.width, physics.height) / 2 : size / 2;
      const points: number[] = [];

      for (let i = 0; i < sides; i++) {
        const angle = ((Math.PI * 2) / sides) * i - Math.PI / 2; // Start from top
        const x = Math.cos(angle) * radius;
        const y = Math.sin(angle) * radius;
        points.push(x, y);
      }

      graphics
        .poly(points)
        .fill(render.color);
    }
  }

  private updateCameraFollow(deltaTime: number): void {
    if (!this.followTarget) return;

    const physics = this.followTarget.get(PhysicsComponent);
    if (!physics) return;

    // Calculate velocity-based prediction for smoother following
    const currentPosition = { x: physics.position.x, y: physics.position.y };
    const velocity = {
      x: (currentPosition.x - this.previousTargetPosition.x) / deltaTime,
      y: (currentPosition.y - this.previousTargetPosition.y) / deltaTime,
    };

    // Predictive offset based on velocity (look ahead)
    const predictionStrength = 0.08; // Reduced for less aggressive prediction
    const targetX = currentPosition.x + velocity.x * predictionStrength;
    const targetY = currentPosition.y + velocity.y * predictionStrength;

    // Much softer camera following for smoother movement
    const followStrength = 2.5; // Significantly reduced for softer following
    const deadZone = 0.01; // Very small dead zone to prevent jitter

    const deltaX = targetX - this.camera.x;
    const deltaY = targetY - this.camera.y;

    // Apply soft exponential smoothing for very smooth camera movement
    const smoothingFactor = 1 - Math.exp(-followStrength * deltaTime);

    // Only update if outside dead zone to prevent micro-jittering
    if (Math.abs(deltaX) > deadZone) {
      this.camera.x += deltaX * smoothingFactor;
    }
    if (Math.abs(deltaY) > deadZone) {
      this.camera.y += deltaY * smoothingFactor;
    }

    // Update previous position for next frame
    this.previousTargetPosition = currentPosition;
  }

  private applyCamera(): void {
    // Center the camera on screen and apply zoom
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;

    this.gameContainer.position.set(centerX, centerY);
    this.gameContainer.scale.set(this.camera.zoom);
    this.gameContainer.position.x -= this.camera.x * this.camera.zoom;
    this.gameContainer.position.y -= this.camera.y * this.camera.zoom;
  }

  // Utility methods for UI systems
  worldToScreen(worldX: number, worldY: number): Vector2 {
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;

    return {
      x: centerX + (worldX - this.camera.x) * this.camera.zoom,
      y: centerY + (worldY - this.camera.y) * this.camera.zoom,
    };
  }

  screenToWorld(screenX: number, screenY: number): Vector2 {
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;

    return {
      x: this.camera.x + (screenX - centerX) / this.camera.zoom,
      y: this.camera.y + (screenY - centerY) / this.camera.zoom,
    };
  }

  onEntityDestroyed(entity: Entity): void {
    // Clean up graphics for destroyed entities
    const graphic = this.entityGraphics.get(entity.id);
    if (graphic) {
      this.gameContainer.removeChild(graphic);
      graphic.destroy();
      this.entityGraphics.delete(entity.id);
    }

    // Also clean up debug visualization entity if this is a main entity
    const debugEntityId = entity.id + 100000;
    const debugEntity = World.getEntity(debugEntityId);
    if (debugEntity) {
      const debugGraphic = this.entityGraphics.get(debugEntityId);
      if (debugGraphic) {
        this.gameContainer.removeChild(debugGraphic);
        debugGraphic.destroy();
        this.entityGraphics.delete(debugEntityId);
      }
      World.destroyEntity(debugEntityId);
    }

    if (this.followTarget && this.followTarget.id === entity.id) {
      this.followTarget = null;
    }

    // Also remove shield graphic if entity is being removed
    this.removeShieldGraphic(entity.id);
  }

  private drawShield(
    entity: Entity,
    shield: ShieldComponent,
    physics: PhysicsComponent,
  ): void {
    // Create or get existing shield graphic
    let shieldGraphic = this.shieldGraphics.get(entity.id);
    if (!shieldGraphic) {
      shieldGraphic = new Graphics();
      this.shieldGraphics.set(entity.id, shieldGraphic);
      this.gameContainer.addChild(shieldGraphic);
    }

    // Clear and redraw shield
    shieldGraphic.clear();

    // Calculate shield visual properties
    const chargePercent = shield.charge / shield.maxCharge;
    const alpha = Math.max(0.1, chargePercent * 0.4); // More visible when fully charged
    const color = this.getShieldColor(chargePercent);

    // Draw shield circle
    shieldGraphic
      .circle(0, 0, shield.radius)
      .fill({ color, alpha: alpha * 0.2 }) // Very transparent fill
      .stroke({ width: 2, color, alpha }); // More visible border

    // Position shield at entity position
    shieldGraphic.position.x = physics.position.x;
    shieldGraphic.position.y = physics.position.y;

    // Optional: Add shield charge effect (pulsing when low)
    if (chargePercent < 0.3) {
      const pulseAlpha = 0.5 + 0.3 * Math.sin(Date.now() * 0.01); // Pulsing effect
      shieldGraphic.alpha = pulseAlpha;
    } else {
      shieldGraphic.alpha = 1.0;
    }
  }

  private removeShieldGraphic(entityId: string): void {
    const shieldGraphic = this.shieldGraphics.get(entityId);
    if (shieldGraphic) {
      this.gameContainer.removeChild(shieldGraphic);
      shieldGraphic.destroy();
      this.shieldGraphics.delete(entityId);
    }
  }

  private getShieldColor(chargePercent: number): number {
    // Color transitions: Red (low) -> Yellow (medium) -> Blue (high)
    if (chargePercent < 0.3) {
      // Red when low charge
      return 0xff4444;
    } else if (chargePercent < 0.7) {
      // Yellow when medium charge
      return 0xffff44;
    } else {
      // Blue when high charge
      return 0x4444ff;
    }
  }

  private handleRenderLine(event: Event): void {
    const customEvent = event as CustomEvent;
    const { origin, hit, color, duration } = customEvent.detail;

    // Create a new graphics object for the line
    const lineGraphic = new Graphics();

    // Draw the line
    lineGraphic
      .moveTo(origin.x, origin.y)
      .lineTo(hit.x, hit.y)
      .stroke({ width: 2, color: color || 0xff0000 });

    // Add to the game container
    this.gameContainer.addChild(lineGraphic);
    this.lineGraphics.push(lineGraphic);

    // Remove the line after the specified duration
    setTimeout(() => {
      if (this.gameContainer.children.includes(lineGraphic)) {
        this.gameContainer.removeChild(lineGraphic);
        lineGraphic.destroy();
      }
      const index = this.lineGraphics.indexOf(lineGraphic);
      if (index > -1) {
        this.lineGraphics.splice(index, 1);
      }
    }, duration || 1000);
  }

}
