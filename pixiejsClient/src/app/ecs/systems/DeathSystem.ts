import { System1 } from "../core/System";
import { Entity } from "../core/Entity";
import { DeathTagComponent } from "../components/DeathTagComponent";
import { ParentChildComponent } from "../components/ParentChildComponent";
import { World } from "../core/World";
import { EntityType } from "../core/types";
import { EffectComponent } from "../components/EffectComponent";
import { EffectType } from "../../enums/EffectType";
import { LifeTimeComponent } from "../components/LifeTimeComponent";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { ShipPartManager } from "../../managers/ShipPartManager";
import { RenderComponent } from "../components/RenderComponent";

export class DeathSystem extends System1<DeathTagComponent> {
  constructor() {
    super(DeathTagComponent);
  }

  protected updateEntity(entity: Entity, _deathTag: DeathTagComponent, _deltaTime: number): void {
    this.spawnDeathEffect(entity);

    if (entity === World.Me) return;

    const parentChild = entity.get(ParentChildComponent);
    if (parentChild && parentChild.parentId === World.Me?.id) {
      ShipPartManager.getInstance().notifyPartDestroyed(entity.id);
    }

    this.cleanupGraphics(entity);

    World.destroyEntity(entity);
  }

  private cleanupGraphics(entity: Entity): void {
    const render = entity.get(RenderComponent);
    if (!render) return;

    for (const graphic of render.renderers.values()) {
      graphic.parent?.removeChild(graphic);
      graphic.destroy();
    }
    render.renderers.clear();
  }

  private spawnDeathEffect(entity: Entity): void {
    const physics = entity.get(PhysicsComponent);
    if (!physics) return;

    const effectEntity = World.createEntity(EntityType.Debug);
    effectEntity.set(new EffectComponent(effectEntity.id, EffectType.Despawn, 0xff4444));
    effectEntity.set(new LifeTimeComponent(effectEntity.id, 1.0));

    const effectPhysics = new PhysicsComponent(effectEntity.id);
    effectPhysics.position = { x: physics.position.x, y: physics.position.y };
    effectPhysics.rotationRadians = physics.rotationRadians;
    effectPhysics.sides = 4;
    effectEntity.set(effectPhysics);
  }
}
