import { Component, component, serverField } from "../core/Component";
import { NTT } from "../core/NTT";
import { ServerComponentType } from "../../enums/ComponentIds";

export interface ShipPartConfig {
  gridX?: number;
  gridY?: number;
  type?: number;
  shape?: number;
  rotation?: number;
}

/**
 * Component for ship parts/modules that can be attached to ships
 */
@component(ServerComponentType.ShipPart)
export class ShipPartComponent extends Component {
  // Match C# struct layout - changedTick inherited from Component
  @serverField(1, "i8") gridX: number; // SByte in C#
  @serverField(2, "i8") gridY: number; // SByte in C#
  @serverField(3, "u8") type: number; // Byte in C#
  @serverField(4, "u8") shape: number; // Byte in C#
  @serverField(5, "u8") rotation: number; // Byte in C#

  constructor(ntt: NTT, config: ShipPartConfig = {}) {
    super(ntt);
    this.gridX = config.gridX ?? 0;
    this.gridY = config.gridY ?? 0;
    this.type = config.type ?? 0;
    this.shape = config.shape ?? 0;
    this.rotation = config.rotation ?? 0;
  }
}
