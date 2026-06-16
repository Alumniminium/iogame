import { System2 } from "../core/System";
import { NTT } from "../core/NTT";
import { HealthComponent } from "../components/HealthComponent";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { ImpactParticleManager } from "../effects/ImpactParticleManager";

export class HealthDamageSystem extends System2<HealthComponent, PhysicsComponent> {
  private previousHealth = new Map<string, number>();

  constructor() {
    super(HealthComponent, PhysicsComponent);
  }

  protected updateEntity(ntt: NTT, hc: HealthComponent, phy: PhysicsComponent, _deltaTime: number): void {
    const previousValue = this.previousHealth.get(ntt.id);

    if (previousValue !== undefined && hc.Health < previousValue) {
      ImpactParticleManager.getInstance().spawnBurst(phy.position.x, phy.position.y, {
        count: 25,
        color: 0xcccccc,
        speed: 12,
        lifetime: 1.2,
        size: 0.3,
      });
    }

    this.previousHealth.set(ntt.id, hc.Health);
  }
}
