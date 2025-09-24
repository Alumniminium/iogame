import { Entity } from "./Entity";
import { Component } from "./Component";
import { EntityType } from "./types";

export abstract class System {
  abstract readonly componentTypes: (new (...args: unknown[]) => Component)[];

  onEntityChanged?(entity: Entity): void;
  onEntityDestroyed?(entity: Entity): void;

  initialize?(): void;

  cleanup?(): void;

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

  protected abstract updateEntity(entity: Entity, deltaTime: number): void;

  protected queryEntities(
    componentTypes: (new (...args: unknown[]) => Component)[],
  ): Entity[] {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    return WorldClass
      ? WorldClass.queryEntitiesWithComponents(...componentTypes)
      : [];
  }

  protected getEntity(id: string): Entity | undefined {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    return WorldClass ? WorldClass.getEntity(id) : undefined;
  }

  protected createEntity(type: EntityType, parentId?: string): Entity {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    if (!WorldClass) throw new Error("World not initialized");
    return WorldClass.createEntity(type, parentId);
  }

  protected destroyEntity(entity: Entity | string): void {
    const WorldClass = (globalThis as any).__WORLD_CLASS;
    if (WorldClass) {
      WorldClass.destroyEntity(entity);
    }
  }
}
