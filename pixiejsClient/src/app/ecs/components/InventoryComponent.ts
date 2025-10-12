import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.Inventory)
export class InventoryComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "i32") totalCapacity: number;
  @serverField(2, "i32") triangles: number;
  @serverField(3, "i32") squares: number;
  @serverField(4, "i32") pentagons: number;

  constructor(ntt: NTT, totalCapacity: number = 0, triangles: number = 0, squares: number = 0, pentagons: number = 0) {
    super(ntt);
    this.totalCapacity = totalCapacity;
    this.triangles = triangles;
    this.squares = squares;
    this.pentagons = pentagons;
  }
}
