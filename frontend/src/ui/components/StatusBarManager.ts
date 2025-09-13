import { StatusBars, StatusBarConfig, StatusBarData, BarData } from './StatusBars';

export interface StatusBarManagerConfig {
  canvas: HTMLCanvasElement;
  playerBarsConfig: {
    position: { x: number; y: number };
    barWidth: number;
    barHeight: number;
    barSpacing: number;
  };
  targetBarsConfig: {
    barWidth: number;
    barHeight: number;
    barSpacing: number;
    scale: number; // Smaller for target bars
  };
}

export class StatusBarManager {
  private canvas: HTMLCanvasElement;
  private config: StatusBarManagerConfig;
  private playerBars: StatusBars;
  private targetBars: Map<number, StatusBars> = new Map();

  constructor(config: StatusBarManagerConfig) {
    this.canvas = config.canvas;
    this.config = config;

    // Create player status bars (fixed position UI)
    this.playerBars = new StatusBars({
      canvas: this.canvas,
      ...this.config.playerBarsConfig,
      title: 'Player',
      scale: 1.0
    });
  }

  renderPlayerBars(health?: BarData, energy?: BarData, shield?: BarData): void {
    this.playerBars.render(health, energy, shield);
  }

  renderTargetBars(targets: StatusBarData[]): void {
    // Clear old target bars that are no longer needed
    const currentTargetIds = new Set(targets.map(t => t.entityId));
    for (const [id, _] of this.targetBars) {
      if (!currentTargetIds.has(id)) {
        this.targetBars.delete(id);
      }
    }

    // Render each target's bars
    targets.forEach(target => {
      let targetBar = this.targetBars.get(target.entityId);

      if (!targetBar) {
        // Create new target bar instance
        targetBar = new StatusBars({
          canvas: this.canvas,
          position: target.position || { x: 0, y: 0 },
          ...this.config.targetBarsConfig,
          entityId: target.entityId,
          title: target.title
        });
        this.targetBars.set(target.entityId, targetBar);
      }

      // Update position if provided
      if (target.position) {
        targetBar.setPosition(target.position.x, target.position.y);
      }

      // Render the target bars
      targetBar.render(target);
    });
  }

  // Utility method to convert world position to screen position
  worldToScreen(worldX: number, worldY: number, camera: { x: number; y: number; zoom: number }): { x: number; y: number } {
    const screenCenterX = this.canvas.width / 2;
    const screenCenterY = this.canvas.height / 2;

    return {
      x: screenCenterX + (worldX - camera.x) * camera.zoom,
      y: screenCenterY + (worldY - camera.y) * camera.zoom
    };
  }

  // Get bars below entity position
  getEntityBarPosition(entityX: number, entityY: number, entitySize: number, camera: { x: number; y: number; zoom: number }): { x: number; y: number } {
    const screenPos = this.worldToScreen(entityX, entityY, camera);

    // Position bars below the entity
    return {
      x: screenPos.x - (this.config.targetBarsConfig.barWidth * this.config.targetBarsConfig.scale) / 2,
      y: screenPos.y + entitySize * camera.zoom + 10 // 10px below entity
    };
  }

  clearTargetBars(): void {
    this.targetBars.clear();
  }

  updateConfig(config: Partial<StatusBarManagerConfig>): void {
    this.config = { ...this.config, ...config };
  }
}