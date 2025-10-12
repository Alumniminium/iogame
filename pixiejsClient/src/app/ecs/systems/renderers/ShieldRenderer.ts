import { Container, Graphics } from "pixi.js";
import { System2 } from "../../core/System";
import { NTT } from "../../core/NTT";
import { PhysicsComponent } from "../../components/PhysicsComponent";
import { ShieldComponent } from "../../components/ShieldComponent";
import { RenderComponent } from "../../components/RenderComponent";

export class ShieldRenderer extends System2<PhysicsComponent, ShieldComponent> {
  private gameContainer: Container;

  constructor(gameContainer: Container) {
    super(PhysicsComponent, ShieldComponent);
    this.gameContainer = gameContainer;
  }

  protected updateEntity(ntt: NTT, phy: PhysicsComponent, sc: ShieldComponent, _deltaTime: number): void {
    const render = ntt.get(RenderComponent);
    if (!render) return;

    if (!sc.powerOn || sc.charge <= 0) {
      const graphic = render.renderers.get(ShieldComponent);
      if (graphic) {
        graphic.parent?.removeChild(graphic);
        graphic.destroy();
        render.renderers.delete(ShieldComponent);
      }
      return;
    }

    this.drawShield(ntt, sc, phy, render);
  }

  private drawShield(_ntt: NTT, sc: ShieldComponent, phy: PhysicsComponent, rc: RenderComponent): void {
    let graphic = rc.renderers.get(ShieldComponent);
    if (!graphic) {
      graphic = new Graphics();
      this.gameContainer.addChild(graphic);
      rc.renderers.set(ShieldComponent, graphic);
    }

    graphic.clear();

    const chargePercent = sc.charge / sc.maxCharge;
    const color = this.getShieldColor(chargePercent);
    const time = Date.now() * 0.001;

    const layers = 3;
    for (let i = 0; i < layers; i++) {
      const offset = i * 0.3;
      const radius = sc.radius - offset;
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

      graphic.poly(points).stroke({ width: 0.2, color, alpha });
    }

    graphic.position.x = phy.position.x;
    graphic.position.y = phy.position.y;

    if (chargePercent < 0.3) {
      const pulseAlpha = 0.5 + 0.3 * Math.sin(time * 10);
      graphic.alpha = pulseAlpha;
    } else {
      graphic.alpha = 1.0;
    }
  }

  private getShieldColor(chargePercent: number): number {
    if (chargePercent < 0.5) {
      const t = chargePercent / 0.5;
      const r = 0xff;
      const g = Math.floor(0x44 + (0xff - 0x44) * t);
      const b = 0x44;
      return (r << 16) | (g << 8) | b;
    } else {
      const t = (chargePercent - 0.5) / 0.5;
      const r = Math.floor(0xff - (0xff - 0x44) * t);
      const g = Math.floor(0xff - (0xff - 0x44) * t);
      const b = Math.floor(0x44 + (0xff - 0x44) * t);
      return (r << 16) | (g << 8) | b;
    }
  }
}
