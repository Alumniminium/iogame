import { Container, Graphics, Text, TextStyle } from "pixi.js";
import { World } from "../../ecs/core/World";
import { PhysicsComponent } from "../../ecs/components/PhysicsComponent";
import { HealthComponent } from "../../ecs/components/HealthComponent";
import { EnergyComponent } from "../../ecs/components/EnergyComponent";
import { ShieldComponent } from "../../ecs/components/ShieldComponent";
import type { Camera } from "../../ecs/systems/RenderSystem";

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

  public updateFromWorld(
    camera?: Camera,
    localPlayerId?: string,
    viewDistance?: number,
    hoveredEntityId?: string | null,
  ): void {
    if (!this.visible_) {
      this.hideAllTargets();
      return;
    }

    // Only show target bars if an entity is being hovered
    if (!hoveredEntityId) {
      this.hideAllTargets();
      return;
    }

    // Get all entities with physics components
    const entities = World.queryEntitiesWithComponents(PhysicsComponent);
    const targets: TargetBarData[] = [];

    // Use provided values or fallback to defaults
    const activeCamera: Camera = camera || { x: 0, y: 0, zoom: 1 };
    const activeViewDistance = viewDistance || 300;
    let localPlayerPosition = { x: 0, y: 0 };

    // Get local player position if available
    if (localPlayerId) {
      const localPlayer = World.getEntity(localPlayerId);
      if (localPlayer) {
        const localPhysics = localPlayer.get(PhysicsComponent);
        if (localPhysics) {
          localPlayerPosition = localPhysics.position;
        }
      }
    }

    entities.forEach((entity) => {
      // Only show target bar for the hovered entity
      if (entity.id !== hoveredEntityId) return;

      // Skip local player
      if (entity.id === localPlayerId) return;

      const physics = entity.get(PhysicsComponent)!;

      // Calculate screen position for bars below entity
      const barPosition = this.getEntityBarPosition(
        physics.position.x,
        physics.position.y,
        physics.size,
        activeCamera,
      );

      // Only show bars if entity is visible on screen
      if (
        barPosition.x > -150 &&
        barPosition.x < this.canvasWidth + 50 &&
        barPosition.y > -50 &&
        barPosition.y < this.canvasHeight + 50
      ) {
        // Extract health/energy/shield data from components
        const health = entity.get(HealthComponent);
        const energy = entity.get(EnergyComponent);
        const shield = entity.get(ShieldComponent);

        const targetBarData: TargetBarData = {
          entityId: entity.id,
          position: barPosition,
          title: `Entity ${entity.id}`,
          health: health
            ? { current: health.current, max: health.max }
            : undefined,
          energy: energy
            ? { current: energy.availableCharge, max: energy.batteryCapacity }
            : undefined,
          shield: shield
            ? { current: shield.charge, max: shield.maxCharge }
            : undefined,
        };

        targets.push(targetBarData);
      }
    });

    this.renderTargets(targets);
  }

  private renderTargets(targets: TargetBarData[]): void {
    const currentTargetIds = new Set(targets.map((t) => t.entityId));

    // Remove old targets that are no longer needed
    for (const [entityId, _] of this.targetElements) {
      if (!currentTargetIds.has(entityId)) {
        this.removeTarget(entityId);
      }
    }

    // Update or create targets
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

    // Update position
    element.position.set(target.position.x, target.position.y);

    // Update bars
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

  private getEntityBarPosition(
    entityX: number,
    entityY: number,
    entitySize: number,
    camera: Camera,
  ): { x: number; y: number } {
    const screenPos = this.worldToScreen(entityX, entityY, camera);

    // Position bars below the entity
    return {
      x: screenPos.x, // Center horizontally
      y: screenPos.y + entitySize * camera.zoom + 10, // 10px below entity
    };
  }

  private worldToScreen(
    worldX: number,
    worldY: number,
    camera: Camera,
  ): { x: number; y: number } {
    const screenCenterX = this.canvasWidth / 2;
    const screenCenterY = this.canvasHeight / 2;

    return {
      x: screenCenterX + (worldX - camera.x) * camera.zoom,
      y: screenCenterY + (worldY - camera.y) * camera.zoom,
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

// Helper class for individual target bar elements
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

    // Center the element on its position
    this.pivot.set(70, 0); // Half width for centering
  }

  private createBackground(): void {
    this.background = new Graphics();
    this.background.beginFill(0x000000, 0.85);
    this.background.lineStyle(1, 0x666666);
    this.background.drawRoundedRect(0, 0, 140, 50, 3);
    this.background.endFill();
    this.addChild(this.background);
  }

  private createTitle(title: string): void {
    this.titleText = new Text(title, this.titleStyle);
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

// Helper class for mini bars within target displays
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
    this.labelText = new Text(labelText, this.labelStyle);
    this.labelText.position.set(0, 0);
    this.addChild(this.labelText);
  }

  private createBar(): void {
    // Background
    this.background = new Graphics();
    this.background.beginFill(0x222222);
    this.background.lineStyle(1, 0x444444);
    this.background.drawRoundedRect(0, 0, 80, 8, 2);
    this.background.endFill();
    this.background.position.set(18, 0);
    this.addChild(this.background);

    // Fill
    this.fill = new Graphics();
    this.fill.position.set(19, 1);
    this.addChild(this.fill);
  }

  private createText(): void {
    this.text = new Text("", this.textStyle);
    this.text.anchor.set(0.5, 0.5);
    this.text.position.set(58, 4); // Center of bar
    this.addChild(this.text);
  }

  public updateBar(data?: { current: number; max: number }): void {
    if (data && data.max > 0) {
      this.visible = true;

      const percent = Math.min(100, (data.current / data.max) * 100);
      const barWidth = 78; // Bar width minus padding

      // Update fill
      this.fill.clear();
      this.fill.beginFill(this.fillColor);
      this.fill.drawRoundedRect(0, 0, (barWidth * percent) / 100, 6, 1);
      this.fill.endFill();

      // Update text
      this.text.text = `${Math.round(data.current)}/${Math.round(data.max)}`;
    } else {
      this.visible = false;
    }
  }
}
