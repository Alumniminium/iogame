import { Component, component, serverField } from "../core/Component";
import { NTT } from "../core/NTT";
import { ServerComponentType } from "../../enums/ComponentIds";

@component(ServerComponentType.Spawner)
export class SpawnerComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "i32") unitIdToSpawn: number;
  @serverField(2, "i64") interval: bigint; // TimeSpan as ticks
  @serverField(3, "f32") timeSinceLastSpawn: number;
  @serverField(4, "i32") amountPerInterval: number;
  @serverField(5, "i32") maxPopulation: number;
  @serverField(6, "i32") minPopulation: number;

  constructor(
    ntt: NTT,
    unitIdToSpawn: number = 0,
    interval: bigint = 0n,
    timeSinceLastSpawn: number = 0,
    amountPerInterval: number = 1,
    maxPopulation: number = 10,
    minPopulation: number = 0,
  ) {
    super(ntt);
    this.unitIdToSpawn = unitIdToSpawn;
    this.interval = interval;
    this.timeSinceLastSpawn = timeSinceLastSpawn;
    this.amountPerInterval = amountPerInterval;
    this.maxPopulation = maxPopulation;
    this.minPopulation = minPopulation;
  }
}
