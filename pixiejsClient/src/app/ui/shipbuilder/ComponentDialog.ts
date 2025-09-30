import { Container, Graphics, Text } from "pixi.js";
import { Button } from "../Button";

export interface ComponentConfig {
  type: "empty" | "engine" | "shield" | "weapon";
  engineThrust?: number;
  shieldCharge?: number;
  shieldRadius?: number;
  weaponDamage?: number;
  weaponRateOfFire?: number;
}

export class ComponentDialog extends Container {
  private background: Graphics;
  private titleText: Text;
  private emptyButton: Button;
  private engineButton: Button;
  private shieldButton: Button;
  private weaponButton: Button;
  private placeButton: Button;
  private cancelButton: Button;

  private selectedComponent: ComponentConfig | null = null;
  private onConfirm: ((config: ComponentConfig) => void) | null = null;

  constructor() {
    super();

    this.background = new Graphics();
    this.background.rect(0, 0, 400, 350).fill({ color: 0x1a1a1a, alpha: 0.95 });

    this.titleText = new Text({
      text: "Attach Components",
      style: { fill: 0xffffff, fontSize: 18, fontWeight: "bold" },
    });
    this.titleText.x = 20;
    this.titleText.y = 15;

    const buttonY = 60;
    const buttonSpacing = 50;

    this.emptyButton = new Button({
      text: "Empty (Hull Only)",
      width: 360,
      height: 40,
      onPress: () => this.selectComponent({ type: "empty" }),
    });
    this.emptyButton.x = 20;
    this.emptyButton.y = buttonY;

    this.engineButton = new Button({
      text: "Engine (1000N thrust)",
      width: 360,
      height: 40,
      onPress: () =>
        this.selectComponent({
          type: "engine",
          engineThrust: 1000,
        }),
    });
    this.engineButton.x = 20;
    this.engineButton.y = buttonY + buttonSpacing;

    this.shieldButton = new Button({
      text: "Shield (100 charge, 2.0 radius)",
      width: 360,
      height: 40,
      onPress: () =>
        this.selectComponent({
          type: "shield",
          shieldCharge: 100,
          shieldRadius: 2.0,
        }),
    });
    this.shieldButton.x = 20;
    this.shieldButton.y = buttonY + buttonSpacing * 2;

    this.weaponButton = new Button({
      text: "Weapon (10 damage, 5 RPS)",
      width: 360,
      height: 40,
      onPress: () =>
        this.selectComponent({
          type: "weapon",
          weaponDamage: 10,
          weaponRateOfFire: 5,
        }),
    });
    this.weaponButton.x = 20;
    this.weaponButton.y = buttonY + buttonSpacing * 3;

    this.placeButton = new Button({
      text: "Place Block",
      width: 175,
      height: 40,
      onPress: () => this.handleConfirm(),
    });
    this.placeButton.x = 20;
    this.placeButton.y = 280;

    this.cancelButton = new Button({
      text: "Cancel",
      width: 175,
      height: 40,
      onPress: () => this.hide(),
    });
    this.cancelButton.x = 205;
    this.cancelButton.y = 280;

    this.addChild(this.background);
    this.addChild(this.titleText);
    this.addChild(this.emptyButton);
    this.addChild(this.engineButton);
    this.addChild(this.shieldButton);
    this.addChild(this.weaponButton);
    this.addChild(this.placeButton);
    this.addChild(this.cancelButton);

    this.visible = false;
  }

  private selectComponent(config: ComponentConfig): void {
    this.selectedComponent = config;

    this.emptyButton.setPressed(config.type === "empty");
    this.engineButton.setPressed(config.type === "engine");
    this.shieldButton.setPressed(config.type === "shield");
    this.weaponButton.setPressed(config.type === "weapon");
  }

  private handleConfirm(): void {
    if (this.selectedComponent && this.onConfirm) {
      this.onConfirm(this.selectedComponent);
      this.hide();
    }
  }

  setOnConfirm(callback: (config: ComponentConfig) => void): void {
    this.onConfirm = callback;
  }

  show(): void {
    this.selectedComponent = { type: "empty" };
    this.selectComponent(this.selectedComponent);
    this.visible = true;
  }

  hide(): void {
    this.visible = false;
    this.selectedComponent = null;
  }
}
