import { Entity } from "./Entity";
import { System } from "./System";
import { EntityType } from "./types";
import { Component } from "./Component";

export interface ComponentQuery {
  with: (new (entityId: string, ...args: unknown[]) => Component)[];
  without?: (new (entityId: string, ...args: unknown[]) => Component)[];
}

export interface SystemDefinition {
  system: System;
  dependencies?: string[];
  priority?: number;
}

export class World {
  private static instance: World | null = null;
  private static entities = new Map<string, Entity>();
  private static systems = new Map<string, SystemDefinition>();
  private static systemExecutionOrder: System[] = [];
  private static entityComponentCache = new Map<string, Set<Entity>>();
  private static changedEntities = new Set<Entity>();
  private static nextEntityId = 1;
  private static destroyed = false;

  private constructor() {}

  static getInstance(): World {
    if (!World.instance) {
      World.instance = new World();
      (globalThis as any).__WORLD_INSTANCE = World;
      (globalThis as any).__WORLD_CLASS = World;
    }
    return World.instance;
  }

  static initialize(): void {
    if (!World.instance) {
      World.instance = new World();
      (globalThis as any).__WORLD_INSTANCE = World;
      (globalThis as any).__WORLD_CLASS = World;
    }
  }

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

  static removeSystem(name: string): void {
    World.systems.delete(name);
    World.rebuildSystemOrder();
  }

  static getSystem<T extends System>(name: string): T | undefined {
    return World.systems.get(name)?.system as T;
  }

  static getSystems(): System[] {
    return World.systemExecutionOrder.slice(); // Return a copy to prevent external modification
  }

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
      return bPriority - aPriority; // Higher priority first
    });

    systemNames.forEach((name) => visit(name));
    World.systemExecutionOrder = sorted;
  }

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

  static getEntity(id: string): Entity | undefined {
    return World.entities.get(id);
  }

  static getAllEntities(): Entity[] {
    return Array.from(World.entities.values());
  }

  static getEntitiesByType(type: EntityType): Entity[] {
    return Array.from(World.entities.values()).filter(
      (entity) => entity.type === type,
    );
  }

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

  static queryEntitiesWithComponents<T extends Component>(
    ...componentTypes: (new (entityId: string, ...args: unknown[]) => T)[]
  ): Entity[] {
    return World.queryEntities({ with: componentTypes });
  }

  static notifyComponentChange(entity: Entity): void {
    if (!World.destroyed) {
      World.changedEntities.add(entity);
    }
  }

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

    World.systemExecutionOrder.forEach((system) => {
      if (!World.destroyed) {
        system.update(deltaTime);
      }
    });
  }

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

  static getEntityCount(): number {
    return World.entities.size;
  }

  static getSystemCount(): number {
    return World.systems.size;
  }

  static clear(): void {
    const entityIds = Array.from(World.entities.keys());
    entityIds.forEach((id) => World.destroyEntity(id));

    World.systems.clear();
    World.systemExecutionOrder = [];

    World.entityComponentCache.clear();
    World.changedEntities.clear();

    World.nextEntityId = 1;
  }

  static destroy(): void {
    World.clear();
    World.destroyed = true;
  }

  static isDestroyed(): boolean {
    return World.destroyed;
  }
}
