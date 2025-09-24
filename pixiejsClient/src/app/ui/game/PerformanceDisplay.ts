import { Container, Graphics, Text, TextStyle } from "pixi.js";

export interface PerformanceDisplayConfig {
  position?: "top-left" | "top-right";
  visible?: boolean;
}

export class PerformanceDisplay extends Container {
  private background!: Graphics;
  private text!: Text;
  private config: PerformanceDisplayConfig;
  private visible_: boolean;

  private readonly textStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 12,
    fill: "#ffffff",
    align: "left",
  });

  constructor(config: PerformanceDisplayConfig = {}) {
    super();

    this.config = config;
    this.visible_ = config.visible !== false;

    this.createBackground();
    this.createText();
    this.applyPosition();
    this.visible = this.visible_;
  }

  private createBackground(): void {
    this.background = new Graphics();
    this.background.roundRect(0, 0, 180, 80, 4);
    this.background.fill({ color: 0x000000, alpha: 0.7 });
    this.background.stroke({ color: 0x333333, width: 1 });
    this.addChild(this.background);
  }

  private createText(): void {
    this.text = new Text({
      text: "FPS: --\nClient: --\nServer: --\nDelta: --",
      style: this.textStyle,
    });
    this.text.position.set(8, 8);
    this.addChild(this.text);
  }

  private applyPosition(): void {}

  public updatePerformance(
    fps: number,
    clientTick: number,
    serverTick: number | undefined,
    deltaMs: number,
  ): void {
    if (!this.visible_) return;

    const serverTickText =
      serverTick !== undefined ? serverTick.toString() : "--";

    this.text.text = `FPS: ${fps}\nClient: ${clientTick}\nServer: ${serverTickText}\nDelta: ${deltaMs.toFixed(1)}ms`;
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

  public resize(screenWidth: number, _screenHeight: number): void {
    const position = this.config.position || "top-left";
    const margin = 10;

    switch (position) {
      case "top-left":
        this.position.set(margin, margin);
        break;
      case "top-right":
        this.position.set(screenWidth - this.background.width - margin, margin);
        break;
    }
  }
}
