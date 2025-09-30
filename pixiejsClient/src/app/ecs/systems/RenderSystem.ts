import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { World } from "../core/World";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { RenderComponent } from "../components/RenderComponent";
import { ShieldComponent } from "../components/ShieldComponent";
import { ParticleSystemComponent } from "../components/ParticleSystemComponent";
import { GravityComponent } from "../components/GravityComponent";
import { Container, Graphics } from "pixi.js";
import { Vector2 } from "../core/types";

export interface Camera {
  x: number;
  y: number;
  zoom: number;
  rotation: number; // Camera rotation in radians
}

export class RenderSystem extends System {
  readonly componentTypes = [PhysicsComponent, RenderComponent];

  private gameContainer: Container;
  private camera: Camera = { x: 0, y: 0, zoom: 1, rotation: 0 };
  private viewDistance: number = 300; // Default view distance
  private entityGraphics = new Map<string, Graphics>();
  private shieldGraphics = new Map<string, Graphics>();
  private particleGraphics = new Map<string, Graphics>();
  private lineGraphics: Graphics[] = [];
  private renderLineListener: (event: Event) => void;
  private canvasWidth = 800;
  private canvasHeight = 600;
  private mapWidth = 150;
  private mapHeight = 1000;
  private backgroundRect: Graphics;
  private backgroundGrid: Graphics;
  private hoveredEntityId: string | null = null;
  private localPlayerId: string | null = null;
  private buildModeActive = false;

  constructor(gameContainer: Container, app: any) {
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

    this.particleGraphics.forEach((graphic) => graphic.destroy());
    this.particleGraphics.clear();

    this.lineGraphics.forEach((graphic) => {
      if (this.gameContainer.children.includes(graphic)) {
        this.gameContainer.removeChild(graphic);
      }
      graphic.destroy();
    });
    this.lineGraphics = [];

    this.backgroundRect.destroy();
    this.backgroundGrid.destroy();
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
    this.updateGrid();
  }

  private updateZoomFromViewDistance(): void {
    const fieldOfView = Math.PI / 4; // 45 degrees
    const distance = this.viewDistance;

    const viewportWidth = distance * Math.tan(fieldOfView);

    this.camera.zoom = this.canvasWidth / viewportWidth;
  }

  private updateGrid(): void {
    this.backgroundGrid.clear();

    const gridSize = 10;

    this.backgroundGrid
      .moveTo(0, 0)
      .lineTo(100, 100)
      .stroke({ width: 5, color: 0xff0000 }); // Bright red test line

    for (let x = 0; x <= this.mapWidth; x += gridSize) {
      this.backgroundGrid
        .moveTo(x, 0)
        .lineTo(x, this.mapHeight)
        .stroke({ width: 0.1, color: 0xffffff, alpha: 0.8 }); // Bright white for testing
    }

    for (let y = 0; y <= this.mapHeight; y += gridSize) {
      this.backgroundGrid
        .moveTo(0, y)
        .lineTo(this.mapWidth, y)
        .stroke({ width: 0.1, color: 0xffffff, alpha: 0.8 }); // Bright white for testing
    }
  }

  getCamera(): Camera {
    return { ...this.camera };
  }

  update(deltaTime: number): void {
    super.update(deltaTime);

    // Simple camera follow - just look at local player's PhysicsComponent
    const localPlayerId = (window as any).localPlayerId;
    if (!localPlayerId) {
      // Don't spam console - this is called every frame
      return;
    }
    const playerEntity = World.getEntity(localPlayerId);

    if (!playerEntity) {
      // Don't spam console - this is called every frame
      return;
    }

    if (!playerEntity.has(PhysicsComponent)) {
      // Don't spam console - this is called every frame
      return;
    }

    const physics = playerEntity.get(PhysicsComponent);
    this.camera.x = physics!.position.x;
    this.camera.y = physics!.position.y;

    this.applyCamera();
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const physics = entity.get(PhysicsComponent)!;
    const render = entity.get(RenderComponent)!;
    const shield = entity.get(ShieldComponent); // Optional shield component
    const particleSystem = entity.get(ParticleSystemComponent); // Optional particle system

    if (!physics || !render) return;

    let graphic = this.entityGraphics.get(entity.id);
    if (!graphic) {
      graphic = this.createEntityGraphic(render, entity.id);
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

    // Render particles if the entity has a particle system
    if (particleSystem) {
      this.renderParticles(entity, particleSystem);
    } else {
      this.removeParticleGraphic(entity.id);
    }

    graphic.alpha = render.alpha;
    graphic.visible = render.visible;

    this.updateGraphicTransform(graphic, physics, deltaTime);
  }

  render(): void {}

  private createEntityGraphic(
    render: RenderComponent,
    entityId?: string,
  ): Graphics {
    const graphics = new Graphics();

    // Disable interactivity for local player in build mode
    const isLocalPlayerInBuildMode =
      entityId === this.localPlayerId && this.buildModeActive;

    graphics.interactive = !isLocalPlayerInBuildMode;
    (graphics as any).eventMode = isLocalPlayerInBuildMode ? "none" : "static";
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

  setBuildModeActive(active: boolean): void {
    this.buildModeActive = active;
    // Update interactivity for existing local player graphic
    if (this.localPlayerId) {
      const graphic = this.entityGraphics.get(this.localPlayerId);
      if (graphic) {
        (graphic as any).eventMode = active ? "none" : "static";
        graphic.interactive = !active;
      }
    }
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
    physics?: PhysicsComponent,
  ): void {
    const gridSize = 1.0; // Each grid cell is 1 world unit

    if (!render.shipParts || render.shipParts.length === 0) {
      // Draw a fallback shape when no ship parts are available
      this.drawFallbackShape(graphics, render, physics);
      return;
    }

    for (const part of render.shipParts) {
      const gridX = part.gridX * gridSize;
      const gridY = part.gridY * gridSize;

      const partRotation = part.rotation * (Math.PI / 2); // Convert 0-3 to radians

      let points: number[] = [];
      const halfSize = gridSize / 2;

      if (part.shape === 1) {
        // Triangle
        points = [
          0,
          -halfSize, // Top
          -halfSize,
          halfSize, // Bottom left
          halfSize,
          halfSize, // Bottom right
        ];
      } else {
        // Square (shape === 2 or default)
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
        points[i] += gridX;
        points[i + 1] += gridY;
      }

      const partColor = this.getPartColor(part.type);

      graphics.poly(points).fill(partColor);

      if (part.type === 2) {
        // Engine parts
        this.drawEngineNozzle(graphics, gridX, gridY, partRotation, gridSize);
      }
    }
  }

  private drawFallbackShape(
    graphics: Graphics,
    render: RenderComponent,
    physics?: PhysicsComponent,
  ): void {
    const size = physics ? physics.size : 1.0;
    const halfSize = size / 2;

    let points: number[] = [];

    // Use the render component's shapeType to determine the shape
    if (render.shapeType === 1) {
      // Triangle
      points = [
        0,
        -halfSize, // Top
        -halfSize,
        halfSize, // Bottom left
        halfSize,
        halfSize, // Bottom right
      ];
    } else if (render.shapeType === 0) {
      // Circle - approximate with octagon
      const segments = 8;
      points = [];
      for (let i = 0; i < segments; i++) {
        const angle = (i / segments) * Math.PI * 2;
        points.push(Math.cos(angle) * halfSize, Math.sin(angle) * halfSize);
      }
    } else {
      // Square/Box (default)
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
    }

    // Ensure color is valid - mask to 24-bit RGB (strip alpha channel)
    const color = this.normalizeColor(render.color);
    graphics.poly(points).fill(color);
  }

  private normalizeColor(color: number | undefined | null): number {
    if (color === undefined || color === null) {
      return 0xffffff; // White fallback
    }
    // Mask to 24-bit RGB (0x00FFFFFF) to strip alpha channel
    return color & 0xffffff;
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

  private applyCamera(): void {
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;

    // Set the pivot point to the camera's world position (where the player is)
    this.gameContainer.pivot.set(this.camera.x, this.camera.y);

    // Position the container so the pivot point appears at screen center
    this.gameContainer.position.set(centerX, centerY);
    this.gameContainer.scale.set(this.camera.zoom);
    this.gameContainer.rotation = this.camera.rotation;
  }

  worldToScreen(worldX: number, worldY: number): Vector2 {
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;

    // Get relative position from camera/pivot point
    const relativeX = worldX - this.camera.x;
    const relativeY = worldY - this.camera.y;

    // Apply camera rotation around the pivot (camera position)
    const cos = Math.cos(this.camera.rotation);
    const sin = Math.sin(this.camera.rotation);
    const rotatedX = relativeX * cos - relativeY * sin;
    const rotatedY = relativeX * sin + relativeY * cos;

    // Apply zoom and translate to screen center
    return {
      x: centerX + rotatedX * this.camera.zoom,
      y: centerY + rotatedY * this.camera.zoom,
    };
  }

  screenToWorld(screenX: number, screenY: number): Vector2 {
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;

    // Get screen coordinates relative to center
    const relativeX = (screenX - centerX) / this.camera.zoom;
    const relativeY = (screenY - centerY) / this.camera.zoom;

    // Apply inverse camera rotation
    const cos = Math.cos(-this.camera.rotation);
    const sin = Math.sin(-this.camera.rotation);
    const unrotatedX = relativeX * cos - relativeY * sin;
    const unrotatedY = relativeX * sin + relativeY * cos;

    // Add to camera position to get world coordinates
    return {
      x: this.camera.x + unrotatedX,
      y: this.camera.y + unrotatedY,
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

    this.removeShieldGraphic(entity.id);
    this.removeParticleGraphic(entity.id);
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

  private renderParticles(
    entity: Entity,
    particleSystem: ParticleSystemComponent,
  ): void {
    let particleGraphic = this.particleGraphics.get(entity.id);
    if (!particleGraphic) {
      particleGraphic = new Graphics();
      this.particleGraphics.set(entity.id, particleGraphic);
      this.gameContainer.addChild(particleGraphic);
    }

    particleGraphic.clear();

    // Draw all particles
    for (const particle of particleSystem.particles) {
      if (particle.alpha <= 0) continue;

      const size = particle.size;
      const color = this.normalizeColor(particle.color);
      particleGraphic
        .circle(particle.x, particle.y, size)
        .fill({ color, alpha: particle.alpha });
    }
  }

  private removeParticleGraphic(entityId: string): void {
    const particleGraphic = this.particleGraphics.get(entityId);
    if (particleGraphic) {
      this.gameContainer.removeChild(particleGraphic);
      particleGraphic.destroy();
      this.particleGraphics.delete(entityId);
    }
  }
}
