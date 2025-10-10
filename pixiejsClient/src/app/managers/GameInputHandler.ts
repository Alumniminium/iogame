import { InputManager } from "./InputManager";
import { KeybindManager } from "./KeybindManager";
import { NetworkManager } from "../network/NetworkManager";
import { ComponentStatePacket } from "../network/packets/ComponentStatePacket";
import type { CameraState } from "../ecs/Camera";

/**
 * Handles game input processing and UI toggle key states.
 * Manages keyboard state tracking and input packet generation.
 */
export class GameInputHandler {
  private inputManager: InputManager;
  private networkManager: NetworkManager;
  private getLocalPlayerId: () => string | null;
  private getCamera: () => CameraState;

  // UI toggle key states
  private f10WasPressed = false;
  private f11WasPressed = false;
  private f12WasPressed = false;
  private bWasPressed = false;
  private enterWasPressed = false;

  // UI toggle callbacks
  private onToggleStatsPanel?: () => void;
  private onToggleInputDisplay?: () => void;
  private onTogglePlayerBars?: () => void;
  private onToggleBuildMode?: () => void;
  private onStartChatTyping?: () => void;
  private isChatTyping?: () => boolean;

  constructor(
    inputManager: InputManager,
    networkManager: NetworkManager,
    getLocalPlayerId: () => string | null,
    getCamera: () => CameraState,
  ) {
    this.inputManager = inputManager;
    this.networkManager = networkManager;
    this.getLocalPlayerId = getLocalPlayerId;
    this.getCamera = getCamera;
  }

  /**
   * Set callbacks for UI toggle actions.
   */
  setUIToggleCallbacks(callbacks: {
    onToggleStatsPanel?: () => void;
    onToggleInputDisplay?: () => void;
    onTogglePlayerBars?: () => void;
    onToggleBuildMode?: () => void;
    onStartChatTyping?: () => void;
    isChatTyping?: () => boolean;
  }): void {
    this.onToggleStatsPanel = callbacks.onToggleStatsPanel;
    this.onToggleInputDisplay = callbacks.onToggleInputDisplay;
    this.onTogglePlayerBars = callbacks.onTogglePlayerBars;
    this.onToggleBuildMode = callbacks.onToggleBuildMode;
    this.onStartChatTyping = callbacks.onStartChatTyping;
    this.isChatTyping = callbacks.isChatTyping;
  }

  /**
   * Handle UI toggle inputs (F10, F11, F12, B, Enter).
   * Should be called in fixed update loop.
   */
  handleUIToggleInputs(): void {
    if (!this.inputManager) return;

    const input = this.inputManager.getInputState();

    if (this.isChatTyping?.()) {
      return; // ChatBox handles its own input via direct keyboard events
    }

    if (input.keys.has("F11") && !this.f11WasPressed) {
      this.onToggleStatsPanel?.();
    }
    this.f11WasPressed = input.keys.has("F11");

    if (input.keys.has("F12") && !this.f12WasPressed) {
      this.onToggleInputDisplay?.();
    }
    this.f12WasPressed = input.keys.has("F12");

    if (input.keys.has("F10") && !this.f10WasPressed) {
      this.onTogglePlayerBars?.();
    }
    this.f10WasPressed = input.keys.has("F10");

    const keybindManager = KeybindManager.getInstance();
    const buildKeys = keybindManager.getKeysForAction("buildMode");
    const buildPressed = buildKeys.some((key) => input.keys.has(key));
    if (buildPressed && !this.bWasPressed) {
      this.onToggleBuildMode?.();
    }
    this.bWasPressed = buildPressed;

    if (input.keys.has("Enter") && !this.enterWasPressed) {
      this.onStartChatTyping?.();
    }
    this.enterWasPressed = input.keys.has("Enter");
  }

  /**
   * Send input state to server.
   * Should be called in fixed update loop.
   */
  sendInput(): void {
    if (!this.inputManager || !this.networkManager) return;

    const localPlayerId = this.getLocalPlayerId();
    if (!localPlayerId) return;

    const input = this.inputManager.getInputState();
    const cameraState = this.getCamera();
    const mouseWorld = this.inputManager.getMouseWorldPosition(
      cameraState as any,
    );

    // Convert input state to button flags
    let buttonStates = 0;
    if (input.thrust) buttonStates |= 1; // W
    if (input.invThrust) buttonStates |= 2; // S
    if (input.left) buttonStates |= 4; // A
    if (input.right) buttonStates |= 8; // D
    if (input.boost) buttonStates |= 16; // Shift
    if (input.rcs) buttonStates |= 32; // R
    if (input.fire) buttonStates |= 64; // Fire
    if (input.drop) buttonStates |= 128; // Q
    if (input.shield) buttonStates |= 256; // Space

    const packet = ComponentStatePacket.createInput(
      localPlayerId,
      buttonStates,
      mouseWorld.x,
      mouseWorld.y,
    );

    this.networkManager.send(packet);
  }

  /**
   * Reset key state tracking (e.g., when pausing).
   */
  reset(): void {
    this.f10WasPressed = false;
    this.f11WasPressed = false;
    this.f12WasPressed = false;
    this.bWasPressed = false;
    this.enterWasPressed = false;
  }
}
