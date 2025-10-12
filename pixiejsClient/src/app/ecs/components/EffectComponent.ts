import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { EffectType } from "../../enums/EffectType";

@component(ServerComponentType.Effect)
export class EffectComponent extends Component {
  @serverField(1, "u8") effectType: EffectType;
  @serverField(2, "u32") color: number;

  constructor(entityId: string, effectType: EffectType = EffectType.Spawn, color: number = 0xffffff) {
    super(entityId);
    this.effectType = effectType;
    this.color = color;
  }
}
