import { Component } from '../core/Component';

export interface RenderConfig {
  sides?: number;
  color?: number;
  shapeType?: number;
}

export class RenderComponent extends Component {
  sides: number;
  color: number;
  shapeType: number;

  constructor(entityId: number, config: RenderConfig = {}) {
    super(entityId);

    this.sides = config.sides || 3;
    this.color = config.color || 0xffffff;
    this.shapeType = config.shapeType || 0;
  }

  serialize(): Record<string, any> {
    return {
      ...super.serialize(),
      sides: this.sides,
      color: this.color,
      shapeType: this.shapeType
    };
  }
}