import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { LifetimeComponent } from "../components/LifetimeComponent";
import { World } from "../core/World";

/**
 * Destroys entities when their lifetime expires.
 * Useful for temporary entities like projectiles or particles.
 */
export class LifetimeSystem extends System {
  readonly componentTypes = [LifetimeComponent];

  update(deltaTime: number): void {
    const entities = World.queryEntitiesWithComponents(LifetimeComponent);

    for (const entity of entities) {
      this.updateEntity(entity, deltaTime);
    }
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const lifetime = entity.get(LifetimeComponent)!;

    lifetime.lifetimeSeconds -= deltaTime / 1000;

    if (lifetime.lifetimeSeconds <= 0)
      World.destroyEntity(entity);
  }
}
