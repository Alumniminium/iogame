import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { PhysicsComponent } from '../components/PhysicsComponent';
import { NetworkComponent } from '../components/NetworkComponent';
import { InputManager } from '../../input/InputManager';
import { NetworkManager } from '../../network/NetworkManager';
import { Vector2 } from '../core/types';

export interface InputState {
  moveX: number;
  moveY: number;
  fire: boolean;
  thrust: boolean;
  invThrust: boolean;
  rcs: boolean;
  shield: boolean;
  mouseButtons: number;
  keys: Set<string>;
}

export interface Camera {
  x: number;
  y: number;
  zoom: number;
}

export class InputSystem extends System {
  readonly componentTypes = [PhysicsComponent, NetworkComponent];

  private inputManager: InputManager;
  private networkManager: NetworkManager;
  private localEntityId: number | null = null;
  private lastInputSentTime = 0;
  private inputSendRate = 1000 / 60; // 60 Hz

  constructor(inputManager: InputManager, networkManager: NetworkManager) {
    super();
    this.inputManager = inputManager;
    this.networkManager = networkManager;
  }

  setLocalEntity(entityId: number): void {
    this.localEntityId = entityId;
  }

  getLocalEntity(): number | null {
    return this.localEntityId;
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    // Only process the local player entity
    if (!this.localEntityId || entity.id !== this.localEntityId) {
      return;
    }

    const physics = entity.getComponent(PhysicsComponent)!
    const network = entity.getComponent(NetworkComponent)!

    if (!network.isLocallyControlled) return;

    // Get current input state
    const input = this.inputManager.getInputState();

    // Get mouse world position
    const camera = this.getCamera(entity);
    const mouseWorld = this.inputManager.getMouseWorldPosition(camera);

    // Apply input locally for immediate response (client-side prediction)
    this.applyInputToEntity(entity, input, mouseWorld, deltaTime);

    // Send input to server at fixed rate
    const currentTime = Date.now();
    if (currentTime - this.lastInputSentTime >= this.inputSendRate) {
      this.sendInputToServer(input, mouseWorld);
      this.lastInputSentTime = currentTime;
    }
  }

  private applyInputToEntity(
    entity: Entity,
    input: InputState,
    mouseWorld: Vector2,
    deltaTime: number
  ): void {
    const physics = entity.getComponent(PhysicsComponent)!
    const network = entity.getComponent(NetworkComponent)!

    // Server is authoritative for both movement AND rotation
    // We don't modify physics locally - all updates come from server
    // The server will rotate the player based on the mouse position we send
  }

  private sendInputToServer(input: InputState, mouseWorld: Vector2): void {
    // Use NetworkManager to send input - this maintains the network protocol
    this.networkManager.sendInput(
      input,
      mouseWorld.x,
      mouseWorld.y
    );
  }

  private getCamera(entity: Entity): Camera {
    const physics = entity.getComponent(PhysicsComponent);
    if (physics) {
      return {
        x: physics.position.x,
        y: physics.position.y,
        zoom: 1
      };
    }

    return { x: 0, y: 0, zoom: 1 };
  }
}