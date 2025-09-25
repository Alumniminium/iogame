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
  private localPlayerId: string | null = null;

  constructor(gameContainer: Container) {
    super();
    this.gameContainer = gameContainer;

    this.backgroundRect = new Graphics();
    this.gameContainer.addChildAt(this.backgroundRect, 0);

    this.backgroundGrid = new Graphics();
    this.gameContainer.addChildAt(this.backgroundGrid, 1);

    this.renderLineListener = this.handleRenderLine.bind(this);
    window.addEventListener("render-line", this.renderLineListener);
  }

  initialize(): void {
    this.updateGrid(); // Draw initial grid
  }

  cleanup(): void {
    window.removeEventListener("render-line", this.renderLineListener);

    this.entityGraphics.forEach((graphic) => graphic.destroy());
    this.entityGraphics.clear();

    this.shieldGraphics.forEach((graphic) => graphic.destroy());
    this.shieldGraphics.clear();

    this.lineGraphics.forEach((graphic) => {
      if (this.gameContainer.children.includes(graphic)) {
        this.gameContainer.removeChild(graphic);
      }
      graphic.destroy();
    });
    this.lineGraphics = [];

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
    const fieldOfView = Math.PI / 4; // 45 degrees
    const distance = this.viewDistance;

    const viewportWidth = distance * Math.tan(fieldOfView);

    this.camera.zoom = this.canvasWidth / viewportWidth;
  }

  private updateGrid(): void {
    if (!this.backgroundDrawn) {
      this.backgroundRect
        .rect(0, 0, this.mapWidth, this.mapHeight)
        .fill(0x0a0a0a); // Very dark gray background
      this.backgroundDrawn = true;
    }

    this.backgroundGrid.clear();

    const gridSize = 100;

    this.backgroundGrid
      .moveTo(0, 0)
      .lineTo(100, 100)
      .stroke({ width: 5, color: 0xff0000 }); // Bright red test line

    for (let x = 0; x <= this.mapWidth; x += gridSize) {
      this.backgroundGrid
        .moveTo(x, 0)
        .lineTo(x, this.mapHeight)
        .stroke({ width: 1, color: 0xffffff, alpha: 0.8 }); // Bright white for testing
    }

    for (let y = 0; y <= this.mapHeight; y += gridSize) {
      this.backgroundGrid
        .moveTo(0, y)
        .lineTo(this.mapWidth, y)
        .stroke({ width: 1, color: 0xffffff, alpha: 0.8 }); // Bright white for testing
    }
  }

  followEntity(entity: Entity): void {
    const newTarget = entity;

    if (this.followTarget !== newTarget) {
      const physics = newTarget.get(PhysicsComponent);
      if (physics) {
        if (!this.followTarget) {
          this.camera.x = physics.position.x;
          this.camera.y = physics.position.y;
        }
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
    super.update(deltaTime);

    if (this.followTarget) {
      this.updateCameraFollow(deltaTime);
    }

    this.applyCamera();
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const physics = entity.get(PhysicsComponent)!;
    const render = entity.get(RenderComponent)!;
    const shield = entity.get(ShieldComponent); // Optional shield component

    if (!physics || !render) return;

    let graphic = this.entityGraphics.get(entity.id);
    if (!graphic) {
      graphic = this.createEntityGraphic(render);
      this.setupEntityHoverEvents(graphic, entity.id);
      this.entityGraphics.set(entity.id, graphic);
      this.gameContainer.addChild(graphic);
    }

    this.drawPolygon(graphic, render, physics.size, physics);

    if (entity.id === this.localPlayerId) {
      this.drawDirectionArrow(graphic, physics.size);
    }

    if (shield && shield.powerOn && shield.charge > 0) {
      this.drawShield(entity, shield, physics);
    } else {
      this.removeShieldGraphic(entity.id);
    }

    graphic.alpha = render.alpha;
    graphic.visible = render.visible;

    this.updateGraphicTransform(graphic, physics, deltaTime);
  }

  render(): void {}

  private createEntityGraphic(render: RenderComponent): Graphics {
    const graphics = new Graphics();

    graphics.interactive = true;
    graphics.cursor = "pointer";

    this.drawPolygon(graphics, render, 16); // Default size, will be updated in updateEntity
    return graphics;
  }

  private setupEntityHoverEvents(graphic: Graphics, entityId: string): void {
    graphic.on("pointerenter", () => {
      this.hoveredEntityId = entityId;
    });

    graphic.on("pointerleave", () => {
      if (this.hoveredEntityId === entityId) {
        this.hoveredEntityId = null;
      }
    });
  }

  getHoveredEntityId(): string | null {
    return this.hoveredEntityId;
  }

  setLocalPlayerId(playerId: string | null): void {
    this.localPlayerId = playerId;
  }

  private updateGraphicTransform(
    graphic: Graphics,
    physics: PhysicsComponent,
    deltaTime: number,
  ): void {
    const lerpFactor = Math.min(deltaTime * 60, 1); // 60 FPS interpolation

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
    _size: number = 16,
    physics?: PhysicsComponent,
  ): void {
    graphics.clear();

    this.drawCompoundShape(graphics, render, physics);
  }

  private drawCompoundShape(
    graphics: Graphics,
    render: RenderComponent,
    _physics?: PhysicsComponent,
  ): void {
    const gridSize = 1.0; // Each grid cell is 1 world unit

    if (!render.shipParts || render.shipParts.length === 0) return;

    for (const part of render.shipParts) {
      const offsetX = (part.gridX - render.centerX) * gridSize;
      const offsetY = (part.gridY - render.centerY) * gridSize;

      const partRotation = part.rotation * (Math.PI / 2); // Convert 0-3 to radians

      let points: number[] = [];
      const halfSize = gridSize / 2;

      if (part.shape === 0) {
        points = [
          0,
          -halfSize, // Top
          -halfSize,
          halfSize, // Bottom left
          halfSize,
          halfSize, // Bottom right
        ];
      } else if (part.shape === 1) {
        points = [
          -halfSize,
          -halfSize, // Top-left
          halfSize,
          -halfSize, // Top-right
          halfSize,
          halfSize, // Bottom-right
          -halfSize,
          halfSize, // Bottom-left
        ];
      } else {
        points = [
          -halfSize,
          -halfSize,
          halfSize,
          -halfSize,
          halfSize,
          halfSize,
          -halfSize,
          halfSize,
        ];
      }

      if (partRotation !== 0) {
        const cos = Math.cos(partRotation);
        const sin = Math.sin(partRotation);

        for (let i = 0; i < points.length; i += 2) {
          const x = points[i];
          const y = points[i + 1];

          points[i] = x * cos - y * sin;
          points[i + 1] = x * sin + y * cos;
        }
      }

      for (let i = 0; i < points.length; i += 2) {
        points[i] += offsetX;
        points[i + 1] += offsetY;
      }

      const partColor = this.getPartColor(part.type);

      graphics.poly(points).fill(partColor);

      if (part.type === 2) {
        // Engine parts
        this.drawEngineNozzle(
          graphics,
          offsetX,
          offsetY,
          partRotation,
          gridSize,
        );
      }
    }
  }

  private getPartColor(type: number): number {
    switch (type) {
      case 0: // hull
        return 0x808080; // Gray
      case 1: // shield
        return 0x0080ff; // Blue
      case 2: // engine
        return 0xff8000; // Orange
      default:
        return 0xffffff; // White fallback
    }
  }

  private drawEngineNozzle(
    graphics: Graphics,
    engineX: number,
    engineY: number,
    engineRotation: number,
    gridSize: number,
  ): void {
    const exhaustDirection = engineRotation + Math.PI; // Add 180 degrees

    const nozzleLength = gridSize * 0.3;
    const nozzleWidth = gridSize * 0.15;

    const nozzleDistance = gridSize * 0.4;
    const nozzleCenterX = engineX + Math.cos(exhaustDirection) * nozzleDistance;
    const nozzleCenterY = engineY + Math.sin(exhaustDirection) * nozzleDistance;

    const nozzlePoints = [
      nozzleCenterX + Math.cos(exhaustDirection) * nozzleLength,
      nozzleCenterY + Math.sin(exhaustDirection) * nozzleLength,

      nozzleCenterX -
        Math.cos(exhaustDirection) * nozzleLength * 0.3 +
        Math.cos(exhaustDirection + Math.PI / 2) * nozzleWidth,
      nozzleCenterY -
        Math.sin(exhaustDirection) * nozzleLength * 0.3 +
        Math.sin(exhaustDirection + Math.PI / 2) * nozzleWidth,

      nozzleCenterX -
        Math.cos(exhaustDirection) * nozzleLength * 0.3 +
        Math.cos(exhaustDirection - Math.PI / 2) * nozzleWidth,
      nozzleCenterY -
        Math.sin(exhaustDirection) * nozzleLength * 0.3 +
        Math.sin(exhaustDirection - Math.PI / 2) * nozzleWidth,
    ];

    const nozzleColor = 0xcc4400; // Darker orange
    graphics.poly(nozzlePoints).fill(nozzleColor);
  }

  private drawDirectionArrow(graphics: Graphics, entitySize: number): void {
    const arrowSize = entitySize * 0.4; // Arrow is 40% of entity size
    const arrowDistance = entitySize * 0.2; // Much closer to center, on top of the box

    const arrowPoints = [
      arrowDistance,
      0, // Tip point (pointing right in local coords)
      arrowDistance - arrowSize,
      -arrowSize * 0.4, // Bottom left
      arrowDistance - arrowSize,
      arrowSize * 0.4, // Top left
    ];

    graphics.poly(arrowPoints).fill(0x000000); // Black arrow
  }

  private updateCameraFollow(deltaTime: number): void {
    if (!this.followTarget) return;

    const physics = this.followTarget.get(PhysicsComponent);
    if (!physics) return;

    const currentPosition = { x: physics.position.x, y: physics.position.y };
    const velocity = {
      x: (currentPosition.x - this.previousTargetPosition.x) / deltaTime,
      y: (currentPosition.y - this.previousTargetPosition.y) / deltaTime,
    };

    const predictionStrength = 0.08; // Reduced for less aggressive prediction
    const targetX = currentPosition.x + velocity.x * predictionStrength;
    const targetY = currentPosition.y + velocity.y * predictionStrength;

    const followStrength = 2.5; // Significantly reduced for softer following
    const deadZone = 0.01; // Very small dead zone to prevent jitter

    const deltaX = targetX - this.camera.x;
    const deltaY = targetY - this.camera.y;

    const smoothingFactor = 1 - Math.exp(-followStrength * deltaTime);

    if (Math.abs(deltaX) > deadZone) {
      this.camera.x += deltaX * smoothingFactor;
    }
    if (Math.abs(deltaY) > deadZone) {
      this.camera.y += deltaY * smoothingFactor;
    }

    this.previousTargetPosition = currentPosition;
  }

  private applyCamera(): void {
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;

    this.gameContainer.position.set(centerX, centerY);
    this.gameContainer.scale.set(this.camera.zoom);
    this.gameContainer.position.x -= this.camera.x * this.camera.zoom;
    this.gameContainer.position.y -= this.camera.y * this.camera.zoom;
  }

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
    const graphic = this.entityGraphics.get(entity.id);
    if (graphic) {
      this.gameContainer.removeChild(graphic);
      graphic.destroy();
      this.entityGraphics.delete(entity.id);
    }

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

    this.removeShieldGraphic(entity.id);
  }

  private drawShield(
    entity: Entity,
    shield: ShieldComponent,
    physics: PhysicsComponent,
  ): void {
    let shieldGraphic = this.shieldGraphics.get(entity.id);
    if (!shieldGraphic) {
      shieldGraphic = new Graphics();
      this.shieldGraphics.set(entity.id, shieldGraphic);
      this.gameContainer.addChild(shieldGraphic);
    }

    shieldGraphic.clear();

    const chargePercent = shield.charge / shield.maxCharge;
    const alpha = Math.max(0.1, chargePercent * 0.4); // More visible when fully charged
    const color = this.getShieldColor(chargePercent);

    shieldGraphic
      .circle(0, 0, shield.radius)
      .fill({ color, alpha: alpha * 0.2 }) // Very transparent fill
      .stroke({ width: 2, color, alpha }); // More visible border

    shieldGraphic.position.x = physics.position.x;
    shieldGraphic.position.y = physics.position.y;

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
    if (chargePercent < 0.3) {
      return 0xff4444;
    } else if (chargePercent < 0.7) {
      return 0xffff44;
    } else {
      return 0x4444ff;
    }
  }

  private handleRenderLine(event: Event): void {
    const customEvent = event as CustomEvent;
    const { origin, hit, color, duration } = customEvent.detail;

    const lineGraphic = new Graphics();

    lineGraphic
      .moveTo(origin.x, origin.y)
      .lineTo(hit.x, hit.y)
      .stroke({ width: 0.2, color: color || 0xff0000 });

    this.gameContainer.addChild(lineGraphic);
    this.lineGraphics.push(lineGraphic);

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
