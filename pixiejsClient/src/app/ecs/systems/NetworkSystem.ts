import { System } from "../core/System";
import { World } from "../core/World";
import { Entity } from "../core/Entity";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { NetworkComponent } from "../components/NetworkComponent";

/**
 * Handles server state updates for networked entities.
 * Listens for server movement updates and line spawn events,
 * applying them to local entity state.
 */
export class NetworkSystem extends System {
  readonly componentTypes = [NetworkComponent];

  private movementUpdateListener: (event: Event) => void;
  private lineSpawnListener: (event: Event) => void;

  constructor() {
    super();

    this.movementUpdateListener = this.handleMovementUpdate.bind(this);
    this.lineSpawnListener = this.handleLineSpawn.bind(this);

    window.addEventListener(
      "server-movement-update",
      this.movementUpdateListener,
    );

    window.addEventListener("line-spawn", this.lineSpawnListener);
  }

  private handleMovementUpdate(event: Event): void {
    const customEvent = event as CustomEvent;
    const { entityId, position, velocity, rotation } = customEvent.detail;

    const entity = World.getEntity(entityId);
    if (!entity) return;

    if (!entity.has(PhysicsComponent) || !entity.has(NetworkComponent)) return;

    const physics = entity.get(PhysicsComponent);
    if (physics) {
      physics.position.x = position.x;
      physics.position.y = position.y;
      physics.linearVelocity.x = velocity.x;
      physics.linearVelocity.y = velocity.y;
      physics.rotationRadians = rotation;

      World.notifyComponentChange(entity);
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

  updateEntity(entity: Entity, deltaTime: number): void {}

  cleanup(): void {
    window.removeEventListener(
      "server-movement-update",
      this.movementUpdateListener,
    );
    window.removeEventListener("line-spawn", this.lineSpawnListener);
  }
}
