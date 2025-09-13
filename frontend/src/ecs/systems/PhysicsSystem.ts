import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { PhysicsComponent } from '../components/PhysicsComponent';

export class PhysicsSystem extends System {
  readonly componentTypes = [PhysicsComponent];

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const physics = entity.getComponent(PhysicsComponent)!;
    physics.update(deltaTime);
  }
}