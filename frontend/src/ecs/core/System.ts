import { Entity } from './Entity';
import { Component } from './Component';

export abstract class System {
  protected entities: Entity[] = [];
  abstract readonly componentTypes: (new(entityId: number, ...args: any[]) => Component)[];

  update(deltaTime: number): void {
    this.entities.forEach(entity => {
      if (this.matchesEntity(entity)) {
        this.updateEntity(entity, deltaTime);
      }
    });
  }

  private matchesEntity(entity: Entity): boolean {
    return this.componentTypes.every(compType => entity.hasComponent(compType));
  }

  protected abstract updateEntity(entity: Entity, deltaTime: number): void;

  onEntityChanged(entity: Entity): void {
    if (this.matchesEntity(entity)) {
      if (!this.entities.includes(entity)) {
        this.entities.push(entity);
      }
    } else {
      const index = this.entities.indexOf(entity);
      if (index > -1) {
        this.entities.splice(index, 1);
      }
    }
  }
}