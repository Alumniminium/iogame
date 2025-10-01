import { Container, Graphics } from "pixi.js";
import { BaseRenderer } from "./BaseRenderer";

/**
 * Renders the background grid
 */
export class BackgroundRenderer extends BaseRenderer {
  private backgroundRect: Graphics;
  private backgroundGrid: Graphics;
  private mapWidth = 150;
  private mapHeight = 1000;

  constructor(gameContainer: Container) {
    super(gameContainer);
    this.backgroundRect = new Graphics();
    this.gameContainer.addChildAt(this.backgroundRect, 0);
    this.backgroundGrid = new Graphics();
    this.gameContainer.addChildAt(this.backgroundGrid, 1);
  }

  initialize(): void {
    this.updateGrid();
  }

  setMapSize(width: number, height: number): void {
    this.mapWidth = width;
    this.mapHeight = height;
    this.updateGrid();
  }

  updateGrid(): void {
    this.backgroundGrid.clear();

    const gridSize = 10;

    this.backgroundGrid
      .moveTo(0, 0)
      .lineTo(100, 100)
      .stroke({ width: 5, color: 0xff0000 });

    for (let x = 0; x <= this.mapWidth; x += gridSize) {
      this.backgroundGrid
        .moveTo(x, 0)
        .lineTo(x, this.mapHeight)
        .stroke({ width: 0.1, color: 0xffffff, alpha: 0.8 });
    }

    for (let y = 0; y <= this.mapHeight; y += gridSize) {
      this.backgroundGrid
        .moveTo(0, y)
        .lineTo(this.mapWidth, y)
        .stroke({ width: 0.1, color: 0xffffff, alpha: 0.8 });
    }
  }

  update(_deltaTime: number): void {
    // Background is static, no per-frame updates needed
  }

  cleanup(): void {
    super.cleanup();
    this.backgroundRect.destroy();
    this.backgroundGrid.destroy();
  }
}
