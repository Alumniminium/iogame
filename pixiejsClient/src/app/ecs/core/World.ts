import { NTT } from "./NTT";
import { System } from "./System";
import { Component } from "./Component";
import { NetworkManager } from "../../network/NetworkManager";

/**
 * Component query specification
 */
import { NetworkComponent } from "../components/NetworkComponent";

export interface ComponentQuery {
  with: (new (ntt: NTT, ...args: any[]) => Component)[];
  without?: (new (ntt: NTT, ...args: any[]) => Component)[];
}

export class World {
  private static entities = new Map<string, NTT>();
  private static systems: System[] = [];
  private static changedEntities = new Set<NTT>();
  private static toBeRemoved = new Set<string>();
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

  static createEntity(id?: string): NTT {
    const entityId = id !== undefined ? id : crypto.randomUUID();

    if (World.entities.has(entityId)) {
      console.warn(`Entity with ID ${entityId} already exists, returning existing entity`);
      return World.entities.get(entityId)!;
    }

    const entity = new NTT(entityId);
    World.entities.set(entity.id, entity);
    return entity;
  }

  static getEntity(id: string): NTT | undefined {
    return World.entities.get(id);
  }

  static Me: NTT | undefined = undefined;

  static getAllEntities(): NTT[] {
    return Array.from(World.entities.values());
  }

  static queryEntities(query: ComponentQuery): NTT[] {
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

  static queryEntitiesWithComponents(...componentTypes: (new (ntt: NTT, ...args: any[]) => Component)[]): NTT[] {
    return World.queryEntities({ with: componentTypes });
  }

  static informChangesFor(entity: NTT): void {
    World.changedEntities.add(entity);
  }

  private static updateEntities(): void {
    // Process entities queued for removal
    World.toBeRemoved.forEach((ntt) => {
      World.destroyEntityInternal(ntt);
    });
    World.toBeRemoved.clear();

    // Process changed entities
    World.changedEntities.forEach((entity) => {
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
    // Process all queued network packets before updating entities
    NetworkManager.processPackets();

    World.onBeginTick?.();

    for (const system of World.systems) {
      World.updateEntities();
      system.beginUpdate(deltaTime);
    }

    World.updateEntities();

    World.onEndTick?.();
    World.currentTick++;
  }

  static destroyEntity(entityOrId: NTT | string): void {
    const entityId = typeof entityOrId === "string" ? entityOrId : entityOrId.id;
    if (!World.entities.has(entityId)) return;

    World.toBeRemoved.add(entityId);
  }

  private static destroyEntityInternal(ntt: string): void {
    const entity = World.entities.get(ntt);
    if (!entity) return;

    entity.recycle();

    World.systems.forEach((system) => {
      system.entityChanged(entity);
    });

    World.entities.delete(entity.id);
    World.changedEntities.delete(entity);
  }

  static getEntityCount(): number {
    return World.entities.size;
  }
}
