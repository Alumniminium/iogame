import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.ExpReward)
export class ExpRewardComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "i32") experience: number;

  constructor(ntt: NTT, experience: number = 0) {
    super(ntt);
    this.experience = experience;
  }
}
