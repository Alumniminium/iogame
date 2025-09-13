import { Component } from "../core/Component";
import type { Container } from "pixi.js";

export interface RenderConfig {
  sides?: number;
  color?: number;
  shapeType?: number;
  alpha?: number;
  visible?: boolean;
}

export class RenderComponent extends Component {
  sides: number;
  color: number;
  shapeType: number;
  alpha: number;
  visible: boolean;
  displayObject?: Container;

  constructor(entityId: number, config: RenderConfig = {}) {
    super(entityId);

    this.sides = config.sides !== undefined ? config.sides : 3;
    this.color = config.color || 0xffffff;
    this.shapeType = config.shapeType || 0;
    this.alpha = config.alpha !== undefined ? config.alpha : 1;
    this.visible = config.visible !== undefined ? config.visible : true;
  }

  setDisplayObject(displayObject: Container): void {
    this.displayObject = displayObject;
    this.markChanged();
  }
}
