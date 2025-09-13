import type { Ticker } from "pixi.js";
import { Container } from "pixi.js";
import { World } from "../../ecs/core/World";
import { NetworkSystem } from "../../ecs/systems/NetworkSystem";
import { PhysicsSystem } from "../../ecs/systems/PhysicsSystem";
import { InputSystem } from "../../ecs/systems/InputSystem";
import { EngineSystem } from "../../ecs/systems/EngineSystem";
import { RenderSystem } from "../../ecs/systems/RenderSystem";
import { PredictionSystem } from "../../ecs/systems/PredictionSystem";
import { NetworkManager } from "../../network/NetworkManager";
import { InputManager } from "../../input/InputManager";
import { StatsPanel } from "../../ui/game/StatsPanel";
import { PlayerBars } from "../../ui/game/PlayerBars";
import { TargetBars } from "../../ui/game/TargetBars";
import { InputDisplay } from "../../ui/game/InputDisplay";
import { PerformanceDisplay } from "../../ui/game/PerformanceDisplay";
import { EntityType } from "../../ecs/core/types";
import { PhysicsComponent } from "../../ecs/components/PhysicsComponent";
import { RenderComponent } from "../../ecs/components/RenderComponent";
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
  private engineSystem!: EngineSystem;
  private networkSystem!: NetworkSystem;
  private physicsSystem!: PhysicsSystem;
  private predictionSystem!: PredictionSystem;

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
  private localPlayerId: number | null = null;

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
      predictionEnabled: true,
    });

    // Initialize ECS systems
    this.physicsSystem = new PhysicsSystem();
    this.engineSystem = new EngineSystem();
    this.networkSystem = new NetworkSystem();
    this.predictionSystem = new PredictionSystem();
    this.inputSystem = new InputSystem(this.inputManager, this.networkManager);
    this.renderSystem = new RenderSystem(this.gameWorldContainer);

    // Connect systems together
    this.networkSystem.setPredictionSystem(this.predictionSystem);
    this.inputSystem.setPredictionSystem(this.predictionSystem);

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
    World.addSystem("engine", this.engineSystem, ["input"], 98);
    World.addSystem("physics", this.physicsSystem, ["engine"], 95);
    World.addSystem("prediction", this.predictionSystem, ["physics"], 90); // FIXED: Run prediction AFTER physics
    World.addSystem("network", this.networkSystem, ["prediction"], 80);
    World.addSystem("render", this.renderSystem, ["physics", "prediction"], 70);

    // Register callback for when local player ID is set
    this.networkManager.setLocalPlayerCallback((playerId) => {
      this.setLocalPlayer(playerId);
    });

    // Register callback for when view distance is received
    this.networkManager.setViewDistanceCallback((viewDistance) => {
      this.renderSystem.setViewDistance(viewDistance);
    });

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
        const connected = await this.networkManager.connect("Player1");

        if (connected) {
          console.log("Connected successfully!");
          this.start();
        } else {
          console.error("Failed to connect to server");
        }
      } catch (error) {
        console.error("Connection error:", error);
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
    this.lastDeltaMs = (currentTime - this.lastTime);
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
    if (!this.inputManager || !this.networkManager) return;

    const input = this.inputManager.getInputState();
    const camera = this.renderSystem.getCamera();
    const mouseWorld = this.inputManager.getMouseWorldPosition(camera);

    this.networkManager.sendInput(input, mouseWorld.x, mouseWorld.y);
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

    // Get performance metrics
    const currentTick = this.networkManager.getTickSynchronizer().getCurrentServerTick();
    const networkComponent = entity.get(NetworkComponent);
    const lastServerTick = networkComponent?.lastServerTick;

    // Update UI components with ECS data
    this.statsPanel.updateFromEntity(entity, inputState, this.fps, currentTick, lastServerTick);
    this.inputDisplay.updateFromInput(inputState);
    this.playerBars.updateFromEntity(entity);
    this.targetBars.updateFromWorld();

    // Update performance display (always visible)
    this.performanceDisplay.updatePerformance(this.fps, currentTick, lastServerTick, this.lastDeltaMs);
  }

  private updateFPS(currentTime: number): void {
    this.frameCount++;

    if (currentTime - this.lastFpsUpdate >= 1000) {
      this.fps = this.frameCount;
      this.frameCount = 0;
      this.lastFpsUpdate = currentTime;
    }
  }

  public setLocalPlayer(playerId: number): void {
    console.log(`🎮 Setting local player ID: ${playerId}`);
    this.localPlayerId = playerId;

    if (this.inputSystem) {
      this.inputSystem.setLocalEntity(playerId);
    }

    if (this.predictionSystem) {
      this.predictionSystem.setLocalEntity(playerId);
    }

    if (this.networkSystem) {
      this.networkSystem.setLocalEntity(playerId);
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
    this.networkSystem?.disconnect();
    World.destroy();
    super.destroy();
  }

  /** Auto pause when window loses focus */
  public blur(): void {
    console.log("GameScreen blur() called");
    this.stop();
  }

  /** Resume when window gains focus */
  public focus(): void {
    console.log("GameScreen focus() called");
    if (this.networkManager && this.networkManager.isConnected()) {
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
  public getLocalPlayerId(): number | null {
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
