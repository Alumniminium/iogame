import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { NetworkComponent } from "../components/NetworkComponent";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { PredictionSystem } from "./PredictionSystem";

export class NetworkSystem extends System {
  readonly componentTypes = [NetworkComponent, PhysicsComponent];

  private predictionSystem: PredictionSystem | null = null;

  constructor() {
    super();
  }

  setPredictionSystem(predictionSystem: PredictionSystem): void {
    this.predictionSystem = predictionSystem;
  }

  initialize(): void {
    console.log("NetworkSystem initialized");
  }

  cleanup(): void {
    this.disconnect();
  }

  setLocalEntity(entityId: number): void {
    console.log(`NetworkSystem: Set local entity ID to ${entityId}`);
  }

  disconnect(): void {
    console.log("NetworkSystem: Disconnected");
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const network = entity.get(NetworkComponent)!;
    const physics = entity.get(PhysicsComponent);

    // Don't process here - let PredictionSystem handle everything
    // This avoids double-processing entities
  }

  private interpolateRemoteEntity(
    network: NetworkComponent,
    physics: PhysicsComponent,
    deltaTime: number,
  ): void {
    // Simple interpolation toward server position
    // In a full implementation, this would use proper snapshot interpolation
    const lerpFactor = Math.min(deltaTime * 10, 1); // Smooth interpolation

    // Lerp position toward server position
    physics.position.x +=
      (network.serverPosition.x - physics.position.x) * lerpFactor;
    physics.position.y +=
      (network.serverPosition.y - physics.position.y) * lerpFactor;

    // Lerp rotation toward server rotation
    let rotationDiff = network.serverRotation - physics.rotationRadians;

    // Handle rotation wrapping
    if (rotationDiff > Math.PI) rotationDiff -= 2 * Math.PI;
    if (rotationDiff < -Math.PI) rotationDiff += 2 * Math.PI;

    physics.rotationRadians += rotationDiff * lerpFactor;

    // Update velocity toward server velocity
    physics.linearVelocity.x +=
      (network.serverVelocity.x - physics.linearVelocity.x) * lerpFactor;
    physics.linearVelocity.y +=
      (network.serverVelocity.y - physics.linearVelocity.y) * lerpFactor;

    physics.markChanged();
  }

  // Method for NetworkManager to update entity states from server
  public updateEntityFromServer(
    entityId: number,
    position: { x: number; y: number },
    velocity: { x: number; y: number },
    rotation: number,
    timestamp: number,
    inputSequence?: number,
  ): void {
    const entity = this.getEntity(entityId);
    if (!entity) return;

    const network = entity.get(NetworkComponent);
    const physics = entity.get(PhysicsComponent);

    if (network) {
      network.updateServerState(position, velocity, rotation, timestamp);

      // Update server tick tracking
      if (inputSequence !== undefined) {
        network.updateLastServerTick(inputSequence);
      }

      if (network.isLocallyControlled) {
        // For local player, trigger reconciliation through PredictionSystem
        if (this.predictionSystem && inputSequence !== undefined) {
          this.predictionSystem.onServerUpdate(entityId, position, velocity, rotation, inputSequence, inputSequence);
        }
      } else {
        // For remote entities, immediately update physics to server state
        if (physics) {
          physics.position = { ...position };
          physics.linearVelocity = { ...velocity };
          physics.rotationRadians = rotation;
          physics.markChanged();
        }
      }
    }
  }
}
