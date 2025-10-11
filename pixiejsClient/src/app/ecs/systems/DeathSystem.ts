import { System1 } from "../core/System";
import { Entity } from "../core/Entity";
import { DeathTagComponent } from "../components/DeathTagComponent";
import { ParentChildComponent } from "../components/ParentChildComponent";
import { World } from "../core/World";

/**
 * Handles entity death processing on the client.
 * Destroys entities marked with DeathTag and dispatches death events.
 *
 * Uses System1 for automatic entity filtering and type-safe component access.
 */
export class DeathSystem extends System1<DeathTagComponent> {
  constructor() {
    super(DeathTagComponent);
  }

  protected updateEntity(entity: Entity, deathTag: DeathTagComponent, _deltaTime: number): void {
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
