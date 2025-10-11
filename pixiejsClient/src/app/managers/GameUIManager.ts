import { Container } from "pixi.js";
import { StatsPanel } from "../ui/game/StatsPanel";
import { PlayerBars } from "../ui/game/PlayerBars";
import { TargetBars } from "../ui/game/TargetBars";
import { InputDisplay } from "../ui/game/InputDisplay";
import { PerformanceDisplay } from "../ui/game/PerformanceDisplay";
import { ShipStatsDisplay } from "../ui/game/ShipStatsDisplay";
import { ChatBox } from "../ui/game/ChatBox";
import { PauseMenu } from "../ui/game/PauseMenu";
import { SettingsPage } from "../ui/game/SettingsPage";
import { SectorMap } from "../ui/game/SectorMap";
import { World } from "../ecs/core/World";
import { NetworkComponent } from "../ecs/components/NetworkComponent";
import { Box2DBodyComponent } from "../ecs/components/Box2DBodyComponent";
import type { InputManager } from "./InputManager";
import type { PerformanceMonitor } from "./PerformanceMonitor";
import type { CameraState } from "../ecs/Camera";

/**
 * Manages all game UI components and their updates.
 * Centralizes UI lifecycle, visibility, and data updates.
 */
export class GameUIManager {
  private inputManager: InputManager;
  private performanceMonitor: PerformanceMonitor;
  private getLocalPlayerId: () => string | null;
  private getCamera: () => CameraState;
  private getHoveredEntityId: () => string | null;

  // UI Components
  private statsPanel: StatsPanel;
  private playerBars: PlayerBars;
  private targetBars: TargetBars;
  private inputDisplay: InputDisplay;
  private performanceDisplay: PerformanceDisplay;
  private shipStatsDisplay: ShipStatsDisplay;
  private chatBox: ChatBox;
  private pauseMenu: PauseMenu;
  private settingsPage: SettingsPage;
  private sectorMap: SectorMap;

  // Pause state callbacks
  private onPause?: () => void;
  private onResume?: () => void;

  constructor(
    container: Container,
    inputManager: InputManager,
    performanceMonitor: PerformanceMonitor,
    getLocalPlayerId: () => string | null,
    getCamera: () => CameraState,
    getHoveredEntityId: () => string | null,
  ) {
    this.inputManager = inputManager;
    this.performanceMonitor = performanceMonitor;
    this.getLocalPlayerId = getLocalPlayerId;
    this.getCamera = getCamera;
    this.getHoveredEntityId = getHoveredEntityId;

    // Initialize UI components
    this.statsPanel = new StatsPanel({
      position: "left-center",
      visible: true,
    });
    container.addChild(this.statsPanel);

    this.playerBars = new PlayerBars({
      position: "top-center",
      visible: true,
    });
    container.addChild(this.playerBars);

    this.targetBars = new TargetBars({
      visible: true,
    });
    container.addChild(this.targetBars);

    this.inputDisplay = new InputDisplay({
      position: "top-right",
      visible: true,
    });
    container.addChild(this.inputDisplay);

    this.performanceDisplay = new PerformanceDisplay({
      position: "top-left",
      visible: true,
    });
    container.addChild(this.performanceDisplay);

    this.shipStatsDisplay = new ShipStatsDisplay();
    container.addChild(this.shipStatsDisplay);

    this.chatBox = new ChatBox({
      width: 400,
      height: 250,
      visible: true,
    });
    container.addChild(this.chatBox);

    this.pauseMenu = new PauseMenu({
      onContinue: () => this.resumeGame(),
      onSettings: () => this.openSettings(),
      onHelp: () => console.log("Help not implemented yet"),
      onQuit: () => console.log("Quit not implemented yet"),
    });
    container.addChild(this.pauseMenu);

    this.settingsPage = new SettingsPage({
      onClose: () => this.closeSettings(),
    });
    container.addChild(this.settingsPage);

    this.sectorMap = new SectorMap({
      mapWidth: 32000,
      mapHeight: 32000,
      displaySize: 300,
      visible: false,
    });
    container.addChild(this.sectorMap);
  }

  /**
   * Set pause/resume callbacks.
   */
  setPauseCallbacks(callbacks: { onPause?: () => void; onResume?: () => void }): void {
    this.onPause = callbacks.onPause;
    this.onResume = callbacks.onResume;
  }

  /**
   * Update all UI components with latest game state.
   * Should be called every frame.
   */
  updateUI(): void {
    const localPlayerId = this.getLocalPlayerId();
    if (!localPlayerId) return;

    const entity = World.getEntity(localPlayerId);
    if (!entity) return;

    const inputState = this.inputManager.getInputState();

    const networkComponent = entity.get(NetworkComponent);
    const lastServerTick = networkComponent?.lastServerTick;
    const clientTick = Number(World.currentTick);

    this.statsPanel.updateFromEntity(entity, inputState);
    this.inputDisplay.updateFromInput(inputState);
    this.playerBars.updateFromEntity(entity);
    this.shipStatsDisplay.updateFromEntity(entity, inputState);

    const camera = this.getCamera();
    const hoveredEntityId = this.getHoveredEntityId();
    this.targetBars.updateFromWorld(camera, localPlayerId, entity ? 300 : undefined, hoveredEntityId);

    this.performanceDisplay.updatePerformance(this.performanceMonitor.getFPS(), clientTick, lastServerTick, this.performanceMonitor.getLastDeltaMs());

    // Update sector map with player position
    const physics = entity.get(Box2DBodyComponent);
    if (physics) {
      this.sectorMap.updatePlayerPosition(physics.position.x, physics.position.y);
    }
  }

  /**
   * Toggle pause menu visibility.
   */
  togglePauseMenu(isPaused: boolean): void {
    if (isPaused) {
      this.resumeGame();
    } else {
      this.pauseGame();
    }
  }

  /**
   * Pause the game and show pause menu.
   */
  private pauseGame(): void {
    this.pauseMenu.show();
    this.onPause?.();
  }

  /**
   * Resume the game and hide pause menu.
   */
  private resumeGame(): void {
    this.pauseMenu.hide();
    this.onResume?.();
  }

  /**
   * Open settings page.
   */
  private openSettings(): void {
    this.pauseMenu.hide();
    this.settingsPage.show();
  }

  /**
   * Close settings page.
   */
  private closeSettings(): void {
    this.settingsPage.hide();
    this.pauseMenu.show();
  }

  /**
   * Toggle sector map visibility.
   */
  toggleSectorMap(): void {
    this.sectorMap.toggle();
  }

  /**
   * Handle zoom input.
   */
  handleZoom(_delta: number): void {
    // Zoom is handled by input manager callback, this is a placeholder
    // The actual zoom logic may be in RenderSystem or Camera
  }

  /**
   * Get chat box reference for external interaction.
   */
  getChatBox(): ChatBox {
    return this.chatBox;
  }

  /**
   * Get stats panel reference.
   */
  getStatsPanel(): StatsPanel {
    return this.statsPanel;
  }

  /**
   * Get input display reference.
   */
  getInputDisplay(): InputDisplay {
    return this.inputDisplay;
  }

  /**
   * Get player bars reference.
   */
  getPlayerBars(): PlayerBars {
    return this.playerBars;
  }

  /**
   * Get sector map reference.
   */
  getSectorMap(): SectorMap {
    return this.sectorMap;
  }

  /**
   * Resize all UI components.
   */
  resize(width: number, height: number): void {
    this.statsPanel?.resize(width, height);
    this.playerBars?.resize(width, height);
    this.inputDisplay?.resize(width, height);
    this.targetBars?.resize(width, height);
    this.performanceDisplay?.resize(width, height);
    this.shipStatsDisplay?.resize(width, height);
    this.chatBox?.resize(width, height);
    this.pauseMenu?.resize(width, height);
    this.settingsPage?.resize(width, height);
    this.sectorMap?.resize(width, height);
  }
}
