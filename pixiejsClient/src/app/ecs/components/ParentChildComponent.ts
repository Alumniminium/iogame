import { Component, component, serverField } from "../core/Component";
import { NTT } from "../core/NTT";
import { ServerComponentType } from "../../enums/ComponentIds";

export interface ParentChildConfig {
  parentId: string;
  gridX?: number;
  gridY?: number;
  shape?: number;
  rotation?: number;
}

@component(ServerComponentType.ParentChild)
export class ParentChildComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "guid") parentId: string;
  @serverField(2, "i8") gridX: number;
  @serverField(3, "i8") gridY: number;
  @serverField(4, "u8") shape: number;
  @serverField(5, "u8") rotation: number;

  constructor(ntt: NTT, config?: ParentChildConfig | string) {
    super(ntt);

    if (config) {
      if (typeof config === "string") {
        this.parentId = config;
        this.gridX = 0;
        this.gridY = 0;
        this.shape = 0;
        this.rotation = 0;
      } else {
        this.parentId = config.parentId;
        this.gridX = config.gridX || 0;
        this.gridY = config.gridY || 0;
        this.shape = config.shape || 0;
        this.rotation = config.rotation || 0;
      }
    } else {
      // Defaults for deserialization
      this.parentId = "00000000-0000-0000-0000-000000000000";
      this.gridX = 0;
      this.gridY = 0;
      this.shape = 0;
      this.rotation = 0;
    }
  }
}
