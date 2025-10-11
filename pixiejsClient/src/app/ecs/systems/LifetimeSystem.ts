import { System1 } from "../core/System";
import { Entity } from "../core/Entity";
import { LifeTimeComponent } from "../components/LifeTimeComponent";
import { World } from "../core/World";

/**
 * Destroys entities when their lifetime expires.
 * Useful for temporary entities like projectiles or particles.
 *
 * Uses System1 for automatic entity filtering and type-safe component access.
 */
export class LifetimeSystem extends System1<LifeTimeComponent> {
  constructor() {
    super(LifeTimeComponent);
  }

  protected updateEntity(entity: Entity, lifetime: LifeTimeComponent, deltaTime: number): void {
    lifetime.lifetimeSeconds -= deltaTime / 1000;

    if (lifetime.lifetimeSeconds <= 0) {
      World.destroyEntity(entity);
    }
  }
}
