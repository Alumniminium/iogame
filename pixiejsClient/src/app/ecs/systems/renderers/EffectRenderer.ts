import { Container, Graphics } from "pixi.js";
import { System2 } from "../../core/System";
import { NTT } from "../../core/NTT";
import { PhysicsComponent } from "../../components/PhysicsComponent";
import { EffectComponent } from "../../components/EffectComponent";
import { LifeTimeComponent } from "../../components/LifeTimeComponent";
import { EffectType } from "../../../enums/EffectType";
import { RenderComponent } from "../../components/RenderComponent";

export class EffectRenderer extends System2<PhysicsComponent, EffectComponent> {
  private gameContainer: Container;

  constructor(gameContainer: Container) {
    super(PhysicsComponent, EffectComponent);
    this.gameContainer = gameContainer;
  }

  protected updateEntity(ntt: NTT, phy: PhysicsComponent, ec: EffectComponent, _deltaTime: number): void {
    const render = ntt.get(RenderComponent);
    if (!render) return;

    let graphics = render.renderers.get(EffectComponent);
    if (!graphics) {
      graphics = new Graphics();
      this.gameContainer.addChild(graphics);
      render.renderers.set(EffectComponent, graphics);
    }

    graphics.clear();
    graphics.position.set(phy.position.x, phy.position.y);

    const ltc = ntt.get(LifeTimeComponent);
    const progress = ltc ? Math.max(0, Math.min(1, 1 - ltc.lifetimeSeconds / 1)) : 0.5;

    switch (ec.effectType) {
      case EffectType.Spawn:
        this.renderSpawnEffect(graphics, ec.color, progress);
        break;
      case EffectType.Hit:
        this.renderHitEffect(graphics, ec.color, progress);
        break;
      case EffectType.Despawn:
        this.renderDespawnEffect(graphics, ec.color, progress);
        break;
    }
  }

  private renderSpawnEffect(graphics: Graphics, color: number, progress: number): void {
    const radius = progress * 5;
    const alpha = 1 - progress;

    graphics.circle(0, 0, radius).stroke({ width: 0.3, color, alpha });
    graphics.circle(0, 0, radius * 0.7).stroke({ width: 0.2, color, alpha: alpha * 0.5 });
  }

  private renderHitEffect(graphics: Graphics, color: number, progress: number): void {
    const length = progress * 3;
    const alpha = 1 - progress;
    const lineCount = 8;

    for (let i = 0; i < lineCount; i++) {
      const angle = (i / lineCount) * Math.PI * 2;
      const x = Math.cos(angle) * length;
      const y = Math.sin(angle) * length;

      graphics.moveTo(0, 0).lineTo(x, y).stroke({ width: 0.2, color, alpha });
    }

    graphics.circle(0, 0, 0.5 * (1 - progress)).fill({ color, alpha: alpha * 0.8 });
  }

  private renderDespawnEffect(graphics: Graphics, color: number, progress: number): void {
    const radius = (1 - progress) * 3;
    const alpha = 1 - progress;

    graphics.circle(0, 0, radius).stroke({ width: 0.3, color, alpha });
    graphics.circle(0, 0, radius * 0.5).fill({ color, alpha: alpha * 0.3 });
  }
}
