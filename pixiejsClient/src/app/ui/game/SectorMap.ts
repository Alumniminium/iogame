import { Container, Graphics, Text, TextStyle } from "pixi.js";

export interface SectorMapConfig {
  mapWidth: number;
  mapHeight: number;
  displaySize?: number;
  visible?: boolean;
}

/**
 * Minimap display showing the entire world with player position indicator
 */
export class SectorMap extends Container {
  private background!: Graphics;
  private mapGraphics!: Graphics;
  private playerDot!: Graphics;
  private titleText!: Text;
  private coordsText!: Text;
  private config: SectorMapConfig;
  private visible_: boolean;

  private readonly titleStyle = new TextStyle({
    fontFamily: "Arial, sans-serif",
    fontSize: 14,
    fontWeight: "bold",
    fill: "#ffffff",
    align: "center",
  });

  private readonly coordsStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 11,
    fill: "#00ff00",
    align: "center",
  });

  private displaySize: number;
  private mapScale: number;

  constructor(config: SectorMapConfig) {
    super();

    this.config = config;
    this.visible_ = config.visible !== false;
    this.displaySize = config.displaySize || 300;

    // Calculate scale to fit map into display size
    const maxDimension = Math.max(config.mapWidth, config.mapHeight);
    this.mapScale = this.displaySize / maxDimension;

    this.createBackground();
    this.createMapGraphics();
    this.createPlayerDot();
    this.createTitle();
    this.createCoordsText();

    this.visible = this.visible_;
  }

  private createBackground(): void {
    const padding = 30;
    const width = this.displaySize + padding * 2;
    const height = this.displaySize + padding * 2 + 40;

    this.background = new Graphics();
    this.background.roundRect(0, 0, width, height, 8);
    this.background.fill({ color: 0x000000, alpha: 0.8 });
    this.background.stroke({ color: 0x00ff00, width: 2 });
    this.addChild(this.background);
  }

  private createMapGraphics(): void {
    this.mapGraphics = new Graphics();

    const padding = 30;
    const mapWidth = this.config.mapWidth * this.mapScale;
    const mapHeight = this.config.mapHeight * this.mapScale;

    // Draw map border
    this.mapGraphics.rect(padding, padding + 20, mapWidth, mapHeight);
    this.mapGraphics.fill({ color: 0x001100, alpha: 0.5 });
    this.mapGraphics.stroke({ color: 0x00ff00, width: 1 });

    // Draw grid lines
    const gridSize = 4000; // Draw a line every 4000 units
    const gridScale = gridSize * this.mapScale;

    for (let x = 0; x <= this.config.mapWidth; x += gridSize) {
      const screenX = padding + x * this.mapScale;
      this.mapGraphics.moveTo(screenX, padding + 20);
      this.mapGraphics.lineTo(screenX, padding + 20 + mapHeight);
      this.mapGraphics.stroke({ color: 0x003300, width: 1, alpha: 0.3 });
    }

    for (let y = 0; y <= this.config.mapHeight; y += gridSize) {
      const screenY = padding + 20 + y * this.mapScale;
      this.mapGraphics.moveTo(padding, screenY);
      this.mapGraphics.lineTo(padding + mapWidth, screenY);
      this.mapGraphics.stroke({ color: 0x003300, width: 1, alpha: 0.3 });
    }

    this.addChild(this.mapGraphics);
  }

  private createPlayerDot(): void {
    this.playerDot = new Graphics();
    this.playerDot.circle(0, 0, 4);
    this.playerDot.fill({ color: 0xff0000 });
    this.playerDot.stroke({ color: 0xffffff, width: 1 });
    this.addChild(this.playerDot);
  }

  private createTitle(): void {
    this.titleText = new Text({
      text: "SECTOR MAP",
      style: this.titleStyle,
    });
    this.titleText.anchor.set(0.5, 0);
    const padding = 30;
    this.titleText.position.set(padding + this.displaySize / 2, 5);
    this.addChild(this.titleText);
  }

  private createCoordsText(): void {
    this.coordsText = new Text({
      text: "X: 0 Y: 0",
      style: this.coordsStyle,
    });
    this.coordsText.anchor.set(0.5, 0);
    const padding = 30;
    this.coordsText.position.set(
      padding + this.displaySize / 2,
      padding + this.displaySize + 25,
    );
    this.addChild(this.coordsText);
  }

  public updatePlayerPosition(worldX: number, worldY: number): void {
    if (!this.visible_) return;

    const padding = 30;
    const screenX = padding + worldX * this.mapScale;
    const screenY = padding + 20 + worldY * this.mapScale;

    this.playerDot.position.set(screenX, screenY);

    // Update coordinates text
    this.coordsText.text = `X: ${Math.round(worldX)} Y: ${Math.round(worldY)}`;
  }

  public toggle(): void {
    this.visible_ = !this.visible_;
    this.visible = this.visible_;
  }

  public setVisible(visible: boolean): void {
    this.visible_ = visible;
    this.visible = visible;
  }

  public isVisible(): boolean {
    return this.visible_;
  }

  public resize(screenWidth: number, screenHeight: number): void {
    // Center the map on screen
    const bgWidth = this.background.width;
    const bgHeight = this.background.height;
    this.position.set(
      (screenWidth - bgWidth) / 2,
      (screenHeight - bgHeight) / 2,
    );
  }
}
