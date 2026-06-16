import { Component, component } from "../core/Component";
import { NTT } from "../core/NTT";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.PickableTag)
export class PickableTagComponent extends Component {
  // changedTick is inherited from Component base class
  // No additional fields - this is just a tag component

  constructor(ntt: NTT) {
    super(ntt);
  }
}
