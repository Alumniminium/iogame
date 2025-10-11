import { Entity } from "./Entity";
import { System } from "./System";
import { EntityType } from "./types";
import { Component } from "./Component";

/**
 * Component query specification
 */
export interface ComponentQuery {
  with: (new (entityId: string, ...args: any[]) => Component)[];
  without?: (new (entityId: string, ...args: any[]) => Component)[];
}

/**
 * Central ECS World coordinator - aligned with server NttECS architecture.
 *
 * Key architecture:
 * - Systems are processed in order (no dependency resolution)
 * - Entity changes processed between each system (like server)
 * - Simplified API: informChangesFor, updateEntities pattern
 * - Static-only (no singleton instance)
 */
export class World {
  private static entities = new Map<string, Entity>();
  private static systems: System[] = [];
  private static changedEntities = new Set<Entity>();
  private static nextEntityId = 1;
  private static destroyed = false;
  public static currentTick = 0n;

  private static onBeginTick?: () => void;
  private static onEndTick?: () => void;

  /**
   * Initialize the World
   */
  static initialize(): void {
    (globalThis as any).__WORLD_CLASS = World;
    World.destroyed = false;
  }

  /**
   * Register systems in execution order.
   * Systems will run in the order provided (no automatic dependency resolution).
   */
  static setSystems(...systems: System[]): void {
    World.systems = systems;

    // Initialize systems
    systems.forEach((system) => {
      if (system.initialize) {
        system.initialize();
      }
    });

    // Build initial entity lists for all systems
    World.entities.forEach((entity) => {
      systems.forEach((system) => {
        system.entityChanged(entity);
      });
    });
  }

  /**
   * Add a system to the end of the execution order
   */
  static addSystem(system: System): void {
    World.systems.push(system);

    if (system.initialize) {
      system.initialize();
    }

    // Build initial entity list for new system
    World.entities.forEach((entity) => {
      system.entityChanged(entity);
    });
  }

  /**
   * Remove a system
   */
  static removeSystem(system: System): void {
    const index = World.systems.indexOf(system);
    if (index !== -1) {
      if (system.cleanup) {
        system.cleanup();
      }
      World.systems.splice(index, 1);
    }
  }

  /**
   * Get all registered systems
   */
  static getSystems(): System[] {
    return World.systems.slice();
  }

  /**
   * Register callback to run at the beginning of each tick
   */
  static registerOnBeginTick(callback: () => void): void {
    World.onBeginTick = callback;
  }

  /**
   * Register callback to run at the end of each tick
   */
  static registerOnEndTick(callback: () => void): void {
    World.onEndTick = callback;
  }

  /**
   * Create a new entity
   */
  static createEntity(type: EntityType, id?: string): Entity {
    if (World.destroyed) {
      throw new Error("Cannot create entity on destroyed world");
    }

    const entityId = id !== undefined ? id : `client_${Date.now()}_${World.nextEntityId++}`;

    if (World.entities.has(entityId)) {
      console.warn(`Entity with ID ${entityId} already exists, returning existing entity`);
      return World.entities.get(entityId)!;
    }

    const entity = new Entity(entityId, type);
    World.entities.set(entity.id, entity);
    return entity;
  }

  /**
   * Get an entity by ID
   */
  static getEntity(id: string): Entity | undefined {
    return World.entities.get(id);
  }

  /**
   * Get all entities in the World
   */
  static getAllEntities(): Entity[] {
    return Array.from(World.entities.values());
  }

  /**
   * Get all entities of a specific type
   */
  static getEntitiesByType(type: EntityType): Entity[] {
    return Array.from(World.entities.values()).filter((entity) => entity.type === type);
  }

  /**
   * Query entities by component requirements
   */
  static queryEntities(query: ComponentQuery): Entity[] {
    return Array.from(World.entities.values()).filter((entity) => {
      const hasRequired = query.with.every((componentType) => entity.has(componentType));
      if (!hasRequired) return false;

      if (query.without) {
        const hasExcluded = query.without.some((componentType) => entity.has(componentType));
        if (hasExcluded) return false;
      }

      return true;
    });
  }

  /**
   * Query entities that have all specified component types
   */
  static queryEntitiesWithComponents(...componentTypes: (new (entityId: string, ...args: any[]) => Component)[]): Entity[] {
    return World.queryEntities({ with: componentTypes });
  }

  /**
   * Inform world that an entity's components have changed.
   * Entity will be re-filtered against all systems.
   * (Matches server's NttWorld.InformChangesFor)
   */
  static informChangesFor(entity: Entity): void {
    if (!World.destroyed) {
      World.changedEntities.add(entity);
    }
  }

  /**
   * Process all changed entities by notifying systems.
   * Called between each system update (like server's UpdateNTTs).
   */
  private static updateEntities(): void {
    World.changedEntities.forEach((entity) => {
      World.systems.forEach((system) => {
        system.entityChanged(entity);
      });
    });
    World.changedEntities.clear();
  }

  /**
   * Main World update loop - aligned with server NttWorld.UpdateSystems().
   *
   * Flow:
   * 1. OnBeginTick callbacks
   * 2. For each system:
   *    - Process entity changes
   *    - Execute system update
   * 3. Final entity change processing
   * 4. OnEndTick callbacks
   * 5. Increment tick
   */
  static update(deltaTime: number): void {
    if (World.destroyed) return;

    World.onBeginTick?.();

    for (const system of World.systems) {
      World.updateEntities();
      system.beginUpdate(deltaTime);
    }

    World.updateEntities();

    World.onEndTick?.();
    World.currentTick++;
  }

  /**
   * Destroy an entity and notify all systems
   */
  static destroyEntity(entityOrId: Entity | string): void {
    const entity = typeof entityOrId === "string" ? World.getEntity(entityOrId) : entityOrId;
    if (!entity) return;

    World.systems.forEach((system) => {
      system.entityDestroyed(entity);
    });

    World.entities.delete(entity.id);
    World.changedEntities.delete(entity);
  }

  /**
   * Get total entity count
   */
  static getEntityCount(): number {
    return World.entities.size;
  }

  /**
   * Get total system count
   */
  static getSystemCount(): number {
    return World.systems.length;
  }

  /**
   * Clear all entities and systems but keep World alive
   */
  static clear(): void {
    const entityIds = Array.from(World.entities.keys());
    entityIds.forEach((id) => World.destroyEntity(id));

    World.systems.forEach((system) => {
      if (system.cleanup) {
        system.cleanup();
      }
    });
    World.systems = [];

    World.changedEntities.clear();
    World.nextEntityId = 1;
    World.currentTick = 0n;
  }

  /**
   * Destroy the World completely
   */
  static destroy(): void {
    World.clear();
    World.destroyed = true;
  }

  /**
   * Check if World has been destroyed
   */
  static isDestroyed(): boolean {
    return World.destroyed;
  }
}
