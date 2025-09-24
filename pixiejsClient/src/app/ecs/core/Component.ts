export abstract class Component {
  readonly entityId: string;
  protected changedTick: number = 0;
  private created: number;

  constructor(entityId: string) {
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

  getTypeName(): string {
    return this.constructor.name;
  }
}
