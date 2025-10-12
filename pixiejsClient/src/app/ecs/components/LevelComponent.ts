import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.Level)
export class LevelComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "i32") level: number;
  @serverField(2, "i32") experienceToNextLevel: number;
  @serverField(3, "i32") experience: number;

  constructor(ntt: NTT, level: number = 1, experienceToNextLevel: number = 100, experience: number = 0) {
    super(ntt);
    this.level = level;
    this.experienceToNextLevel = experienceToNextLevel;
    this.experience = experience;
  }
}
