import { Container, Graphics, Text, TextStyle } from "pixi.js";
import type { NTT } from "../../ecs/core/NTT";
import { HealthComponent } from "../../ecs/components/HealthComponent";
import { EnergyComponent } from "../../ecs/components/EnergyComponent";
import { ShieldComponent } from "../../ecs/components/ShieldComponent";

export interface BarData {
  current: number;
  max: number;
  label: string;
}

export interface PlayerBarsConfig {
  position?: "top-left" | "top-right" | "bottom-left" | "bottom-right" | "top-center";
  visible?: boolean;
}

export class PlayerBars extends Container {
  private background!: Graphics;
  private titleText!: Text;
  private healthBar!: BarDisplay;
  private energyBar!: BarDisplay;
  private shieldBar!: BarDisplay;
  private config: PlayerBarsConfig;
  private visible_: boolean;

  private readonly titleStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 13,
    fill: "#ffffff",
    fontWeight: "bold",
  });

  constructor(config: PlayerBarsConfig = {}) {
    super();

    this.config = config;
    this.visible_ = config.visible !== false;

    this.createBackground();
    this.createTitle();
    this.createBars();
    this.visible = this.visible_;
  }

  private createBackground(): void {
    this.background = new Graphics();
    this.background.roundRect(0, 0, 260, 120, 4);
    this.background.fill({ color: 0x000000, alpha: 0.7 });
    this.background.stroke({ color: 0x444444, width: 1 });
    this.addChild(this.background);
  }

  private createTitle(): void {
    this.titleText = new Text({
      text: "PLAYER STATUS",
      style: this.titleStyle,
    });
    this.titleText.position.set(10, 8);
    this.addChild(this.titleText);
  }

  private createBars(): void {
    this.healthBar = new BarDisplay("Health:", 0xff4444, 0xcc3333);
    this.healthBar.position.set(10, 30);
    this.addChild(this.healthBar);

    this.energyBar = new BarDisplay("Energy:", 0x44ff44, 0x33cc33);
    this.energyBar.position.set(10, 55);
    this.addChild(this.energyBar);

    this.shieldBar = new BarDisplay("Shield:", 0x4444ff, 0x3333cc);
    this.shieldBar.position.set(10, 80);
    this.addChild(this.shieldBar);
  }

  public updateFromEntity(entity: NTT): void {
    if (!this.visible_) return;
    const health = entity.get(HealthComponent);
    const energy = entity.get(EnergyComponent);
    const shield = entity.get(ShieldComponent);

    if (health) {
      this.healthBar.updateBar({
        current: health.current,
        max: health.max,
        label: "Health",
      });
      this.healthBar.visible = true;
    } else {
      this.healthBar.visible = false;
    }

    if (energy) {
      this.energyBar.updateBar({
        current: energy.availableCharge,
        max: energy.batteryCapacity,
        label: "Energy",
      });
      this.energyBar.visible = true;
    } else {
      this.energyBar.visible = false;
    }

    if (shield) {
      this.shieldBar.updateBar({
        current: shield.charge,
        max: shield.maxCharge,
        label: "Shield",
      });
      this.shieldBar.visible = true;
    } else {
      this.shieldBar.visible = false;
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
    const position = this.config.position || "top-left";
    const margin = 20;

    switch (position) {
      case "top-left":
        this.position.set(margin, margin);
        break;
      case "top-right":
        this.position.set(screenWidth - this.background.width - margin, margin);
        break;
      case "top-center":
        this.position.set(screenWidth / 2 - this.background.width / 2, margin);
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

class BarDisplay extends Container {
  private labelText!: Text;
  private barBackground!: Graphics;
  private barFill!: Graphics;
  private barText!: Text;
  private valueText!: Text;

  private readonly labelStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 11,
    fill: "#cccccc",
  });

  private readonly barTextStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 11,
    fill: "#ffffff",
    fontWeight: "bold",
  });

  private readonly valueStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 11,
    fill: "#ffffff",
  });

  constructor(
    labelText: string,
    private fillColor: number,
    _fillColorDark: number,
  ) {
    super();

    this.createLabel(labelText);
    this.createBar();
    this.createTexts();
  }

  private createLabel(labelText: string): void {
    this.labelText = new Text({ text: labelText, style: this.labelStyle });
    this.labelText.position.set(0, 0);
    this.addChild(this.labelText);
  }

  private createBar(): void {
    this.barBackground = new Graphics();
    this.barBackground.roundRect(0, 0, 160, 18, 3);
    this.barBackground.fill(0x222222);
    this.barBackground.stroke({ color: 0x555555, width: 1 });
    this.barBackground.position.set(60, -2);
    this.addChild(this.barBackground);

    this.barFill = new Graphics();
    this.barFill.position.set(62, 0);
    this.addChild(this.barFill);

    this.barText = new Text({ text: "", style: this.barTextStyle });
    this.barText.anchor.set(0.5, 0.5);
    this.barText.position.set(140, 7); // Center of bar
    this.addChild(this.barText);
  }

  private createTexts(): void {
    this.valueText = new Text({ text: "", style: this.valueStyle });
    this.valueText.position.set(225, 0);
    this.addChild(this.valueText);
  }

  public updateBar(data: BarData): void {
    const percent = Math.min(100, (data.current / data.max) * 100);
    const barWidth = 156; // Bar width minus padding

    this.barFill.clear();
    this.barFill.roundRect(0, 0, (barWidth * percent) / 100, 16, 2);
    this.barFill.fill(this.fillColor);

    this.barText.text = `${Math.round(data.current)} / ${Math.round(data.max)}`;
    this.valueText.text = `${Math.round(percent)}%`;
  }
}
