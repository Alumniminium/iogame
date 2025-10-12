import { Component } from "../core/Component";
import { Vector2 } from "../core/types";

export interface NetworkConfig {
  serverId: string;
  isLocallyControlled?: boolean;
  serverPosition?: Vector2;
  serverVelocity?: Vector2;
  serverRotation?: number;
}

export class NetworkComponent extends Component {
  serverId: string;
  lastServerUpdate: number;
  serverPosition: Vector2;
  serverVelocity: Vector2;
  serverRotation: number;
  isLocallyControlled: boolean;

  isDirty: boolean;

  lastServerTick: number;
  serverTick?: number; // The tick associated with the last server update

  lastCollisionTick: number;
  collisionGracePeriod: number;

  constructor(entityId: string, config?: NetworkConfig) {
    super(entityId);

    this.serverId = config?.serverId || entityId;
    this.isLocallyControlled = config?.isLocallyControlled || false;
    this.lastServerUpdate = 0;
    this.lastServerTick = 0;

    this.lastCollisionTick = 0;
    this.collisionGracePeriod = 5; // ticks to avoid fighting server after collision

    this.serverPosition = config?.serverPosition ? { ...config.serverPosition } : { x: 0, y: 0 };
    this.serverVelocity = config?.serverVelocity ? { ...config.serverVelocity } : { x: 0, y: 0 };
    this.serverRotation = config?.serverRotation || 0;

    this.isDirty = false;
  }

  updateServerState(position: Vector2, velocity: Vector2, rotation: number, timestamp: number): void {
    this.serverPosition = { ...position };
    this.serverVelocity = { ...velocity };
    this.serverRotation = rotation;
    this.lastServerUpdate = timestamp;
  }

  updateLastServerTick(serverTick: number): void {
    this.lastServerTick = serverTick;
  }

  markServerCollision(serverTick: number): void {
    this.lastCollisionTick = serverTick;
  }

  isInCollisionGracePeriod(currentTick: number): boolean {
    return currentTick - this.lastCollisionTick < this.collisionGracePeriod;
  }

  getTimeSinceLastUpdate(): number {
    return Date.now() - this.lastServerUpdate;
  }
}
