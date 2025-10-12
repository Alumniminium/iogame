import { System1 } from "../core/System";
import { Entity } from "../core/Entity";
import { LifeTimeComponent } from "../components/LifeTimeComponent";
import { DeathTagComponent } from "../components/DeathTagComponent";

export class LifetimeSystem extends System1<LifeTimeComponent> {
  constructor() {
    super(LifeTimeComponent);
  }

  protected updateEntity(entity: Entity, lifetime: LifeTimeComponent, deltaTime: number): void {
    lifetime.lifetimeSeconds -= deltaTime;

    if (lifetime.lifetimeSeconds <= 0) {
      entity.set(new DeathTagComponent(entity.id));
    }
  }
}
