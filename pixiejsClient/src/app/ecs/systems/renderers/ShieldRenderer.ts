import { Graphics } from "pixi.js";
import { BaseRenderer } from "./BaseRenderer";
import { Entity } from "../../core/Entity";
import { Box2DBodyComponent } from "../../components/Box2DBodyComponent";
import { ShieldComponent } from "../../components/ShieldComponent";
import { World } from "../../core/World";

/**
 * Renders shield effects
 */
export class ShieldRenderer extends BaseRenderer {
  update(_deltaTime: number): void {
    const entities = World.queryEntitiesWithComponents(Box2DBodyComponent, ShieldComponent);

    for (const entity of entities) {
      this.updateEntity(entity);
    }
  }

  private updateEntity(entity: Entity): void {
    const physics = entity.get(Box2DBodyComponent)!;
    const shield = entity.get(ShieldComponent)!;

    if (!physics || !shield) return;

    if (shield.powerOn && shield.charge > 0) {
      this.drawShield(entity, shield, physics);
    } else {
      this.removeGraphic(entity.id);
    }
  }

  private drawShield(entity: Entity, shield: ShieldComponent, physics: Box2DBodyComponent): void {
    let graphics = this.graphics.get(entity.id);
    if (!graphics) {
      graphics = new Graphics();
      this.graphics.set(entity.id, graphics);
      this.gameContainer.addChild(graphics);
    }

    graphics.clear();

    const chargePercent = shield.charge / shield.maxCharge;
    const color = this.getShieldColor(chargePercent);
    const time = Date.now() * 0.001;

    const layers = 3;
    for (let i = 0; i < layers; i++) {
      const offset = i * 0.3;
      const radius = shield.radius - offset;
      const baseAlpha = 0.15 - i * 0.04;
      const minAlpha = 0.3;
      const alpha = baseAlpha * (minAlpha + chargePercent * (1 - minAlpha));

      const rotation = time * (0.5 + i * 0.3);
      const sides = 6;

      const points: number[] = [];
      for (let j = 0; j < sides; j++) {
        const angle = (j / sides) * Math.PI * 2 + rotation;
        points.push(Math.cos(angle) * radius, Math.sin(angle) * radius);
      }

      graphics.poly(points).stroke({ width: 0.2, color, alpha });
    }

    graphics.position.x = physics.position.x;
    graphics.position.y = physics.position.y;

    if (chargePercent < 0.3) {
      const pulseAlpha = 0.5 + 0.3 * Math.sin(time * 10);
      graphics.alpha = pulseAlpha;
    } else {
      graphics.alpha = 1.0;
    }
  }

  private getShieldColor(chargePercent: number): number {
    // Smooth color interpolation between red -> yellow -> blue
    if (chargePercent < 0.5) {
      // Red (0xff4444) to Yellow (0xffff44)
      const t = chargePercent / 0.5;
      const r = 0xff;
      const g = Math.floor(0x44 + (0xff - 0x44) * t);
      const b = 0x44;
      return (r << 16) | (g << 8) | b;
    } else {
      // Yellow (0xffff44) to Blue (0x4444ff)
      const t = (chargePercent - 0.5) / 0.5;
      const r = Math.floor(0xff - (0xff - 0x44) * t);
      const g = Math.floor(0xff - (0xff - 0x44) * t);
      const b = Math.floor(0x44 + (0xff - 0x44) * t);
      return (r << 16) | (g << 8) | b;
    }
  }
}
