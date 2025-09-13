import { Component } from '../core/Component';

export interface NetworkComponent extends Component {
  serverId: number;
  lastServerUpdate: number;
  serverPosition: { x: number; y: number };
  serverVelocity: { x: number; y: number };
  serverRotation: number;
  isLocallyControlled: boolean;
  sequenceNumber: number;
}

export function createNetworkComponent(serverId: number, isLocal: boolean = false): NetworkComponent {
  return {
    serverId,
    lastServerUpdate: 0,
    serverPosition: { x: 0, y: 0 },
    serverVelocity: { x: 0, y: 0 },
    serverRotation: 0,
    isLocallyControlled: isLocal,
    sequenceNumber: 0
  };
}