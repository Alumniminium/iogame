import { Entity } from "./Entity";
import { System } from "./System";
import { EntityType } from "./types";
import { Component } from "./Component";

/**
 * Query specification for filtering entities by component types
 */
export interface ComponentQuery {
  with: (new (entityId: string, ...args: any[]) => Component)[];
  without?: (new (entityId: string, ...args: any[]) => Component)[];
}

/**
 * System registration with dependency and priority information
 */
export interface SystemDefinition {
  system: System;
  dependencies?: string[];
  priority?: number;
}

/**
 * Central ECS World coordinator managing all entities and systems.
 * Implements singleton pattern for global access.
 * Handles entity lifecycle, system registration, and update loop orchestration.
 */
export class World {
  private static instance: World | null = null;
  private static entities = new Map<string, Entity>();
  private static systems = new Map<string, SystemDefinition>();
  private static systemExecutionOrder: System[] = [];
  private static changedEntities = new Set<Entity>();
  private static nextEntityId = 1;
  private static destroyed = false;
  public static currentTick = 0n;

  private constructor() {}

  /**
   * Get the singleton World instance, creating it if necessary
   */
  static getInstance(): World {
    if (!World.instance) {
      World.instance = new World();
      (globalThis as any).__WORLD_INSTANCE = World;
      (globalThis as any).__WORLD_CLASS = World;
    }
    return World.instance;
  }

  /**
   * Initialize the World singleton explicitly
   */
  static initialize(): void {
    if (!World.instance) {
      World.instance = new World();
      (globalThis as any).__WORLD_INSTANCE = World;
      (globalThis as any).__WORLD_CLASS = World;
    }
  }

  /**
   * Register a system with the World
   * @param name Unique identifier for the system
   * @param system The system instance
   * @param dependencies Names of systems that must run before this one
   * @param priority Higher priority systems run first (default 0)
   */
  static addSystem(
    name: string,
    system: System,
    dependencies: string[] = [],
    priority: number = 0,
  ): void {
    const definition: SystemDefinition = { system, dependencies, priority };
    World.systems.set(name, definition);
    World.rebuildSystemOrder();
  }

  /**
   * Remove a system from the World
   */
  static removeSystem(name: string): void {
    World.systems.delete(name);
    World.rebuildSystemOrder();
  }

  /**
   * Get a system by name
   */
  static getSystem<T extends System>(name: string): T | undefined {
    return World.systems.get(name)?.system as T;
  }

  /**
   * Get all registered systems in execution order
   */
  static getSystems(): System[] {
    return World.systemExecutionOrder.slice();
  }

  /**
   * Rebuild system execution order based on dependencies and priorities.
   * Uses topological sort to respect dependencies.
   */
  private static rebuildSystemOrder(): void {
    const sorted: System[] = [];
    const visited = new Set<string>();
    const visiting = new Set<string>();

    const visit = (name: string) => {
      if (visiting.has(name)) {
        throw new Error(`Circular dependency detected in system: ${name}`);
      }
      if (visited.has(name)) return;

      visiting.add(name);
      const definition = World.systems.get(name);
      if (definition) {
        definition.dependencies?.forEach((dep) => {
          if (World.systems.has(dep)) {
            visit(dep);
          }
        });
        sorted.push(definition.system);
      }
      visiting.delete(name);
      visited.add(name);
    };

    const systemNames = Array.from(World.systems.keys()).sort((a, b) => {
      const aPriority = World.systems.get(a)!.priority || 0;
      const bPriority = World.systems.get(b)!.priority || 0;
      return bPriority - aPriority;
    });

    systemNames.forEach((name) => visit(name));
    World.systemExecutionOrder = sorted;
  }

  /**
   * Create a new entity
   * @param type The entity type
   * @param id Optional custom ID, otherwise auto-generated
   */
  static createEntity(type: EntityType, id?: string): Entity {
    if (World.destroyed) {
      throw new Error("Cannot create entity on destroyed world");
    }

    const entityId =
      id !== undefined ? id : `client_${Date.now()}_${World.nextEntityId++}`;

    if (World.entities.has(entityId)) {
      console.warn(
        `Entity with ID ${entityId} already exists, returning existing entity`,
      );
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
    return Array.from(World.entities.values()).filter(
      (entity) => entity.type === type,
    );
  }

  /**
   * Query entities by component requirements
   */
  static queryEntities(query: ComponentQuery): Entity[] {
    return Array.from(World.entities.values()).filter((entity) => {
      const hasRequired = query.with.every((componentType) =>
        entity.has(componentType),
      );

      if (!hasRequired) return false;

      if (query.without) {
        const hasExcluded = query.without.some((componentType) =>
          entity.has(componentType),
        );
        if (hasExcluded) return false;
      }

      return true;
    });
  }

  /**
   * Query entities that have all specified component types
   */
  static queryEntitiesWithComponents<T extends Component>(
    ...componentTypes: (new (entityId: string, ...args: any[]) => T)[]
  ): Entity[] {
    return World.queryEntities({ with: componentTypes });
  }

  /**
   * Notify systems that an entity's components have changed
   */
  static notifyComponentChange(entity: Entity): void {
    if (!World.destroyed) {
      World.changedEntities.add(entity);
    }
  }

  /**
   * Main World update loop - processes all changed entities and runs all systems
   * @param deltaTime Time elapsed since last frame in seconds
   */
  static update(deltaTime: number): void {
    if (World.destroyed) return;

    World.changedEntities.forEach((entity) => {
      World.systemExecutionOrder.forEach((system) => {
        if (system.onEntityChanged) {
          system.onEntityChanged(entity);
        }
      });
    });
    World.changedEntities.clear();

    World.currentTick++;

    World.systemExecutionOrder.forEach((system) => {
      if (!World.destroyed) {
        system.update(deltaTime);
      }
    });
  }

  /**
   * Destroy an entity and notify all systems
   */
  static destroyEntity(entityOrId: Entity | string): void {
    const entity =
      typeof entityOrId === "string" ? World.getEntity(entityOrId) : entityOrId;

    if (!entity) return;

    World.systemExecutionOrder.forEach((system) => {
      if (system.onEntityDestroyed) {
        system.onEntityDestroyed(entity);
      }
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
    return World.systems.size;
  }

  /**
   * Clear all entities and systems but keep World instance alive
   */
  static clear(): void {
    const entityIds = Array.from(World.entities.keys());
    entityIds.forEach((id) => World.destroyEntity(id));

    World.systems.clear();
    World.systemExecutionOrder = [];

    World.changedEntities.clear();

    World.nextEntityId = 1;
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
