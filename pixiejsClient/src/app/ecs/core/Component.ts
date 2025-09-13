export abstract class Component {
  readonly entityId: number;
  private changedTick: number = 0;
  private created: number;

  constructor(entityId: number) {
    this.entityId = entityId;
    this.created = Date.now();
    this.markChanged();
  }

  markChanged(): void {
    this.changedTick = Date.now();
  }

  getLastChangedTick(): number {
    return this.changedTick;
  }

  getCreatedTime(): number {
    return this.created;
  }

  // Get the component type name for debugging/logging
  getTypeName(): string {
    return this.constructor.name;
  }
}
