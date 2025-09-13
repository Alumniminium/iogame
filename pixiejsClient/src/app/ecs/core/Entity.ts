import { EntityType } from "./types";
import { Component } from "./Component";

// Forward declare World to avoid circular import
declare class World {
  static notifyComponentChange(entity: Entity): void;
  static destroyEntity(entity: Entity): void;
}

export class Entity {
  readonly id: number;
  readonly type: EntityType;
  readonly parentId: number;
  private components = new Map<string, Component>();
  private children: Entity[] = [];

  constructor(id: number, type: EntityType, parentId = -1) {
    this.id = id;
    this.type = type;
    this.parentId = parentId;
  }

  set<T extends Component>(component: T): void {
    const key = component.constructor.name;
    this.components.set(key, component);
    // Access World through global reference set by World itself
    if ((globalThis as any).__WORLD_INSTANCE) {
      (globalThis as any).__WORLD_INSTANCE.notifyComponentChange(this);
    }
  }

  get<T extends Component>(
    componentClass: new (entityId: number, ...args: any[]) => T,
  ): T | undefined {
    return this.components.get(componentClass.name) as T;
  }

  has<T extends Component>(
    componentClass: new (entityId: number, ...args: any[]) => T,
  ): boolean {
    return this.components.has(componentClass.name);
  }

  hasAll<T extends Component>(
    ...componentClasses: (new (entityId: number, ...args: any[]) => T)[]
  ): boolean {
    return componentClasses.every((compClass) => this.has(compClass));
  }

  remove<T extends Component>(
    componentClass: new (entityId: number, ...args: any[]) => T,
  ): void {
    const key = componentClass.name;
    const removed = this.components.delete(key);
    if (removed) {
      // Access World through global reference set by World itself
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

  addChild(child: Entity): void {
    if (!this.children.includes(child)) {
      this.children.push(child);
    }
  }

  removeChild(child: Entity): void {
    const index = this.children.indexOf(child);
    if (index > -1) {
      this.children.splice(index, 1);
    }
  }

  getChildren(): Entity[] {
    return [...this.children];
  }

  getChildCount(): number {
    return this.children.length;
  }

  hasChildren(): boolean {
    return this.children.length > 0;
  }

  destroy(): void {
    // Access World through global reference set by World itself
    if ((globalThis as any).__WORLD_INSTANCE) {
      (globalThis as any).__WORLD_INSTANCE.destroyEntity(this);
    }
  }
}
