import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.RespawnTag)
export class RespawnTagComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "i32") expPenalty: number;
  @serverField(2, "i64") respawnTimeTick: bigint;

  constructor(entityId: string, expPenalty: number = 0, respawnTimeTick: bigint = 0n) {
    super(entityId);
    this.expPenalty = expPenalty;
    this.respawnTimeTick = respawnTimeTick;
  }
}
