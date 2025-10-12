import { Container, Graphics } from "pixi.js";
import { System2 } from "../../core/System";
import { Entity } from "../../core/Entity";
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

  protected updateEntity(entity: Entity, physics: PhysicsComponent, effect: EffectComponent, _deltaTime: number): void {
    const render = entity.get(RenderComponent);
    if (!render) return;

    let graphic = render.renderers.get(EffectComponent);
    if (!graphic) {
      graphic = new Graphics();
      this.gameContainer.addChild(graphic);
      render.renderers.set(EffectComponent, graphic);
    }

    graphic.clear();
    graphic.position.set(physics.position.x, physics.position.y);

    const lifetime = entity.get(LifeTimeComponent);
    const progress = lifetime ? Math.max(0, Math.min(1, 1 - lifetime.lifetimeSeconds / 1)) : 0.5;

    switch (effect.effectType) {
      case EffectType.Spawn:
        this.renderSpawnEffect(graphic, effect.color, progress);
        break;
      case EffectType.Hit:
        this.renderHitEffect(graphic, effect.color, progress);
        break;
      case EffectType.Despawn:
        this.renderDespawnEffect(graphic, effect.color, progress);
        break;
    }
  }

  private renderSpawnEffect(graphic: Graphics, color: number, progress: number): void {
    const radius = progress * 5;
    const alpha = 1 - progress;

    graphic.circle(0, 0, radius).stroke({ width: 0.3, color, alpha });
    graphic.circle(0, 0, radius * 0.7).stroke({ width: 0.2, color, alpha: alpha * 0.5 });
  }

  private renderHitEffect(graphic: Graphics, color: number, progress: number): void {
    const length = progress * 3;
    const alpha = 1 - progress;
    const lineCount = 8;

    for (let i = 0; i < lineCount; i++) {
      const angle = (i / lineCount) * Math.PI * 2;
      const x = Math.cos(angle) * length;
      const y = Math.sin(angle) * length;

      graphic.moveTo(0, 0).lineTo(x, y).stroke({ width: 0.2, color, alpha });
    }

    graphic.circle(0, 0, 0.5 * (1 - progress)).fill({ color, alpha: alpha * 0.8 });
  }

  private renderDespawnEffect(graphic: Graphics, color: number, progress: number): void {
    const radius = (1 - progress) * 3;
    const alpha = 1 - progress;

    graphic.circle(0, 0, radius).stroke({ width: 0.3, color, alpha });
    graphic.circle(0, 0, radius * 0.5).fill({ color, alpha: alpha * 0.3 });
  }
}
