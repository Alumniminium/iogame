import type { Ticker } from "pixi.js";
import { Container } from "pixi.js";
import { World } from "../../ecs/core/World";
import { DeathSystem } from "../../ecs/systems/DeathSystem";
import { NetworkManager } from "../../network/NetworkManager";
import { InputManager } from "../../managers/InputManager";
import { CameraManager } from "../../managers/CameraManager";
import { BuildModeManager } from "../../managers/BuildModeManager";
import { ParticleSystem } from "../../ecs/systems/ParticleSystem";
import { LifetimeSystem } from "../../ecs/systems/LifetimeSystem";
import { HealthDamageSystem } from "../../ecs/systems/HealthDamageSystem";
import { EntityRenderer } from "../../ecs/systems/renderers/EntityRenderer";
import { ShieldRenderer } from "../../ecs/systems/renderers/ShieldRenderer";
import { ParticleRenderer } from "../../ecs/systems/renderers/ParticleRenderer";
import { EffectRenderer } from "../../ecs/systems/renderers/EffectRenderer";
import { LineRenderer } from "../../ecs/systems/renderers/LineRenderer";
import { BackgroundRenderer } from "../../managers/BackgroundRenderer";
import { PlayerNameManager } from "../../managers/PlayerNameManager";
import { ShipPartManager } from "../../managers/ShipPartManager";
import { ChatPacket } from "../../network/packets/ChatPacket";
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
  private cameraManager!: CameraManager;
  private buildModeManager!: BuildModeManager;
  private connectionManager!: GameConnectionManager;
  private uiManager!: GameUIManager;

  private gameWorldContainer!: Container;

  private performanceMonitor!: PerformanceMonitor;
  private inputHandler!: GameInputHandler;
  private buildModeController!: BuildModeController;

  private running = false;
  private localPlayerId: string | null = null;
  private isPaused = false;
  private viewDistance = 300;
  private backgroundRenderer!: BackgroundRenderer;

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

    this.inputHandler = new GameInputHandler(this.inputManager, this.networkManager, () => this.cameraManager.getCamera());

    this.backgroundRenderer = new BackgroundRenderer(this.gameWorldContainer);
    this.cameraManager = new CameraManager(this.gameWorldContainer);
    ShipPartManager.getInstance().initialize(this.networkManager);
    this.buildModeManager = BuildModeManager.getInstance();

    this.uiManager = new GameUIManager(
      this,
      this.inputManager,
      this.performanceMonitor,
      () => this.localPlayerId,
      () => this.cameraManager.getCamera(),
    );

    World.setSystems(
      new HealthDamageSystem(),
      new LifetimeSystem(),
      new ParticleSystem(this.inputManager),
      new EntityRenderer(this.gameWorldContainer),
      new ShieldRenderer(this.gameWorldContainer),
      new ParticleRenderer(this.gameWorldContainer),
      new EffectRenderer(this.gameWorldContainer),
      new LineRenderer(this.gameWorldContainer),
      new DeathSystem(),
    );

    this.buildModeController = new BuildModeController(this.buildModeManager, this.gameWorldContainer, this, () => this.localPlayerId);

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
        this.inputManager.setPaused(true);
      },
      onExit: () => {
        this.inputManager.setPaused(false);
      },
    });

    this.connectionManager.monitorConnectionState();
    this.setupEventListeners();
    this.inputManager.initialize();
    this.inputManager.onEscapePressed(() => this.uiManager.togglePauseMenu(this.isPaused));
    this.inputManager.onMapKeyPressed(() => this.uiManager.toggleSectorMap());
    this.inputManager.onWheel((delta) => this.handleZoom(delta));
    this.connectionManager.startConnection("Player");
    this.backgroundRenderer.initialize();
  }

  /** Setup event listeners for packet handling */
  private setupEventListeners(): void {
    window.addEventListener("login-response", (event: any) => {
      const { playerId, mapSize, viewDistance } = event.detail;

      this.setLocalPlayer(playerId);
      this.backgroundRenderer.setMapSize(mapSize.width, mapSize.height);
      this.viewDistance = viewDistance;
      this.cameraManager.setViewDistance(this.viewDistance);
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

    this.cameraManager.update(timing.deltaTime);
    this.buildModeController.update();

    this.uiManager.updateUI();
  }

  private fixedUpdate(deltaTime: number): void {
    this.inputHandler.handleUIToggleInputs();

    // Skip game simulation when paused or in build mode
    if (!this.isPaused && !this.buildModeController.isInBuildMode()) {
      this.inputManager.applyInputToLocalPlayer();
      this.inputHandler.sendInput();

      World.update(deltaTime);

      this.networkManager.update(deltaTime);
    }
  }

  private handleZoom(delta: number): void {
    const zoomSpeed = 0.1;
    const minViewDistance = 50;
    const maxViewDistance = 1000;

    this.viewDistance += delta * zoomSpeed;
    this.viewDistance = Math.max(minViewDistance, Math.min(maxViewDistance, this.viewDistance));

    this.cameraManager.setViewDistance(this.viewDistance);
  }

  private pauseGame(): void {
    this.isPaused = true;
    // Pause input processing for ship movement
    this.inputManager.setPaused(true);
    // Disable input capture except ESC (handled in InputManager)
    this.inputManager.setEnabled(false);
  }

  private resumeGame(): void {
    this.isPaused = false;
    this.inputManager.setPaused(false);
    this.inputManager.setEnabled(true);
  }

  public setLocalPlayer(playerId: string): void {
    this.localPlayerId = playerId;
    // Note: ShipPartManager now uses World.Me directly instead of needing setLocalPlayerId
  }

  /** Resize the screen, fired whenever window size changes */
  public resize(width: number, height: number) {
    this.cameraManager?.resize(width, height);

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
    super.destroy();
  }

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
    World.initialize();
  }

  public getNetworkManager(): NetworkManager {
    return this.networkManager;
  }

  public getFPS(): number {
    return this.performanceMonitor.getFPS();
  }
}
