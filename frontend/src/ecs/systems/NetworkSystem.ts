import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { World } from '../core/World';
import { NetworkComponent } from '../components/NetworkComponent';
import { PhysicsComponent } from '../components/PhysicsComponent';
import { SnapshotBuffer, WorldSnapshot, EntitySnapshot } from '../../network/SnapshotBuffer';
import { PredictionBuffer, InputCommand } from '../../network/PredictionBuffer';

export class NetworkSystem extends System {
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
    
    // Save prediction state for reconciliation
    if (this.localEntityId !== null) {
      const entity = World.instance.getEntity(this.localEntityId);
      if (entity) {
        const physics = entity.getComponent<PhysicsComponent>('physics');
        if (physics) {
          this.predictionBuffer.savePredictionState(command.sequenceNumber, {
            position: { ...physics.position },
            velocity: { ...physics.velocity },
            rotation: physics.rotation
          });
        }
      }
    }
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
    
    // If we need to reconcile, replay unacknowledged inputs
    if (inputsToReplay.length > 0 && this.localEntityId !== null) {
      const entity = World.instance.getEntity(this.localEntityId);
      if (entity) {
        const physics = entity.getComponent<PhysicsComponent>('physics');
        const network = entity.getComponent<NetworkComponent>('network');
        
        if (physics && network) {
          // Reset to server state
          physics.position = { x: serverX, y: serverY };
          physics.velocity = { x: serverVx, y: serverVy };
          physics.rotation = serverRotation;
          
          // Replay inputs
          for (const input of inputsToReplay) {
            this.applyInput(physics, input);
          }
        }
      }
    }
  }
  
  private applyInput(physics: PhysicsComponent, input: InputCommand): void {
    const speed = 200; // pixels per second
    const dt = 1 / 60; // fixed timestep
    
    physics.velocity.x = input.moveX * speed;
    physics.velocity.y = input.moveY * speed;
    
    physics.position.x += physics.velocity.x * dt;
    physics.position.y += physics.velocity.y * dt;
    
    // Calculate rotation based on mouse position
    const dx = input.mouseX - physics.position.x;
    const dy = input.mouseY - physics.position.y;
    physics.rotation = Math.atan2(dy, dx);
  }
  
  update(deltaTime: number): void {
    this.clientTime += deltaTime * 1000;
    
    // Get interpolated state for rendering
    const interpolatedState = this.snapshotBuffer.getInterpolatedState(this.clientTime);
    
    if (interpolatedState) {
      // Update all non-local entities with interpolated positions
      interpolatedState.entities.forEach((snapshot, id) => {
        if (id === this.localEntityId) return; // Skip local entity
        
        const entity = World.instance.getEntity(id);
        if (!entity) {
          // Create entity if it doesn't exist
          this.createEntityFromSnapshot(snapshot);
        } else {
          // Update entity position with interpolated values
          const physics = entity.getComponent<PhysicsComponent>('physics');
          const network = entity.getComponent<NetworkComponent>('network');
          
          if (physics && network && !network.isLocallyControlled) {
            physics.position = { ...snapshot.position };
            physics.velocity = { ...snapshot.velocity };
            physics.rotation = snapshot.rotation;
          }
        }
      });
    }
  }
  
  private createEntityFromSnapshot(snapshot: EntitySnapshot): void {
    const entity = World.instance.createEntity('player');
    
    // Add physics component
    entity.addComponent('physics', {
      position: { ...snapshot.position },
      velocity: { ...snapshot.velocity },
      rotation: snapshot.rotation,
      size: snapshot.size || 32
    });
    
    // Add network component
    entity.addComponent('network', {
      serverId: snapshot.id,
      lastServerUpdate: snapshot.timestamp,
      serverPosition: { ...snapshot.position },
      serverVelocity: { ...snapshot.velocity },
      serverRotation: snapshot.rotation,
      isLocallyControlled: false,
      sequenceNumber: 0
    });
    
    // Add health if provided
    if (snapshot.health !== undefined) {
      entity.addComponent('health', {
        current: snapshot.health,
        max: 100
      });
    }
    
    // Add energy if provided
    if (snapshot.energy !== undefined) {
      entity.addComponent('energy', {
        current: snapshot.energy,
        max: 100
      });
    }
  }
  
  onEntityChanged(entity: Entity): void {
    // Handle entity component changes if needed
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
}