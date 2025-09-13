import { EntityType } from './types';
import { Component } from './Component';
import { World } from './World';

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

  addComponent<T extends Component>(component: T): void {
    const key = component.constructor.name;
    this.components.set(key, component);
    World.instance.notifyComponentChange(this);
  }

  getComponent<T extends Component>(componentClass: new(entityId: number, ...args: any[]) => T): T | undefined {
    return this.components.get(componentClass.name) as T;
  }

  hasComponent<T extends Component>(componentClass: new(entityId: number, ...args: any[]) => T): boolean {
    return this.components.has(componentClass.name);
  }

  hasComponents<T extends Component>(...componentClasses: (new(entityId: number, ...args: any[]) => T)[]): boolean {
    return componentClasses.every(compClass => this.hasComponent(compClass));
  }

  removeComponent<T extends Component>(componentClass: new(entityId: number, ...args: any[]) => T): void {
    this.components.delete(componentClass.name);
    World.instance.notifyComponentChange(this);
  }

  addChild(child: Entity): void {
    this.children.push(child);
  }

  getChildren(): Entity[] {
    return [...this.children];
  }

  destroy(): void {
    World.instance.destroyEntity(this);
  }
}