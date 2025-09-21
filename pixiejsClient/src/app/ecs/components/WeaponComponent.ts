import { Component } from "../core/Component";
import { Vector2 } from "../core/types";

export interface WeaponConfig {
  frequency: number; // Fire rate limit in milliseconds
  bulletDamage: number;
  bulletCount?: number; // Bullets per shot
  bulletSize?: number;
  bulletSpeed: number;
  powerUse: number; // Power per bullet
  direction?: Vector2; // Local firing direction
}

export class WeaponComponent extends Component {
  fire: boolean;
  frequency: number; // Fire rate limit in milliseconds
  lastShot: number; // Timestamp of last shot
  bulletDamage: number;
  bulletCount: number; // Bullets per shot
  bulletSize: number;
  bulletSpeed: number;
  powerUse: number; // Power per bullet
  direction: Vector2; // Local firing direction

  constructor(entityId: string, config: WeaponConfig) {
    super(entityId);

    this.fire = false;
    this.frequency = config.frequency;
    this.lastShot = 0;
    this.bulletDamage = config.bulletDamage;
    this.bulletCount = config.bulletCount || 1;
    this.bulletSize = config.bulletSize || 3;
    this.bulletSpeed = config.bulletSpeed;
    this.powerUse = config.powerUse;
    this.direction = config.direction
      ? { ...config.direction }
      : { x: 0, y: 1 };
  }

  canFire(currentTime: number): boolean {
    return currentTime - this.lastShot >= this.frequency;
  }

  getTotalPowerCost(): number {
    return (this.powerUse * this.bulletCount * this.bulletSpeed) / 100;
  }
}
