import { Entity } from './Entity';
import { Component } from './Component';

// Forward declare World to avoid circular import
declare class World {
  static queryEntitiesWithComponents<T extends Component>(...componentTypes: (new(entityId: number, ...args: any[]) => T)[]): Entity[];
  static getEntity(id: number): Entity | undefined;
  static createEntity(type: any, parentId?: number): Entity;
  static destroyEntity(entity: Entity | number): void;
}

export abstract class System {
  // Remove internal entity management - systems query World directly
  abstract readonly componentTypes: (new(entityId: number, ...args: any[]) => Component)[];

  // Optional lifecycle methods
  onEntityChanged?(entity: Entity): void;
  onEntityDestroyed?(entity: Entity): void;

  // Initialize system - called once when added to world
  initialize?(): void;

  // Cleanup system - called when system is removed or world is destroyed
  cleanup?(): void;

  // Main update method - now uses static World methods
  update(deltaTime: number): void {
    // Access World through global reference
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    if (!WorldClass) return;

    // Query entities with required components
    const entities = WorldClass.queryEntitiesWithComponents(...this.componentTypes);

    // Process each matching entity
    entities.forEach((entity: Entity) => {
      this.updateEntity(entity, deltaTime);
    });
  }

  protected abstract updateEntity(entity: Entity, deltaTime: number): void;

  // Utility method for systems that need to query specific component combinations
  protected queryEntities(componentTypes: (new(entityId: number, ...args: any[]) => Component)[]): Entity[] {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    return WorldClass ? WorldClass.queryEntitiesWithComponents(...componentTypes) : [];
  }

  // Utility methods for common World operations
  protected getEntity(id: number): Entity | undefined {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    return WorldClass ? WorldClass.getEntity(id) : undefined;
  }

  protected createEntity(type: any, parentId?: number): Entity {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    if (!WorldClass) throw new Error('World not initialized');
    return WorldClass.createEntity(type, parentId);
  }

  protected destroyEntity(entity: Entity | number): void {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    if (WorldClass) {
      WorldClass.destroyEntity(entity);
    }
  }
}