import { Graphics } from "pixi.js";
import { Box2DBodyComponent } from "../../components/Box2DBodyComponent";
import { RenderComponent } from "../../components/RenderComponent";

/**
 * Renders ship parts and engine nozzles.
 * Does not manage Graphics objects - receives them from EntityRenderer.
 */
export class ShipPartRenderer {
  private localPlayerId: string | null = null;

  update(_deltaTime: number): void {
    // No per-frame updates needed - drawing is done on-demand by EntityRenderer
  }

  drawEntity(
    graphics: Graphics,
    render: RenderComponent,
    physics: Box2DBodyComponent,
    entityId: string,
  ): void {
    if (!render.shipParts || render.shipParts.length === 0) {
      this.drawFallbackShape(graphics, render, physics);
      return;
    }

    this.drawCompoundShape(graphics, render);

    if (entityId === this.localPlayerId) {
      this.drawDirectionArrow(graphics, physics.size);
    }
  }

  private drawCompoundShape(graphics: Graphics, render: RenderComponent): void {
    const gridSize = 1.0;

    for (const part of render.shipParts || []) {
      const gridX = part.gridX * gridSize;
      const gridY = part.gridY * gridSize;

      const partRotation = part.rotation * (Math.PI / 2);

      let points: number[] = [];
      const halfSize = gridSize / 2;

      if (part.shape === 1) {
        points = [0, -halfSize, -halfSize, halfSize, halfSize, halfSize];
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
        points[i] += gridX;
        points[i + 1] += gridY;
      }

      const partColor = this.getPartColor(part.type);

      graphics.poly(points).fill(partColor);

      if (part.type === 2)
        this.drawEngineNozzle(graphics, gridX, gridY, partRotation, gridSize);
    }
  }

  private drawFallbackShape(
    graphics: Graphics,
    render: RenderComponent,
    physics: Box2DBodyComponent,
  ): void {
    const size = physics.size;
    const halfSize = size / 2;

    let points: number[] = [];

    if (render.shapeType === 1) {
      points = [0, -halfSize, -halfSize, halfSize, halfSize, halfSize];
    } else if (render.shapeType === 0) {
      const segments = 8;
      points = [];
      for (let i = 0; i < segments; i++) {
        const angle = (i / segments) * Math.PI * 2;
        points.push(Math.cos(angle) * halfSize, Math.sin(angle) * halfSize);
      }
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

    const color = this.normalizeColor(render.color);
    graphics.poly(points).fill(color);
  }

  private normalizeColor(color: number | undefined | null): number {
    if (color === undefined || color === null) return 0xffffff;
    return color & 0xffffff;
  }

  private getPartColor(type: number): number {
    switch (type) {
      case 0:
        return 0x808080;
      case 1:
        return 0x0080ff;
      case 2:
        return 0xff8000;
      default:
        return 0xffffff;
    }
  }

  private drawEngineNozzle(
    graphics: Graphics,
    engineX: number,
    engineY: number,
    engineRotation: number,
    gridSize: number,
  ): void {
    const exhaustDirection = engineRotation + Math.PI;

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

    const nozzleColor = 0xcc4400;
    graphics.poly(nozzlePoints).fill(nozzleColor);
  }

  private drawDirectionArrow(graphics: Graphics, entitySize: number): void {
    const arrowSize = entitySize * 0.4;
    const arrowDistance = entitySize * 0.2;

    const arrowPoints = [
      arrowDistance,
      0,
      arrowDistance - arrowSize,
      -arrowSize * 0.4,
      arrowDistance - arrowSize,
      arrowSize * 0.4,
    ];

    graphics.poly(arrowPoints).fill(0x000000);
  }

  setLocalPlayerId(playerId: string | null): void {
    this.localPlayerId = playerId;
  }

  initialize(): void {}
  cleanup(): void {}
}
