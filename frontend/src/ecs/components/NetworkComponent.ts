import { Component } from '../core/Component';
import { Vector2 } from '../core/types';

export interface NetworkConfig {
  serverId: number;
  isLocallyControlled?: boolean;
  serverPosition?: Vector2;
  serverVelocity?: Vector2;
  serverRotation?: number;
}

export class NetworkComponent extends Component {
  serverId: number;
  lastServerUpdate: number;
  serverPosition: Vector2;
  serverVelocity: Vector2;
  serverRotation: number;
  isLocallyControlled: boolean;
  sequenceNumber: number;

  // Network prediction/reconciliation data
  predictedPosition: Vector2;
  predictedVelocity: Vector2;
  predictedRotation: number;
  lastReconciliationTime: number;

  // Network state flags
  isDirty: boolean;
  needsReconciliation: boolean;
  lastInputSequence: number;

  constructor(entityId: number, config: NetworkConfig) {
    super(entityId);

    this.serverId = config.serverId;
    this.isLocallyControlled = config.isLocallyControlled || false;
    this.lastServerUpdate = 0;
    this.sequenceNumber = 0;
    this.lastInputSequence = 0;

    this.serverPosition = config.serverPosition ? { ...config.serverPosition } : { x: 0, y: 0 };
    this.serverVelocity = config.serverVelocity ? { ...config.serverVelocity } : { x: 0, y: 0 };
    this.serverRotation = config.serverRotation || 0;

    this.predictedPosition = { ...this.serverPosition };
    this.predictedVelocity = { ...this.serverVelocity };
    this.predictedRotation = this.serverRotation;
    this.lastReconciliationTime = 0;

    this.isDirty = false;
    this.needsReconciliation = false;
  }

  updateServerState(
    position: Vector2,
    velocity: Vector2,
    rotation: number,
    timestamp: number
  ): void {
    this.serverPosition = { ...position };
    this.serverVelocity = { ...velocity };
    this.serverRotation = rotation;
    this.lastServerUpdate = timestamp;
    this.markChanged();
  }

  updatePredictedState(
    position: Vector2,
    velocity: Vector2,
    rotation: number
  ): void {
    this.predictedPosition = { ...position };
    this.predictedVelocity = { ...velocity };
    this.predictedRotation = rotation;
    this.markChanged();
  }

  markForReconciliation(): void {
    this.needsReconciliation = true;
    this.lastReconciliationTime = Date.now();
    this.markChanged();
  }

  clearReconciliation(): void {
    this.needsReconciliation = false;
    this.markChanged();
  }

  incrementSequence(): number {
    return ++this.sequenceNumber;
  }

  getTimeSinceLastUpdate(): number {
    return Date.now() - this.lastServerUpdate;
  }

  serialize(): Record<string, any> {
    return {
      ...super.serialize(),
      serverId: this.serverId,
      lastServerUpdate: this.lastServerUpdate,
      serverPosition: this.serverPosition,
      serverVelocity: this.serverVelocity,
      serverRotation: this.serverRotation,
      isLocallyControlled: this.isLocallyControlled,
      sequenceNumber: this.sequenceNumber,
      isDirty: this.isDirty,
      needsReconciliation: this.needsReconciliation
    };
  }
}