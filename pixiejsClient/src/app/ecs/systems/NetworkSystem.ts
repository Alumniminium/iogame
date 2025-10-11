import { System } from "../core/System";
import { World } from "../core/World";
import { Entity } from "../core/Entity";
import { Box2DBodyComponent } from "../components/Box2DBodyComponent";
import { NetworkComponent } from "../components/NetworkComponent";

/**
 * Handles server state updates for networked entities.
 * Listens for server movement updates and line spawn events,
 * applying them to local entity state.
 *
 * Uses base System class since it doesn't filter entities by components
 * (it works via event listeners instead).
 */
export class NetworkSystem extends System {
  private movementUpdateListener: (event: Event) => void;
  private lineSpawnListener: (event: Event) => void;

  constructor() {
    super();

    this.movementUpdateListener = this.handleMovementUpdate.bind(this);
    this.lineSpawnListener = this.handleLineSpawn.bind(this);

    window.addEventListener("server-movement-update", this.movementUpdateListener);
    window.addEventListener("line-spawn", this.lineSpawnListener);
  }

  protected matchesFilter(_entity: Entity): boolean {
    // This system doesn't filter entities - it works via events
    return false;
  }

  beginUpdate(_deltaTime: number): void {
    // No per-frame updates needed - all work done in event listeners
  }

  private handleMovementUpdate(event: Event): void {
    const customEvent = event as CustomEvent;
    const { entityId, position, velocity, rotation } = customEvent.detail;

    const entity = World.getEntity(entityId);
    if (!entity) return;

    if (!entity.has(Box2DBodyComponent) || !entity.has(NetworkComponent)) return;

    const physics = entity.get(Box2DBodyComponent);
    if (physics) {
      physics.position.x = position.x;
      physics.position.y = position.y;
      physics.linearVelocity.x = velocity.x;
      physics.linearVelocity.y = velocity.y;
      physics.rotationRadians = rotation;

      World.informChangesFor(entity);
    }
  }

  private handleLineSpawn(event: Event): void {
    const customEvent = event as CustomEvent;
    const { origin, hit } = customEvent.detail;

    const renderEvent = new CustomEvent("render-line", {
      detail: {
        origin,
        hit,
        color: 0xff0000,
        duration: 1000,
      },
    });
    window.dispatchEvent(renderEvent);
  }

  cleanup(): void {
    window.removeEventListener("server-movement-update", this.movementUpdateListener);
    window.removeEventListener("line-spawn", this.lineSpawnListener);
  }
}
