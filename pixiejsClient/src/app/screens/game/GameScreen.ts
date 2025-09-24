import type { Ticker } from "pixi.js";
import { Container, Text } from "pixi.js";
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
import { PhysicsComponent } from "../../ecs/components/PhysicsComponent";
import { BuildModeSystem } from "../../ecs/systems/BuildModeSystem";
import { ShipBuilderUI } from "../../ui/shipbuilder/ShipBuilderUI";
import { BuildGrid } from "../../ui/shipbuilder/BuildGrid";
import { Button } from "../../ui/Button";
import { ChatBox } from "../../ui/game/ChatBox";
import { PlayerNameManager } from "../../managers/PlayerNameManager";
import { ChatPacket } from "../../network/packets/ChatPacket";
import { ShipConfigurationPacket } from "../../network/packets/ShipConfigurationPacket";

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
  private buildModeSystem!: BuildModeSystem;

  private gameWorldContainer!: Container;

  private shipBuilderUI!: ShipBuilderUI;
  private worldBuildGrid!: BuildGrid;
  private buildModeButton!: Button;
  private buildControlsText!: Text;

  private statsPanel!: StatsPanel;
  private playerBars!: PlayerBars;
  private targetBars!: TargetBars;
  private inputDisplay!: InputDisplay;
  private performanceDisplay!: PerformanceDisplay;
  private chatBox!: ChatBox;

  private lastTime = 0;
  private accumulator = 0;
  private fixedTimeStep = 1 / 30; // 30 Hz fixed update (matching server TPS)
  private maxAccumulator = 0.2; // Max 200ms worth of updates

  private running = false;
  private localPlayerId: string | null = null;

  private fps = 0;
  private frameCount = 0;
  private lastFpsUpdate = 0;
  private lastDeltaMs = 0;

  private f10WasPressed = false;
  private f11WasPressed = false;
  private f12WasPressed = false;
  private bWasPressed = false;
  private enterWasPressed = false;

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

    this.gameWorldContainer = new Container();
    this.addChild(this.gameWorldContainer);

    this.inputManager = new InputManager();
    this.networkManager = NetworkManager.getInstance({
      serverUrl: "ws://localhost:5000/ws",
      interpolationDelay: 100,
      predictionEnabled: false,
    });

    this.inputSystem = new InputSystem(this.inputManager);
    this.renderSystem = new RenderSystem(this.gameWorldContainer);
    this.networkSystem = new NetworkSystem();
    this.buildModeSystem = new BuildModeSystem();

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

    this.chatBox = new ChatBox({
      width: 400,
      height: 250,
      visible: true,
    });
    this.addChild(this.chatBox);

    this.buildModeButton = new Button({
      text: "Build",
      width: 80,
      height: 40,
    });
    this.buildModeButton.onPress.connect(() => {
      this.toggleBuildMode();
    });
    this.addChild(this.buildModeButton);

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

    this.shipBuilderUI = new ShipBuilderUI(this.buildModeSystem);
    this.shipBuilderUI.visible = false;
    this.shipBuilderUI.on("buildModeExit", () => {
      this.exitBuildMode();
    });
    this.addChild(this.shipBuilderUI);

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

    World.addSystem("input", this.inputSystem, [], 100);
    World.addSystem("network", this.networkSystem, [], 90);
    World.addSystem("render", this.renderSystem, ["physics"], 70);

    this.monitorConnectionState();

    this.setupEventListeners();

    this.inputManager.initialize();

    this.startConnection();
  }

  private startConnection(): void {
    setTimeout(async () => {
      try {
        const connected = await this.networkManager.connect("Player");

        if (connected) {
          this.start();
        }
      } catch (error: unknown) {}
    }, 100);
  }

  /** Setup event listeners for packet handling */
  private setupEventListeners(): void {
    window.addEventListener("login-response", (event: unknown) => {
      const { playerId, mapSize, viewDistance } = event.detail;

      this.setLocalPlayer(playerId);

      this.renderSystem.setMapSize(mapSize.width, mapSize.height);
      this.renderSystem.setViewDistance(viewDistance);
    });

    window.addEventListener("chat-message", (event: unknown) => {
      const { playerId, message } = event.detail;
      const nameManager = PlayerNameManager.getInstance();
      const playerName = nameManager.getPlayerName(playerId);

      this.chatBox.addMessage(playerId, playerName, message);
    });

    this.chatBox.onSend((message: string) => {
      if (this.localPlayerId && this.networkManager) {
        const packet = ChatPacket.create(this.localPlayerId, message);
        this.networkManager.send(packet);
      }
    });
  }

  /** Monitor connection state and update physics system accordingly */
  private monitorConnectionState(): void {
    let wasConnected = this.networkManager.isConnected();

    setInterval(() => {
      const isConnected = this.networkManager.isConnected();
      if (isConnected !== wasConnected) {
        wasConnected = isConnected;

        if (!isConnected && !this.running) {
          this.start();
        }
      }
    }, 100);
  }

  private start(): void {
    if (this.running) return;

    this.running = true;
    this.lastTime = performance.now();
  }

  private stop(): void {
    this.running = false;
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

    this.updateFPS(currentTime);

    this.accumulator += deltaTime;

    while (this.accumulator >= this.fixedTimeStep) {
      this.fixedUpdate(this.fixedTimeStep);
      this.accumulator -= this.fixedTimeStep;
    }

    this.variableUpdate(deltaTime);

    this.render();
  }

  private fixedUpdate(deltaTime: number): void {
    this.handleUIToggleInputs();

    this.sendInput();

    World.update(deltaTime);

    this.networkManager.update(deltaTime);
  }

  private handleUIToggleInputs(): void {
    if (!this.inputManager) return;

    const input = this.inputManager.getInputState();

    if (this.chatBox.isCurrentlyTyping()) {
      return; // ChatBox handles its own input via direct keyboard events
    }

    if (input.keys.has("F11") && !this.f11WasPressed) {
      this.statsPanel.toggle();
    }
    this.f11WasPressed = input.keys.has("F11");

    if (input.keys.has("F12") && !this.f12WasPressed) {
      this.inputDisplay.toggle();
    }
    this.f12WasPressed = input.keys.has("F12");

    if (input.keys.has("F10") && !this.f10WasPressed) {
      this.playerBars.toggle();
    }
    this.f10WasPressed = input.keys.has("F10");

    if (input.keys.has("KeyB") && !this.bWasPressed) {
      this.toggleBuildMode();
    }
    this.bWasPressed = input.keys.has("KeyB");

    if (input.keys.has("Enter") && !this.enterWasPressed) {
      this.chatBox.startTyping();
    }
    this.enterWasPressed = input.keys.has("Enter");
  }

  private sendInput(): void {
    if (!this.inputManager || !this.networkManager || !this.localPlayerId)
      return;

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
    this.renderSystem.update(deltaTime);

    if (this.localPlayerId) {
      const entity = World.getEntity(this.localPlayerId);
      if (entity) {
        this.renderSystem.followEntity(entity);
      }
    }

    if (this.worldBuildGrid.visible && this.buildModeSystem.isInBuildMode()) {
      this.positionBuildGridAroundPlayer();
    } else if (this.worldBuildGrid.visible) {
    }
  }

  private render(): void {
    this.renderSystem.render();

    this.updateUI();
  }

  private updateUI(): void {
    if (!this.localPlayerId) return;

    const entity = World.getEntity(this.localPlayerId);
    if (!entity) return;

    const inputState = this.inputManager.getInputState();

    const networkComponent = entity.get(NetworkComponent);
    const lastServerTick = networkComponent?.lastServerTick;

    this.statsPanel.updateFromEntity(
      entity,
      inputState,
      this.fps,
      0,
      lastServerTick,
    );
    this.inputDisplay.updateFromInput(inputState);
    this.playerBars.updateFromEntity(entity);
    const camera = this.renderSystem.getCamera();
    const hoveredEntityId = this.renderSystem.getHoveredEntityId();
    this.targetBars.updateFromWorld(
      camera,
      this.localPlayerId,
      entity ? 300 : undefined,
      hoveredEntityId,
    );

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

  private toggleBuildMode(): void {
    if (this.buildModeSystem.isInBuildMode()) {
      this.exitBuildMode();
    } else {
      this.enterBuildMode();
    }
  }

  private enterBuildMode(): void {
    this.buildModeSystem.enterBuildMode();

    this.buildModeSystem.selectPart("hull", "square");

    this.positionBuildGridAroundPlayer();

    this.worldBuildGrid.visible = true;

    this.showBuildModeControls();

    this.inputSystem.setPaused(true);
  }

  private exitBuildMode(): void {
    this.buildModeSystem.exitBuildMode();

    this.worldBuildGrid.visible = false;
    this.shipBuilderUI.hide();

    this.hideBuildModeControls();

    this.sendShipConfiguration();

    this.inputSystem.setPaused(false);
  }

  private positionBuildGridAroundPlayer(): void {
    if (!this.localPlayerId) return;

    const playerEntity = World.getEntity(this.localPlayerId);
    if (!playerEntity) return;

    const physics = playerEntity.get(PhysicsComponent);
    if (!physics) return;

    this.worldBuildGrid.x = physics.position.x;
    this.worldBuildGrid.y = physics.position.y;

    this.worldBuildGrid.rotation = physics.rotationRadians;

    this.worldBuildGrid.position.set(
      this.worldBuildGrid.x,
      this.worldBuildGrid.y,
    );
  }

  private sendShipConfiguration(): void {
    const parts = this.worldBuildGrid.getAllParts();

    const gridDims = this.worldBuildGrid.getGridDimensions();
    const centerX = Math.floor(gridDims.width / 2);
    const centerY = Math.floor(gridDims.height / 2);

    const shipParts = parts.map((part) => ({
      gridX: part.gridX - centerX,
      gridY: part.gridY - centerY,
      type: part.type === "hull" ? 0 : part.type === "shield" ? 1 : 2,
      shape: part.shape === "triangle" ? 1 : 2, // triangle=1, square=2 (match server ShapeType enum)
      rotation: part.rotation,
    }));

    const hasPlayerPart = shipParts.some(
      (part) => part.gridX === 0 && part.gridY === 0,
    );

    if (!hasPlayerPart) {
      shipParts.push({
        gridX: 0,
        gridY: 0,
        type: 0, // hull
        shape: 2, // square/box (to match original cube)
        rotation: 0,
      });
    }

    if (this.localPlayerId && shipParts.length > 0) {
      const packet = ShipConfigurationPacket.create(
        this.localPlayerId,
        shipParts,
      );
      this.networkManager.send(packet);
    }
  }

  private showBuildModeControls(): void {
    const selected = this.buildModeSystem.getSelectedPart();
    this.buildControlsText.text = `BUILD MODE - ${selected.type?.toUpperCase() || "HULL"} (${selected.shape?.toUpperCase() || "SQUARE"}) - ADDITIONS ONLY
1: Hull   2: Shield   3: Engine   T: Toggle Shape   R: Rotate   ESC: Exit`;
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

  private setupWorldGridEvents(): void {
    this.worldBuildGrid.eventMode = "static";
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
  }

  private onWorldGridPointerMove(event: unknown): void {
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
            this.sendShipConfiguration();
          }
        } else if (this.dragMode === "remove" && existingPart) {
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

  private onWorldGridPointerDown(event: unknown): void {
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

      if (event.shiftKey || event.button === 2) {
        this.dragMode = "remove";
      } else {
        this.dragMode = "place";
        if (!existingPart) {
          const placed = this.buildModeSystem.placePart(
            gridPos.gridX,
            gridPos.gridY,
          );
          if (placed) {
            this.sendShipConfiguration();
          }
        }
      }
    }
  }

  private onWorldGridPointerUp(): void {
    this.isDragging = false;
    this.dragMode = null;
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

    if (this.inputSystem) {
      this.inputSystem.setLocalEntity(playerId);
    }

    if (this.renderSystem) {
      this.renderSystem.setLocalPlayerId(playerId);
    }

    const entity = World.getEntity(playerId);
    if (entity && this.renderSystem) {
      this.renderSystem.followEntity(entity);
    }
  }

  /** Resize the screen, fired whenever window size changes */
  public resize(width: number, height: number) {
    this.renderSystem?.resize(width, height);

    this.statsPanel?.resize(width, height);
    this.playerBars?.resize(width, height);
    this.inputDisplay?.resize(width, height);
    this.targetBars?.resize(width, height);
    this.performanceDisplay?.resize(width, height);

    if (this.buildModeButton) {
      this.buildModeButton.x = width - 100;
      this.buildModeButton.y = 50;
    }

    if (this.buildControlsText) {
      this.buildControlsText.x = width / 2 - this.buildControlsText.width / 2;
      this.buildControlsText.y = 20;
    }

    this.shipBuilderUI?.resize(width, height);

    this.chatBox?.resize(width, height);
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
    return this.fps;
  }
}
