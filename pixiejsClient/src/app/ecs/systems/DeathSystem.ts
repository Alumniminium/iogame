import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { DeathTagComponent } from "../components/DeathTagComponent";
import { ParentChildComponent } from "../components/ParentChildComponent";
import { World } from "../core/World";

/**
 * Handles entity death processing on the client.
 * Destroys entities marked with DeathTag and dispatches death events.
 */
export class DeathSystem extends System {
  readonly componentTypes = [DeathTagComponent];

  update(_deltaTime: number): void {
    const entities = World.queryEntitiesWithComponents(DeathTagComponent);

    for (const entity of entities) {
      this.updateEntity(entity);
    }
  }

  protected updateEntity(entity: Entity): void {
    const deathTag = entity.get(DeathTagComponent)!;
    const localPlayerId = (window as any).localPlayerId;

    console.log(`[DeathSystem] Processing death for ${entity.id}`);

    // Handle entity death
    if (entity.id !== localPlayerId) {
      // Dispatch parent-child update for ship parts
      const parentChild = entity.get(ParentChildComponent);
      if (parentChild) {
        window.dispatchEvent(
          new CustomEvent("parent-child-update", {
            detail: {
              childId: entity.id,
              parentId: parentChild.parentId,
            },
          }),
        );
      }

      // Destroy the entity
      World.destroyEntity(entity);
      console.log(`[DeathSystem] Destroyed entity ${entity.id}`);
    } else {
      console.log(`[DeathSystem] Skipping local player ${entity.id}`);
    }

    // Dispatch death event for UI/effects
    window.dispatchEvent(
      new CustomEvent("entity-death", {
        detail: { entityId: entity.id, killerId: deathTag.killer },
      }),
    );
  }
}
