import { Container, Graphics, Text, TextStyle } from "pixi.js";
import type { Entity } from "../../ecs/core/Entity";
import { BatteryComponent } from "../../ecs/components/BatteryComponent";
import { HealthComponent } from "../../ecs/components/HealthComponent";
import { EnergyComponent } from "../../ecs/components/EnergyComponent";
import { ShieldComponent } from "../../ecs/components/ShieldComponent";
import { PhysicsComponent } from "../../ecs/components/PhysicsComponent";
import type { InputState } from "../../ecs/systems/InputSystem";

export interface StatsPanelConfig {
  position?:
    | "top-left"
    | "top-right"
    | "bottom-left"
    | "bottom-right"
    | "right-center"
    | "left-center";
  visible?: boolean;
}

export class StatsPanel extends Container {
  private background!: Graphics;
  private titleText!: Text;
  private contentText!: Text;
  private config: StatsPanelConfig;
  private visible_: boolean;

  private readonly textStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 14,
    fill: "#ffffff",
    align: "left",
  });

  private readonly titleStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 16,
    fill: "#ffffff",
    fontWeight: "bold",
    align: "left",
  });

  constructor(config: StatsPanelConfig = {}) {
    super();

    this.config = config;
    this.visible_ = config.visible !== false;

    this.createBackground();
    this.createTexts();
    this.applyPosition();
    this.visible = this.visible_;
  }

  private createBackground(): void {
    this.background = new Graphics();
    this.background.roundRect(0, 0, 320, 750, 4);
    this.background.fill({ color: 0x000000, alpha: 0.8 });
    this.background.stroke({ color: 0x444444, width: 1 });
    this.addChild(this.background);
  }

  private createTexts(): void {
    this.titleText = new Text({
      text: "PLAYER STATISTICS",
      style: this.titleStyle,
    });
    this.titleText.position.set(10, 8);
    this.addChild(this.titleText);

    this.contentText = new Text({ text: "", style: this.textStyle });
    this.contentText.position.set(10, 30);
    this.addChild(this.contentText);
  }

  private applyPosition(): void {}

  public updateFromEntity(
    entity: Entity,
    inputState: InputState,
    fps?: number,
    currentTick?: number,
    lastServerTick?: number,
  ): void {
    if (!this.visible_) return;

    const battery = entity.get(BatteryComponent);
    const health = entity.get(HealthComponent);
    const energy = entity.get(EnergyComponent);
    const shield = entity.get(ShieldComponent);
    const physics = entity.get(PhysicsComponent);

    let content = "";

    content += "━━━ PERFORMANCE ━━━\n";
    content += `FPS: ${fps !== undefined ? fps.toString() : "N/A"}\n`;
    content += `Client Tick: ${currentTick !== undefined ? currentTick.toString() : "N/A"}\n`;
    content += `Server Tick: ${lastServerTick !== undefined ? lastServerTick.toString() : "N/A"}\n`;
    if (currentTick !== undefined && lastServerTick !== undefined) {
      const tickDiff = currentTick - lastServerTick;
      content += `Tick Diff: ${tickDiff}\n`;
    } else {
      content += `Tick Diff: N/A\n`;
    }

    content += "━━━ STORAGE ━━━\n";
    if (
      battery &&
      battery.currentCharge !== undefined &&
      battery.capacity !== undefined
    ) {
      const chargePercent = (
        (battery.currentCharge / battery.capacity) *
        100
      ).toFixed(1);
      content += `Battery: ${battery.currentCharge.toFixed(1)}/${battery.capacity.toFixed(1)} kWh (${chargePercent}%)\n`;
      content += `Charge Rate: ${(battery.chargeRate || 0).toFixed(1)} kW\n`;
      content += `Discharge Rate: ${(battery.dischargeRate || 0).toFixed(1)} kW\n`;
    } else {
      content += "Battery: No Data\n";
      content += "Charge Rate: No Data\n";
      content += "Discharge Rate: No Data\n";
    }
    content += "\n";

    content += "━━━ HEALTH ━━━\n";
    if (health && health.current !== undefined && health.max !== undefined) {
      const healthPercent = ((health.current / health.max) * 100).toFixed(1);
      content += `Hull: ${health.current.toFixed(1)}/${health.max.toFixed(1)} HP (${healthPercent}%)\n`;
      content += `Regen Rate: ${(health.regenRate || 0).toFixed(1)} HP/s\n`;
      content += `Status: ${health.isDead ? "DESTROYED" : "OPERATIONAL"}\n`;
    } else {
      content += "Hull: No Data\n";
      content += "Regen Rate: No Data\n";
      content += "Status: No Data\n";
    }
    content += "\n";

    content += "━━━ ENERGY ━━━\n";
    if (
      energy &&
      energy.availableCharge !== undefined &&
      energy.batteryCapacity !== undefined
    ) {
      const energyPercent = (
        (energy.availableCharge / energy.batteryCapacity) *
        100
      ).toFixed(1);
      content += `Energy: ${energy.availableCharge.toFixed(1)}/${energy.batteryCapacity.toFixed(1)} EU (${energyPercent}%)\n`;
      content += `Charge Rate: ${(energy.chargeRate || 0).toFixed(1)} EU/s\n`;
      content += `Discharge Rate: ${(energy.dischargeRate || 0).toFixed(1)} EU/s\n`;
      content += `Status: ${(energy.chargeRate || 0) > (energy.dischargeRate || 0) ? "CHARGING" : "DISCHARGING"}\n`;
    } else {
      content += "Energy: No Data\n";
      content += "Charge Rate: No Data\n";
      content += "Discharge Rate: No Data\n";
      content += "Status: No Data\n";
    }
    content += "\n";

    content += "━━━ SHIELD ━━━\n";
    if (
      shield &&
      shield.charge !== undefined &&
      shield.maxCharge !== undefined
    ) {
      const shieldPercent = ((shield.charge / shield.maxCharge) * 100).toFixed(
        1,
      );
      content += `Shield: ${shield.charge.toFixed(1)}/${shield.maxCharge.toFixed(1)} SP (${shieldPercent}%)\n`;
      content += `Recharge Rate: ${(shield.rechargeRate || 0).toFixed(1)} SP/s\n`;
      content += `Power Use: ${(shield.powerUse || 0).toFixed(1)} kW\n`;
      content += `Radius: ${(shield.radius || 0).toFixed(1)} m\n`;
      content += `Status: ${shield.powerOn ? "ACTIVE" : "INACTIVE"}\n`;
    } else {
      content += "Shield: No Data\n";
      content += "Recharge Rate: No Data\n";
      content += "Power Use: No Data\n";
      content += "Radius: No Data\n";
      content += "Status: No Data\n";
    }
    content += "\n";

    content += "━━━ ENGINE ━━━\n";
    if (physics) {
      const speed = physics.getSpeed();
      const throttlePercent = inputState.thrust
        ? 100
        : inputState.invThrust
          ? 50
          : 0;
      const powerDraw = throttlePercent > 0 ? 50.0 : 0;

      content += `Speed: ${speed?.toFixed(1) || "0.0"} m/s\n`;
      content += `Throttle: ${throttlePercent}%\n`;
      content += `Power Draw: ${powerDraw?.toFixed(1) || "0.0"} kW\n`;
      content += `RCS: ${inputState.rcs ? "ACTIVE" : "INACTIVE"}\n`;
      content += `Position: (${physics.position?.x?.toFixed(1) || "0.0"}, ${physics.position?.y?.toFixed(1) || "0.0"})\n`;
      content += `Velocity: (${physics.linearVelocity?.x?.toFixed(1) || "0.0"}, ${physics.linearVelocity?.y?.toFixed(1) || "0.0"})\n`;
      content += `Rotation: ${physics.rotationRadians ? ((physics.rotationRadians * 180) / Math.PI).toFixed(1) : "0.0"}°\n`;
    } else {
      content += "Speed: No Data\n";
      content += "Throttle: No Data\n";
      content += "Power Draw: No Data\n";
      content += "RCS: No Data\n";
      content += "Position: No Data\n";
      content += "Velocity: No Data\n";
      content += "Rotation: No Data\n";
    }

    content += "\n━━━ POWER DRAW ━━━\n";
    if (battery) {
      content += `Engine: ${(battery.enginePowerDraw || 0).toFixed(1)} kW\n`;
      content += `Shield: ${(battery.shieldPowerDraw || 0).toFixed(1)} kW\n`;
      content += `Weapons: ${(battery.weaponPowerDraw || 0).toFixed(1)} kW\n`;
      const totalDraw =
        (battery.enginePowerDraw || 0) +
        (battery.shieldPowerDraw || 0) +
        (battery.weaponPowerDraw || 0);
      content += `Total: ${totalDraw.toFixed(1)} kW\n`;
    } else {
      content += "Engine: No Data\n";
      content += "Shield: No Data\n";
      content += "Weapons: No Data\n";
      content += "Total: No Data\n";
    }

    this.contentText.text = content;
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
    const position = this.config.position || "bottom-left";
    const margin = 20;

    switch (position) {
      case "top-left":
        this.position.set(margin, margin);
        break;
      case "top-right":
        this.position.set(screenWidth - this.background.width - margin, margin);
        break;
      case "right-center":
        this.position.set(
          screenWidth - this.background.width - margin,
          screenHeight / 2 - this.background.height / 2,
        );
        break;
      case "left-center":
        this.position.set(
          margin,
          screenHeight / 2 - this.background.height / 2,
        );
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
