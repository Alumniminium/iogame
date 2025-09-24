import { EntityType } from "./types";
import { Component } from "./Component";

declare class World {
  static notifyComponentChange(entity: Entity): void;
  static destroyEntity(entity: Entity): void;
}

export class Entity {
  readonly id: string;
  readonly type: EntityType;
  private components = new Map<string, Component>();

  constructor(id: string, type: EntityType) {
    this.id = id;
    this.type = type;
  }

  set<T extends Component>(component: T): void {
    const key = component.constructor.name;
    this.components.set(key, component);
    if ((globalThis as any).__WORLD_INSTANCE) {
      (globalThis as any).__WORLD_INSTANCE.notifyComponentChange(this);
    }
  }

  get<T extends Component>(
    componentClass: new (entityId: string, ...args: any[]) => T,
  ): T | undefined {
    return this.components.get(componentClass.name) as T;
  }

  has<T extends Component>(
    componentClass: new (entityId: string, ...args: any[]) => T,
  ): boolean {
    return this.components.has(componentClass.name);
  }

  hasAll<T extends Component>(
    ...componentClasses: (new (entityId: string, ...args: any[]) => T)[]
  ): boolean {
    return componentClasses.every((compClass) => this.has(compClass));
  }

  remove<T extends Component>(
    componentClass: new (entityId: string, ...args: any[]) => T,
  ): void {
    const key = componentClass.name;
    const removed = this.components.delete(key);
    if (removed) {
      if ((globalThis as any).__WORLD_INSTANCE) {
        (globalThis as any).__WORLD_INSTANCE.notifyComponentChange(this);
      }
    }
  }

  getAllComponents(): Component[] {
    return Array.from(this.components.values());
  }

  getCount(): number {
    return this.components.size;
  }

  destroy(): void {
    if ((globalThis as any).__WORLD_INSTANCE) {
      (globalThis as any).__WORLD_INSTANCE.destroyEntity(this);
    }
  }
}
