import { Component } from "../core/Component";

export interface Particle {
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

export interface ParticleSystemConfig {
  maxParticles?: number;
  emissionRate?: number;
  particleLifetime?: number;
  startSize?: number;
  endSize?: number;
  startColor?: number;
  endColor?: number;
  velocityVariance?: number;
  spread?: number;
}

export class ParticleSystemComponent extends Component {
  particles: Particle[] = [];
  maxParticles: number;
  emissionRate: number; // particles per second
  particleLifetime: number; // seconds
  startSize: number;
  endSize: number;
  startColor: number;
  endColor: number;
  velocityVariance: number;
  spread: number; // angle spread in radians

  private timeSinceLastEmission: number = 0;

  constructor(entityId: string, config: ParticleSystemConfig = {}) {
    super(entityId);

    this.maxParticles = config.maxParticles || 50;
    this.emissionRate = config.emissionRate || 30; // 30 particles per second
    this.particleLifetime = config.particleLifetime || 0.8;
    this.startSize = config.startSize || 0.3;
    this.endSize = config.endSize || 0.1;
    this.startColor = config.startColor || 0xff8000; // Orange
    this.endColor = config.endColor || 0xff0000; // Red
    this.velocityVariance = config.velocityVariance || 2.0;
    this.spread = config.spread || Math.PI / 6; // 30 degrees
  }

  update(deltaTime: number): void {
    // Update existing particles
    for (let i = this.particles.length - 1; i >= 0; i--) {
      const particle = this.particles[i];

      // Update position
      particle.x += particle.velocityX * deltaTime;
      particle.y += particle.velocityY * deltaTime;

      // Update life
      particle.life -= deltaTime;

      // Update alpha based on life remaining
      const lifeRatio = particle.life / particle.maxLife;
      particle.alpha = lifeRatio;

      // Update size (shrink over time)
      particle.size =
        this.startSize * lifeRatio + this.endSize * (1 - lifeRatio);

      // Remove dead particles
      if (particle.life <= 0) {
        this.particles.splice(i, 1);
      }
    }

    this.timeSinceLastEmission += deltaTime;
  }

  emitParticles(
    emissionX: number,
    emissionY: number,
    direction: number,
    intensity: number,
  ): void {
    if (intensity <= 0) return;

    // Calculate how many particles to emit this frame based on emission rate and intensity
    const particlesToEmit = Math.floor(
      this.emissionRate * intensity * (1 / 60),
    ); // Assuming 60 FPS

    for (
      let i = 0;
      i < particlesToEmit && this.particles.length < this.maxParticles;
      i++
    ) {
      // Create new particle
      const spreadAngle = (Math.random() - 0.5) * this.spread;
      const particleDirection = direction + spreadAngle;

      const speed = 8 + (Math.random() - 0.5) * this.velocityVariance;

      const particle: Particle = {
        x: emissionX,
        y: emissionY,
        velocityX: Math.cos(particleDirection) * speed,
        velocityY: Math.sin(particleDirection) * speed,
        life: this.particleLifetime * (0.8 + Math.random() * 0.4), // Some variance in lifetime
        maxLife: this.particleLifetime,
        alpha: 1.0,
        color: this.startColor,
        size: this.startSize,
      };

      this.particles.push(particle);
    }
  }
}
