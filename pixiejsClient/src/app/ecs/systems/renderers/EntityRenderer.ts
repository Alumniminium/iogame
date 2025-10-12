import { Container, Graphics } from "pixi.js";
import { System1 } from "../../core/System";
import { Entity } from "../../core/Entity";
import { World } from "../../core/World";
import { PhysicsComponent } from "../../components/PhysicsComponent";
import { RenderComponent } from "../../components/RenderComponent";
import { ParentChildComponent } from "../../components/ParentChildComponent";
import { HoverTagComponent } from "../../components/HoverTagComponent";
import { BuildModeManager } from "../../../managers/BuildModeManager";

/**
 * Renders all entities with RenderComponent.
 * Handles both regular entities (with PhysicsComponent) and child entities (with ParentChildComponent).
 */
export class EntityRenderer extends System1<RenderComponent> {
  private gameContainer: Container;
  private readonly gridSize = 1.0;

  constructor(gameContainer: Container) {
    super(RenderComponent);
    this.gameContainer = gameContainer;
  }

  protected updateEntity(ntt: Entity, rc: RenderComponent, deltaTime: number): void {
    const pcc = ntt.get(ParentChildComponent);

    if (pcc) {
      this.renderChildEntity(pcc, rc, deltaTime);
    } else {
      this.renderPhysicsEntity(ntt, rc, deltaTime);
    }
  }

  private renderPhysicsEntity(ntt: Entity, rc: RenderComponent, deltaTime: number): void {
    const phy = ntt.get(PhysicsComponent);
    if (!phy) return;

    let graphic = rc.renderers.get(RenderComponent);
    if (!graphic) {
      graphic = this.createEntityGraphic(ntt.id);
      this.setupEntityHoverEvents(graphic, ntt.id);
      this.gameContainer.addChild(graphic);
      rc.renderers.set(RenderComponent, graphic);
    }

    graphic.clear();
    this.drawEntityShape(graphic, phy, rc, ntt.id);

    graphic.alpha = rc.alpha;
    graphic.visible = rc.visible;

    this.updatePhysicsTransform(graphic, phy, deltaTime);
  }

  private renderChildEntity(pcc: ParentChildComponent, rc: RenderComponent, deltaTime: number): void {
    const parent = World.getEntity(pcc.parentId);
    if (!parent) return;

    const parentPhy = parent.get(PhysicsComponent);
    if (!parentPhy) return;

    let graphic = rc.renderers.get(ParentChildComponent);
    if (!graphic) {
      graphic = new Graphics();
      this.gameContainer.addChild(graphic);
      rc.renderers.set(ParentChildComponent, graphic);
    }

    graphic.clear();
    this.drawChildShape(graphic, pcc, rc);

    graphic.alpha = rc.alpha;
    graphic.visible = rc.visible;

    this.updateChildTransform(graphic, parentPhy, pcc, deltaTime);
  }

  private drawEntityShape(graphics: Graphics, phy: PhysicsComponent, rc: RenderComponent, nttId: string): void {
    const size = phy.size;
    const halfSize = size / 2;
    let points: number[] = [];

    // Shape types: 0 = circle, 1 = triangle, 2 = square
    if (rc.shapeType === 1) {
      points = [0, -halfSize, -halfSize, halfSize, halfSize, halfSize];
    } else if (rc.shapeType === 0) {
      // Circle
      const segments = 8;
      points = [];
      for (let i = 0; i < segments; i++) {
        const angle = (i / segments) * Math.PI * 2;
        points.push(Math.cos(angle) * halfSize, Math.sin(angle) * halfSize);
      }
    } else {
      // Square
      points = [-halfSize, -halfSize, halfSize, -halfSize, halfSize, halfSize, -halfSize, halfSize];
    }

    const color = this.normalizeColor(rc.color);
    graphics.poly(points).fill(color);

    // Draw direction arrow for local player
    if (World.Me && nttId === World.Me.id) {
      this.drawDirectionArrow(graphics, size);
    }
  }

  private drawDirectionArrow(graphics: Graphics, entitySize: number): void {
    const arrowSize = entitySize * 0.4;
    const arrowDistance = entitySize * 0.2;

    const arrowPoints = [arrowDistance, 0, arrowDistance - arrowSize, -arrowSize * 0.4, arrowDistance - arrowSize, arrowSize * 0.4];

    graphics.poly(arrowPoints).fill(0x000000);
  }

  private normalizeColor(color: number | undefined | null): number {
    if (color === undefined || color === null) return 0xffffff;
    return color & 0xffffff;
  }

  private createEntityGraphic(nttId?: string): Graphics {
    const graphics = new Graphics();

    const isPlayerInBuildMode = World.Me && nttId === World.Me.id && BuildModeManager.getInstance().isInBuildMode();

    graphics.interactive = !isPlayerInBuildMode;
    (graphics as any).eventMode = isPlayerInBuildMode ? "none" : "static";
    graphics.cursor = "pointer";

    return graphics;
  }

  private setupEntityHoverEvents(graphic: Graphics, entityId: string): void {
    graphic.on("pointerenter", () => {
      const entity = World.getEntity(entityId);
      entity?.set(new HoverTagComponent(entityId));
    });

    graphic.on("pointerleave", () => {
      const entity = World.getEntity(entityId);
      entity?.remove(HoverTagComponent);
    });
  }

  private updatePhysicsTransform(graphic: Graphics, physics: PhysicsComponent, deltaTime: number): void {
    const lerpFactor = Math.min(deltaTime * 60, 1);

    const targetX = physics.position.x;
    const targetY = physics.position.y;
    const targetRotation = physics.rotationRadians;

    graphic.position.x += (targetX - graphic.position.x) * lerpFactor;
    graphic.position.y += (targetY - graphic.position.y) * lerpFactor;
    graphic.rotation += this.normalizeRotationDiff(targetRotation - graphic.rotation) * lerpFactor;
  }

  private updateChildTransform(graphic: Graphics, parentPhysics: PhysicsComponent, parentChild: ParentChildComponent, deltaTime: number): void {
    const gridX = parentChild.gridX * this.gridSize;
    const gridY = parentChild.gridY * this.gridSize;

    const cos = Math.cos(parentPhysics.rotationRadians);
    const sin = Math.sin(parentPhysics.rotationRadians);
    const rotatedX = gridX * cos - gridY * sin;
    const rotatedY = gridX * sin + gridY * cos;

    const worldX = parentPhysics.position.x + rotatedX;
    const worldY = parentPhysics.position.y + rotatedY;

    const lerpFactor = Math.min(deltaTime * 60, 1);
    graphic.position.x += (worldX - graphic.position.x) * lerpFactor;
    graphic.position.y += (worldY - graphic.position.y) * lerpFactor;

    const partRotation = parentChild.rotation * (Math.PI / 2);
    const totalRotation = parentPhysics.rotationRadians + partRotation;
    graphic.rotation += this.normalizeRotationDiff(totalRotation - graphic.rotation) * lerpFactor;
  }

  private drawChildShape(graphic: Graphics, parentChild: ParentChildComponent, render: RenderComponent): void {
    const halfSize = this.gridSize / 2;
    let points: number[] = [];

    if (parentChild.shape === 1) {
      points = [0, -halfSize, -halfSize, halfSize, halfSize, halfSize];
    } else {
      points = [-halfSize, -halfSize, halfSize, -halfSize, halfSize, halfSize, -halfSize, halfSize];
    }

    const color = this.normalizeColor(render.color);
    graphic.poly(points).fill(color);

    if (this.isEnginePart(render)) this.drawEngineNozzle(graphic);
  }

  private isEnginePart(render: RenderComponent): boolean {
    const color = this.normalizeColor(render.color);
    return color === 0xff8000;
  }

  private drawEngineNozzle(graphic: Graphics): void {
    const exhaustDirection = Math.PI;

    const nozzleLength = this.gridSize * 0.3;
    const nozzleWidth = this.gridSize * 0.15;
    const nozzleDistance = this.gridSize * 0.4;

    const nozzleCenterX = Math.cos(exhaustDirection) * nozzleDistance;
    const nozzleCenterY = Math.sin(exhaustDirection) * nozzleDistance;

    const nozzlePoints = [
      nozzleCenterX + Math.cos(exhaustDirection) * nozzleLength,
      nozzleCenterY + Math.sin(exhaustDirection) * nozzleLength,

      nozzleCenterX - Math.cos(exhaustDirection) * nozzleLength * 0.3 + Math.cos(exhaustDirection + Math.PI / 2) * nozzleWidth,
      nozzleCenterY - Math.sin(exhaustDirection) * nozzleLength * 0.3 + Math.sin(exhaustDirection + Math.PI / 2) * nozzleWidth,

      nozzleCenterX - Math.cos(exhaustDirection) * nozzleLength * 0.3 + Math.cos(exhaustDirection - Math.PI / 2) * nozzleWidth,
      nozzleCenterY - Math.sin(exhaustDirection) * nozzleLength * 0.3 + Math.sin(exhaustDirection - Math.PI / 2) * nozzleWidth,
    ];

    const nozzleColor = 0xcc4400;
    graphic.poly(nozzlePoints).fill(nozzleColor);
  }

  private normalizeRotationDiff(diff: number): number {
    while (diff > Math.PI) diff -= 2 * Math.PI;
    while (diff < -Math.PI) diff += 2 * Math.PI;
    return diff;
  }

  getGraphic(entityId: string): Graphics | undefined {
    const entity = World.getEntity(entityId);
    if (!entity) return undefined;

    const render = entity.get(RenderComponent);
    return render?.renderers.get(RenderComponent);
  }
}
