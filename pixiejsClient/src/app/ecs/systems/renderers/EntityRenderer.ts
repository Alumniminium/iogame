import { Container, Graphics } from "pixi.js";
import { BaseRenderer } from "./BaseRenderer";
import { Entity } from "../../core/Entity";
import { Box2DBodyComponent } from "../../components/Box2DBodyComponent";
import { RenderComponent } from "../../components/RenderComponent";
import { World } from "../../core/World";
import { ShipPartRenderer } from "./ShipPartRenderer";

/**
 * Renders basic entity graphics and handles interactivity
 */
export class EntityRenderer extends BaseRenderer {
  private hoveredEntityId: string | null = null;
  private localPlayerId: string | null = null;
  private buildModeActive = false;
  private shipPartRenderer: ShipPartRenderer;

  constructor(gameContainer: Container, shipPartRenderer: ShipPartRenderer) {
    super(gameContainer);
    this.shipPartRenderer = shipPartRenderer;
  }

  update(deltaTime: number): void {
    const entities = World.queryEntitiesWithComponents(Box2DBodyComponent, RenderComponent);

    for (const entity of entities) {
      this.updateEntity(entity, deltaTime);
    }
  }

  private updateEntity(entity: Entity, deltaTime: number): void {
    const physics = entity.get(Box2DBodyComponent)!;
    const render = entity.get(RenderComponent)!;

    if (!physics || !render) return;

    let graphic = this.graphics.get(entity.id);
    if (!graphic) {
      graphic = this.createEntityGraphic(entity.id);
      this.setupEntityHoverEvents(graphic, entity.id);
      this.graphics.set(entity.id, graphic);
      this.gameContainer.addChild(graphic);
    }

    // Draw entity shape using ShipPartRenderer
    graphic.clear();
    this.shipPartRenderer.drawEntity(graphic, render, physics, entity.id);

    graphic.alpha = render.alpha;
    graphic.visible = render.visible;

    this.updateGraphicTransform(graphic, physics, deltaTime);
  }

  private createEntityGraphic(entityId?: string): Graphics {
    const graphics = new Graphics();

    const isLocalPlayerInBuildMode = entityId === this.localPlayerId && this.buildModeActive;

    graphics.interactive = !isLocalPlayerInBuildMode;
    (graphics as any).eventMode = isLocalPlayerInBuildMode ? "none" : "static";
    graphics.cursor = "pointer";

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

  private updateGraphicTransform(graphic: Graphics, physics: Box2DBodyComponent, deltaTime: number): void {
    const lerpFactor = Math.min(deltaTime * 60, 1);

    const targetX = physics.position.x;
    const targetY = physics.position.y;
    const targetRotation = physics.rotationRadians;

    graphic.position.x += (targetX - graphic.position.x) * lerpFactor;
    graphic.position.y += (targetY - graphic.position.y) * lerpFactor;
    graphic.rotation += this.normalizeRotationDiff(targetRotation - graphic.rotation) * lerpFactor;
  }

  private normalizeRotationDiff(diff: number): number {
    while (diff > Math.PI) diff -= 2 * Math.PI;
    while (diff < -Math.PI) diff += 2 * Math.PI;
    return diff;
  }

  getHoveredEntityId(): string | null {
    return this.hoveredEntityId;
  }

  setLocalPlayerId(playerId: string | null): void {
    this.localPlayerId = playerId;
  }

  setBuildModeActive(active: boolean): void {
    this.buildModeActive = active;
    if (this.localPlayerId) {
      const graphic = this.graphics.get(this.localPlayerId);
      if (graphic) {
        (graphic as any).eventMode = active ? "none" : "static";
        graphic.interactive = !active;
      }
    }
  }

  getGraphic(entityId: string): Graphics | undefined {
    return this.graphics.get(entityId);
  }

  onEntityDestroyed(entity: Entity): void {
    super.onEntityDestroyed(entity);

    // Clean up debug entity if it exists
    const debugEntityId = entity.id + 100000;
    const debugEntity = World.getEntity(debugEntityId);
    if (debugEntity) {
      const debugGraphic = this.graphics.get(debugEntityId);
      if (debugGraphic) {
        this.gameContainer.removeChild(debugGraphic);
        debugGraphic.destroy();
        this.graphics.delete(debugEntityId);
      }
      World.destroyEntity(debugEntityId);
    }
  }
}
