import { Container, Graphics, Text, TextStyle } from "pixi.js";
import type { Entity } from "../../ecs/core/Entity";
import { EnergyComponent } from "../../ecs/components/EnergyComponent";
import { Box2DBodyComponent } from "../../ecs/components/Box2DBodyComponent";
import type { InputState } from "../../ecs/systems/InputSystem";

export class ShipStatsDisplay extends Container {
  private background!: Graphics;
  private titleText!: Text;
  private statsText!: Text;
  private visible_ = true;

  private readonly titleStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 13,
    fill: "#ffffff",
    fontWeight: "bold",
    align: "left",
  });

  private readonly statsStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 12,
    fill: "#ffffff",
    align: "left",
  });

  constructor() {
    super();
    this.createBackground();
    this.createText();
  }

  private createBackground(): void {
    this.background = new Graphics();
    this.background.roundRect(0, 0, 200, 105, 4);
    this.background.fill({ color: 0x000000, alpha: 0.8 });
    this.background.stroke({ color: 0x444444, width: 1 });
    this.addChild(this.background);
  }

  private createText(): void {
    this.titleText = new Text({ text: "FLIGHT DATA", style: this.titleStyle });
    this.titleText.position.set(8, 6);
    this.addChild(this.titleText);

    this.statsText = new Text({ text: "", style: this.statsStyle });
    this.statsText.position.set(8, 28);
    this.addChild(this.statsText);
  }

  public updateFromEntity(entity: Entity, inputState: InputState): void {
    if (!this.visible_) return;

    const energy = entity.get(EnergyComponent);
    const physics = entity.get(Box2DBodyComponent);

    let content = "";

    if (physics) {
      const speed = physics.getSpeed();
      const throttlePercent = inputState.thrust ? 100 : inputState.invThrust ? 50 : 0;

      content += `Speed: ${speed.toFixed(1)} m/s\n`;
      content += `Throttle: ${throttlePercent}%\n`;
      content += `RCS: ${inputState.rcs ? "ON" : "OFF"}\n`;
    }

    if (energy) {
      content += `Power: ${energy.dischargeRate.toFixed(1)} kW\n`;
      const chargePercent = ((energy.availableCharge / energy.batteryCapacity) * 100).toFixed(1);
      content += `Battery: ${chargePercent}%`;
    }

    this.statsText.text = content;
  }

  public toggle(): void {
    this.visible_ = !this.visible_;
    this.visible = this.visible_;
  }

  public setVisible(visible: boolean): void {
    this.visible_ = visible;
    this.visible = visible;
  }

  public resize(_screenWidth: number, screenHeight: number): void {
    this.position.set(20, screenHeight - this.background.height - 20);
  }
}
