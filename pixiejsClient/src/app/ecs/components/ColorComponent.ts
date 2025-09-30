import { Component } from "../core/Component";

export class ColorComponent extends Component {
  public color: number;

  constructor(entityId: string, color: number) {
    super(entityId);
    this.color = color;
  }

  static getPartColor(type: "hull" | "shield" | "engine"): number {
    switch (type) {
      case "hull":
        return 0x808080;
      case "shield":
        return 0x0080ff;
      case "engine":
        return 0xff8000;
      default:
        return 0xffffff;
    }
  }
}
