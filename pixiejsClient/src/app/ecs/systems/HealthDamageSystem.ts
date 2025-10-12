import { System2 } from "../core/System";
import { Entity } from "../core/Entity";
import { HealthComponent } from "../components/HealthComponent";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { ImpactParticleManager } from "../effects/ImpactParticleManager";

export class HealthDamageSystem extends System2<HealthComponent, PhysicsComponent> {
  private previousHealth = new Map<string, number>();

  constructor() {
    super(HealthComponent, PhysicsComponent);
  }

  protected updateEntity(entity: Entity, health: HealthComponent, physics: PhysicsComponent, _deltaTime: number): void {
    const previousValue = this.previousHealth.get(entity.id);

    if (previousValue !== undefined && health.Health < previousValue) {
      ImpactParticleManager.getInstance().spawnBurst(physics.position.x, physics.position.y, {
        count: 25,
        color: 0xcccccc,
        speed: 12,
        lifetime: 1.2,
        size: 0.3,
      });
    }

    this.previousHealth.set(entity.id, health.Health);
  }
}
