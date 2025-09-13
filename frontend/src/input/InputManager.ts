export interface InputState {
  keys: Set<string>;
  mouseX: number;
  mouseY: number;
  mouseButtons: number;
  moveX: number;
  moveY: number;
  // Server PlayerInput flags
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
  private canvas: HTMLCanvasElement;
  private enabled = true;

  // Toggle states - both default to ON
  private rcsToggled = true;
  private shieldToggled = true;
  private lastKeys = new Set<string>();
  
  constructor(canvas: HTMLCanvasElement) {
    this.canvas = canvas;
    this.setupEventListeners();
  }
  
  private setupEventListeners(): void {
    // Keyboard events
    window.addEventListener('keydown', this.handleKeyDown.bind(this));
    window.addEventListener('keyup', this.handleKeyUp.bind(this));
    
    // Mouse events
    this.canvas.addEventListener('mousemove', this.handleMouseMove.bind(this));
    this.canvas.addEventListener('mousedown', this.handleMouseDown.bind(this));
    this.canvas.addEventListener('mouseup', this.handleMouseUp.bind(this));
    this.canvas.addEventListener('contextmenu', (e) => e.preventDefault());
    
    // Touch events for mobile
    this.canvas.addEventListener('touchstart', this.handleTouchStart.bind(this));
    this.canvas.addEventListener('touchmove', this.handleTouchMove.bind(this));
    this.canvas.addEventListener('touchend', this.handleTouchEnd.bind(this));
    
    // Prevent scrolling with arrow keys and game keys
    window.addEventListener('keydown', (e) => {
      if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', ' ', 'KeyW', 'KeyA', 'KeyS', 'KeyD', 'KeyQ', 'KeyE', 'KeyR'].includes(e.code) || e.key === ' ') {
        e.preventDefault();
      }
    });
  }
  
  private handleKeyDown(e: KeyboardEvent): void {
    if (!this.enabled) return;

    // Ignore if typing in input field
    if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement) {
      return;
    }

    this.keys.add(e.code);

    // Debug logging for WASD keys
    if (['KeyW', 'KeyA', 'KeyS', 'KeyD'].includes(e.code)) {
      console.log(`⌨️ KEY DOWN: ${e.code}`);
    }
  }
  
  private handleKeyUp(e: KeyboardEvent): void {
    this.keys.delete(e.code);
  }
  
  private handleMouseMove(e: MouseEvent): void {
    if (!this.enabled) return;
    
    const rect = this.canvas.getBoundingClientRect();
    this.mouseX = e.clientX - rect.left;
    this.mouseY = e.clientY - rect.top;
  }
  
  private handleMouseDown(e: MouseEvent): void {
    if (!this.enabled) return;
    
    e.preventDefault();
    this.mouseButtons |= (1 << e.button);
  }
  
  private handleMouseUp(e: MouseEvent): void {
    this.mouseButtons &= ~(1 << e.button);
  }
  
  private handleTouchStart(e: TouchEvent): void {
    if (!this.enabled) return;
    
    e.preventDefault();
    if (e.touches.length > 0) {
      const touch = e.touches[0];
      const rect = this.canvas.getBoundingClientRect();
      this.mouseX = touch.clientX - rect.left;
      this.mouseY = touch.clientY - rect.top;
      this.mouseButtons = 1; // Simulate left click
    }
  }
  
  private handleTouchMove(e: TouchEvent): void {
    if (!this.enabled) return;
    
    e.preventDefault();
    if (e.touches.length > 0) {
      const touch = e.touches[0];
      const rect = this.canvas.getBoundingClientRect();
      this.mouseX = touch.clientX - rect.left;
      this.mouseY = touch.clientY - rect.top;
    }
  }
  
  private handleTouchEnd(e: TouchEvent): void {
    e.preventDefault();
    if (e.touches.length === 0) {
      this.mouseButtons = 0;
    }
  }
  
  getInputState(): InputState {
    // Handle RCS toggle
    if (this.keys.has('KeyR') && !this.lastKeys.has('KeyR')) {
      this.rcsToggled = !this.rcsToggled;
      console.log(`🔧 RCS ${this.rcsToggled ? 'ON' : 'OFF'}`);
    }

    // Handle Shield toggle
    if (this.keys.has('Space') && !this.lastKeys.has('Space')) {
      this.shieldToggled = !this.shieldToggled;
      console.log(`🛡️ Shield ${this.shieldToggled ? 'ON' : 'OFF'}`);
    }

    // Update last keys for next frame
    this.lastKeys = new Set(this.keys);

    // Calculate movement vector from keyboard input
    let moveX = 0;
    let moveY = 0;

    // WASD and Arrow keys for movement
    const thrust = this.keys.has('KeyW') || this.keys.has('ArrowUp');
    const invThrust = this.keys.has('KeyS') || this.keys.has('ArrowDown');
    const left = this.keys.has('KeyA') || this.keys.has('ArrowLeft');
    const right = this.keys.has('KeyD') || this.keys.has('ArrowRight');

    if (thrust) moveY -= 1;
    if (invThrust) moveY += 1;
    if (left) moveX -= 1;
    if (right) moveX += 1;

    // Normalize diagonal movement
    if (moveX !== 0 && moveY !== 0) {
      const magnitude = Math.sqrt(moveX * moveX + moveY * moveY);
      moveX /= magnitude;
      moveY /= magnitude;
    }

    // Additional keybinds
    const boost = this.keys.has('ShiftLeft') || this.keys.has('ShiftRight');
    const rcs = this.rcsToggled; // Use toggle state instead of key press
    const fire = this.isMouseButtonPressed(0); // Left click
    const drop = this.keys.has('KeyQ') || this.keys.has('KeyE');
    const shield = this.shieldToggled; // Use toggle state instead of key press

    return {
      keys: new Set(this.keys),
      mouseX: this.mouseX,
      mouseY: this.mouseY,
      mouseButtons: this.mouseButtons,
      moveX,
      moveY,
      // Server PlayerInput flags
      thrust,
      invThrust,
      left,
      right,
      boost,
      rcs,
      fire,
      drop,
      shield
    };
  }
  
  isKeyPressed(key: string): boolean {
    return this.keys.has(key);
  }
  
  isMouseButtonPressed(button: number): boolean {
    return (this.mouseButtons & (1 << button)) !== 0;
  }
  
  getMouseWorldPosition(camera: { x: number; y: number; zoom: number }): { x: number; y: number } {
    const centerX = this.canvas.width / 2;
    const centerY = this.canvas.height / 2;
    
    return {
      x: camera.x + (this.mouseX - centerX) / camera.zoom,
      y: camera.y + (this.mouseY - centerY) / camera.zoom
    };
  }
  
  setEnabled(enabled: boolean): void {
    this.enabled = enabled;
    if (!enabled) {
      this.keys.clear();
      this.mouseButtons = 0;
    }
  }
  
  clear(): void {
    this.keys.clear();
    this.mouseButtons = 0;
    this.lastKeys.clear();
    this.rcsToggled = true; // Reset to default ON
    this.shieldToggled = true; // Reset to default ON
  }

  getRcsToggleState(): boolean {
    return this.rcsToggled;
  }

  setRcsToggleState(state: boolean): void {
    this.rcsToggled = state;
  }

  getShieldToggleState(): boolean {
    return this.shieldToggled;
  }

  setShieldToggleState(state: boolean): void {
    this.shieldToggled = state;
  }
}