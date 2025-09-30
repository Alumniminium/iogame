import { Entity } from "./Entity";
import { Component } from "./Component";
import { EntityType } from "./types";

/**
 * Base class for all ECS systems.
 * Systems contain the logic that processes entities with specific component combinations.
 * Each system defines which component types it operates on and implements update logic.
 */
export abstract class System {
  /**
   * The component types this system requires to process an entity.
   * Only entities with all of these components will be processed.
   */
  abstract readonly componentTypes: (new (
    entityId: string,
    ...args: any[]
  ) => Component)[];

  /**
   * Optional lifecycle hook called when an entity's components change
   */
  onEntityChanged?(entity: Entity): void;

  /**
   * Optional lifecycle hook called when an entity is destroyed
   */
  onEntityDestroyed?(entity: Entity): void;

  /**
   * Optional initialization hook called when the system is first added
   */
  initialize?(): void;

  /**
   * Optional cleanup hook called when the system is removed
   */
  cleanup?(): void;

  /**
   * Main update loop. Queries entities with required components and processes each.
   * @param deltaTime Time elapsed since last frame in seconds
   */
  update(deltaTime: number): void {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    if (!WorldClass) return;

    const entities = WorldClass.queryEntitiesWithComponents(
      ...this.componentTypes,
    );

    entities.forEach((entity: Entity) => {
      this.updateEntity(entity, deltaTime);
    });
  }

  /**
   * Process a single entity. Must be implemented by derived systems.
   * @param entity The entity to process
   * @param deltaTime Time elapsed since last frame in seconds
   */
  protected abstract updateEntity(entity: Entity, deltaTime: number): void;

  /**
   * Query entities with specific component types
   */
  protected queryEntities(
    componentTypes: (new (entityId: string, ...args: any[]) => Component)[],
  ): Entity[] {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    return WorldClass
      ? WorldClass.queryEntitiesWithComponents(...componentTypes)
      : [];
  }

  /**
   * Get an entity by its ID
   */
  protected getEntity(id: string): Entity | undefined {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    return WorldClass ? WorldClass.getEntity(id) : undefined;
  }

  /**
   * Create a new entity
   */
  protected createEntity(type: EntityType, parentId?: string): Entity {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    if (!WorldClass) throw new Error("World not initialized");
    return WorldClass.createEntity(type, parentId);
  }

  /**
   * Destroy an entity
   */
  protected destroyEntity(entity: Entity | string): void {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    if (WorldClass) WorldClass.destroyEntity(entity);
  }
}
