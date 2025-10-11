import { Graphics } from "pixi.js";
import { BaseRenderer } from "./BaseRenderer";
import { Entity } from "../../core/Entity";
import { ParticleSystemComponent } from "../../components/ParticleSystemComponent";
import { World } from "../../core/World";
import { ImpactParticleManager } from "../../effects/ImpactParticleManager";

/**
 * Renders particle systems and one-off impact particles
 */
export class ParticleRenderer extends BaseRenderer {
  private impactParticleGraphic: Graphics | null = null;

  update(deltaTime: number): void {
    const entities = World.queryEntitiesWithComponents(ParticleSystemComponent);

    for (const entity of entities) {
      this.updateEntity(entity);
    }

    // Update and render impact particles
    const impactManager = ImpactParticleManager.getInstance();
    impactManager.update(deltaTime);
    this.renderImpactParticles();
  }

  private updateEntity(entity: Entity): void {
    const particleSystem = entity.get(ParticleSystemComponent)!;

    if (!particleSystem) return;

    this.renderParticles(entity, particleSystem);
  }

  private renderParticles(entity: Entity, particleSystem: ParticleSystemComponent): void {
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
      particleGraphic.circle(particle.x, particle.y, size).fill({ color, alpha: particle.alpha });
    }
  }

  private normalizeColor(color: number | undefined | null): number {
    if (color === undefined || color === null) return 0xffffff;
    return color & 0xffffff;
  }

  private renderImpactParticles(): void {
    if (!this.impactParticleGraphic) {
      this.impactParticleGraphic = new Graphics();
      this.gameContainer.addChild(this.impactParticleGraphic);
    }

    this.impactParticleGraphic.clear();

    const impactManager = ImpactParticleManager.getInstance();
    const particles = impactManager.getParticles();

    if (particles.length > 0) {
      console.log(`[ParticleRenderer] Rendering ${particles.length} impact particles`);
    }

    for (const particle of particles) {
      if (particle.alpha <= 0) continue;

      const color = this.normalizeColor(particle.color);
      this.impactParticleGraphic.circle(particle.x, particle.y, particle.size).fill({ color, alpha: particle.alpha });
    }
  }

  cleanup(): void {
    super.cleanup();

    if (this.impactParticleGraphic) {
      this.gameContainer.removeChild(this.impactParticleGraphic);
      this.impactParticleGraphic.destroy();
      this.impactParticleGraphic = null;
    }
  }
}
