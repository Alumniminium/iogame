import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.DeathTag)
export class DeathTagComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "guid") killer: string;

  constructor(entityId: string, killer: string = "") {
    super(entityId);
    this.killer = killer;
  }
}
