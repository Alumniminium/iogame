import type { Ticker } from "pixi.js";
import { Container } from "pixi.js";
import { World } from "../../ecs/core/World";
import { InputSystem } from "../../ecs/systems/InputSystem";
import { RenderSystem } from "../../ecs/systems/RenderSystem";
import { NetworkSystem } from "../../ecs/systems/NetworkSystem";
import { DeathSystem } from "../../ecs/systems/DeathSystem";
import { NetworkManager } from "../../network/NetworkManager";
import { InputManager } from "../../managers/InputManager";
import { BuildModeSystem } from "../../ecs/systems/BuildModeSystem";
import { ParticleSystem } from "../../ecs/systems/ParticleSystem";
import { LifetimeSystem } from "../../ecs/systems/LifetimeSystem";
import { PlayerNameManager } from "../../managers/PlayerNameManager";
import { ShipPartManager } from "../../managers/ShipPartManager";
import { ChatPacket } from "../../network/packets/ChatPacket";
import { engine } from "../../getEngine";
import { PerformanceMonitor } from "../../managers/PerformanceMonitor";
import { GameConnectionManager } from "../../managers/GameConnectionManager";
import { GameInputHandler } from "../../managers/GameInputHandler";
import { GameUIManager } from "../../managers/GameUIManager";
import { BuildModeController } from "../../managers/BuildModeController";

/** The main game screen for the IO game */
export class GameScreen extends Container {
  /** Assets bundles required by this screen */
  public static assetBundles = ["main"];

  private networkManager!: NetworkManager;
  private inputManager!: InputManager;
  private renderSystem!: RenderSystem;
  private inputSystem!: InputSystem;
  private networkSystem!: NetworkSystem;
  private deathSystem!: DeathSystem;
  private buildModeSystem!: BuildModeSystem;
  private particleSystem!: ParticleSystem;
  private lifetimeSystem!: LifetimeSystem;
  private shipPartManager!: ShipPartManager;

  private gameWorldContainer!: Container;

  private performanceMonitor!: PerformanceMonitor;
  private connectionManager!: GameConnectionManager;
  private inputHandler!: GameInputHandler;
  private uiManager!: GameUIManager;
  private buildModeController!: BuildModeController;

  private running = false;
  private localPlayerId: string | null = null;
  private isPaused = false;
  private viewDistance = 300;

  constructor() {
    super();
  }

  /** Prepare the screen just before showing */
  public prepare() {
    this.initializeGame();
  }

  private initializeGame(): void {
    World.initialize();

    this.performanceMonitor = new PerformanceMonitor();

    this.gameWorldContainer = new Container();
    this.addChild(this.gameWorldContainer);

    this.inputManager = new InputManager();
    this.networkManager = NetworkManager.getInstance({
      serverUrl: "ws://localhost:5000/ws",
      interpolationDelay: 100,
      predictionEnabled: false,
    });

    this.connectionManager = new GameConnectionManager(
      this.networkManager,
      () => this.start(),
      (connected) => {
        if (!connected && !this.running) {
          this.start();
        }
      },
    );

    this.inputHandler = new GameInputHandler(
      this.inputManager,
      this.networkManager,
      () => this.localPlayerId,
      () => this.renderSystem.getCamera(),
    );

    this.inputSystem = new InputSystem(this.inputManager);
    this.renderSystem = new RenderSystem(this.gameWorldContainer, engine());
    this.networkSystem = new NetworkSystem();
    this.deathSystem = new DeathSystem();
    this.buildModeSystem = new BuildModeSystem();
    this.particleSystem = new ParticleSystem();
    this.lifetimeSystem = new LifetimeSystem();
    this.shipPartManager = new ShipPartManager(this.networkManager);

    this.uiManager = new GameUIManager(
      this,
      this.inputManager,
      this.performanceMonitor,
      () => this.localPlayerId,
      () => this.renderSystem.getCamera(),
      () => this.renderSystem.getHoveredEntityId(),
    );

    this.buildModeController = new BuildModeController(
      this.buildModeSystem,
      this.shipPartManager,
      this.gameWorldContainer,
      this,
      () => this.localPlayerId,
    );

    // Set up input handler callbacks
    this.inputHandler.setUIToggleCallbacks({
      onToggleStatsPanel: () => this.uiManager.getStatsPanel().toggle(),
      onToggleInputDisplay: () => this.uiManager.getInputDisplay().toggle(),
      onTogglePlayerBars: () => this.uiManager.getPlayerBars().toggle(),
      onToggleBuildMode: () => this.buildModeController.toggle(),
      onStartChatTyping: () => this.uiManager.getChatBox().startTyping(),
      isChatTyping: () => this.uiManager.getChatBox().isCurrentlyTyping(),
    });

    // Set up UI manager pause callbacks
    this.uiManager.setPauseCallbacks({
      onPause: () => this.pauseGame(),
      onResume: () => this.resumeGame(),
    });

    // Set up build mode controller callbacks
    this.buildModeController.setCallbacks({
      onEnter: () => {
        this.inputSystem.setPaused(true);
        this.renderSystem.setBuildModeActive(true);
      },
      onExit: () => {
        this.inputSystem.setPaused(false);
        this.renderSystem.setBuildModeActive(false);
      },
    });

    // Register systems in execution order (matches server pattern)
    // Order: Input → Network → Lifetime → Effects → Death (cleanup last, like server)
    // Note: RenderSystem runs separately in variableUpdate() at display refresh rate
    World.setSystems(
      this.inputSystem, // 1. Capture input, send to server
      this.networkSystem, // 2. Apply server state updates
      this.lifetimeSystem, // 3. Remove expired entities (before death cleanup)
      this.particleSystem, // 4. Update visual effects
      this.deathSystem, // 5. LAST - final cleanup (matches server line 54)
    );

    this.connectionManager.monitorConnectionState();

    this.setupEventListeners();

    this.inputManager.initialize();

    // Set up ESC key handler for pause menu
    this.inputManager.onEscapePressed(() => this.uiManager.togglePauseMenu(this.isPaused));

    // Set up M key handler for sector map
    this.inputManager.onMapKeyPressed(() => this.uiManager.toggleSectorMap());

    // Set up mousewheel handler for camera zoom
    this.inputManager.onWheel((delta) => this.handleZoom(delta));

    this.connectionManager.startConnection("Player");
  }

  /** Setup event listeners for packet handling */
  private setupEventListeners(): void {
    window.addEventListener("login-response", (event: any) => {
      const { playerId, mapSize, viewDistance } = event.detail;

      this.setLocalPlayer(playerId);
      this.renderSystem.setMapSize(mapSize.width, mapSize.height);
      this.viewDistance = viewDistance;
      this.renderSystem.setViewDistance(this.viewDistance);
    });

    window.addEventListener("chat-message", (event: any) => {
      const { playerId, message } = event.detail;
      const nameManager = PlayerNameManager.getInstance();
      const playerName = nameManager.getPlayerName(playerId);

      this.uiManager.getChatBox().addMessage(playerId, playerName, message);
    });

    this.uiManager.getChatBox().onSend((message: string) => {
      if (this.localPlayerId && this.networkManager) {
        const packet = ChatPacket.create(this.localPlayerId, message);
        this.networkManager.send(packet);
      }
    });
  }

  private start(): void {
    if (this.running) return;

    this.running = true;
  }

  private stop(): void {
    this.running = false;
  }

  /** Update the screen */
  public update(_ticker: Ticker) {
    if (!this.running) return;

    const timing = this.performanceMonitor.update();

    while (this.performanceMonitor.shouldRunFixedUpdate()) {
      this.fixedUpdate(timing.fixedTimeStep);
      this.performanceMonitor.consumeFixedTimestep();
    }

    this.variableUpdate(timing.deltaTime);

    this.render();
  }

  private fixedUpdate(deltaTime: number): void {
    this.inputHandler.handleUIToggleInputs();

    // Skip game simulation when paused or in build mode
    if (!this.isPaused && !this.buildModeController.isInBuildMode()) {
      this.inputHandler.sendInput();

      World.update(deltaTime);

      this.networkManager.update(deltaTime);
    }
  }

  private variableUpdate(deltaTime: number): void {
    this.renderSystem.beginUpdate(deltaTime);

    this.buildModeController.update();
  }

  private render(): void {
    // Rendering is handled by RenderSystem.update()
    this.uiManager.updateUI();
  }

  private handleZoom(delta: number): void {
    const zoomSpeed = 0.1;
    const minViewDistance = 50;
    const maxViewDistance = 1000;

    this.viewDistance += delta * zoomSpeed;
    this.viewDistance = Math.max(minViewDistance, Math.min(maxViewDistance, this.viewDistance));

    this.renderSystem.setViewDistance(this.viewDistance);
  }

  private pauseGame(): void {
    this.isPaused = true;
    // Pause input processing for ship movement
    this.inputSystem.setPaused(true);
    // Disable input capture except ESC (handled in InputManager)
    this.inputManager.setEnabled(false);
  }

  private resumeGame(): void {
    this.isPaused = false;
    // Resume input processing for ship movement
    this.inputSystem.setPaused(false);
    // Re-enable input capture
    this.inputManager.setEnabled(true);
  }

  public setLocalPlayer(playerId: string): void {
    this.localPlayerId = playerId;

    // Also set it globally for other systems to access
    (window as any).localPlayerId = playerId;

    if (this.inputSystem) {
      this.inputSystem.setLocalEntity(playerId);
    }

    if (this.renderSystem) {
      this.renderSystem.setLocalPlayerId(playerId);
    }

    if (this.particleSystem) {
      this.particleSystem.setInputManager(this.inputManager, playerId);
    }

    if (this.shipPartManager) {
      this.shipPartManager.setLocalPlayerId(playerId);
    }
  }

  /** Resize the screen, fired whenever window size changes */
  public resize(width: number, height: number) {
    this.renderSystem?.resize(width, height);

    this.uiManager?.resize(width, height);

    this.buildModeController?.resize(width, height);
  }

  /** Show screen with animations */
  public async show(): Promise<void> {}

  /** Hide screen with animations */
  public async hide(): Promise<void> {
    this.stop();
  }

  /** Cleanup when screen is destroyed */
  public destroy(): void {
    this.stop();
    this.networkManager?.disconnect();
    World.destroy();
    super.destroy();
  }

  /** Auto pause when window loses focus - keep physics running for client prediction */
  public blur(): void {}

  /** Resume when window gains focus */
  public focus(): void {
    if (!this.running) {
      this.start();
    }
  }

  /** Pause gameplay */
  public async pause(): Promise<void> {
    this.stop();
  }

  /** Resume gameplay */
  public async resume(): Promise<void> {
    if (this.networkManager && this.networkManager.isConnected()) {
      this.performanceMonitor.reset();
      this.start();
    }
  }

  /** Reset the game state */
  public reset(): void {
    this.stop();
    World.destroy();
    World.initialize();
  }

  public getLocalPlayerId(): string | null {
    return this.localPlayerId;
  }

  public getWorld(): typeof World {
    return World;
  }

  public getNetworkManager(): NetworkManager {
    return this.networkManager;
  }

  public getFPS(): number {
    return this.performanceMonitor.getFPS();
  }
}
