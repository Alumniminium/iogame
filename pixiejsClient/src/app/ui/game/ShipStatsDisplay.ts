import { Container, Graphics, Text, TextStyle } from "pixi.js";
import type { Entity } from "../../ecs/core/Entity";
import { BatteryComponent } from "../../ecs/components/BatteryComponent";
import { PhysicsComponent } from "../../ecs/components/PhysicsComponent";
import type { InputState } from "../../ecs/systems/InputSystem";

export class ShipStatsDisplay extends Container {
  private background!: Graphics;
  private statsText!: Text;
  private visible_ = true;

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
    this.background.roundRect(0, 0, 200, 80, 4);
    this.background.fill({ color: 0x000000, alpha: 0.8 });
    this.background.stroke({ color: 0x444444, width: 1 });
    this.addChild(this.background);
  }

  private createText(): void {
    this.statsText = new Text({ text: "", style: this.statsStyle });
    this.statsText.position.set(8, 8);
    this.addChild(this.statsText);
  }

  public updateFromEntity(entity: Entity, inputState: InputState): void {
    if (!this.visible_) return;

    const battery = entity.get(BatteryComponent);
    const physics = entity.get(PhysicsComponent);

    let content = "";

    if (physics) {
      const speed = physics.getSpeed();
      const throttlePercent = inputState.thrust
        ? 100
        : inputState.invThrust
          ? 50
          : 0;

      content += `Speed: ${speed.toFixed(1)} m/s\n`;
      content += `Throttle: ${throttlePercent}%\n`;
      content += `RCS: ${inputState.rcs ? "ON" : "OFF"}\n`;
    }

    if (battery) {
      const totalDraw =
        battery.enginePowerDraw +
        battery.shieldPowerDraw +
        battery.weaponPowerDraw;
      content += `Power: ${totalDraw.toFixed(1)} kW\n`;
      const chargePercent = (
        (battery.currentCharge / battery.capacity) *
        100
      ).toFixed(1);
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

  public resize(screenWidth: number, _screenHeight: number): void {
    this.position.set(screenWidth - this.background.width - 20, 110);
  }
}
