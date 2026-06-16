import { Container, Graphics } from "pixi.js";
import { System1 } from "../../core/System";
import { NTT } from "../../core/NTT";
import { ParticleSystemComponent } from "../../components/ParticleSystemComponent";
import { ImpactParticleManager } from "../../effects/ImpactParticleManager";
import { RenderComponent } from "../../components/RenderComponent";

export class ParticleRenderer extends System1<ParticleSystemComponent> {
  private gameContainer: Container;
  private impactParticleGraphic: Graphics | null = null;

  constructor(gameContainer: Container) {
    super(ParticleSystemComponent);
    this.gameContainer = gameContainer;
  }

  beginUpdate(deltaTime: number): void {
    for (const ntt of this._entitiesList) {
      const psc = ntt.get(ParticleSystemComponent)!;
      this.updateEntity(ntt, psc, deltaTime);
    }

    const impactManager = ImpactParticleManager.getInstance();
    impactManager.update(deltaTime);
    this.renderImpactParticles();
  }

  protected updateEntity(ntt: NTT, psc: ParticleSystemComponent, _deltaTime: number): void {
    this.renderParticles(ntt, psc);
  }

  private renderParticles(ntt: NTT, psc: ParticleSystemComponent): void {
    const render = ntt.get(RenderComponent);
    if (!render) return;

    let graphic = render.renderers.get(ParticleSystemComponent);
    if (!graphic) {
      graphic = new Graphics();
      this.gameContainer.addChild(graphic);
      render.renderers.set(ParticleSystemComponent, graphic);
    }

    graphic.clear();

    for (const particle of psc.particles) {
      if (particle.alpha <= 0) continue;

      const size = particle.size;
      const color = this.normalizeColor(particle.color);
      graphic.circle(particle.x, particle.y, size).fill({ color, alpha: particle.alpha });
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

    for (const particle of particles) {
      if (particle.alpha <= 0) continue;

      const color = this.normalizeColor(particle.color);
      this.impactParticleGraphic.circle(particle.x, particle.y, particle.size).fill({ color, alpha: particle.alpha });
    }
  }
}
