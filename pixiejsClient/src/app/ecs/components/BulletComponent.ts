import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.Bullet)
export class BulletComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "guid") owner: string;

  constructor(ntt: NTT, owner: string = "") {
    super(ntt);
    this.owner = owner;
  }
}
