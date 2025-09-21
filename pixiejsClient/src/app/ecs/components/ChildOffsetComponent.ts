import { Component } from "../core/Component";
import { Vector2 } from "../core/types";

export class ChildOffsetComponent extends Component {
  public offset: Vector2 = { x: 0, y: 0 };
  public rotation = 0;

  constructor(entityId: string, offset: Vector2, rotation: number) {
    super(entityId);
    this.offset = offset;
    this.rotation = rotation;
  }
}
