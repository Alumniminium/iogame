import { Container, Graphics, Text, TextStyle } from "pixi.js";
import type { Entity } from "../../ecs/core/Entity";
import { HealthComponent } from "../../ecs/components/HealthComponent";
import { EnergyComponent } from "../../ecs/components/EnergyComponent";
import { ShieldComponent } from "../../ecs/components/ShieldComponent";
import { Box2DBodyComponent } from "../../ecs/components/Box2DBodyComponent";
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

// Type for component getters
type ComponentGetter<T> = (entity: Entity) => T | null;

// Formatter function type
type Formatter<TComponent, TContext = never> = [TContext] extends [never]
  ? (component: TComponent) => string
  : (component: TComponent, context: TContext) => string;

// Stat field definition
interface StatField<TComponent = any, TContext = never> {
  label: string;
  getComponent: ComponentGetter<TComponent>;
  format: Formatter<TComponent, TContext>;
  fallback?: string;
}

// Section definition
interface StatSection<TContext = never> {
  title: string;
  fields: StatField<any, TContext>[];
}

// Helper functions for common formatting patterns
const fmt = {
  decimal: (val: number | undefined, decimals = 1): string =>
    val?.toFixed(decimals) ?? "0.0",
  percent: (current: number, max: number): string =>
    ((current / max) * 100).toFixed(1),
  ratio: (current: number, max: number, unit: string, decimals = 1): string =>
    `${current.toFixed(decimals)}/${max.toFixed(decimals)} ${unit} (${fmt.percent(current, max)}%)`,
  vector: (x: number | undefined, y: number | undefined): string =>
    `(${fmt.decimal(x)}, ${fmt.decimal(y)})`,
  degrees: (radians: number | undefined): string =>
    radians ? ((radians * 180) / Math.PI).toFixed(1) : "0.0",
  status: (
    active: boolean,
    activeText = "ACTIVE",
    inactiveText = "INACTIVE",
  ): string => (active ? activeText : inactiveText),
};

// Stat section definitions
const STAT_SECTIONS: StatSection<InputState>[] = [
  {
    title: "STORAGE",
    fields: [
      {
        label: "Battery",
        getComponent: (e) => e.get(EnergyComponent),
        format: (c) => fmt.ratio(c.availableCharge, c.batteryCapacity, "kWh"),
      },
      {
        label: "Charge Rate",
        getComponent: (e) => e.get(EnergyComponent),
        format: (c) => `${fmt.decimal(c.chargeRate || 0)} kW`,
      },
      {
        label: "Discharge Rate",
        getComponent: (e) => e.get(EnergyComponent),
        format: (c) => `${fmt.decimal(c.dischargeRate || 0)} kW`,
      },
    ],
  },
  {
    title: "HEALTH",
    fields: [
      {
        label: "Hull",
        getComponent: (e) => e.get(HealthComponent),
        format: (c) => fmt.ratio(c.current, c.max, "HP"),
      },
      {
        label: "Status",
        getComponent: (e) => e.get(HealthComponent),
        format: (c) => fmt.status(!c.isDead, "OPERATIONAL", "DESTROYED"),
      },
    ],
  },
  {
    title: "SHIELD",
    fields: [
      {
        label: "Shield",
        getComponent: (e) => e.get(ShieldComponent),
        format: (c) => fmt.ratio(c.charge, c.maxCharge, "SP"),
      },
      {
        label: "Recharge Rate",
        getComponent: (e) => e.get(ShieldComponent),
        format: (c) => `${fmt.decimal(c.rechargeRate || 0)} SP/s`,
      },
      {
        label: "Power Use",
        getComponent: (e) => e.get(ShieldComponent),
        format: (c) => `${fmt.decimal(c.powerUse || 0)} kW`,
      },
      {
        label: "Radius",
        getComponent: (e) => e.get(ShieldComponent),
        format: (c) => `${fmt.decimal(c.radius || 0)} m`,
      },
      {
        label: "Status",
        getComponent: (e) => e.get(ShieldComponent),
        format: (c) => fmt.status(c.powerOn),
      },
    ],
  },
  {
    title: "ENGINE",
    fields: [
      {
        label: "Speed",
        getComponent: (e) => e.get(Box2DBodyComponent),
        format: (c) => `${fmt.decimal(c.getSpeed())} m/s`,
      },
      {
        label: "Throttle",
        getComponent: (e) => e.get(Box2DBodyComponent),
        format: (_c, input) => {
          const percent = input.thrust ? 100 : input.invThrust ? 50 : 0;
          return `${percent}%`;
        },
      },
      {
        label: "Power Draw",
        getComponent: (e) => e.get(Box2DBodyComponent),
        format: (_c, input) => {
          const percent = input.thrust ? 100 : input.invThrust ? 50 : 0;
          const powerDraw = percent > 0 ? 50.0 : 0;
          return `${fmt.decimal(powerDraw)} kW`;
        },
      },
      {
        label: "RCS",
        getComponent: (e) => e.get(Box2DBodyComponent),
        format: (_c, input) => fmt.status(input.rcs),
      },
      {
        label: "Position",
        getComponent: (e) => e.get(Box2DBodyComponent),
        format: (c) => fmt.vector(c.position?.x, c.position?.y),
      },
      {
        label: "Velocity",
        getComponent: (e) => e.get(Box2DBodyComponent),
        format: (c) => fmt.vector(c.linearVelocity?.x, c.linearVelocity?.y),
      },
      {
        label: "Rotation",
        getComponent: (e) => e.get(Box2DBodyComponent),
        format: (c) => `${fmt.degrees(c.rotationRadians)}°`,
      },
    ],
  },
  {
    title: "POWER DRAW",
    fields: [
      {
        label: "Total",
        getComponent: (e) => e.get(EnergyComponent),
        format: (c) => `${fmt.decimal(c.dischargeRate || 0)} kW`,
      },
      {
        label: "Net Power",
        getComponent: (e) => e.get(EnergyComponent),
        format: (c) => {
          const net = (c.chargeRate || 0) - (c.dischargeRate || 0);
          return `${fmt.decimal(net)} kW`;
        },
      },
      {
        label: "Status",
        getComponent: (e) => e.get(EnergyComponent),
        format: (c) => {
          const net = (c.chargeRate || 0) - (c.dischargeRate || 0);
          return fmt.status(net >= 0, "CHARGING", "DISCHARGING");
        },
      },
    ],
  },
];

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
    this.background.roundRect(0, 0, 320, 550, 4);
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

  public updateFromEntity(entity: Entity, inputState: InputState): void {
    if (!this.visible_) return;

    let content = "";

    for (const section of STAT_SECTIONS) {
      content += `━━━ ${section.title} ━━━\n`;

      for (const field of section.fields) {
        const component = field.getComponent(entity);

        if (component) {
          try {
            const value = field.format(component, inputState);
            content += `${field.label}: ${value}\n`;
          } catch {
            content += `${field.label}: ${field.fallback || "No Data"}\n`;
          }
        } else {
          content += `${field.label}: ${field.fallback || "No Data"}\n`;
        }
      }

      content += "\n";
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
