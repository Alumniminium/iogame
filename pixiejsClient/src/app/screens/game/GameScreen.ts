import type { Ticker } from "pixi.js";
import { Container, Text } from "pixi.js";
import { World } from "../../ecs/core/World";
import { InputSystem } from "../../ecs/systems/InputSystem";
import { RenderSystem } from "../../ecs/systems/RenderSystem";
import { NetworkSystem } from "../../ecs/systems/NetworkSystem";
import { NetworkManager } from "../../network/NetworkManager";
import { InputManager } from "../../managers/InputManager";
import { Box2DBodyComponent } from "../../ecs/components/Box2DBodyComponent";
import { BuildModeSystem } from "../../ecs/systems/BuildModeSystem";
import { ParticleSystem } from "../../ecs/systems/ParticleSystem";
import { LifetimeSystem } from "../../ecs/systems/LifetimeSystem";
import { BuildGrid } from "../../ui/shipbuilder/BuildGrid";
import {
  ShapeSelector,
  type ShapeType,
} from "../../ui/shipbuilder/ShapeSelector";
import {
  ComponentDialog,
  type ComponentConfig,
} from "../../ui/shipbuilder/ComponentDialog";
import { PlayerNameManager } from "../../managers/PlayerNameManager";
import { ShipPartManager } from "../../managers/ShipPartManager";
import { ChatPacket } from "../../network/packets/ChatPacket";
import { engine } from "../../getEngine";
import { PerformanceMonitor } from "../../managers/PerformanceMonitor";
import { GameConnectionManager } from "../../managers/GameConnectionManager";
import { GameInputHandler } from "../../managers/GameInputHandler";
import { GameUIManager } from "../../managers/GameUIManager";

/** The main game screen for the IO game */
export class GameScreen extends Container {
  /** Assets bundles required by this screen */
  public static assetBundles = ["main"];

  private networkManager!: NetworkManager;
  private inputManager!: InputManager;
  private renderSystem!: RenderSystem;
  private inputSystem!: InputSystem;
  private networkSystem!: NetworkSystem;
  private buildModeSystem!: BuildModeSystem;
  private particleSystem!: ParticleSystem;
  private lifetimeSystem!: LifetimeSystem;
  private shipPartManager!: ShipPartManager;

  private gameWorldContainer!: Container;

  private worldBuildGrid!: BuildGrid;
  private buildControlsText!: Text;
  private shapeSelector!: ShapeSelector;
  private componentDialog!: ComponentDialog;
  private pendingShapeType: ShapeType | null = null;

  private performanceMonitor!: PerformanceMonitor;
  private connectionManager!: GameConnectionManager;
  private inputHandler!: GameInputHandler;
  private uiManager!: GameUIManager;

  private running = false;
  private localPlayerId: string | null = null;
  private isPaused = false;
  private inBuildMode = false;
  private viewDistance = 300;

  private isDragging = false;
  private dragMode: "place" | "remove" | null = null;

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

    this.buildControlsText = new Text({
      text: "",
      style: {
        fontFamily: "Arial",
        fontSize: 16,
        fill: 0x00ff00,
        align: "left",
      },
    });
    this.buildControlsText.visible = false;
    this.addChild(this.buildControlsText);

    this.shapeSelector = new ShapeSelector();
    this.shapeSelector.setOnShapeSelected((shape) =>
      this.onShapeSelected(shape),
    );
    this.addChild(this.shapeSelector);

    this.componentDialog = new ComponentDialog();
    this.componentDialog.setOnConfirm((config) =>
      this.onComponentSelected(config),
    );
    this.addChild(this.componentDialog);

    this.worldBuildGrid = new BuildGrid({
      cellSize: 1, // 1x1 world units per cell
      gridWidth: 39, // Odd number so there's a true center cell
      gridHeight: 39,
      lineColor: 0x00ff00,
      lineAlpha: 0.8,
      backgroundColor: 0x000000,
      backgroundAlpha: 0.2,
    });
    this.worldBuildGrid.visible = false;

    const gridPixelWidth =
      this.worldBuildGrid.getGridDimensions().width *
      this.worldBuildGrid.getCellSize();
    const gridPixelHeight =
      this.worldBuildGrid.getGridDimensions().height *
      this.worldBuildGrid.getCellSize();
    this.worldBuildGrid.pivot.set(gridPixelWidth / 2, gridPixelHeight / 2);

    this.gameWorldContainer.addChild(this.worldBuildGrid);
    this.buildModeSystem.setBuildGrid(this.worldBuildGrid);

    this.setupWorldGridEvents();

    this.setupBuildModeKeyboard();

    // Set up input handler callbacks
    this.inputHandler.setUIToggleCallbacks({
      onToggleStatsPanel: () => this.uiManager.getStatsPanel().toggle(),
      onToggleInputDisplay: () => this.uiManager.getInputDisplay().toggle(),
      onTogglePlayerBars: () => this.uiManager.getPlayerBars().toggle(),
      onToggleBuildMode: () => this.toggleBuildMode(),
      onStartChatTyping: () => this.uiManager.getChatBox().startTyping(),
      isChatTyping: () => this.uiManager.getChatBox().isCurrentlyTyping(),
    });

    // Set up UI manager pause callbacks
    this.uiManager.setPauseCallbacks({
      onPause: () => this.pauseGame(),
      onResume: () => this.resumeGame(),
    });

    World.addSystem("input", this.inputSystem, [], 100);
    World.addSystem("network", this.networkSystem, [], 90);
    World.addSystem("lifetime", this.lifetimeSystem, [], 85);
    World.addSystem("particles", this.particleSystem, ["physics"], 80);
    World.addSystem("render", this.renderSystem, ["physics"], 70);

    this.connectionManager.monitorConnectionState();

    this.setupEventListeners();

    this.inputManager.initialize();

    // Set up ESC key handler for pause menu
    this.inputManager.onEscapePressed(() =>
      this.uiManager.togglePauseMenu(this.isPaused),
    );

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
    if (!this.isPaused && !this.inBuildMode) {
      this.inputHandler.sendInput();

      World.update(deltaTime);

      this.networkManager.update(deltaTime);
    }
  }

  private variableUpdate(deltaTime: number): void {
    this.renderSystem.update(deltaTime);

    if (this.worldBuildGrid.visible && this.buildModeSystem.isInBuildMode()) {
      this.positionBuildGridAroundPlayer();
    }
  }

  private render(): void {
    // Rendering is handled by RenderSystem.update()
    this.uiManager.updateUI();
  }

  private toggleBuildMode(): void {
    if (this.buildModeSystem.isInBuildMode()) {
      this.exitBuildMode();
    } else {
      this.enterBuildMode();
    }
  }

  private enterBuildMode(): void {
    this.inBuildMode = true;
    this.buildModeSystem.enterBuildMode();

    this.positionBuildGridAroundPlayer();

    this.worldBuildGrid.visible = true;

    this.showBuildModeControls();
    this.shapeSelector.show();

    // InputManager stays enabled for build mode keyboard/mouse
    // InputSystem paused to stop ship movement
    this.inputSystem.setPaused(true);

    // Disable interactivity for local player entity so grid clicks work
    this.renderSystem.setBuildModeActive(true);
  }

  private exitBuildMode(): void {
    this.inBuildMode = false;
    this.buildModeSystem.exitBuildMode();

    this.worldBuildGrid.visible = false;

    this.hideBuildModeControls();
    this.shapeSelector.hide();
    this.componentDialog.hide();

    this.inputSystem.setPaused(false);

    // Re-enable interactivity for local player entity
    this.renderSystem.setBuildModeActive(false);
  }

  /**
   * PAUSE STATE ARCHITECTURE:
   *
   * Three pause-related states work together:
   *
   * 1. isPaused (GameScreen) - Master pause flag
   *    - Controls game loop (sendInput, World.update, network updates)
   *    - Single source of truth for "game is paused"
   *
   * 2. inBuildMode (GameScreen) - Build mode flag
   *    - Pauses game simulation while allowing build UI
   *    - InputManager stays enabled for build keyboard/mouse
   *    - InputSystem paused to prevent ship movement
   *
   * 3. InputSystem.paused - ECS input processing
   *    - Paused when: isPaused OR inBuildMode
   *    - Stops applying input to entity components
   *
   * 4. InputManager.enabled - Raw input capture
   *    - Disabled only when isPaused (ESC always works)
   *    - Enabled in build mode for UI controls
   */

  private handleZoom(delta: number): void {
    const zoomSpeed = 0.1;
    const minViewDistance = 50;
    const maxViewDistance = 1000;

    this.viewDistance += delta * zoomSpeed;
    this.viewDistance = Math.max(
      minViewDistance,
      Math.min(maxViewDistance, this.viewDistance),
    );

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

  private positionBuildGridAroundPlayer(): void {
    if (!this.localPlayerId) return;

    const playerEntity = World.getEntity(this.localPlayerId);
    if (!playerEntity) return;

    const physics = playerEntity.get(Box2DBodyComponent);
    if (!physics) return;

    this.worldBuildGrid.x = physics.position.x;
    this.worldBuildGrid.y = physics.position.y;

    this.worldBuildGrid.rotation = physics.rotationRadians;

    this.worldBuildGrid.position.set(
      this.worldBuildGrid.x,
      this.worldBuildGrid.y,
    );
  }

  private showBuildModeControls(): void {
    const selected = this.buildModeSystem.getSelectedPart();
    this.buildControlsText.text = `BUILD MODE - ${selected.type?.toUpperCase() || "HULL"} (${selected.shape?.toUpperCase() || "SQUARE"})
1: Hull □   2: Shield   3: Engine   4: Hull △   T: Toggle Shape   R: Rotate   Right-Click: Remove   ESC: Exit`;
    this.buildControlsText.visible = true;
  }

  private hideBuildModeControls(): void {
    this.buildControlsText.visible = false;
  }

  private updateBuildModeControls(): void {
    if (this.buildControlsText.visible) {
      this.showBuildModeControls();
    }
  }

  private onShapeSelected(shape: ShapeType): void {
    this.pendingShapeType = shape;
    this.componentDialog.show();
  }

  private onComponentSelected(config: ComponentConfig): void {
    if (!this.pendingShapeType) return;

    const shape = this.pendingShapeType === "box" ? "square" : "triangle";
    this.buildModeSystem.setPendingBlock(shape, config);

    this.shapeSelector.hide();
    this.pendingShapeType = null;
  }

  private setupWorldGridEvents(): void {
    (this.worldBuildGrid as any).eventMode = "static";
    // Enable right-click events
    (this.worldBuildGrid as any).cursor = "pointer";

    this.worldBuildGrid.on(
      "pointermove",
      this.onWorldGridPointerMove.bind(this),
    );
    this.worldBuildGrid.on(
      "pointerdown",
      this.onWorldGridPointerDown.bind(this),
    );
    this.worldBuildGrid.on("pointerup", this.onWorldGridPointerUp.bind(this));
    this.worldBuildGrid.on(
      "pointerupoutside",
      this.onWorldGridPointerUp.bind(this),
    );
    this.worldBuildGrid.on("rightclick", this.onWorldGridRightClick.bind(this));
  }

  private onWorldGridPointerMove(event: any): void {
    if (!this.buildModeSystem.isInBuildMode()) return;

    const gridPos = this.worldBuildGrid.worldToGrid(
      event.global.x,
      event.global.y,
    );

    if (this.worldBuildGrid.isValidGridPosition(gridPos.gridX, gridPos.gridY)) {
      if (this.isDragging && this.dragMode) {
        const existingPart = this.buildModeSystem.getPartAt(
          gridPos.gridX,
          gridPos.gridY,
        );

        if (this.dragMode === "place" && !existingPart) {
          const placed = this.buildModeSystem.placePart(
            gridPos.gridX,
            gridPos.gridY,
          );
          if (placed) {
            const part = this.buildModeSystem.getPartAt(
              gridPos.gridX,
              gridPos.gridY,
            );
            if (part) {
              this.shipPartManager.createShipPart(part.gridX, part.gridY, {
                type: part.type,
                shape: part.shape,
                rotation: part.rotation,
                attachedComponents: part.attachedComponents,
              });
            }
          }
        } else if (this.dragMode === "remove" && existingPart) {
          const removed = this.worldBuildGrid.removePart(
            gridPos.gridX,
            gridPos.gridY,
          );
          if (removed) {
            this.shipPartManager.removeShipPart(gridPos.gridX, gridPos.gridY);
          }
        }
      }

      this.buildModeSystem.updateGhostPosition(gridPos.gridX, gridPos.gridY);

      const existingPart = this.buildModeSystem.getPartAt(
        gridPos.gridX,
        gridPos.gridY,
      );
      const highlightColor = existingPart ? 0xff0000 : 0x00ff00; // Red if occupied, green if free
      this.worldBuildGrid.highlightCell(
        gridPos.gridX,
        gridPos.gridY,
        highlightColor,
      );
    } else {
      this.worldBuildGrid.clearHighlight();
      this.buildModeSystem.hideGhost();
    }
  }

  private onWorldGridPointerDown(event: any): void {
    if (!this.buildModeSystem.isInBuildMode()) return;

    const gridPos = this.worldBuildGrid.worldToGrid(
      event.global.x,
      event.global.y,
    );

    if (this.worldBuildGrid.isValidGridPosition(gridPos.gridX, gridPos.gridY)) {
      const existingPart = this.buildModeSystem.getPartAt(
        gridPos.gridX,
        gridPos.gridY,
      );

      this.isDragging = true;

      // PixiJS FederatedPointerEvent: button 0=left, 1=middle, 2=right
      // Also check pointerType to ensure it's a right-click from mouse
      const isRightClick =
        event.button === 2 || event.nativeEvent?.button === 2;
      if (event.shiftKey || isRightClick) {
        this.dragMode = "remove";
        if (existingPart) {
          const removed = this.worldBuildGrid.removePart(
            gridPos.gridX,
            gridPos.gridY,
          );
          if (removed) {
            this.shipPartManager.removeShipPart(gridPos.gridX, gridPos.gridY);
          }
        }
      } else {
        this.dragMode = "place";
        if (!existingPart) {
          const placed = this.buildModeSystem.placePart(
            gridPos.gridX,
            gridPos.gridY,
          );
          if (placed) {
            const part = this.buildModeSystem.getPartAt(
              gridPos.gridX,
              gridPos.gridY,
            );
            if (part) {
              this.shipPartManager.createShipPart(part.gridX, part.gridY, {
                type: part.type,
                shape: part.shape,
                rotation: part.rotation,
                attachedComponents: part.attachedComponents,
              });
            }
          }
        }
      }
    }
  }

  private onWorldGridPointerUp(): void {
    this.isDragging = false;
    this.dragMode = null;
  }

  private onWorldGridRightClick(event: any): void {
    if (!this.buildModeSystem.isInBuildMode()) return;

    const gridPos = this.worldBuildGrid.worldToGrid(
      event.global.x,
      event.global.y,
    );

    if (this.worldBuildGrid.isValidGridPosition(gridPos.gridX, gridPos.gridY)) {
      const existingPart = this.buildModeSystem.getPartAt(
        gridPos.gridX,
        gridPos.gridY,
      );

      if (existingPart) {
        const removed = this.worldBuildGrid.removePart(
          gridPos.gridX,
          gridPos.gridY,
        );
        if (removed) {
          this.shipPartManager.removeShipPart(gridPos.gridX, gridPos.gridY);
        }
      }
    }
  }

  private setupBuildModeKeyboard(): void {
    document.addEventListener("keydown", (event) => {
      if (!this.buildModeSystem.isInBuildMode()) return;

      switch (event.code) {
        case "Digit1":
          this.buildModeSystem.selectPart(
            "hull",
            (this.buildModeSystem.getSelectedPart().shape as
              | "triangle"
              | "square") || "square",
          );
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        case "Digit2":
          this.buildModeSystem.selectPart(
            "shield",
            (this.buildModeSystem.getSelectedPart().shape as
              | "triangle"
              | "square") || "square",
          );
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        case "Digit3":
          this.buildModeSystem.selectPart(
            "engine",
            (this.buildModeSystem.getSelectedPart().shape as
              | "triangle"
              | "square") || "square",
          );
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        case "Digit4":
          this.buildModeSystem.selectPart("hull", "triangle");
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        case "KeyT": {
          const current = this.buildModeSystem.getSelectedPart();
          const newShape = current.shape === "square" ? "triangle" : "square";
          this.buildModeSystem.selectPart(
            (current.type as "hull" | "shield" | "engine") || "hull",
            newShape,
          );
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        }
        case "KeyR":
          this.buildModeSystem.rotatePart();
          this.updateBuildModeControls();
          event.preventDefault();
          break;
        case "Escape":
          this.exitBuildMode();
          event.preventDefault();
          break;
      }
    });
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

    if (this.buildControlsText) {
      this.buildControlsText.x = width / 2 - this.buildControlsText.width / 2;
      this.buildControlsText.y = 20;
    }

    if (this.shapeSelector) {
      this.shapeSelector.x = width / 2 - 150;
      this.shapeSelector.y = height - 80;
    }

    if (this.componentDialog) {
      this.componentDialog.x = width / 2 - 200;
      this.componentDialog.y = height / 2 - 175;
    }
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

  public getWorld(): unknown {
    return World.getInstance();
  }

  public getNetworkManager(): NetworkManager {
    return this.networkManager;
  }

  public getFPS(): number {
    return this.performanceMonitor.getFPS();
  }
}
