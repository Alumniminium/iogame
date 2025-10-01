import { Graphics } from "pixi.js";
import { BaseRenderer } from "./BaseRenderer";
import { Entity } from "../../core/Entity";
import { ParticleSystemComponent } from "../../components/ParticleSystemComponent";
import { World } from "../../core/World";

/**
 * Renders particle systems
 */
export class ParticleRenderer extends BaseRenderer {
  update(_deltaTime: number): void {
    const entities = World.queryEntitiesWithComponents(ParticleSystemComponent);

    for (const entity of entities) {
      this.updateEntity(entity);
    }
  }

  private updateEntity(entity: Entity): void {
    const particleSystem = entity.get(ParticleSystemComponent)!;

    if (!particleSystem) return;

    this.renderParticles(entity, particleSystem);
  }

  private renderParticles(
    entity: Entity,
    particleSystem: ParticleSystemComponent,
  ): void {
    let particleGraphic = this.graphics.get(entity.id);
    if (!particleGraphic) {
      particleGraphic = new Graphics();
      this.graphics.set(entity.id, particleGraphic);
      this.gameContainer.addChild(particleGraphic);
    }

    particleGraphic.clear();

    for (const particle of particleSystem.particles) {
      if (particle.alpha <= 0) continue;

      const size = particle.size;
      const color = this.normalizeColor(particle.color);
      particleGraphic
        .circle(particle.x, particle.y, size)
        .fill({ color, alpha: particle.alpha });
    }
  }

  private normalizeColor(color: number | undefined | null): number {
    if (color === undefined || color === null) return 0xffffff;
    return color & 0xffffff;
  }
}
