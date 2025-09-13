import { Entity } from '../../ecs/core/Entity';
import { EntityType } from '../../ecs/core/types';
import { HealthComponent } from '../../ecs/components/HealthComponent';
import { PhysicsComponent } from '../../ecs/components/PhysicsComponent';
import { LevelComponent } from '../../ecs/components/LevelComponent';
import { EnergyComponent } from '../../ecs/components/EnergyComponent';
import { HealthRegenComponent } from '../../ecs/components/HealthRegenComponent';

export class Player extends Entity {
  constructor(id: number, position: { x: number; y: number }) {
    super(id, EntityType.Player);

    // Add all player components
    this.addComponent(new HealthComponent(id, 100, 100));
    this.addComponent(new PhysicsComponent(id, position, 50));
    this.addComponent(new LevelComponent(id, 1, 0, 100));
    this.addComponent(new EnergyComponent(id, 1000, 10));
    this.addComponent(new HealthRegenComponent(id, 5));
  }

  move(direction: { x: number; y: number }): void {
    const physics = this.getComponent(PhysicsComponent);
    if (physics) {
      physics.acceleration.x = direction.x * 100;
      physics.acceleration.y = direction.y * 100;
    }
  }

  stop(): void {
    const physics = this.getComponent(PhysicsComponent);
    if (physics) {
      physics.acceleration.x = 0;
      physics.acceleration.y = 0;
    }
  }

  takeDamage(amount: number): void {
    const health = this.getComponent(HealthComponent);
    if (health) {
      health.takeDamage(amount);
    }
  }

  addExperience(amount: number): void {
    const level = this.getComponent(LevelComponent);
    if (level) {
      level.addExperience(amount);
    }
  }
}