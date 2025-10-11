import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.ExpReward)
export class ExpRewardComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "i32") experience: number;

  constructor(entityId: string, experience: number = 0) {
    super(entityId);
    this.experience = experience;
  }
}
