import { World } from "./World";

/**
 * Base class for all ECS components.
 * Components are data containers attached to entities that define their properties and behavior.
 */
export abstract class Component {
  readonly entityId: string;
  public changedTick: bigint = 0n;
  public created: number;

  constructor(entityId: string) {
    this.entityId = entityId;
    this.created = Date.now();
    this.changedTick = World.currentTick;
  }

  /**
   * Returns the class name of this component type
   */
  getTypeName(): string {
    return this.constructor.name;
  }
}
