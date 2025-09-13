import { Entity } from './Entity';
import { System } from './System';
import { EntityType } from './types';

export class World {
  static instance: World;

  private entities = new Map<number, Entity>();
  private systems: System[] = [];
  private changedEntities = new Set<Entity>();
  private nextEntityId = 1;

  constructor() {
    World.instance = this;
  }

  addSystem(system: System): void {
    this.systems.push(system);
  }

  createEntity(type: EntityType, parentId = -1): Entity {
    const entity = new Entity(this.nextEntityId++, type, parentId);
    this.entities.set(entity.id, entity);
    return entity;
  }

  getEntity(id: number): Entity | undefined {
    return this.entities.get(id);
  }

  notifyComponentChange(entity: Entity): void {
    this.changedEntities.add(entity);
  }

  update(deltaTime: number): void {
    // Process changed entities
    this.changedEntities.forEach(entity => {
      this.systems.forEach(system => system.onEntityChanged(entity));
    });
    this.changedEntities.clear();

    // Update all systems
    this.systems.forEach(system => system.update(deltaTime));
  }

  destroyEntity(entity: Entity): void {
    // Remove from systems
    this.systems.forEach(system => system.onEntityChanged(entity));

    // Destroy children
    entity.getChildren().forEach(child => this.destroyEntity(child));

    // Remove from world
    this.entities.delete(entity.id);
  }

  getEntitiesByType(type: EntityType): Entity[] {
    return Array.from(this.entities.values()).filter(entity => entity.type === type);
  }
}