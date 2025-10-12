import { Entity } from "./Entity";
import { System } from "./System";
import { EntityType } from "./types";
import { Component } from "./Component";

/**
 * Component query specification
 */
import { NetworkComponent } from "../components/NetworkComponent";

export interface ComponentQuery {
  with: (new (entityId: string, ...args: any[]) => Component)[];
  without?: (new (entityId: string, ...args: any[]) => Component)[];
}

export class World {
  private static entities = new Map<string, Entity>();
  private static systems: System[] = [];
  private static changedEntities = new Set<Entity>();
  public static currentTick = 0n;

  private static onBeginTick?: () => void;
  private static onEndTick?: () => void;

  static initialize(): void {
    (globalThis as any).__WORLD_CLASS = World;
  }

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

  static registerOnBeginTick(callback: () => void): void {
    World.onBeginTick = callback;
  }

  static registerOnEndTick(callback: () => void): void {
    World.onEndTick = callback;
  }

  static createEntity(type: EntityType, id?: string): Entity {
    const entityId = id !== undefined ? id : crypto.randomUUID();

    if (World.entities.has(entityId)) {
      console.warn(`Entity with ID ${entityId} already exists, returning existing entity`);
      return World.entities.get(entityId)!;
    }

    const entity = new Entity(entityId, type);
    World.entities.set(entity.id, entity);
    return entity;
  }

  static getEntity(id: string): Entity | undefined {
    return World.entities.get(id);
  }

  static Me: Entity | undefined = undefined;

  static getAllEntities(): Entity[] {
    return Array.from(World.entities.values());
  }

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

  static queryEntitiesWithComponents(...componentTypes: (new (entityId: string, ...args: any[]) => Component)[]): Entity[] {
    return World.queryEntities({ with: componentTypes });
  }

  static informChangesFor(entity: Entity): void {
    World.changedEntities.add(entity);
  }

  private static updateEntities(): void {
    World.changedEntities.forEach((entity) => {
      // Update World.Me if this entity has isLocallyControlled=true
      const network = entity.get(NetworkComponent);
      if (network?.isLocallyControlled) {
        World.Me = entity;
      }

      World.systems.forEach((system) => {
        system.entityChanged(entity);
      });
    });
    World.changedEntities.clear();
  }

  static update(deltaTime: number): void {
    World.onBeginTick?.();

    for (const system of World.systems) {
      World.updateEntities();
      system.beginUpdate(deltaTime);
    }

    World.updateEntities();

    World.onEndTick?.();
    World.currentTick++;
  }

  static destroyEntity(entityOrId: Entity | string): void {
    const entity = typeof entityOrId === "string" ? World.getEntity(entityOrId) : entityOrId;
    if (!entity) return;

    World.systems.forEach((system) => {
      system.entityDestroyed(entity);
    });

    World.entities.delete(entity.id);
    World.changedEntities.delete(entity);
  }

  static getEntityCount(): number {
    return World.entities.size;
  }
}
