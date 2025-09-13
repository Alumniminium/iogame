import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { NetworkComponent } from '../components/NetworkComponent';
import { PhysicsComponent } from '../components/PhysicsComponent';
import { SnapshotBuffer, WorldSnapshot, EntitySnapshot } from '../../network/SnapshotBuffer';
import { PredictionBuffer, InputCommand } from '../../network/PredictionBuffer';
import { EntityType } from '../core/types';

export class NetworkSystem extends System {
  readonly componentTypes = [NetworkComponent, PhysicsComponent];

  private snapshotBuffer = new SnapshotBuffer();
  private predictionBuffer = new PredictionBuffer();
  private localEntityId: number | null = null;
  private serverTime = 0;
  private clientTime = 0;
  private timeOffset = 0;
  private ws: WebSocket | null = null;
  private connected = false;

  constructor() {
    super();
  }

  initialize(): void {
    console.log('NetworkSystem initialized');
  }

  cleanup(): void {
    this.disconnect();
  }

  connect(url: string): void {
    this.ws = new WebSocket(url);
    this.ws.binaryType = 'arraybuffer';

    this.ws.onopen = () => {
      this.connected = true;
      console.log('Connected to server');
    };

    this.ws.onmessage = (event) => {
      this.handleServerMessage(event.data);
    };

    this.ws.onerror = (error) => {
      console.error('WebSocket error:', error);
    };

    this.ws.onclose = () => {
      this.connected = false;
      console.log('Disconnected from server');
    };
  }

  setLocalEntity(entityId: number): void {
    this.localEntityId = entityId;
  }

  getLocalEntity(): number | null {
    return this.localEntityId;
  }

  isConnected(): boolean {
    return this.connected;
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const network = entity.getComponent(NetworkComponent)!;
    const physics = entity.getComponent(PhysicsComponent)!;

    // Update client time
    this.clientTime += deltaTime * 1000;

    // Handle local entity differently
    if (entity.id === this.localEntityId && network.isLocallyControlled) {
      this.updateLocalEntity(entity, deltaTime);
    } else {
      this.updateRemoteEntity(entity, deltaTime);
    }
  }

  private updateLocalEntity(entity: Entity, deltaTime: number): void {
    const network = entity.getComponent(NetworkComponent)!;
    const physics = entity.getComponent(PhysicsComponent)!;

    // Handle prediction reconciliation if needed
    if (network.needsReconciliation) {
      this.performReconciliation(entity);
      network.clearReconciliation();
    }

    // Update predicted state
    network.updatePredictedState(
      physics.position,
      physics.velocity,
      physics.rotation
    );
  }

  private updateRemoteEntity(entity: Entity, deltaTime: number): void {
    const network = entity.getComponent(NetworkComponent)!;
    const physics = entity.getComponent(PhysicsComponent)!;

    // Get interpolated state for rendering
    const interpolatedState = this.snapshotBuffer.getInterpolatedState(this.clientTime);

    if (interpolatedState && interpolatedState.entities.has(network.serverId)) {
      const snapshot = interpolatedState.entities.get(network.serverId)!;

      // Update physics with interpolated values
      physics.setPosition(snapshot.position);
      physics.setVelocity(snapshot.velocity);
      physics.setRotation(snapshot.rotation);

      // Update network state
      network.updateServerState(
        snapshot.position,
        snapshot.velocity,
        snapshot.rotation,
        snapshot.timestamp
      );
    }
  }

  sendInput(input: Omit<InputCommand, 'sequenceNumber'>): void {
    if (!this.connected || !this.ws) return;

    const command = this.predictionBuffer.addInput(input);

    // Send input to server
    const buffer = new ArrayBuffer(24);
    const view = new DataView(buffer);

    // Packet structure (simplified)
    view.setInt16(0, 24, true); // packet length
    view.setInt16(2, 30, true); // packet id for input
    view.setInt32(4, command.sequenceNumber, true);
    view.setFloat32(8, command.moveX, true);
    view.setFloat32(12, command.moveY, true);
    view.setFloat32(16, command.mouseX, true);
    view.setFloat32(20, command.mouseY, true);

    this.ws.send(buffer);
  }

  private handleServerMessage(data: ArrayBuffer): void {
    const view = new DataView(data);
    let offset = 0;

    while (offset < data.byteLength) {
      const packetLength = view.getInt16(offset, true);
      const packetId = view.getInt16(offset + 2, true);

      switch (packetId) {
        case 20: // Snapshot packet
          this.handleSnapshot(view, offset);
          break;
        case 21: // Acknowledgment packet
          this.handleAcknowledgment(view, offset);
          break;
      }

      offset += packetLength;
    }
  }

  private handleSnapshot(view: DataView, offset: number): void {
    const timestamp = view.getFloat32(offset + 4, true);
    const entityCount = view.getInt16(offset + 8, true);

    const snapshot: WorldSnapshot = {
      timestamp,
      entities: new Map()
    };

    let pos = offset + 10;
    for (let i = 0; i < entityCount; i++) {
      const id = view.getInt32(pos, true);
      const x = view.getFloat32(pos + 4, true);
      const y = view.getFloat32(pos + 8, true);
      const vx = view.getFloat32(pos + 12, true);
      const vy = view.getFloat32(pos + 16, true);
      const rotation = view.getFloat32(pos + 20, true);
      const health = view.getFloat32(pos + 24, true);
      const energy = view.getFloat32(pos + 28, true);
      const size = view.getFloat32(pos + 32, true);

      snapshot.entities.set(id, {
        id,
        timestamp,
        position: { x, y },
        velocity: { x: vx, y: vy },
        rotation,
        health,
        energy,
        size
      });

      pos += 36;
    }

    this.snapshotBuffer.addSnapshot(snapshot);
    this.serverTime = timestamp;
  }

  private handleAcknowledgment(view: DataView, offset: number): void {
    const acknowledgedSequence = view.getInt32(offset + 4, true);
    const serverX = view.getFloat32(offset + 8, true);
    const serverY = view.getFloat32(offset + 12, true);
    const serverVx = view.getFloat32(offset + 16, true);
    const serverVy = view.getFloat32(offset + 20, true);
    const serverRotation = view.getFloat32(offset + 24, true);

    // Reconcile with server state
    const inputsToReplay = this.predictionBuffer.reconcile(
      acknowledgedSequence,
      {
        position: { x: serverX, y: serverY },
        velocity: { x: serverVx, y: serverVy },
        rotation: serverRotation
      }
    );

    // Mark local entity for reconciliation if needed
    if (inputsToReplay.length > 0 && this.localEntityId !== null) {
      // Store reconciliation data for next frame
      this.storeReconciliationData({
        position: { x: serverX, y: serverY },
        velocity: { x: serverVx, y: serverVy },
        rotation: serverRotation,
        inputsToReplay
      });
    }
  }

  private storeReconciliationData(data: any): void {
    // Store reconciliation data to be processed in updateLocalEntity
    (this as any).pendingReconciliation = data;
  }

  private performReconciliation(entity: Entity): void {
    const data = (this as any).pendingReconciliation;
    if (!data) return;

    const physics = entity.getComponent(PhysicsComponent)!;

    // Reset to server state
    physics.setPosition(data.position);
    physics.setVelocity(data.velocity);
    physics.setRotation(data.rotation);

    // Replay inputs
    for (const input of data.inputsToReplay) {
      this.applyInput(physics, input);
    }

    delete (this as any).pendingReconciliation;
  }

  private applyInput(physics: PhysicsComponent, input: InputCommand): void {
    const speed = 200; // pixels per second
    const dt = 1 / 60; // fixed timestep

    physics.setVelocity({
      x: input.moveX * speed,
      y: input.moveY * speed
    });

    physics.setPosition({
      x: physics.position.x + physics.velocity.x * dt,
      y: physics.position.y + physics.velocity.y * dt
    });

    // Calculate rotation based on mouse position
    const dx = input.mouseX - physics.position.x;
    const dy = input.mouseY - physics.position.y;
    physics.setRotation(Math.atan2(dy, dx));
  }

  createEntityFromSnapshot(snapshot: EntitySnapshot): Entity {
    const entity = this.createEntity(EntityType.Player);

    // Add physics component
    const physics = new PhysicsComponent(entity.id, {
      position: { ...snapshot.position },
      size: snapshot.size || 32,
      velocity: { ...snapshot.velocity }
    });
    physics.setRotation(snapshot.rotation);
    entity.addComponent(physics);

    // Add network component
    const network = new NetworkComponent(entity.id, {
      serverId: snapshot.id,
      isLocallyControlled: false,
      serverPosition: { ...snapshot.position },
      serverVelocity: { ...snapshot.velocity },
      serverRotation: snapshot.rotation
    });
    network.updateServerState(
      snapshot.position,
      snapshot.velocity,
      snapshot.rotation,
      snapshot.timestamp
    );
    entity.addComponent(network);

    return entity;
  }

  onEntityDestroyed(entity: Entity): void {
    // Clean up entity-specific network data if needed
    if (entity.id === this.localEntityId) {
      this.localEntityId = null;
    }
  }

  disconnect(): void {
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
    this.connected = false;
    this.snapshotBuffer.clear();
    this.predictionBuffer.clear();
  }

  // Utility methods for other systems
  getServerTime(): number {
    return this.serverTime;
  }

  getClientTime(): number {
    return this.clientTime;
  }

  getPing(): number {
    return this.timeOffset;
  }
}