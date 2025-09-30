import { EntityType } from "./types";
import { Component } from "./Component";

/**
 * Entity represents a game object in the ECS architecture.
 * Entities are containers for components and are identified by unique IDs.
 */
export class Entity {
  readonly id: string;
  readonly type: EntityType;
  private components = new Map<string, Component>();

  constructor(id: string, type: EntityType) {
    this.id = id;
    this.type = type;
  }

  /**
   * Adds or updates a component on this entity.
   * Notifies the World of the component change.
   */
  set<T extends Component>(component: T): void {
    const key = component.constructor.name;
    this.components.set(key, component);
    if ((globalThis as any).__WORLD_INSTANCE)
      (globalThis as any).__WORLD_INSTANCE.notifyComponentChange(this);
  }

  /**
   * Retrieves a component of the specified type from this entity.
   * @returns The component instance, or undefined if not found
   */
  get<T extends Component>(
    componentClass: new (entityId: string, ...args: any[]) => T,
  ): T | undefined {
    return this.components.get(componentClass.name) as T;
  }

  /**
   * Checks if this entity has a component of the specified type.
   */
  has<T extends Component>(
    componentClass: new (entityId: string, ...args: any[]) => T,
  ): boolean {
    return this.components.has(componentClass.name);
  }

  /**
   * Checks if this entity has all of the specified component types.
   */
  hasAll<T extends Component>(
    ...componentClasses: (new (entityId: string, ...args: any[]) => T)[]
  ): boolean {
    return componentClasses.every((compClass) => this.has(compClass));
  }

  /**
   * Removes a component of the specified type from this entity.
   * Notifies the World if the component was successfully removed.
   */
  remove<T extends Component>(
    componentClass: new (entityId: string, ...args: any[]) => T,
  ): void {
    const key = componentClass.name;
    const removed = this.components.delete(key);
    if (removed) {
      if ((globalThis as any).__WORLD_INSTANCE)
        (globalThis as any).__WORLD_INSTANCE.notifyComponentChange(this);
    }
  }

  /**
   * Returns all components attached to this entity.
   */
  getAllComponents(): Component[] {
    return Array.from(this.components.values());
  }

  /**
   * Returns the number of components attached to this entity.
   */
  getCount(): number {
    return this.components.size;
  }

  /**
   * Destroys this entity, removing it from the World.
   */
  destroy(): void {
    if ((globalThis as any).__WORLD_INSTANCE)
      (globalThis as any).__WORLD_INSTANCE.destroyEntity(this);
  }
}
