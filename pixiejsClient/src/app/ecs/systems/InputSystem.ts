import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { NetworkComponent } from "../components/NetworkComponent";
import { EngineComponent } from "../components/EngineComponent";
import { ShieldComponent } from "../components/ShieldComponent";
import type { InputManager } from "../../input/InputManager";
import type { NetworkManager } from "../../network/NetworkManager";
import { PredictionSystem } from "./PredictionSystem";
import { InputSnapshot } from "./InputBuffer";

export interface InputState {
  moveX: number;
  moveY: number;
  fire: boolean;
  thrust: boolean;
  invThrust: boolean;
  left: boolean;
  right: boolean;
  boost: boolean;
  rcs: boolean;
  shield: boolean;
  drop: boolean;
  mouseX: number;
  mouseY: number;
  mouseButtons: number;
  keys: Set<string>;
}

export class InputSystem extends System {
  readonly componentTypes = [PhysicsComponent, NetworkComponent];

  private inputManager: InputManager;
  private networkManager: NetworkManager;
  private localEntityId: number | null = null;
  private predictionSystem: PredictionSystem | null = null;

  constructor(inputManager: InputManager, networkManager: NetworkManager) {
    super();
    this.inputManager = inputManager;
    this.networkManager = networkManager;
  }

  setPredictionSystem(predictionSystem: PredictionSystem): void {
    this.predictionSystem = predictionSystem;
  }

  setLocalEntity(entityId: number): void {
    this.localEntityId = entityId;
    console.log(`InputSystem: Set local entity ID to ${entityId}`);
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const network = entity.get(NetworkComponent)!;
    const physics = entity.get(PhysicsComponent);

    // Only process input for locally controlled entities
    if (!network.isLocallyControlled || entity.id !== this.localEntityId) {
      return;
    }

    if (!physics) return;

    // Get current input state
    const input = this.inputManager.getInputState();

    // Create input snapshot for prediction system
    if (this.predictionSystem) {
      const tickSynchronizer = this.networkManager.getTickSynchronizer();
      const serverTick = tickSynchronizer.getCurrentServerTick();

      const snapshot: InputSnapshot = {
        sequenceNumber: serverTick,
        timestamp: Date.now(),
        position: { ...physics.position },
        velocity: { ...physics.linearVelocity },
        rotation: physics.rotationRadians,
        inputState: {
          thrust: input.thrust,
          invThrust: input.invThrust,
          left: input.left,
          right: input.right,
          boost: input.boost,
          rcs: input.rcs,
          shield: input.shield,
          mouseDirection: { x: input.mouseX, y: input.mouseY }
        }
      };

      this.predictionSystem.addInputSnapshot(snapshot);
    }

    // Apply input to components (matching server InputSystem.cs)
    this.applyInputToComponents(entity, input);

    physics.markChanged();
  }

  private applyInputToComponents(entity: Entity, input: InputState): void {
    // Configure engine component (matches server InputSystem.cs ConfigureEngine method)
    if (entity.has(EngineComponent)) {
      this.configureEngine(entity, input);
    }

    // Configure shield component
    if (entity.has(ShieldComponent)) {
      this.configureShield(entity, input);
    }

    // Configure weapons (TODO: implement WeaponComponent)
    // Configure inventory (TODO: implement InventoryComponent)
  }

  private configureEngine(entity: Entity, input: InputState): void {
    const engine = entity.get(EngineComponent)!;

    engine.rcs = input.rcs;

    // Handle rotation input (matches server lines 109-129)
    if (input.left) {
      engine.rotation = -1;
    } else if (input.right) {
      engine.rotation = 1;
    } else {
      engine.rotation = 0;
    }

    // Handle boost (matches server lines 111-116)
    if (input.boost) {
      engine.throttle = 1;
    } else if (input.thrust) {
      // Gradual throttle increase (server lines 119-121)
      const deltaTime = 1 / 60; // 60hz tick rate
      engine.throttle = Math.max(0, Math.min(1, engine.throttle + (1 * deltaTime)));
    } else if (input.invThrust) {
      // Gradual throttle decrease (server lines 122-126)
      const deltaTime = 1 / 60; // 60hz tick rate
      engine.throttle = Math.max(0, Math.min(1, engine.throttle - (1 * deltaTime)));
    }

    engine.markChanged();
  }
  private configureShield(entity: Entity, input: InputState): void {
    const shield = entity.get(ShieldComponent);
    if (!shield) return;

    const powerOn = input.shield;
    if (shield.powerOn !== powerOn) {
      shield.powerOn = powerOn;
      shield.markChanged();
    }
  }
}
