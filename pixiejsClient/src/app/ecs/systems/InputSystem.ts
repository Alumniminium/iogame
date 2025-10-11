import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { Box2DBodyComponent } from "../components/Box2DBodyComponent";
import { NetworkComponent } from "../components/NetworkComponent";
import { EngineComponent } from "../components/EngineComponent";
import { ShieldComponent } from "../components/ShieldComponent";
import type { InputManager } from "../../managers/InputManager";

/**
 * Current input state from keyboard and mouse
 */
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

/**
 * Processes player input and applies it to locally-controlled entities.
 * Reads from InputManager and updates engine and shield components.
 */
export class InputSystem extends System {
  readonly componentTypes = [Box2DBodyComponent, NetworkComponent];

  private inputManager: InputManager;
  private localEntityId: string | null = null;
  private paused: boolean = false;

  constructor(inputManager: InputManager) {
    super();
    this.inputManager = inputManager;
  }

  /**
   * Set which entity is controlled by local player input
   */
  setLocalEntity(entityId: string): void {
    this.localEntityId = entityId;
  }

  /**
   * Pause/unpause input processing
   */
  setPaused(paused: boolean): void {
    this.paused = paused;
  }

  protected updateEntity(entity: Entity, _deltaTime: number): void {
    if (this.paused) return;

    const network = entity.get(NetworkComponent)!;
    const physics = entity.get(Box2DBodyComponent);

    if (!network.isLocallyControlled || entity.id !== this.localEntityId) return;

    if (!physics) return;

    const input = this.inputManager.getInputState();

    this.applyInputToComponents(entity, input);
  }

  private applyInputToComponents(entity: Entity, input: InputState): void {
    if (entity.has(EngineComponent)) this.configureEngine(entity, input);

    if (entity.has(ShieldComponent)) this.configureShield(entity, input);
  }

  private configureEngine(entity: Entity, input: InputState): void {
    const engine = entity.get(EngineComponent)!;

    engine.rcs = input.rcs;

    if (input.boost) engine.throttle = 1;
    else if (input.thrust) engine.throttle = 1;
    else if (input.invThrust) engine.throttle = -1;
    else engine.throttle = 0;
  }

  private configureShield(entity: Entity, input: InputState): void {
    const shield = entity.get(ShieldComponent);
    if (!shield) return;

    const powerOn = input.shield;
    if (shield.powerOn !== powerOn) {
      shield.powerOn = powerOn;
    }
  }
}
