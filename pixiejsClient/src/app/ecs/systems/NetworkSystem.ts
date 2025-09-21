import { System } from "../core/System";
import { World } from "../core/World";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { NetworkComponent } from "../components/NetworkComponent";

export class NetworkSystem extends System {
  readonly componentTypes = [NetworkComponent];

  private movementUpdateListener: (event: Event) => void;
  private lineSpawnListener: (event: Event) => void;

  constructor() {
    super();

    // Bind the event listeners
    this.movementUpdateListener = this.handleMovementUpdate.bind(this);
    this.lineSpawnListener = this.handleLineSpawn.bind(this);

    // Listen for server movement updates
    window.addEventListener(
      "server-movement-update",
      this.movementUpdateListener,
    );

    // Listen for line/ray spawn events
    window.addEventListener("line-spawn", this.lineSpawnListener);
  }

  private handleMovementUpdate(event: Event): void {
    const customEvent = event as CustomEvent;
    const { entityId, position, velocity, rotation } = customEvent.detail;

    // Get the entity
    const entity = World.getEntity(entityId);
    if (!entity) {
      console.warn(
        `NetworkSystem: Entity ${entityId} not found for movement update`,
      );
      return;
    }

    // Ensure entity has required components
    if (!entity.has(PhysicsComponent) || !entity.has(NetworkComponent)) {
      console.warn(
        `NetworkSystem: Entity ${entityId} missing required components for movement update`,
      );
      return;
    }

    // Update physics component with server data
    const physics = entity.get(PhysicsComponent);
    if (physics) {
      physics.position.x = position.x;
      physics.position.y = position.y;
      physics.linearVelocity.x = velocity.x;
      physics.linearVelocity.y = velocity.y;
      physics.rotationRadians = rotation;

      // Mark component as changed to trigger render updates
      World.notifyComponentChange(entity);
    }
  }

  private handleLineSpawn(event: Event): void {
    const customEvent = event as CustomEvent;
    const { origin, hit } = customEvent.detail;

    // For now, just dispatch a line render event for the RenderSystem to handle
    // Lines are temporary visual effects, not persistent entities
    const renderEvent = new CustomEvent("render-line", {
      detail: {
        origin,
        hit,
        color: 0xff0000, // Default red color for rays
        duration: 1000, // Show for 1 second
      },
    });
    window.dispatchEvent(renderEvent);
  }

  updateEntity(_entity: any, _deltaTime: number): void {
    // This system primarily responds to events, no entity-specific updates needed
  }

  cleanup(): void {
    // Clean up event listeners
    window.removeEventListener(
      "server-movement-update",
      this.movementUpdateListener,
    );
    window.removeEventListener("line-spawn", this.lineSpawnListener);
  }
}
