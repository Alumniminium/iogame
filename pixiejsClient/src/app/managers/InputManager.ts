import type { Camera } from "../ecs/systems/RenderSystem";
import { KeybindManager } from "./KeybindManager";

export interface InputState {
  keys: Set<string>;
  mouseX: number;
  mouseY: number;
  mouseButtons: number;
  moveX: number;
  moveY: number;
  thrust: boolean;
  invThrust: boolean;
  left: boolean;
  right: boolean;
  boost: boolean;
  rcs: boolean;
  fire: boolean;
  drop: boolean;
  shield: boolean;
}

export class InputManager {
  private keys = new Set<string>();
  private mouseX = 0;
  private mouseY = 0;
  private mouseButtons = 0;
  private enabled = true;
  private canvas: HTMLCanvasElement | null = null;

  private rcsToggled = true;
  private shieldToggled = true;
  private lastKeys = new Set<string>();

  private escapeCallbacks: (() => void)[] = [];
  private mapCallbacks: (() => void)[] = [];
  private wheelCallbacks: ((delta: number) => void)[] = [];

  private keybindManager = KeybindManager.getInstance();

  constructor() {}

  initialize(): void {
    this.canvas = document.querySelector("canvas") as HTMLCanvasElement;
    if (!this.canvas) {
      return;
    }

    this.setupEventListeners();
  }

  private setupEventListeners(): void {
    if (!this.canvas) return;

    window.addEventListener("keydown", this.handleKeyDown.bind(this));
    window.addEventListener("keyup", this.handleKeyUp.bind(this));

    this.canvas.addEventListener("mousemove", this.handleMouseMove.bind(this));
    this.canvas.addEventListener("mousedown", this.handleMouseDown.bind(this));
    this.canvas.addEventListener("mouseup", this.handleMouseUp.bind(this));
    this.canvas.addEventListener("contextmenu", (e) => e.preventDefault());
    this.canvas.addEventListener("wheel", this.handleWheel.bind(this), {
      passive: false,
    });

    this.canvas.addEventListener(
      "touchstart",
      this.handleTouchStart.bind(this),
      { passive: false },
    );
    this.canvas.addEventListener("touchmove", this.handleTouchMove.bind(this), {
      passive: false,
    });
    this.canvas.addEventListener("touchend", this.handleTouchEnd.bind(this));

    window.addEventListener("keydown", (e) => {
      if (
        [
          "ArrowUp",
          "ArrowDown",
          "ArrowLeft",
          "ArrowRight",
          " ",
          "KeyW",
          "KeyA",
          "KeyS",
          "KeyD",
          "KeyQ",
          "KeyE",
          "KeyR",
        ].includes(e.code) ||
        e.key === " "
      ) {
        e.preventDefault();
      }
    });
  }

  private handleKeyDown(e: KeyboardEvent): void {
    if (
      e.target instanceof HTMLInputElement ||
      e.target instanceof HTMLTextAreaElement
    ) {
      return;
    }

    // Always allow ESC key, even when input is disabled
    if (e.code === "Escape") {
      this.keys.add(e.code);
      return;
    }

    if (!this.enabled) return;

    this.keys.add(e.code);
  }

  private handleKeyUp(e: KeyboardEvent): void {
    // Always allow ESC key up, even when input is disabled
    if (e.code === "Escape") {
      this.keys.delete(e.code);
      return;
    }

    if (!this.enabled) {
      // Still remove keys when disabled to prevent stuck keys
      this.keys.delete(e.code);
      return;
    }

    this.keys.delete(e.code);
  }

  private handleMouseMove(e: MouseEvent): void {
    if (!this.enabled || !this.canvas) return;

    const rect = this.canvas.getBoundingClientRect();
    this.mouseX = e.clientX - rect.left;
    this.mouseY = e.clientY - rect.top;
  }

  private handleMouseDown(e: MouseEvent): void {
    if (!this.enabled) return;

    e.preventDefault();
    this.mouseButtons |= 1 << e.button;
  }

  private handleMouseUp(e: MouseEvent): void {
    this.mouseButtons &= ~(1 << e.button);
  }

  private handleTouchStart(e: TouchEvent): void {
    if (!this.enabled || !this.canvas) return;

    e.preventDefault();
    if (e.touches.length > 0) {
      const rect = this.canvas.getBoundingClientRect();
      const touch = e.touches[0];
      this.mouseX = touch.clientX - rect.left;
      this.mouseY = touch.clientY - rect.top;
      this.mouseButtons |= 1; // Treat as left mouse button
    }
  }

  private handleTouchMove(e: TouchEvent): void {
    if (!this.enabled || !this.canvas) return;

    e.preventDefault();
    if (e.touches.length > 0) {
      const rect = this.canvas.getBoundingClientRect();
      const touch = e.touches[0];
      this.mouseX = touch.clientX - rect.left;
      this.mouseY = touch.clientY - rect.top;
    }
  }

  private handleTouchEnd(e: TouchEvent): void {
    e.preventDefault();
    this.mouseButtons = 0;
  }

  private handleWheel(e: WheelEvent): void {
    e.preventDefault();
    this.wheelCallbacks.forEach((callback) => callback(e.deltaY));
  }

  getInputState(): InputState {
    const keybinds = this.keybindManager.getKeybinds();

    // Check for RCS toggle
    const rcsPressed = keybinds.rcs.some((key) => this.keys.has(key));
    const rcsWasPressed = keybinds.rcs.some((key) => this.lastKeys.has(key));
    if (rcsPressed && !rcsWasPressed) this.rcsToggled = !this.rcsToggled;

    // Check for shield toggle
    const shieldPressed = keybinds.shield.some((key) => this.keys.has(key));
    const shieldWasPressed = keybinds.shield.some((key) =>
      this.lastKeys.has(key),
    );
    if (shieldPressed && !shieldWasPressed)
      this.shieldToggled = !this.shieldToggled;

    // Handle ESC key press
    if (this.keys.has("Escape") && !this.lastKeys.has("Escape"))
      this.escapeCallbacks.forEach((callback) => callback());

    // Handle map key press
    const mapPressed = keybinds.map.some((key) => this.keys.has(key));
    const mapWasPressed = keybinds.map.some((key) => this.lastKeys.has(key));
    if (mapPressed && !mapWasPressed)
      this.mapCallbacks.forEach((callback) => callback());

    const centerX = this.canvas ? this.canvas.width / 2 : 400;
    const centerY = this.canvas ? this.canvas.height / 2 : 300;
    const moveX = (this.mouseX - centerX) / centerX; // Normalized [-1, 1]
    const moveY = (this.mouseY - centerY) / centerY; // Normalized [-1, 1]

    const inputState: InputState = {
      keys: new Set(this.keys),
      mouseX: this.mouseX,
      mouseY: this.mouseY,
      mouseButtons: this.mouseButtons,
      moveX,
      moveY,

      thrust: keybinds.thrust.some((key) => this.keys.has(key)),
      invThrust: keybinds.invThrust.some((key) => this.keys.has(key)),
      left: keybinds.left.some((key) => this.keys.has(key)),
      right: keybinds.right.some((key) => this.keys.has(key)),
      boost: keybinds.boost.some((key) => this.keys.has(key)),

      fire: (this.mouseButtons & 1) !== 0, // Left mouse button
      drop: keybinds.drop.some((key) => this.keys.has(key)),

      rcs: this.rcsToggled,
      shield: this.shieldToggled,
    };

    this.lastKeys = new Set(this.keys);

    return inputState;
  }

  getMouseWorldPosition(camera: Camera): { x: number; y: number } {
    if (!this.canvas) {
      return { x: 0, y: 0 };
    }

    const centerX = this.canvas.width / 2;
    const centerY = this.canvas.height / 2;

    const worldX = camera.x + (this.mouseX - centerX) / camera.zoom;
    const worldY = camera.y + (this.mouseY - centerY) / camera.zoom;

    return { x: worldX, y: worldY };
  }

  isKeyPressed(keyCode: string): boolean {
    return this.keys.has(keyCode);
  }

  setEnabled(enabled: boolean): void {
    this.enabled = enabled;
    if (!enabled) {
      this.keys.clear();
      this.mouseButtons = 0;
    }
  }

  onEscapePressed(callback: () => void): void {
    this.escapeCallbacks.push(callback);
  }

  removeEscapeCallback(callback: () => void): void {
    const index = this.escapeCallbacks.indexOf(callback);
    if (index > -1) {
      this.escapeCallbacks.splice(index, 1);
    }
  }

  onMapKeyPressed(callback: () => void): void {
    this.mapCallbacks.push(callback);
  }

  removeMapCallback(callback: () => void): void {
    const index = this.mapCallbacks.indexOf(callback);
    if (index > -1) {
      this.mapCallbacks.splice(index, 1);
    }
  }

  onWheel(callback: (delta: number) => void): void {
    this.wheelCallbacks.push(callback);
  }

  removeWheelCallback(callback: (delta: number) => void): void {
    const index = this.wheelCallbacks.indexOf(callback);
    if (index > -1) {
      this.wheelCallbacks.splice(index, 1);
    }
  }

  destroy(): void {
    if (!this.canvas) return;

    window.removeEventListener("keydown", this.handleKeyDown.bind(this));
    window.removeEventListener("keyup", this.handleKeyUp.bind(this));

    this.canvas.removeEventListener(
      "mousemove",
      this.handleMouseMove.bind(this),
    );
    this.canvas.removeEventListener(
      "mousedown",
      this.handleMouseDown.bind(this),
    );
    this.canvas.removeEventListener("mouseup", this.handleMouseUp.bind(this));
    this.canvas.removeEventListener("contextmenu", (e) => e.preventDefault());

    this.canvas.removeEventListener(
      "touchstart",
      this.handleTouchStart.bind(this),
      { passive: false } as any,
    );
    this.canvas.removeEventListener(
      "touchmove",
      this.handleTouchMove.bind(this),
      { passive: false } as any,
    );
    this.canvas.removeEventListener("touchend", this.handleTouchEnd.bind(this));
  }
}
