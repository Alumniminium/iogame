import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.Inventory)
export class InventoryComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "i32") totalCapacity: number;
  @serverField(2, "i32") triangles: number;
  @serverField(3, "i32") squares: number;
  @serverField(4, "i32") pentagons: number;

  constructor(entityId: string, totalCapacity: number = 0, triangles: number = 0, squares: number = 0, pentagons: number = 0) {
    super(entityId);
    this.totalCapacity = totalCapacity;
    this.triangles = triangles;
    this.squares = squares;
    this.pentagons = pentagons;
  }
}
