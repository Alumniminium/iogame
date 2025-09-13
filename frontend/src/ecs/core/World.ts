import { Entity } from './Entity';
import { System } from './System';
import { EntityType } from './types';
import { Component } from './Component';

export interface ComponentQuery {
  with: (new(entityId: number, ...args: any[]) => Component)[];
  without?: (new(entityId: number, ...args: any[]) => Component)[];
}

export interface SystemDefinition {
  system: System;
  dependencies?: string[];
  priority?: number;
}

export class World {
  private static instance: World | null = null;
  private static entities = new Map<number, Entity>();
  private static systems = new Map<string, SystemDefinition>();
  private static systemExecutionOrder: System[] = [];
  private static entityComponentCache = new Map<string, Set<Entity>>();
  private static changedEntities = new Set<Entity>();
  private static nextEntityId = 1;
  private static destroyed = false;

  private constructor() {
    // Private constructor for singleton
  }

  static getInstance(): World {
    if (!World.instance) {
      World.instance = new World();
      // Set global references for circular dependency resolution
      (globalThis as any).__WORLD_INSTANCE = World;
      (globalThis as any).__WORLD_CLASS = World;
    }
    return World.instance;
  }

  static initialize(): void {
    if (!World.instance) {
      World.instance = new World();
      // Set global references for circular dependency resolution
      (globalThis as any).__WORLD_INSTANCE = World;
      (globalThis as any).__WORLD_CLASS = World;
    }
  }

  static addSystem(name: string, system: System, dependencies: string[] = [], priority: number = 0): void {
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
        definition.dependencies?.forEach(dep => {
          if (World.systems.has(dep)) {
            visit(dep);
          }
        });
        sorted.push(definition.system);
      }
      visiting.delete(name);
      visited.add(name);
    };

    // Sort by priority first, then resolve dependencies
    const systemNames = Array.from(World.systems.keys())
      .sort((a, b) => {
        const aPriority = World.systems.get(a)!.priority || 0;
        const bPriority = World.systems.get(b)!.priority || 0;
        return bPriority - aPriority; // Higher priority first
      });

    systemNames.forEach(name => visit(name));
    World.systemExecutionOrder = sorted;
  }

  static createEntity(type: EntityType, id?: number, parentId = -1): Entity {
    if (World.destroyed) {
      throw new Error('Cannot create entity on destroyed world');
    }

    // Use provided ID or generate a new one
    const entityId = id !== undefined ? id : World.nextEntityId++;

    // Update nextEntityId if we're using a specific ID that's higher
    if (id !== undefined && id >= World.nextEntityId) {
      World.nextEntityId = id + 1;
    }

    // Check if entity with this ID already exists
    if (World.entities.has(entityId)) {
      console.warn(`Entity with ID ${entityId} already exists, returning existing entity`);
      return World.entities.get(entityId)!;
    }

    const entity = new Entity(entityId, type, parentId);
    World.entities.set(entity.id, entity);

    // Set up parent-child relationship
    if (parentId !== -1) {
      const parent = World.getEntity(parentId);
      if (parent) {
        parent.addChild(entity);
      }
    }

    return entity;
  }

  static getEntity(id: number): Entity | undefined {
    return World.entities.get(id);
  }

  static getAllEntities(): Entity[] {
    return Array.from(World.entities.values());
  }

  static getEntitiesByType(type: EntityType): Entity[] {
    return Array.from(World.entities.values()).filter(entity => entity.type === type);
  }

  static queryEntities(query: ComponentQuery): Entity[] {
    return Array.from(World.entities.values()).filter(entity => {
      // Check required components
      const hasRequired = query.with.every(componentType =>
        entity.hasComponent(componentType)
      );

      if (!hasRequired) return false;

      // Check excluded components
      if (query.without) {
        const hasExcluded = query.without.some(componentType =>
          entity.hasComponent(componentType)
        );
        if (hasExcluded) return false;
      }

      return true;
    });
  }

  static queryEntitiesWithComponents<T extends Component>(
    ...componentTypes: (new(entityId: number, ...args: any[]) => T)[]
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

    // Process changed entities - notify systems about component changes
    World.changedEntities.forEach(entity => {
      World.systemExecutionOrder.forEach(system => {
        if (system.onEntityChanged) {
          system.onEntityChanged(entity);
        }
      });
    });
    World.changedEntities.clear();

    // Update all systems in dependency order
    World.systemExecutionOrder.forEach(system => {
      if (!World.destroyed) {
        system.update(deltaTime);
      }
    });
  }

  static destroyEntity(entityOrId: Entity | number): void {
    const entity = typeof entityOrId === 'number'
      ? World.getEntity(entityOrId)
      : entityOrId;

    if (!entity) return;

    // Notify systems about entity destruction
    World.systemExecutionOrder.forEach(system => {
      if (system.onEntityDestroyed) {
        system.onEntityDestroyed(entity);
      }
    });

    // Destroy children first
    const children = [...entity.getChildren()]; // Copy array to avoid modification during iteration
    children.forEach(child => World.destroyEntity(child));

    // Remove from parent if it has one
    if (entity.parentId !== -1) {
      const parent = World.getEntity(entity.parentId);
      if (parent) {
        parent.removeChild(entity);
      }
    }

    // Remove from world
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
    // Destroy all entities
    const entityIds = Array.from(World.entities.keys());
    entityIds.forEach(id => World.destroyEntity(id));

    // Clear systems
    World.systems.clear();
    World.systemExecutionOrder = [];

    // Clear caches
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