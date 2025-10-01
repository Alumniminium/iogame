import type { Ticker } from "pixi.js";
import { Container, Text } from "pixi.js";
import { World } from "../../ecs/core/World";
import { InputSystem } from "../../ecs/systems/InputSystem";
import { RenderSystem } from "../../ecs/systems/RenderSystem";
import { NetworkSystem } from "../../ecs/systems/NetworkSystem";
import { NetworkManager } from "../../network/NetworkManager";
import { ComponentStatePacket } from "../../network/packets/ComponentStatePacket";
import { InputManager } from "../../managers/InputManager";
import { StatsPanel } from "../../ui/game/StatsPanel";
import { PlayerBars } from "../../ui/game/PlayerBars";
import { TargetBars } from "../../ui/game/TargetBars";
import { InputDisplay } from "../../ui/game/InputDisplay";
import { PerformanceDisplay } from "../../ui/game/PerformanceDisplay";
import { ShipStatsDisplay } from "../../ui/game/ShipStatsDisplay";
import { NetworkComponent } from "../../ecs/components/NetworkComponent";
import { PhysicsComponent } from "../../ecs/components/PhysicsComponent";
import { BuildModeSystem } from "../../ecs/systems/BuildModeSystem";
import { ParticleSystem } from "../../ecs/systems/ParticleSystem";
import { ShipPartSyncSystem } from "../../ecs/systems/ShipPartSyncSystem";
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
import { ChatBox } from "../../ui/game/ChatBox";
import { PlayerNameManager } from "../../managers/PlayerNameManager";
import { ShipPartManager } from "../../managers/ShipPartManager";
import { ChatPacket } from "../../network/packets/ChatPacket";
import { PauseMenu } from "../../ui/game/PauseMenu";
import { SectorMap } from "../../ui/game/SectorMap";
import { engine } from "../../getEngine";

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
  private shipPartSyncSystem!: ShipPartSyncSystem;
  private lifetimeSystem!: LifetimeSystem;
  private shipPartManager!: ShipPartManager;

  private gameWorldContainer!: Container;

  private worldBuildGrid!: BuildGrid;
  private buildControlsText!: Text;
  private shapeSelector!: ShapeSelector;
  private componentDialog!: ComponentDialog;
  private pendingShapeType: ShapeType | null = null;

  private statsPanel!: StatsPanel;
  private playerBars!: PlayerBars;
  private targetBars!: TargetBars;
  private inputDisplay!: InputDisplay;
  private performanceDisplay!: PerformanceDisplay;
  private shipStatsDisplay!: ShipStatsDisplay;
  private chatBox!: ChatBox;
  private pauseMenu!: PauseMenu;
  private sectorMap!: SectorMap;

  private lastTime = 0;
  private accumulator = 0;
  private fixedTimeStep = 1 / 30; // 30 Hz fixed update (matching server TPS)
  private maxAccumulator = 0.2; // Max 200ms worth of updates

  private running = false;
  private localPlayerId: string | null = null;
  private isPaused = false;
  private inBuildMode = false;

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
    this.renderSystem = new RenderSystem(this.gameWorldContainer, engine());
    this.networkSystem = new NetworkSystem();
    this.buildModeSystem = new BuildModeSystem();
    this.particleSystem = new ParticleSystem();
    this.shipPartSyncSystem = new ShipPartSyncSystem();
    this.lifetimeSystem = new LifetimeSystem();
    this.shipPartManager = new ShipPartManager(this.networkManager);

    this.statsPanel = new StatsPanel({
      position: "left-center",
      visible: true,
    });
    this.addChild(this.statsPanel);

    this.playerBars = new PlayerBars({
      position: "top-center",
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

    this.shipStatsDisplay = new ShipStatsDisplay();
    this.addChild(this.shipStatsDisplay);

    this.chatBox = new ChatBox({
      width: 400,
      height: 250,
      visible: true,
    });
    this.addChild(this.chatBox);

    this.pauseMenu = new PauseMenu({
      onContinue: () => this.resumeGame(),
      onSettings: () => console.log("Settings not implemented yet"),
      onHelp: () => console.log("Help not implemented yet"),
      onQuit: () => console.log("Quit not implemented yet"),
    });
    this.addChild(this.pauseMenu);

    this.sectorMap = new SectorMap({
      mapWidth: 32000,
      mapHeight: 32000,
      displaySize: 300,
      visible: false,
    });
    this.addChild(this.sectorMap);

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

    World.addSystem("input", this.inputSystem, [], 100);
    World.addSystem("network", this.networkSystem, [], 90);
    World.addSystem("lifetime", this.lifetimeSystem, [], 85);
    World.addSystem("particles", this.particleSystem, ["physics"], 80);
    World.addSystem("shipPartSync", this.shipPartSyncSystem, [], 75);
    World.addSystem("render", this.renderSystem, ["physics"], 70);

    this.monitorConnectionState();

    this.setupEventListeners();

    this.inputManager.initialize();

    // Set up ESC key handler for pause menu
    this.inputManager.onEscapePressed(() => this.togglePauseMenu());

    // Set up M key handler for sector map
    this.inputManager.onMapKeyPressed(() => this.toggleSectorMap());

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
    window.addEventListener("login-response", (event: any) => {
      const { playerId, mapSize, viewDistance } = event.detail;

      this.setLocalPlayer(playerId);
      this.renderSystem.setMapSize(mapSize.width, mapSize.height);
      this.renderSystem.setViewDistance(viewDistance);
    });

    window.addEventListener("chat-message", (event: any) => {
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

    // Skip game simulation when paused or in build mode
    if (!this.isPaused && !this.inBuildMode) {
      this.sendInput();

      World.update(deltaTime);

      this.networkManager.update(deltaTime);
    }
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
      this.localPlayerId,
      buttonStates,
      mouseWorld.x,
      mouseWorld.y,
    );

    this.networkManager.send(packet);
  }

  private variableUpdate(deltaTime: number): void {
    this.renderSystem.update(deltaTime);

    if (this.worldBuildGrid.visible && this.buildModeSystem.isInBuildMode()) {
      this.positionBuildGridAroundPlayer();
    }
  }

  private render(): void {
    // Rendering is handled by RenderSystem.update()
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
    this.shipStatsDisplay.updateFromEntity(entity, inputState);
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

    // Update sector map with player position
    const physics = entity.get(PhysicsComponent);
    if (physics) {
      this.sectorMap.updatePlayerPosition(
        physics.position.x,
        physics.position.y,
      );
    }
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
  private togglePauseMenu(): void {
    if (this.isPaused) {
      this.resumeGame();
    } else {
      this.pauseGame();
    }
  }

  private toggleSectorMap(): void {
    this.sectorMap.toggle();
  }

  private pauseGame(): void {
    this.isPaused = true;
    // Pause input processing for ship movement
    this.inputSystem.setPaused(true);
    // Disable input capture except ESC (handled in InputManager)
    this.inputManager.setEnabled(false);
    this.pauseMenu.show();
  }

  private resumeGame(): void {
    this.isPaused = false;
    // Resume input processing for ship movement
    this.inputSystem.setPaused(false);
    // Re-enable input capture
    this.inputManager.setEnabled(true);
    this.pauseMenu.hide();
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

    this.statsPanel?.resize(width, height);
    this.playerBars?.resize(width, height);
    this.inputDisplay?.resize(width, height);
    this.targetBars?.resize(width, height);
    this.performanceDisplay?.resize(width, height);
    this.shipStatsDisplay?.resize(width, height);

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

    this.chatBox?.resize(width, height);
    this.sectorMap?.resize(width, height);
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
