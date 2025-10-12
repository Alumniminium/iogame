import { Container, Graphics, Text, TextStyle } from "pixi.js";
import { World } from "../../ecs/core/World";
import { PhysicsComponent } from "../../ecs/components/PhysicsComponent";
import { HealthComponent } from "../../ecs/components/HealthComponent";
import { EnergyComponent } from "../../ecs/components/EnergyComponent";
import { ShieldComponent } from "../../ecs/components/ShieldComponent";
import type { Camera } from "../../managers/CameraManager";

export interface TargetBarData {
  entityId: string;
  title: string;
  position: { x: number; y: number }; // Screen coordinates
  health?: { current: number; max: number };
  energy?: { current: number; max: number };
  shield?: { current: number; max: number };
}

export interface TargetBarsConfig {
  visible?: boolean;
}

export class TargetBars extends Container {
  private visible_: boolean;
  private targetElements = new Map<string, TargetBarElement>();
  private canvasWidth = 800;
  private canvasHeight = 600;

  constructor(config: TargetBarsConfig = {}) {
    super();
    this.visible_ = config.visible !== false;
    this.visible = this.visible_;
  }

  public updateFromWorld(camera?: Camera, localPlayerId?: string, _viewDistance?: number, hoveredEntityId?: string | null): void {
    if (!this.visible_) {
      this.hideAllTargets();
      return;
    }

    if (!hoveredEntityId) {
      this.hideAllTargets();
      return;
    }

    const entities = World.queryEntitiesWithComponents(PhysicsComponent);
    const targets: TargetBarData[] = [];

    const activeCamera: Camera = camera || { x: 0, y: 0, zoom: 1, rotation: 0 };

    entities.forEach((entity) => {
      if (entity.id !== hoveredEntityId) return;
      if (entity.id === localPlayerId) return;

      const physics = entity.get(PhysicsComponent)!;
      const barPosition = this.getEntityBarPosition(physics.position.x, physics.position.y, physics.size, activeCamera);

      // Early return if out of screen bounds
      if (barPosition.x <= -150 || barPosition.x >= this.canvasWidth + 50) return;
      if (barPosition.y <= -50 || barPosition.y >= this.canvasHeight + 50) return;

      const health = entity.get(HealthComponent);
      const energy = entity.get(EnergyComponent);
      const shield = entity.get(ShieldComponent);

      const targetBarData: TargetBarData = {
        entityId: entity.id,
        position: barPosition,
        title: `Entity ${entity.id}`,
        health: health ? { current: health.current, max: health.max } : undefined,
        energy: energy ? { current: energy.availableCharge, max: energy.batteryCapacity } : undefined,
        shield: shield ? { current: shield.charge, max: shield.maxCharge } : undefined,
      };

      targets.push(targetBarData);
    });

    this.renderTargets(targets);
  }

  private renderTargets(targets: TargetBarData[]): void {
    const currentTargetIds = new Set(targets.map((t) => t.entityId));

    for (const [entityId, _] of this.targetElements) {
      if (!currentTargetIds.has(entityId)) {
        this.removeTarget(entityId);
      }
    }

    targets.forEach((target) => {
      this.renderTarget(target);
    });
  }

  private renderTarget(target: TargetBarData): void {
    let element = this.targetElements.get(target.entityId);
    if (!element) {
      element = new TargetBarElement(target.title);
      this.targetElements.set(target.entityId, element);
      this.addChild(element);
    }

    element.position.set(target.position.x, target.position.y);

    element.updateBars(target.health, target.energy, target.shield);
  }

  private removeTarget(entityId: string): void {
    const element = this.targetElements.get(entityId);
    if (element) {
      this.removeChild(element);
      element.destroy();
      this.targetElements.delete(entityId);
    }
  }

  private hideAllTargets(): void {
    for (const element of this.targetElements.values()) {
      element.visible = false;
    }
  }

  private getEntityBarPosition(entityX: number, entityY: number, entitySize: number, camera: Camera): { x: number; y: number } {
    const screenPos = this.worldToScreen(entityX, entityY, camera);

    return {
      x: screenPos.x,
      y: screenPos.y + entitySize * camera.zoom + 10, // 10px below entity
    };
  }

  private worldToScreen(worldX: number, worldY: number, camera: Camera): { x: number; y: number } {
    const screenCenterX = this.canvasWidth / 2;
    const screenCenterY = this.canvasHeight / 2;

    // Calculate position relative to camera
    const relativeX = worldX - camera.x;
    const relativeY = worldY - camera.y;

    // Apply camera rotation
    const cos = Math.cos(camera.rotation);
    const sin = Math.sin(camera.rotation);
    const rotatedX = relativeX * cos - relativeY * sin;
    const rotatedY = relativeX * sin + relativeY * cos;

    // Apply zoom and translate to screen space
    return {
      x: screenCenterX + rotatedX * camera.zoom,
      y: screenCenterY + rotatedY * camera.zoom,
    };
  }

  public toggle(): void {
    this.visible_ = !this.visible_;
    this.visible = this.visible_;
  }

  public setVisible(visible: boolean): void {
    this.visible_ = visible;
    this.visible = visible;

    if (visible) {
      for (const element of this.targetElements.values()) {
        element.visible = true;
      }
    } else {
      this.hideAllTargets();
    }
  }

  public isVisible(): boolean {
    return this.visible_;
  }

  public resize(screenWidth: number, screenHeight: number): void {
    this.canvasWidth = screenWidth;
    this.canvasHeight = screenHeight;
  }

  public clearAll(): void {
    for (const [entityId] of this.targetElements) {
      this.removeTarget(entityId);
    }
  }

  public destroy(): void {
    this.clearAll();
    super.destroy();
  }
}

class TargetBarElement extends Container {
  private background!: Graphics;
  private titleText!: Text;
  private healthBar!: MiniBar;
  private energyBar!: MiniBar;
  private shieldBar!: MiniBar;

  private readonly titleStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 9,
    fill: "#cccccc",
    align: "center",
  });

  constructor(title: string) {
    super();

    this.createBackground();
    this.createTitle(title);
    this.createBars();

    this.pivot.set(70, 0); // Half width for centering
  }

  private createBackground(): void {
    this.background = new Graphics();
    this.background.roundRect(0, 0, 140, 50, 3);
    this.background.fill({ color: 0x000000, alpha: 0.85 });
    this.background.stroke({ color: 0x666666, width: 1 });
    this.addChild(this.background);
  }

  private createTitle(title: string): void {
    this.titleText = new Text({ text: title, style: this.titleStyle });
    this.titleText.anchor.set(0.5, 0);
    this.titleText.position.set(70, 4);
    this.addChild(this.titleText);
  }

  private createBars(): void {
    this.healthBar = new MiniBar("H:", 0xcc2222);
    this.healthBar.position.set(6, 18);
    this.addChild(this.healthBar);

    this.energyBar = new MiniBar("E:", 0x22cc22);
    this.energyBar.position.set(6, 28);
    this.addChild(this.energyBar);

    this.shieldBar = new MiniBar("S:", 0x2222cc);
    this.shieldBar.position.set(6, 38);
    this.addChild(this.shieldBar);
  }

  public updateBars(
    health?: { current: number; max: number },
    energy?: { current: number; max: number },
    shield?: { current: number; max: number },
  ): void {
    this.healthBar.updateBar(health);
    this.energyBar.updateBar(energy);
    this.shieldBar.updateBar(shield);
  }
}

class MiniBar extends Container {
  private labelText!: Text;
  private background!: Graphics;
  private fill!: Graphics;
  private text!: Text;

  private readonly labelStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 8,
    fill: "#aaaaaa",
  });

  private readonly textStyle = new TextStyle({
    fontFamily: "Courier New, monospace",
    fontSize: 7,
    fill: "#ffffff",
    fontWeight: "bold",
  });

  constructor(
    labelText: string,
    private fillColor: number,
  ) {
    super();

    this.createLabel(labelText);
    this.createBar();
    this.createText();
    this.visible = false; // Start hidden
  }

  private createLabel(labelText: string): void {
    this.labelText = new Text({ text: labelText, style: this.labelStyle });
    this.labelText.position.set(0, 0);
    this.addChild(this.labelText);
  }

  private createBar(): void {
    this.background = new Graphics();
    this.background.roundRect(0, 0, 80, 8, 2);
    this.background.fill(0x222222);
    this.background.stroke({ color: 0x444444, width: 1 });
    this.background.position.set(18, 0);
    this.addChild(this.background);

    this.fill = new Graphics();
    this.fill.position.set(19, 1);
    this.addChild(this.fill);
  }

  private createText(): void {
    this.text = new Text({ text: "", style: this.textStyle });
    this.text.anchor.set(0.5, 0.5);
    this.text.position.set(58, 4); // Center of bar
    this.addChild(this.text);
  }

  public updateBar(data?: { current: number; max: number }): void {
    if (data && data.max > 0) {
      this.visible = true;

      const percent = Math.min(100, (data.current / data.max) * 100);
      const barWidth = 78; // Bar width minus padding

      this.fill.clear();
      this.fill.roundRect(0, 0, (barWidth * percent) / 100, 6, 1);
      this.fill.fill(this.fillColor);

      this.text.text = `${Math.round(data.current)}/${Math.round(data.max)}`;
    } else {
      this.visible = false;
    }
  }
}
