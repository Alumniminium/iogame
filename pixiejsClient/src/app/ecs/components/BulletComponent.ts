import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.Bullet)
export class BulletComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "guid") owner: string;

  constructor(entityId: string, owner: string = "") {
    super(entityId);
    this.owner = owner;
  }
}
