/**
 * Base class for all ECS components.
 * Components are data containers attached to entities that define their properties and behavior.
 */
export abstract class Component {
  readonly entityId: string;
  protected changedTick: number = 0;
  private created: number;

  constructor(entityId: string) {
    this.entityId = entityId;
    this.created = Date.now();
    this.markChanged();
  }

  /**
   * Marks this component as changed, updating the last changed timestamp
   */
  markChanged(): void {
    this.changedTick = Date.now();
  }

  /**
   * Returns the timestamp when this component was last modified
   */
  getLastChangedTick(): number {
    return this.changedTick;
  }

  /**
   * Returns the timestamp when this component was created
   */
  getCreatedTime(): number {
    return this.created;
  }

  /**
   * Returns the class name of this component type
   */
  getTypeName(): string {
    return this.constructor.name;
  }
}
