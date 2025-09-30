import { Component } from "../core/Component";

export interface ShipPartData {
  gridX: number;
  gridY: number;
  type: number; // 0=hull, 1=shield, 2=engine
  shape: number; // 1=triangle, 2=square
  rotation: number; // 0=0°, 1=90°, 2=180°, 3=270°
}

export class ShipPartComponent extends Component {
  constructor(
    entityId: string,
    public data: ShipPartData,
  ) {
    super(entityId);
  }
}
