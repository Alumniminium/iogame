import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.DeathTag)
export class DeathTagComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "guid") killer: string;

  constructor(ntt: NTT, killer: string = "") {
    super(ntt);
    this.killer = killer;
  }
}
