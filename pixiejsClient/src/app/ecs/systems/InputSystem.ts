import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { NetworkComponent } from "../components/NetworkComponent";
import { EngineComponent } from "../components/EngineComponent";
import { ShieldComponent } from "../components/ShieldComponent";
import type { InputManager } from "../../input/InputManager";
// import { PredictionSystem } from "./PredictionSystem"; // TODO: PredictionSystem not implemented yet
// import { InputSnapshot } from "./InputBuffer"; // TODO: PredictionSystem not implemented yet

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
  private localEntityId: string | null = null;
  private paused: boolean = false;
  // private predictionSystem: PredictionSystem | null = null; // TODO: PredictionSystem not implemented yet

  constructor(inputManager: InputManager) {
    super();
    this.inputManager = inputManager;
  }

  // setPredictionSystem(predictionSystem: PredictionSystem): void {
  //   this.predictionSystem = predictionSystem;
  // } // TODO: PredictionSystem not implemented yet

  setLocalEntity(entityId: string): void {
    this.localEntityId = entityId;
    console.log(`InputSystem: Set local entity ID to ${entityId}`);
  }

  setPaused(paused: boolean): void {
    this.paused = paused;
    console.log(`InputSystem: ${paused ? 'Paused' : 'Resumed'}`);
  }

  protected updateEntity(entity: Entity, _deltaTime: number): void {
    // Skip input processing if paused (e.g., in build mode)
    if (this.paused) {
      return;
    }

    const network = entity.get(NetworkComponent)!;
    const physics = entity.get(PhysicsComponent);

    // Only process input for locally controlled entities
    if (!network.isLocallyControlled || entity.id !== this.localEntityId) {
      return;
    }

    if (!physics) return;

    // Get current input state
    const input = this.inputManager.getInputState();

    // TODO: PredictionSystem not implemented yet - input snapshots for prediction would be created here

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
      engine.throttle = Math.max(
        0,
        Math.min(1, engine.throttle + 1 * deltaTime),
      );
    } else if (input.invThrust) {
      // Gradual throttle decrease (server lines 122-126)
      const deltaTime = 1 / 60; // 60hz tick rate
      engine.throttle = Math.max(
        0,
        Math.min(1, engine.throttle - 1 * deltaTime),
      );
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
