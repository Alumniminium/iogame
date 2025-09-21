import { Vector2 } from "../core/types";

export interface InputSnapshot {
  sequenceNumber: number;
  timestamp: number;
  position: Vector2;
  velocity: Vector2;
  rotation: number;
  inputState: {
    thrust: boolean;
    invThrust: boolean;
    left: boolean;
    right: boolean;
    boost: boolean;
    rcs: boolean;
    shield: boolean;
    mouseDirection: Vector2;
  };
}

export class InputBuffer {
  private buffer: InputSnapshot[] = [];
  private maxSize: number = 240; // 4 seconds at 60hz server rate (allows for higher client framerate)
  private currentSequence: number = 0;

  addInput(snapshot: InputSnapshot): void {
    this.buffer.push(snapshot);

    // Keep buffer within size limits
    if (this.buffer.length > this.maxSize) {
      this.buffer.shift();
    }
  }

  getInput(sequenceNumber: number): InputSnapshot | undefined {
    return this.buffer.find((input) => input.sequenceNumber === sequenceNumber);
  }

  getInputsAfter(sequenceNumber: number): InputSnapshot[] {
    return this.buffer.filter((input) => input.sequenceNumber > sequenceNumber);
  }

  getInputsInRange(
    startSequence: number,
    endSequence: number,
  ): InputSnapshot[] {
    return this.buffer.filter(
      (input) =>
        input.sequenceNumber >= startSequence &&
        input.sequenceNumber <= endSequence,
    );
  }

  getLatestInput(): InputSnapshot | undefined {
    return this.buffer[this.buffer.length - 1];
  }

  removeInputsBefore(sequenceNumber: number): void {
    this.buffer = this.buffer.filter(
      (input) => input.sequenceNumber >= sequenceNumber,
    );
  }

  generateSequenceNumber(): number {
    return ++this.currentSequence;
  }

  clear(): void {
    this.buffer = [];
  }

  size(): number {
    return this.buffer.length;
  }
}
