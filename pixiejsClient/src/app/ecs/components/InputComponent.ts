import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import type { Vector2 } from "../core/types";

export enum PlayerInput {
  None = 0,
  Thrust = 1, // W - Forward thrust
  InvThrust = 2, // S - Reverse thrust
  Left = 4, // A - Turn left
  Right = 8, // D - Turn right
  Boost = 16, // Shift
  RCS = 32, // R - RCS toggle
  Fire = 64, // Left click - Fire weapons
  Drop = 128, // Q - Drop items
  Shield = 256, // Space - Shield activation
}

export interface InputConfig {
  movementAxis?: Vector2;
  mouseDir?: Vector2;
  buttonStates?: PlayerInput;
}

@component(ServerComponentType.Input)
export class InputComponent extends Component {
  // Match C# struct layout - changedTick inherited from Component
  @serverField(1, "vector2") mouseDir: Vector2;
  @serverField(2, "u16") buttonStates: PlayerInput;
  @serverField(3, "bool") didBoostLastFrame: boolean;

  // Client-side only field
  movementAxis: Vector2;

  constructor(entityId: string, config: InputConfig = {}) {
    super(entityId);

    this.movementAxis = config.movementAxis ? { ...config.movementAxis } : { x: 0, y: 0 };
    this.mouseDir = config.mouseDir ? { ...config.mouseDir } : { x: 0, y: 1 };
    this.buttonStates = config.buttonStates || PlayerInput.None;
    this.didBoostLastFrame = false;
  }

  hasInput(input: PlayerInput): boolean {
    return (this.buttonStates & input) !== 0;
  }

  setInput(input: PlayerInput, active: boolean): void {
    if (active) {
      this.buttonStates |= input;
    } else {
      this.buttonStates &= ~input;
    }
  }
}
