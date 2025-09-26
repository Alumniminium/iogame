import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { NetworkComponent } from "../components/NetworkComponent";
import { EngineComponent } from "../components/EngineComponent";
import { ShieldComponent } from "../components/ShieldComponent";
import type { InputManager } from "../../input/InputManager";

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

  constructor(inputManager: InputManager) {
    super();
    this.inputManager = inputManager;
  }

  setLocalEntity(entityId: string): void {
    this.localEntityId = entityId;
  }

  setPaused(paused: boolean): void {
    this.paused = paused;
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    if (this.paused) {
      return;
    }

    const network = entity.get(NetworkComponent)!;
    const physics = entity.get(PhysicsComponent);

    if (!network.isLocallyControlled || entity.id !== this.localEntityId) {
      return;
    }

    if (!physics) return;

    const input = this.inputManager.getInputState();

    this.applyInputToComponents(entity, input);

    physics.markChanged();
  }

  private applyInputToComponents(entity: Entity, input: InputState): void {
    if (entity.has(EngineComponent)) {
      this.configureEngine(entity, input);
    }

    if (entity.has(ShieldComponent)) {
      this.configureShield(entity, input);
    }
  }

  private configureEngine(entity: Entity, input: InputState): void {
    const engine = entity.get(EngineComponent)!;

    engine.rcs = input.rcs;

    if (input.left) {
      engine.rotation = -1;
    } else if (input.right) {
      engine.rotation = 1;
    } else {
      engine.rotation = 0;
    }

    if (input.boost) {
      engine.throttle = 1;
    } else if (input.thrust) {
      engine.throttle = 1;
    } else if (input.invThrust) {
      engine.throttle = -1;
    } else {
      engine.throttle = 0;
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
