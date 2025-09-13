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

  // Override this to provide custom serialization if needed
  serialize(): Record<string, any> {
    return {
      entityId: this.entityId,
      changedTick: this.changedTick,
      created: this.created
    };
  }

  // Override this to provide custom deserialization if needed
  deserialize(data: Record<string, any>): void {
    if (data.changedTick !== undefined) {
      this.changedTick = data.changedTick;
    }
    if (data.created !== undefined) {
      this.created = data.created;
    }
  }

  // Get the component type name for debugging/logging
  getTypeName(): string {
    return this.constructor.name;
  }
}