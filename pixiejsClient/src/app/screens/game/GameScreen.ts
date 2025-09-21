import type { Ticker } from "pixi.js";
import { Container } from "pixi.js";
import { World } from "../../ecs/core/World";
import { InputSystem } from "../../ecs/systems/InputSystem";
import { RenderSystem } from "../../ecs/systems/RenderSystem";
import { NetworkSystem } from "../../ecs/systems/NetworkSystem";
import { NetworkManager } from "../../network/NetworkManager";
import { PlayerMovementPacket } from "../../network/packets/PlayerMovementPacket";
import { InputManager } from "../../input/InputManager";
import { StatsPanel } from "../../ui/game/StatsPanel";
import { PlayerBars } from "../../ui/game/PlayerBars";
import { TargetBars } from "../../ui/game/TargetBars";
import { InputDisplay } from "../../ui/game/InputDisplay";
import { PerformanceDisplay } from "../../ui/game/PerformanceDisplay";
import { NetworkComponent } from "../../ecs/components/NetworkComponent";

export interface GameConfig {
  playerName: string;
  serverUrl?: string;
}

/** The main game screen for the IO game */
export class GameScreen extends Container {
  /** Assets bundles required by this screen */
  public static assetBundles = ["main"];

  private networkManager!: NetworkManager;
  private inputManager!: InputManager;
  private renderSystem!: RenderSystem;
  private inputSystem!: InputSystem;
  private networkSystem!: NetworkSystem;

  // Containers
  private gameWorldContainer!: Container;

  // UI Components
  private statsPanel!: StatsPanel;
  private playerBars!: PlayerBars;
  private targetBars!: TargetBars;
  private inputDisplay!: InputDisplay;
  private performanceDisplay!: PerformanceDisplay;

  private lastTime = 0;
  private accumulator = 0;
  private fixedTimeStep = 1 / 30; // 30 Hz fixed update (matching server TPS)
  private maxAccumulator = 0.2; // Max 200ms worth of updates

  private running = false;
  private localPlayerId: string | null = null;

  // Performance metrics
  private fps = 0;
  private frameCount = 0;
  private lastFpsUpdate = 0;
  private lastDeltaMs = 0;

  // UI toggle states
  private f10WasPressed = false;
  private f11WasPressed = false;
  private f12WasPressed = false;

  constructor() {
    super();
    console.log("GameScreen constructor called");
  }

  /** Prepare the screen just before showing */
  public prepare() {
    console.log("GameScreen prepare() called");
    this.initializeGame();
  }

  private initializeGame(): void {
    console.log("Initializing IO Game...");

    // Initialize static World
    World.initialize();

    // Create separate container for game world (will be transformed by camera)
    this.gameWorldContainer = new Container();
    this.addChild(this.gameWorldContainer);

    // Initialize managers
    this.inputManager = new InputManager();
    this.networkManager = new NetworkManager({
      serverUrl: "ws://localhost:5000/ws",
      interpolationDelay: 100,
      predictionEnabled: false,
    });

    // Initialize ECS systems
    this.inputSystem = new InputSystem(this.inputManager);
    this.renderSystem = new RenderSystem(this.gameWorldContainer);
    this.networkSystem = new NetworkSystem();

    // Initialize UI components
    this.statsPanel = new StatsPanel({
      position: "bottom-left",
      visible: false,
    });
    this.addChild(this.statsPanel);

    this.playerBars = new PlayerBars({
      position: "top-left",
      visible: true,
    });
    this.addChild(this.playerBars);

    this.targetBars = new TargetBars({
      visible: true,
    });
    this.addChild(this.targetBars);

    this.inputDisplay = new InputDisplay({
      position: "top-right",
      visible: true,
    });
    this.addChild(this.inputDisplay);

    this.performanceDisplay = new PerformanceDisplay({
      position: "top-left",
      visible: true,
    });
    this.addChild(this.performanceDisplay);

    // Add systems to world with proper dependencies and priorities (matching server game loop)
    World.addSystem("input", this.inputSystem, [], 100);
    World.addSystem("network", this.networkSystem, [], 90);
    World.addSystem("render", this.renderSystem, ["physics"], 70);

    // Monitor connection state and update physics system
    this.monitorConnectionState();

    // Listen for login response events
    this.setupEventListeners();

    // Setup input handling
    this.inputManager.initialize();

    // Start connection process
    this.startConnection();

    console.log("Game initialized successfully!");
  }

  private startConnection(): void {
    // Use setTimeout to make the connection async without blocking
    setTimeout(async () => {
      try {
        console.log("Connecting to server...");
        const connected = await this.networkManager.connect("Player");

        if (connected) {
          console.log("Connected successfully!");

          this.start();
        }
      } catch (error: any) {
        console.error("Connection error:", error.message);
        console.error(error.stack);
      }
    }, 100);
  }

  /** Setup event listeners for packet handling */
  private setupEventListeners(): void {
    // Listen for login response
    window.addEventListener('login-response', (event: any) => {
      const { playerId, mapSize, viewDistance } = event.detail;
      console.log(`🎮 Received login response for player: ${playerId}`);

      // Set local player
      this.setLocalPlayer(playerId);

      console.log(`Map size: ${mapSize.width}x${mapSize.height}, View distance: ${viewDistance}`);
    });
  }

  /** Monitor connection state and update physics system accordingly */
  private monitorConnectionState(): void {
    let wasConnected = this.networkManager.isConnected();

    // Check connection state every 100ms
    setInterval(() => {
      const isConnected = this.networkManager.isConnected();
      if (isConnected !== wasConnected) {
        console.log(`🔌 Connection state changed: ${wasConnected} -> ${isConnected}`);

        wasConnected = isConnected;

        // When disconnected, ensure simulation continues running
        if (!isConnected && !this.running) {
          console.log("Connection lost - continuing physics simulation offline");
          this.start();
        }
      }
    }, 100);
  }

  private start(): void {
    if (this.running) return;
    console.log("Starting game loop...");

    this.running = true;
    this.lastTime = performance.now();
  }

  private stop(): void {
    this.running = false;
    console.log("Game loop stopped");
  }

  /** Update the screen */
  public update(_ticker: Ticker) {
    if (!this.running) return;

    const currentTime = performance.now();
    const deltaTime = Math.min(
      (currentTime - this.lastTime) / 1000,
      this.maxAccumulator,
    );
    this.lastDeltaMs = currentTime - this.lastTime;
    this.lastTime = currentTime;

    // Update FPS counter
    this.updateFPS(currentTime);

    // Accumulate time for fixed timestep
    this.accumulator += deltaTime;

    // Fixed timestep updates
    while (this.accumulator >= this.fixedTimeStep) {
      this.fixedUpdate(this.fixedTimeStep);
      this.accumulator -= this.fixedTimeStep;
    }

    // Variable timestep update (for smooth rendering)
    this.variableUpdate(deltaTime);

    // Render
    this.render();
  }

  private fixedUpdate(deltaTime: number): void {
    // Handle UI toggle inputs
    this.handleUIToggleInputs();

    // Send input to server
    this.sendInput();

    // Update world at fixed timestep
    World.update(deltaTime);

    // Update network manager
    this.networkManager.update(deltaTime);
  }

  private handleUIToggleInputs(): void {
    if (!this.inputManager) return;

    const input = this.inputManager.getInputState();

    // Toggle stats panel with F11
    if (input.keys.has("F11") && !this.f11WasPressed) {
      this.statsPanel.toggle();
    }
    this.f11WasPressed = input.keys.has("F11");

    // Toggle input display with F12
    if (input.keys.has("F12") && !this.f12WasPressed) {
      this.inputDisplay.toggle();
    }
    this.f12WasPressed = input.keys.has("F12");

    // Toggle player bars with F10
    if (input.keys.has("F10") && !this.f10WasPressed) {
      this.playerBars.toggle();
    }
    this.f10WasPressed = input.keys.has("F10");
  }

  private sendInput(): void {
    if (!this.inputManager || !this.networkManager || !this.localPlayerId) return;

    const input = this.inputManager.getInputState();
    const camera = this.renderSystem.getCamera();
    const mouseWorld = this.inputManager.getMouseWorldPosition(camera);

    const packet = PlayerMovementPacket.create(
      this.localPlayerId,
      0,
      input,
      mouseWorld.x,
      mouseWorld.y,
    );

    this.networkManager.send(packet);
  }

  private variableUpdate(deltaTime: number): void {
    // Update render system interpolation
    this.renderSystem.update(deltaTime);

    // Follow local player with camera
    if (this.localPlayerId) {
      const entity = World.getEntity(this.localPlayerId);
      if (entity) {
        this.renderSystem.followEntity(entity);
      }
    }
  }

  private render(): void {
    // Render the scene using the world
    this.renderSystem.render();

    // Update UI components
    this.updateUI();
  }

  private updateUI(): void {
    if (!this.localPlayerId) return;

    const entity = World.getEntity(this.localPlayerId);
    if (!entity) return;

    // Get current input state for dynamic updates
    const inputState = this.inputManager.getInputState();

    const networkComponent = entity.get(NetworkComponent);
    const lastServerTick = networkComponent?.lastServerTick;

    // Update UI components with ECS data
    this.statsPanel.updateFromEntity(
      entity,
      inputState,
      this.fps,
      0,
      lastServerTick,
    );
    this.inputDisplay.updateFromInput(inputState);
    this.playerBars.updateFromEntity(entity);
    // Get camera from render system and pass to target bars
    const camera = this.renderSystem.getCamera();
    this.targetBars.updateFromWorld(camera, this.localPlayerId, entity ? 300 : undefined);

    // Update performance display (always visible)
    this.performanceDisplay.updatePerformance(
      this.fps,
      0,
      lastServerTick,
      this.lastDeltaMs,
    );
  }

  private updateFPS(currentTime: number): void {
    this.frameCount++;

    if (currentTime - this.lastFpsUpdate >= 1000) {
      this.fps = this.frameCount;
      this.frameCount = 0;
      this.lastFpsUpdate = currentTime;
    }
  }

  public setLocalPlayer(playerId: string): void {
    console.log(`🎮 Setting local player ID: ${playerId}`);
    this.localPlayerId = playerId;

    if (this.inputSystem) {
      this.inputSystem.setLocalEntity(playerId);
    }

    // Follow the local player with the camera
    const entity = World.getEntity(playerId);
    if (entity && this.renderSystem) {
      this.renderSystem.followEntity(entity);
    }
  }

  /** Resize the screen, fired whenever window size changes */
  public resize(width: number, height: number) {
    console.log(`GameScreen resize: ${width}x${height}`);

    // Update render system with new dimensions
    this.renderSystem?.resize(width, height);

    // Update UI positions
    this.statsPanel?.resize(width, height);
    this.playerBars?.resize(width, height);
    this.inputDisplay?.resize(width, height);
    this.targetBars?.resize(width, height);
    this.performanceDisplay?.resize(width, height);
  }

  /** Show screen with animations */
  public async show(): Promise<void> {
    console.log("GameScreen show() called");
    // Could add intro animations here
  }

  /** Hide screen with animations */
  public async hide(): Promise<void> {
    console.log("GameScreen hide() called");
    this.stop();
  }

  /** Cleanup when screen is destroyed */
  public destroy(): void {
    console.log("GameScreen destroy() called");
    this.stop();
    this.networkManager?.disconnect();
    World.destroy();
    super.destroy();
  }

  /** Auto pause when window loses focus - keep physics running for client prediction */
  public blur(): void {
    console.log("GameScreen blur() called - keeping physics simulation running");
    // Don't stop the game loop on blur - client prediction should continue
    // Only pause rendering/audio, keep physics simulation running
  }

  /** Resume when window gains focus */
  public focus(): void {
    console.log("GameScreen focus() called");
    // Always start the simulation, regardless of network connection
    // Client prediction should run even when disconnected
    if (!this.running) {
      this.start();
    }
  }

  /** Pause gameplay */
  public async pause(): Promise<void> {
    console.log("GameScreen pause() called");
    this.stop();
  }

  /** Resume gameplay */
  public async resume(): Promise<void> {
    console.log("GameScreen resume() called");
    if (this.networkManager && this.networkManager.isConnected()) {
      this.start();
    }
  }

  /** Reset the game state */
  public reset(): void {
    console.log("GameScreen reset() called");
    this.stop();
    World.destroy();
    World.initialize();
  }

  // Getters for external access
  public getLocalPlayerId(): string | null {
    return this.localPlayerId;
  }

  public getWorld(): any {
    return World.getInstance();
  }

  public getNetworkManager(): NetworkManager {
    return this.networkManager;
  }

  public getFPS(): number {
    return this.fps;
  }
}
