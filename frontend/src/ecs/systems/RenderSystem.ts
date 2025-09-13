import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { PhysicsComponent } from '../components/PhysicsComponent';
import { ShieldComponent } from '../components/ShieldComponent';

export class RenderSystem extends System {
  readonly componentTypes = [PhysicsComponent];

  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;

  constructor(canvas: HTMLCanvasElement) {
    super();
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d')!;
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const physics = entity.getComponent(PhysicsComponent)!;
    this.drawEntity(entity, physics);

    // Draw shield if entity has one
    const shield = entity.getComponent(ShieldComponent);
    if (shield) {
      this.drawShield(entity, physics, shield);
    }
  }

  private drawEntity(entity: Entity, physics: PhysicsComponent): void {
    this.ctx.save();

    // Move to entity position
    this.ctx.translate(physics.position.x, physics.position.y);
    this.ctx.rotate(physics.rotation);

    // Draw based on entity type
    switch (entity.type) {
      case 'player':
        this.drawPlayer(physics);
        break;
      case 'enemy':
        this.drawEnemy(physics);
        break;
      case 'projectile':
        this.drawProjectile(physics);
        break;
      case 'resource':
        this.drawResource(physics);
        break;
    }

    this.ctx.restore();
  }

  private drawPlayer(physics: PhysicsComponent): void {
    this.ctx.fillStyle = '#008dba';
    this.ctx.strokeStyle = '#005e85';
    this.ctx.lineWidth = 2;

    this.ctx.beginPath();
    this.ctx.rect(-physics.size/2, -physics.size/2, physics.size, physics.size);
    this.ctx.fill();
    this.ctx.stroke();
  }

  private drawEnemy(physics: PhysicsComponent): void {
    this.ctx.fillStyle = '#e74c3c';
    this.ctx.strokeStyle = '#c0392b';
    this.ctx.lineWidth = 2;

    this.ctx.beginPath();
    this.ctx.arc(0, 0, physics.size/2, 0, Math.PI * 2);
    this.ctx.fill();
    this.ctx.stroke();
  }

  private drawProjectile(physics: PhysicsComponent): void {
    this.ctx.fillStyle = '#f39c12';
    this.ctx.strokeStyle = '#e67e22';

    this.ctx.beginPath();
    this.ctx.arc(0, 0, physics.size/2, 0, Math.PI * 2);
    this.ctx.fill();
    this.ctx.stroke();
  }

  private drawResource(physics: PhysicsComponent): void {
    this.ctx.fillStyle = '#27ae60';
    this.ctx.strokeStyle = '#229954';

    this.ctx.beginPath();
    this.ctx.arc(0, 0, physics.size/2, 0, Math.PI * 2);
    this.ctx.fill();
    this.ctx.stroke();
  }

  private drawShield(entity: Entity, physics: PhysicsComponent, shield: ShieldComponent): void {
    if (!shield.isEffective) return;

    this.ctx.save();

    // Move to entity position
    this.ctx.translate(physics.position.x, physics.position.y);

    // Shield appearance based on charge level
    const chargeRatio = shield.chargePercentage / 100;
    const alpha = Math.max(0.15, chargeRatio * 0.6); // Minimum visibility, scales with charge

    // Shield color - blue when high charge, red when low
    let r, g, b;
    if (chargeRatio > 0.5) {
      // High charge: blue to cyan
      r = Math.floor((1 - chargeRatio) * 100);
      g = Math.floor(150 + chargeRatio * 105);
      b = 255;
    } else {
      // Low charge: red to yellow
      r = 255;
      g = Math.floor(chargeRatio * 510); // 0-255 based on charge
      b = 0;
    }

    // Outer glow effect
    const gradient = this.ctx.createRadialGradient(0, 0, shield.radius * 0.8, 0, 0, shield.radius);
    gradient.addColorStop(0, `rgba(${r}, ${g}, ${b}, ${alpha * 0.3})`);
    gradient.addColorStop(0.8, `rgba(${r}, ${g}, ${b}, ${alpha})`);
    gradient.addColorStop(1, `rgba(${r}, ${g}, ${b}, 0)`);

    this.ctx.fillStyle = gradient;
    this.ctx.beginPath();
    this.ctx.arc(0, 0, shield.radius, 0, Math.PI * 2);
    this.ctx.fill();

    // Shield border - more visible when active
    if (shield.active) {
      this.ctx.strokeStyle = `rgba(${r}, ${g}, ${b}, ${alpha + 0.3})`;
      this.ctx.lineWidth = 2;
      this.ctx.setLineDash([5, 5]); // Dashed line effect
      this.ctx.beginPath();
      this.ctx.arc(0, 0, shield.radius, 0, Math.PI * 2);
      this.ctx.stroke();
      this.ctx.setLineDash([]); // Reset dash pattern
    }

    this.ctx.restore();
  }

  clear(): void {
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
  }
}