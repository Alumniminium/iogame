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
    this.background.roundRect(0, 0, 220, 320, 6).fill({ color: 0x000000, alpha: 0.8 }).stroke({ width: 1, color: 0x555555 });
    this.addChild(this.background);
  }

  private createTexts(): void {
    this.titleText = new Text({ text: "INPUT STATE", style: this.titleStyle });
    this.titleText.position.set(10, 8);
    this.addChild(this.titleText);

    this.inputText = new Text({ text: "", style: this.textStyle });
    this.inputText.position.set(10, 30);
    this.addChild(this.inputText);

    this.mouseText = new Text({ text: "", style: this.mouseStyle });
    this.mouseText.position.set(10, 200);
    this.addChild(this.mouseText);

    this.statsText = new Text({ text: "", style: this.textStyle });
    this.statsText.position.set(10, 240);
    this.addChild(this.statsText);
  }

  public updateFromInput(inputState: InputState, entityStats?: EntityStats): void {
    if (!this.visible_) return;

    let inputContent = "";

    Object.entries(this.keyLabels).forEach(([key, label]) => {
      const isActive = inputState[key as keyof InputState] as boolean;
      const indicator = isActive ? "●" : "○";

      inputContent += `${indicator} ${label}\n`;
    });

    this.inputText.text = inputContent;

    let mouseContent = "";
    mouseContent += `Mouse: (${inputState.mouseX}, ${inputState.mouseY})\n`;
    mouseContent += `Move: (${inputState.moveX.toFixed(2)}, ${inputState.moveY.toFixed(2)})`;

    this.mouseText.text = mouseContent;

    if (entityStats) {
      let statsContent = "\n━━━ ENTITY STATS ━━━\n";

      if (entityStats.health) {
        const healthPercent = (entityStats.health.current / entityStats.health.max) * 100;
        statsContent += `Health: ${Math.round(entityStats.health.current)}/${Math.round(entityStats.health.max)} (${Math.round(healthPercent)}%)\n`;
      }

      if (entityStats.energy) {
        const energyPercent = (entityStats.energy.current / entityStats.energy.max) * 100;
        statsContent += `Energy: ${Math.round(entityStats.energy.current)}/${Math.round(entityStats.energy.max)} (${Math.round(energyPercent)}%)\n`;
      }

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

    this.updateInputColors(inputState);
  }

  private updateInputColors(inputState: InputState): void {
    const activeCount = Object.values(this.keyLabels).filter((_, index) => {
      const key = Object.keys(this.keyLabels)[index] as keyof typeof this.keyLabels;
      return inputState[key] as boolean;
    }).length;

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
        this.position.set(margin, screenHeight - this.background.height - margin);
        break;
      case "bottom-right":
        this.position.set(screenWidth - this.background.width - margin, screenHeight - this.background.height - margin);
        break;
    }
  }
}
