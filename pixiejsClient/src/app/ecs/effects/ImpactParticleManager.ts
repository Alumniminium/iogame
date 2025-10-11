interface ImpactParticle {
  x: number;
  y: number;
  velocityX: number;
  velocityY: number;
  life: number;
  maxLife: number;
  alpha: number;
  color: number;
  size: number;
}

/**
 * Manages one-off particle bursts for impact effects (damage, collisions, etc.)
 * Separate from entity-based ParticleSystemComponent for temporary effects
 */
export class ImpactParticleManager {
  private static instance: ImpactParticleManager;
  private particles: ImpactParticle[] = [];

  private constructor() {}

  static getInstance(): ImpactParticleManager {
    if (!ImpactParticleManager.instance)
      ImpactParticleManager.instance = new ImpactParticleManager();
    return ImpactParticleManager.instance;
  }

  /**
   * Spawn a burst of particles at a specific position
   */
  spawnBurst(
    x: number,
    y: number,
    config: {
      count?: number;
      color?: number;
      speed?: number;
      lifetime?: number;
      size?: number;
      spread?: number;
    } = {},
  ): void {
    const count = config.count ?? 12;
    const color = config.color ?? 0x808080; // Gray
    const baseSpeed = config.speed ?? 8;
    const lifetime = config.lifetime ?? 0.6;
    const size = config.size ?? 0.15;

    for (let i = 0; i < count; i++) {
      const angle = (Math.PI * 2 * i) / count + (Math.random() - 0.5) * 0.3;
      const speed = baseSpeed * (0.7 + Math.random() * 0.6);

      const particle: ImpactParticle = {
        x,
        y,
        velocityX: Math.cos(angle) * speed,
        velocityY: Math.sin(angle) * speed,
        life: lifetime * (0.8 + Math.random() * 0.4),
        maxLife: lifetime,
        alpha: 1.0,
        color,
        size,
      };

      this.particles.push(particle);
    }
  }

  /**
   * Update all active particles
   */
  update(deltaTime: number): void {
    for (let i = this.particles.length - 1; i >= 0; i--) {
      const particle = this.particles[i];

      // Update position
      particle.x += particle.velocityX * deltaTime;
      particle.y += particle.velocityY * deltaTime;

      // Apply drag
      particle.velocityX *= 0.96;
      particle.velocityY *= 0.96;

      // Update life
      particle.life -= deltaTime;

      // Fade out based on remaining life
      const lifeRatio = particle.life / particle.maxLife;
      particle.alpha = Math.pow(lifeRatio, 0.6); // Slower fade
      particle.size = particle.size * (0.7 + lifeRatio * 0.3);

      // Remove dead particles
      if (particle.life <= 0) this.particles.splice(i, 1);
    }
  }

  /**
   * Get all active particles for rendering
   */
  getParticles(): readonly ImpactParticle[] {
    return this.particles;
  }

  /**
   * Clear all particles
   */
  clear(): void {
    this.particles = [];
  }
}
