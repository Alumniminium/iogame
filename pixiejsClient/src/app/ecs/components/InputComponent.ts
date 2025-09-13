import { Component } from "../core/Component";
import { Vector2 } from "../core/types";

export enum PlayerInput {
  None = 0,
  W = 1 << 0, // Forward thrust
  A = 1 << 1, // Turn left
  S = 1 << 2, // Reverse thrust
  D = 1 << 3, // Turn right
  Shift = 1 << 4, // Boost
  R = 1 << 5, // RCS toggle
  Fire = 1 << 6, // Fire weapons (Left click)
  Q = 1 << 7, // Drop triangles
  E = 1 << 8, // Drop squares
  Space = 1 << 9, // Shield activation
}

export interface InputConfig {
  movementAxis?: Vector2;
  mouseDir?: Vector2;
  buttonStates?: PlayerInput;
}

export class InputComponent extends Component {
  movementAxis: Vector2;
  mouseDir: Vector2;
  buttonStates: PlayerInput;
  didBoostLastFrame: boolean;

  constructor(entityId: number, config: InputConfig = {}) {
    super(entityId);

    this.movementAxis = config.movementAxis
      ? { ...config.movementAxis }
      : { x: 0, y: 0 };
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
    this.markChanged();
  }
}
