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
 *
 * Uses base System class since it has special filtering logic.
 */
export class InputSystem extends System {
  private inputManager: InputManager;
  private localEntityId: string | null = null;
  private paused: boolean = false;

  constructor(inputManager: InputManager) {
    super();
    this.inputManager = inputManager;
  }

  protected matchesFilter(entity: Entity): boolean {
    return entity.has(Box2DBodyComponent) && entity.has(NetworkComponent);
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

  beginUpdate(_deltaTime: number): void {
    if (this.paused) return;

    // Process only the local player entity
    for (const entity of this._entitiesList) {
      if (entity.id !== this.localEntityId) continue;

      const network = entity.get(NetworkComponent)!;
      if (!network.isLocallyControlled) continue;

      const input = this.inputManager.getInputState();
      this.applyInputToComponents(entity, input);
    }
  }

  private applyInputToComponents(entity: Entity, input: InputState): void {
    // Configure engine if present
    if (entity.has(EngineComponent)) {
      this.configureEngine(entity, input);
    }

    // Configure shield if present
    if (entity.has(ShieldComponent)) {
      this.configureShield(entity, input);
    }
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
