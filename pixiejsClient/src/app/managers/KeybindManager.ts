export type KeybindAction = "thrust" | "invThrust" | "left" | "right" | "boost" | "rcs" | "drop" | "shield" | "map" | "buildMode";

export interface Keybinds {
  thrust: string[];
  invThrust: string[];
  left: string[];
  right: string[];
  boost: string[];
  rcs: string[];
  drop: string[];
  shield: string[];
  map: string[];
  buildMode: string[];
}

const DEFAULT_KEYBINDS: Keybinds = {
  thrust: ["KeyW", "ArrowUp"],
  invThrust: ["KeyS", "ArrowDown"],
  left: ["KeyA", "ArrowLeft"],
  right: ["KeyD", "ArrowRight"],
  boost: ["ShiftLeft", "ShiftRight"],
  rcs: ["KeyR"],
  drop: ["KeyQ", "KeyE"],
  shield: ["Space"],
  map: ["KeyM"],
  buildMode: ["KeyB"],
};

export class KeybindManager {
  private static instance: KeybindManager;
  private keybinds: Keybinds;
  private changeCallbacks: (() => void)[] = [];

  private constructor() {
    this.keybinds = this.loadKeybinds();
  }

  static getInstance(): KeybindManager {
    if (!KeybindManager.instance) KeybindManager.instance = new KeybindManager();
    return KeybindManager.instance;
  }

  private loadKeybinds(): Keybinds {
    const stored = localStorage.getItem("keybinds");
    if (stored) {
      try {
        return JSON.parse(stored) as Keybinds;
      } catch {
        return { ...DEFAULT_KEYBINDS };
      }
    }
    return { ...DEFAULT_KEYBINDS };
  }

  private saveKeybinds(): void {
    localStorage.setItem("keybinds", JSON.stringify(this.keybinds));
    this.changeCallbacks.forEach((callback) => callback());
  }

  getKeybinds(): Keybinds {
    return { ...this.keybinds };
  }

  getKeysForAction(action: KeybindAction): string[] {
    return [...this.keybinds[action]];
  }

  setKeybind(action: KeybindAction, keyIndex: number, keyCode: string): void {
    if (keyIndex >= this.keybinds[action].length) this.keybinds[action].push(keyCode);
    else this.keybinds[action][keyIndex] = keyCode;

    this.saveKeybinds();
  }

  resetToDefaults(): void {
    this.keybinds = { ...DEFAULT_KEYBINDS };
    this.saveKeybinds();
  }

  isKeyBoundToAction(action: KeybindAction, keyCode: string): boolean {
    return this.keybinds[action].includes(keyCode);
  }

  onChange(callback: () => void): void {
    this.changeCallbacks.push(callback);
  }

  removeChangeCallback(callback: () => void): void {
    const index = this.changeCallbacks.indexOf(callback);
    if (index > -1) this.changeCallbacks.splice(index, 1);
  }

  getActionDisplayName(action: KeybindAction): string {
    const names: Record<KeybindAction, string> = {
      thrust: "Forward",
      invThrust: "Backward",
      left: "Turn Left",
      right: "Turn Right",
      boost: "Boost",
      rcs: "Toggle RCS",
      drop: "Drop Item",
      shield: "Toggle Shield",
      map: "Toggle Map",
      buildMode: "Build Mode",
    };
    return names[action];
  }

  static formatKeyCode(keyCode: string): string {
    const keyMap: Record<string, string> = {
      KeyW: "W",
      KeyA: "A",
      KeyS: "S",
      KeyD: "D",
      KeyQ: "Q",
      KeyE: "E",
      KeyR: "R",
      KeyM: "M",
      KeyB: "B",
      Space: "Space",
      ArrowUp: "↑",
      ArrowDown: "↓",
      ArrowLeft: "←",
      ArrowRight: "→",
      ShiftLeft: "Left Shift",
      ShiftRight: "Right Shift",
    };
    return keyMap[keyCode] || keyCode;
  }
}
