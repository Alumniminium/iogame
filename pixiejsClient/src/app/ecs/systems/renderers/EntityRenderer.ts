import { Container, Graphics } from "pixi.js";
import { System1 } from "../../core/System";
import { NTT } from "../../core/NTT";
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

  protected updateEntity(ntt: NTT, rc: RenderComponent, deltaTime: number): void {
    const pcc = ntt.get(ParentChildComponent);

    if (pcc) {
      this.renderChildEntity(pcc, rc, deltaTime);
    } else {
      this.renderPhysicsEntity(ntt, rc, deltaTime);
    }
  }

  private renderPhysicsEntity(ntt: NTT, rc: RenderComponent, deltaTime: number): void {
    const phy = ntt.get(PhysicsComponent);
    if (!phy) return;

    let graphic = rc.renderers.get(RenderComponent);
    if (!graphic) {
      graphic = this.createEntityGraphic(ntt.id);
      this.setupEntityHoverEvents(graphic, ntt);
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

  private drawDirectionArrow(graphics: Graphics, nttSize: number): void {
    const arrowSize = nttSize * 0.4;
    const arrowDistance = nttSize * 0.2;

    const arrowPoints = [arrowDistance, 0, arrowDistance - arrowSize, -arrowSize * 0.4, arrowDistance - arrowSize, arrowSize * 0.4];

    graphics.poly(arrowPoints).fill(0x000000);
  }

  private normalizeColor(color: number | undefined | null): number {
    if (color === undefined || color === null) return 0xffffff;
    return color & 0xffffff;
  }

  private createEntityGraphic(ntt?: string | NTT): Graphics {
    const graphics = new Graphics();

    const id = ntt instanceof NTT ? ntt.id : ntt;
    const isPlayerInBuildMode = World.Me && id === World.Me.id && BuildModeManager.getInstance().isInBuildMode();

    graphics.interactive = !isPlayerInBuildMode;
    (graphics as any).eventMode = isPlayerInBuildMode ? "none" : "static";
    graphics.cursor = "pointer";

    return graphics;
  }

  private setupEntityHoverEvents(graphics: Graphics, ntt: NTT): void {
    graphics.on("pointerenter", () => {
      const entity = World.getEntity(ntt.id);
      if (entity) entity.set(new HoverTagComponent(entity));
    });

    graphics.on("pointerleave", () => {
      const entity = World.getEntity(ntt.id);
      entity?.remove(HoverTagComponent);
    });
  }

  private updatePhysicsTransform(graphics: Graphics, phy: PhysicsComponent, deltaTime: number): void {
    const lerpFactor = Math.min(deltaTime * 60, 1);

    const targetX = phy.position.x;
    const targetY = phy.position.y;
    const targetRotation = phy.rotationRadians;

    graphics.position.x += (targetX - graphics.position.x) * lerpFactor;
    graphics.position.y += (targetY - graphics.position.y) * lerpFactor;
    graphics.rotation += this.normalizeRotationDiff(targetRotation - graphics.rotation) * lerpFactor;
  }

  private updateChildTransform(graphics: Graphics, parentPhy: PhysicsComponent, parentPcc: ParentChildComponent, deltaTime: number): void {
    const gridX = parentPcc.gridX * this.gridSize;
    const gridY = parentPcc.gridY * this.gridSize;

    const cos = Math.cos(parentPhy.rotationRadians);
    const sin = Math.sin(parentPhy.rotationRadians);
    const rotatedX = gridX * cos - gridY * sin;
    const rotatedY = gridX * sin + gridY * cos;

    const worldX = parentPhy.position.x + rotatedX;
    const worldY = parentPhy.position.y + rotatedY;

    const lerpFactor = Math.min(deltaTime * 60, 1);
    graphics.position.x += (worldX - graphics.position.x) * lerpFactor;
    graphics.position.y += (worldY - graphics.position.y) * lerpFactor;

    const partRotation = parentPcc.rotation * (Math.PI / 2);
    const totalRotation = parentPhy.rotationRadians + partRotation;
    graphics.rotation += this.normalizeRotationDiff(totalRotation - graphics.rotation) * lerpFactor;
  }

  private drawChildShape(graphics: Graphics, pcc: ParentChildComponent, rc: RenderComponent): void {
    const halfSize = this.gridSize / 2;
    let points: number[] = [];

    if (pcc.shape === 1) {
      points = [0, -halfSize, -halfSize, halfSize, halfSize, halfSize];
    } else {
      points = [-halfSize, -halfSize, halfSize, -halfSize, halfSize, halfSize, -halfSize, halfSize];
    }

    const color = this.normalizeColor(rc.color);
    graphics.poly(points).fill(color);

    if (this.isEnginePart(rc)) this.drawEngineNozzle(graphics);
  }

  private isEnginePart(rc: RenderComponent): boolean {
    const color = this.normalizeColor(rc.color);
    return color === 0xff8000;
  }

  private drawEngineNozzle(graphics: Graphics): void {
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
    graphics.poly(nozzlePoints).fill(nozzleColor);
  }

  private normalizeRotationDiff(diff: number): number {
    while (diff > Math.PI) diff -= 2 * Math.PI;
    while (diff < -Math.PI) diff += 2 * Math.PI;
    return diff;
  }

  getGraphic(ntt: NTT): Graphics | undefined {
    const entity = World.getEntity(ntt.id);
    if (!entity) return undefined;

    const render = entity.get(RenderComponent);
    return render?.renderers.get(RenderComponent);
  }
}
