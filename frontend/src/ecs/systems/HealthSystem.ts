import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { HealthComponent } from '../components/HealthComponent';
import { HealthRegenComponent } from '../components/HealthRegenComponent';

export class HealthSystem extends System {
  readonly componentTypes = [HealthComponent, HealthRegenComponent];

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const health = entity.getComponent(HealthComponent)!;
    const regen = entity.getComponent(HealthRegenComponent)!;

    if (health.health < health.maxHealth) {
      health.heal(regen.passiveHealPerSec * deltaTime);
    }
  }
}