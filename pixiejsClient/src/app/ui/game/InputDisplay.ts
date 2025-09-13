import { Container, Graphics, Text, TextStyle } from "pixi.js";
import type { InputState } from "../../ecs/systems/InputSystem";

export interface EntityStats {
  health?: { current: number; max: number };
  energy?: { current: number; max: number };
  engine?: { throttle: number; powerDraw: number; rcsActive: boolean };
}

export interface InputDisplayConfig {
  position?: "top-left" | "top-right" | "bottom-left" | "bottom-right";
  visible?: boolean;
}

export class InputDisplay extends Container {
  private background!: Graphics;
  private titleText!: Text;
  private inputText!: Text;
  private mouseText!: Text;
  private statsText!: Text;
  private config: InputDisplayConfig;
  private visible_: boolean;

  // Key binding labels for display
  private keyLabels = {
    thrust: "W/↑ Thrust",
    invThrust: "S/↓ Reverse",
    left: "A/← Left",
    right: "D/→ Right",
    boost: "Shift Boost",
    rcs: "R RCS (Toggle)",
    fire: "Click Fire",
    drop: "Q/E Drop",
    shield: "Space Shield (Toggle)",
  };

  private readonly titleStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 13,
    fill: "#ffffff",
    fontWeight: "bold",
  });

  private readonly textStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 12,
    fill: "#ffffff",
    lineHeight: 16,
  });

  // Could add inactive style for future use

  private readonly mouseStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 11,
    fill: "#cccccc",
    lineHeight: 14,
  });

  constructor(config: InputDisplayConfig = {}) {
    super();

    this.config = config;
    this.visible_ = config.visible !== false;

    this.createBackground();
    this.createTexts();
    this.visible = this.visible_;
  }

  private createBackground(): void {
    this.background = new Graphics();
    this.background.beginFill(0x000000, 0.8);
    this.background.lineStyle(1, 0x555555);
    this.background.drawRoundedRect(0, 0, 220, 320, 6);
    this.background.endFill();
    this.addChild(this.background);
  }

  private createTexts(): void {
    this.titleText = new Text("INPUT STATE", this.titleStyle);
    this.titleText.position.set(10, 8);
    this.addChild(this.titleText);

    this.inputText = new Text("", this.textStyle);
    this.inputText.position.set(10, 30);
    this.addChild(this.inputText);

    this.mouseText = new Text("", this.mouseStyle);
    this.mouseText.position.set(10, 200);
    this.addChild(this.mouseText);

    this.statsText = new Text("", this.textStyle);
    this.statsText.position.set(10, 240);
    this.addChild(this.statsText);
  }

  public updateFromInput(
    inputState: InputState,
    entityStats?: EntityStats,
  ): void {
    if (!this.visible_) return;

    // Build input display
    let inputContent = "";

    Object.entries(this.keyLabels).forEach(([key, label]) => {
      const isActive = inputState[key as keyof InputState] as boolean;
      const indicator = isActive ? "●" : "○";
      // Could use color for future styling enhancements

      // For PixiJS Text, we can't use different colors within the same text easily
      // So we'll use different indicators instead
      inputContent += `${indicator} ${label}\n`;
    });

    this.inputText.text = inputContent;

    // Update mouse info
    let mouseContent = "";
    mouseContent += `Mouse: (${inputState.mouseX}, ${inputState.mouseY})\n`;
    mouseContent += `Move: (${inputState.moveX.toFixed(2)}, ${inputState.moveY.toFixed(2)})`;

    this.mouseText.text = mouseContent;

    // Update entity stats if provided
    if (entityStats) {
      let statsContent = "\n━━━ ENTITY STATS ━━━\n";

      // Health bar
      if (entityStats.health) {
        const healthPercent =
          (entityStats.health.current / entityStats.health.max) * 100;
        statsContent += `Health: ${Math.round(entityStats.health.current)}/${Math.round(entityStats.health.max)} (${Math.round(healthPercent)}%)\n`;
      }

      // Energy bar
      if (entityStats.energy) {
        const energyPercent =
          (entityStats.energy.current / entityStats.energy.max) * 100;
        statsContent += `Energy: ${Math.round(entityStats.energy.current)}/${Math.round(entityStats.energy.max)} (${Math.round(energyPercent)}%)\n`;
      }

      // Engine stats
      if (entityStats.engine) {
        statsContent += `\nThrottle: ${entityStats.engine.throttle}%\n`;
        statsContent += `Power: ${entityStats.engine.powerDraw.toFixed(1)}kW\n`;

        if (entityStats.engine.rcsActive) {
          statsContent += `RCS ACTIVE\n`;
        }

        if (inputState.shield) {
          statsContent += `SHIELD ACTIVE\n`;
        }
      }

      this.statsText.text = statsContent;
      this.statsText.visible = true;
    } else {
      this.statsText.visible = false;
    }

    // Color the input text based on active state
    // Since we can't easily color individual lines, we'll use a different approach
    // Split by lines and color each based on activity
    this.updateInputColors(inputState);
  }

  private updateInputColors(inputState: InputState): void {
    // For now, we'll just use the single text color
    // In a more advanced implementation, we could create separate Text objects for each line
    // and color them individually

    const activeCount = Object.values(this.keyLabels).filter((_, index) => {
      const key = Object.keys(this.keyLabels)[
        index
      ] as keyof typeof this.keyLabels;
      return inputState[key] as boolean;
    }).length;

    // Change text color based on activity
    if (activeCount > 0) {
      this.inputText.style.fill = "#ffffff";
    } else {
      this.inputText.style.fill = "#cccccc";
    }
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
    const position = this.config.position || "top-right";
    const margin = 20;

    switch (position) {
      case "top-left":
        this.position.set(margin, margin);
        break;
      case "top-right":
        this.position.set(screenWidth - this.background.width - margin, margin);
        break;
      case "bottom-left":
        this.position.set(
          margin,
          screenHeight - this.background.height - margin,
        );
        break;
      case "bottom-right":
        this.position.set(
          screenWidth - this.background.width - margin,
          screenHeight - this.background.height - margin,
        );
        break;
    }
  }
}
