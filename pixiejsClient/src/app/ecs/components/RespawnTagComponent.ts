import { Component, component, serverField } from "../core/Component";
import { NTT } from "../core/NTT";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.RespawnTag)
export class RespawnTagComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "i32") expPenalty: number;
  @serverField(2, "i64") respawnTimeTick: bigint;

  constructor(ntt: NTT, expPenalty: number = 0, respawnTimeTick: bigint = 0n) {
    super(ntt);
    this.expPenalty = expPenalty;
    this.respawnTimeTick = respawnTimeTick;
  }
}
