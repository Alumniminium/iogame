export abstract class Component {
  readonly entityId: number;
  changedTick: number = 0;

  constructor(entityId: number) {
    this.entityId = entityId;
  }

  markChanged(): void {
    this.changedTick = Date.now();
  }
}