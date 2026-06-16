import { System1 } from "../core/System";
import { NTT } from "../core/NTT";
import { DeathTagComponent } from "../components/DeathTagComponent";
import { ParentChildComponent } from "../components/ParentChildComponent";
import { World } from "../core/World";
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

  protected updateEntity(ntt: NTT, _dtc: DeathTagComponent, _deltaTime: number): void {
    this.spawnDeathEffect(ntt);

    if (ntt === World.Me) return;

    const parentChild = ntt.get(ParentChildComponent);
    if (parentChild && parentChild.parentId === World.Me?.id) {
      ShipPartManager.getInstance().notifyPartDestroyed(ntt);
    }

    this.cleanupGraphics(ntt);
    World.destroyEntity(ntt);
  }

  private cleanupGraphics(entity: NTT): void {
    const render = entity.get(RenderComponent);
    if (!render) return;

    for (const graphic of render.renderers.values()) {
      graphic.parent?.removeChild(graphic);
      graphic.destroy();
    }
    render.renderers.clear();
  }

  private spawnDeathEffect(ntt: NTT): void {
    const physics = ntt.get(PhysicsComponent);
    if (!physics) return;

    const effectEntity = World.createEntity();
    effectEntity.set(new EffectComponent(effectEntity, EffectType.Despawn, 0xff4444));
    effectEntity.set(new LifeTimeComponent(effectEntity, 1.0));

    const effectPhysics = new PhysicsComponent(effectEntity);
    effectPhysics.position = { x: physics.position.x, y: physics.position.y };
    effectPhysics.rotationRadians = physics.rotationRadians;
    effectPhysics.sides = 4;
    effectEntity.set(effectPhysics);
  }
}
