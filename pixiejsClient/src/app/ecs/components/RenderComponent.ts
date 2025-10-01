import { Component } from "../core/Component";
import type { Container } from "pixi.js";

export interface ShipPart {
  gridX: number;
  gridY: number;
  type: number; // 0=hull, 1=shield, 2=engine
  shape: number; // 0=triangle, 1=square
  rotation: number; // 0=0°, 1=90°, 2=180°, 3=270°
}

export interface RenderConfig {
  sides?: number;
  color?: number;
  shapeType?: number;
  alpha?: number;
  visible?: boolean;
  shipParts?: ShipPart[];
  centerX?: number;
  centerY?: number;
}

export class RenderComponent extends Component {
  sides: number;
  color: number;
  shapeType: number;
  alpha: number;
  visible: boolean;
  displayObject?: Container;
  shipParts: ShipPart[];
  centerX: number;
  centerY: number;

  constructor(entityId: string, config: RenderConfig = {}) {
    super(entityId);

    this.sides = config.sides !== undefined ? config.sides : 3;
    this.color = config.color || 0xffffff;
    this.shapeType = config.shapeType || 0;
    this.alpha = config.alpha !== undefined ? config.alpha : 1;
    this.visible = config.visible !== undefined ? config.visible : true;
    this.shipParts = config.shipParts || [];
    this.centerX = config.centerX || 0;
    this.centerY = config.centerY || 0;
  }

  setDisplayObject(displayObject: Container): void {
    this.displayObject = displayObject;
  }
}
