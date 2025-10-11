import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.Color)
export class ColorComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "u32") public color: number;

  constructor(entityId: string, color?: number) {
    super(entityId);

    if (color !== undefined) {
      this.color = color;
    } else {
      // Defaults for deserialization
      this.color = 0xffffff;
    }
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
