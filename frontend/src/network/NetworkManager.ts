import { World } from '../ecs/core/World';
import { EntityType } from '../ecs/core/types';
import { NetworkSystem } from '../ecs/systems/NetworkSystem';
import { PhysicsComponent } from '../ecs/components/PhysicsComponent';
import { NetworkComponent } from '../ecs/components/NetworkComponent';
import { RenderComponent } from '../ecs/components/RenderComponent';
import { PacketHandler, PacketId } from './PacketHandler';
import { SnapshotBuffer, EntitySnapshot } from './SnapshotBuffer';
import { PredictionBuffer } from './PredictionBuffer';

export interface NetworkConfig {
  serverUrl?: string;
  interpolationDelay?: number;
  predictionEnabled?: boolean;
}

export class NetworkManager {
  private ws: WebSocket | null = null;
  private connected = false;
  private packetHandler = new PacketHandler();
  private snapshotBuffer = new SnapshotBuffer();
  private predictionBuffer = new PredictionBuffer();
  private networkSystem: NetworkSystem;
  private localPlayerId: number | null = null;
  private playerName = '';
  private config: Required<NetworkConfig>;
  private viewDistance = 300; // Default view distance
  private wasThrusting = false; // Track thrust state for decay
  private onLocalPlayerIdSet: ((playerId: number) => void) | null = null;

  // Timing
  private serverTime = 0;
  private clientTime = 0;
  private lastPingTime = 0;
  private pingInterval = 1000;
  private latency = 0;

  // Stats
  private bytesReceived = 0;
  private bytesSent = 0;
  private packetsReceived = 0;
  private packetsSent = 0;

  constructor(config: NetworkConfig = {}) {
    this.config = {
      serverUrl: config.serverUrl || `${window.location.protocol === 'https:' ? 'wss' : 'ws'}://${window.location.hostname}:5000/ws`,
      interpolationDelay: config.interpolationDelay || 100,
      predictionEnabled: config.predictionEnabled !== false
    };

    this.networkSystem = new NetworkSystem();
    this.setupPacketHandlers();
  }

  private setupPacketHandlers(): void {
    // Login response
    this.packetHandler.registerHandler(PacketId.LoginResponse, (view, offset) => {
      // LoginResponsePacket structure:
      // Header (4 bytes): length + id
      // UniqueId (4 bytes)
      // TickCounter (4 bytes)
      // Position (8 bytes): x, y floats
      // MapWidth (4 bytes)
      // MapHeight (4 bytes)
      // ViewDistance (2 bytes)
      // PlayerColor (4 bytes)

      const playerId = view.getInt32(offset + 4, true);
      const tickCounter = view.getUint32(offset + 8, true);
      const posX = view.getFloat32(offset + 12, true);
      const posY = view.getFloat32(offset + 16, true);
      const mapWidth = view.getInt32(offset + 20, true);
      const mapHeight = view.getInt32(offset + 24, true);
      const viewDistance = view.getUint16(offset + 28, true);
      const playerColor = view.getUint32(offset + 30, true);

      this.localPlayerId = playerId;
      this.viewDistance = viewDistance;
      this.networkSystem.setLocalEntity(playerId);
      console.log(`🎮 LOGGED IN: Player ${playerId} spawned at (${posX.toFixed(1)}, ${posY.toFixed(1)})`);
      console.log(`📍 MAP INFO: ${mapWidth}x${mapHeight}, View Distance: ${viewDistance}, Color: 0x${playerColor.toString(16)}`);

      // Notify ClientGame about the local player ID
      if (this.onLocalPlayerIdSet) {
        this.onLocalPlayerIdSet(playerId);
      }

      // Update camera zoom based on view distance
      (window as any).viewDistance = viewDistance;

      // Request spawn
      this.send(PacketHandler.createRequestSpawnPacket());
    });

    // Associate ID (for entity name mapping)
    this.packetHandler.registerHandler(PacketId.AssociateId, (view, offset) => {
      const entityId = view.getInt32(offset + 4, true);
      const nameResult = PacketHandler.readString(view, offset + 8);

      // Store entity name for later use
      const entity = World.getEntity(entityId);
      if (entity) {
        // TODO: Create NameTagComponent when needed
        console.log(`Entity ${entityId} named: ${nameResult.value}`);
      }
    });

    // Status updates (health, energy, etc)
    this.packetHandler.registerHandler(PacketId.Status, (view, offset) => {
      // StatusPacket structure: Header(4) + UniqueId(4) + Value(8) + Type(1) = 17 bytes
      const entityId = view.getInt32(offset + 4, true);        // UniqueId at offset 4
      const value = view.getFloat64(offset + 8, true);         // Value (double) at offset 8
      const statusType = view.getUint8(offset + 16);           // Type (StatusType byte) at offset 16

      // Map status type to readable name for logging
      const statusTypeNames: { [key: number]: string } = {
        0: 'Alive',
        1: 'Health',
        2: 'MaxHealth',
        3: 'Size',
        4: 'Direction',
        5: 'Throttle',
        10: 'BatteryCapacity',
        11: 'BatteryCharge',
        12: 'BatteryChargeRate',
        13: 'BatteryDischargeRate',
        14: 'EnginePowerDraw',
        15: 'ShieldPowerDraw',
        16: 'WeaponPowerDraw',
        20: 'ShieldCharge',
        21: 'ShieldMaxCharge',
        22: 'ShieldRechargeRate',
        23: 'ShieldPowerUse',
        24: 'ShieldPowerUseRecharge',
        25: 'ShieldRadius',
        100: 'InventoryCapacity',
        101: 'InventoryTriangles',
        102: 'InventorySquares',
        103: 'InventoryPentagons',
        200: 'Level',
        201: 'Experience',
        202: 'ExperienceToNextLevel'
      };

      const statusName = statusTypeNames[statusType] || `Unknown(${statusType})`;
      const isLocalPlayer = entityId === this.localPlayerId;
      const entityLabel = isLocalPlayer ? '👤 LOCAL' : `🎯 Entity ${entityId}`;

      // Update global entity store for UI rendering
      const gameEntities = (window as any).gameEntities as Map<number, any>;
      if (gameEntities && gameEntities.has(entityId)) {
        const entity = gameEntities.get(entityId);

        // console.log(`📊 ${entityLabel} ${statusName}: ${entity.health?.toFixed(1) || 'undefined'} → ${value.toFixed(1)}`);

        switch (statusType) {
          case 0: // Alive
            entity.alive = value !== 0;
            break;
          case 1: // Health
            if (entity.health !== value) {
              entity.health = value;
            }
            break;
          case 2: // MaxHealth
            if (entity.maxHealth !== value) {
              entity.maxHealth = value;
            }
            break;
          case 3: // Size
            if (entity.serverSize !== value) {
              entity.serverSize = value;
            }
            break;
          case 4: // Direction
            if (entity.direction !== value) {
              entity.direction = value;
            }
            break;
          case 5: // Throttle
            if (entity.throttle !== value) {
              entity.throttle = value;
            }
            break;
          case 11: // BatteryCharge (Energy)
            if (entity.energy !== value) {
              entity.energy = value;
            }
            break;
          case 10: // BatteryCapacity (Max Energy)
            if (entity.maxEnergy !== value) {
              entity.maxEnergy = value;
            }
            break;
          case 14: // EnginePowerDraw
            if (entity.enginePowerDraw !== value) {
              entity.enginePowerDraw = value;
            }
            break;
          case 20: // ShieldCharge
            if (entity.shieldCharge !== value) {
              entity.shieldCharge = value;
            }
            break;
          case 21: // ShieldMaxCharge
            if (entity.shieldMaxCharge !== value) {
              entity.shieldMaxCharge = value;
            }
            break;
          case 22: // ShieldRechargeRate
            if (entity.shieldRechargeRate !== value) {
              entity.shieldRechargeRate = value;
            }
            break;
          case 25: // ShieldRadius
            if (entity.shieldRadius !== value) {
              entity.shieldRadius = value;
            }
            break;
          case 12: // BatteryChargeRate
            if (entity.batteryChargeRate !== value) {
              entity.batteryChargeRate = value;
            }
            break;
          case 13: // BatteryDischargeRate
            if (entity.batteryDischargeRate !== value) {
              entity.batteryDischargeRate = value;
            }
            break;
          case 15: // ShieldPowerDraw
            if (entity.shieldPowerDraw !== value) {
              entity.shieldPowerDraw = value;
            }
            break;
          case 16: // WeaponPowerDraw
            if (entity.weaponPowerDraw !== value) {
              entity.weaponPowerDraw = value;
            }
            break;
          case 23: // ShieldPowerUse
            if (entity.shieldPowerUse !== value) {
              entity.shieldPowerUse = value;
            }
            break;
          case 24: // ShieldPowerUseRecharge
            if (entity.shieldPowerUseRecharge !== value) {
              entity.shieldPowerUseRecharge = value;
            }
            break;
          case 100: // InventoryCapacity
            if (entity.inventoryCapacity !== value) {
              entity.inventoryCapacity = value;
            }
            break;
          case 101: // InventoryTriangles
            if (entity.inventoryTriangles !== value) {
              entity.inventoryTriangles = value;
            }
            break;
          case 102: // InventorySquares
            if (entity.inventorySquares !== value) {
              entity.inventorySquares = value;
            }
            break;
          case 103: // InventoryPentagons
            if (entity.inventoryPentagons !== value) {
              entity.inventoryPentagons = value;
            }
            break;
          case 200: // Level
            if (entity.level !== value) {
              entity.level = value;
            }
            break;
          case 201: // Experience
            if (entity.experience !== value) {
              entity.experience = value;
            }
            break;
          case 202: // ExperienceToNextLevel
            if (entity.experienceToNextLevel !== value) {
              entity.experienceToNextLevel = value;
            }
            break;
          default:
            // Only log if it's not in our known status types list (to avoid spam)
            if (!statusTypeNames[statusType]) {
              console.warn(`📊 ${entityLabel} Unknown status type ${statusType}: ${value}`);
            } else {
              console.log(`📊 ${entityLabel} ${statusName}: ${value.toFixed(1)} (not handled yet)`);
            }
            break;
        }

        // Update timestamp for entity
        entity.lastStatusUpdate = Date.now();
      } else {
        console.warn(`📊 Status update for unknown entity ${entityId}: ${statusName} = ${value.toFixed(1)}`);
      }

      // Also try to update ECS entity if available (legacy support)
      const ecsEntity = World.getEntity(entityId);
      if (ecsEntity) {
        // TODO: Update components when HealthComponent and EnergyComponent are implemented
        console.log(`ECS entity ${entityId} status update: type=${statusType} value=${value}`);
      }
    });

    // Custom spawn packet
    this.packetHandler.registerHandler(PacketId.CustomSpawn, (view, offset) => {
      // SpawnPacket structure:
      // Header (4 bytes): length + id
      // UniqueId (4 bytes)
      // ShapeType (4 bytes) - enum
      // Width (4 bytes)
      // Height (4 bytes)
      // Rotation (4 bytes)
      // Position (8 bytes): x, y floats
      // Color (4 bytes)
      // Total: 36 bytes

      if (offset + 36 > view.byteLength) {
        console.warn('Incomplete spawn packet');
        return;
      }

      const id = view.getInt32(offset + 4, true);
      const shapeType = view.getInt32(offset + 8, true); // 4 bytes, not 2
      const width = view.getFloat32(offset + 12, true);
      const height = view.getFloat32(offset + 16, true);
      const rotation = view.getFloat32(offset + 20, true);
      const x = view.getFloat32(offset + 24, true);
      const y = view.getFloat32(offset + 28, true);
      const color = view.getUint32(offset + 32, true);

      const entity: EntitySnapshot = {
        id,
        timestamp: Date.now(),
        position: { x, y },
        velocity: { x: 0, y: 0 },
        rotation,
        size: Math.max(width, height)
      };

      // Create or update entity in ECS
      this.createOrUpdateEntity(id, shapeType, entity);
    });

    // Movement updates
    this.packetHandler.registerHandler(PacketId.Movement, (view, offset) => {
      const entityId = view.getInt32(offset + 4, true);
      const tickCounter = view.getUint32(offset + 8, true);
      const x = view.getFloat32(offset + 12, true);
      const y = view.getFloat32(offset + 16, true);
      const rotation = view.getFloat32(offset + 20, true);

      // console.log(`📊 Movement update for entity ${entityId} at (${x.toFixed(1)}, ${y.toFixed(1)})`);

      // Update local player position for camera if this is the local player
      if (entityId === this.localPlayerId) {
        (window as any).localPlayerPosition = { x, y };
      }

      // Also update ECS entity if it exists
      const ecsEntity = World.getEntity(entityId);
      if (ecsEntity) {
        const physics = ecsEntity.getComponent(PhysicsComponent);
        if (physics) {
          physics.setPosition({ x, y });
          physics.setRotation(rotation);
        }
      }

      // Update entity in global store for all entities
      const gameEntities = (window as any).gameEntities as Map<number, any>;
      if (gameEntities && gameEntities.has(entityId)) {
        const entity = gameEntities.get(entityId);
        entity.position = { x, y };
        entity.rotation = rotation;
        entity.timestamp = Date.now();
      }

      // Create snapshot for single entity
      const entities = new Map<number, EntitySnapshot>();
      entities.set(entityId, {
        id: entityId,
        timestamp: tickCounter,
        position: { x, y },
        velocity: { x: 0, y: 0 }, // MovementPacket doesn't include velocity
        rotation
      });

      // Add to snapshot buffer
      this.snapshotBuffer.addSnapshot({
        timestamp: tickCounter,
        entities
      });

      this.serverTime = tickCounter;
    });

    // Chat messages
    this.packetHandler.registerHandler(PacketId.Chat, (view, offset) => {
      // ChatPacket structure:
      // Header (4 bytes): length + id
      // UserId (4 bytes)
      // Channel (1 byte)
      // Message: first byte is length, then string data

      // Check bounds
      if (offset + 10 > view.byteLength) {
        console.warn('Chat packet too small');
        return;
      }

      const playerId = view.getUint32(offset + 4, true);
      const channel = view.getUint8(offset + 8);
      const messageLength = view.getUint8(offset + 9);

      // Check if we have enough bytes for the message
      if (offset + 10 + messageLength > view.byteLength) {
        console.warn(`Chat packet message truncated: expected ${messageLength} bytes`);
        return;
      }

      // Read message string
      let message = '';
      for (let i = 0; i < messageLength; i++) {
        message += String.fromCharCode(view.getUint8(offset + 10 + i));
      }

      // Emit chat event or handle UI update
      if (message.includes('joined') || message.includes('#')) {
        // Only log important messages like joins and leaderboard
        console.log(`[Chat] ${message}`);
      }
    });

    // Ping response
    this.packetHandler.registerHandler(PacketId.Ping, (view, offset) => {
      const serverTime = view.getFloat32(offset + 4, true);
      const clientTime = view.getFloat32(offset + 8, true);

      this.latency = Date.now() - clientTime;
      this.serverTime = serverTime;
    });
  }

  private createOrUpdateEntity(id: number, shapeType: number, snapshot: EntitySnapshot): void {
    // Get proper size from base resources
    const baseResources = (window as any).baseResources || {};
    const resourceData = baseResources[shapeType.toString()];

    // Better fallback handling for missing shape types
    let actualSize, sides;
    if (resourceData) {
      // Use reasonable scaling - server sizes seem to be in small units
      actualSize = Math.max(resourceData.Size * 2, 12); // More reasonable scaling
      sides = resourceData.Sides;
    } else {
      // Fallback: use reasonable fixed sizes for unknown shapes
      // The server size values are in different units - don't use them directly
      actualSize = 20; // Fixed reasonable size for unknown shapes
      sides = Math.max(shapeType, 3); // Minimum 3 sides for polygon
    }

    const isLocalPlayer = id === this.localPlayerId;

    console.log(`🎆 SPAWN: Entity ${id} at (${snapshot.position.x.toFixed(1)}, ${snapshot.position.y.toFixed(1)}), isLocal: ${isLocalPlayer}`);

    // Check if ECS entity already exists
    let entity = World.getEntity(id);

    if (!entity) {
      // Create new ECS entity with the server ID
      entity = World.createEntity(EntityType.Player, id);

      // Add physics component with proper position
      const physics = new PhysicsComponent(entity.id, {
        position: { x: snapshot.position.x, y: snapshot.position.y },
        velocity: snapshot.velocity,
        size: actualSize
      });
      physics.setRotation(snapshot.rotation);
      entity.addComponent(physics);

      // Add network component
      const network = new NetworkComponent(entity.id, {
        serverId: id,
        isLocallyControlled: isLocalPlayer,
        serverPosition: { ...snapshot.position },
        serverVelocity: { ...snapshot.velocity },
        serverRotation: snapshot.rotation
      });
      entity.addComponent(network);

      // Add render component with shape info
      const render = new RenderComponent(entity.id, {
        sides: sides,
        shapeType: shapeType,
        color: 0xffffff
      });
      entity.addComponent(render);

      console.log(`✨ Created ECS entity ${entity.id} with ${entity.getComponentCount()} components, ${sides} sides`);
    } else {
      // Update existing entity
      const physics = entity.getComponent(PhysicsComponent);
      if (physics) {
        physics.setPosition(snapshot.position);
        physics.setVelocity(snapshot.velocity);
        physics.setRotation(snapshot.rotation);
      }

      const network = entity.getComponent(NetworkComponent);
      if (network) {
        network.updateServerState(snapshot.position, snapshot.velocity, snapshot.rotation, snapshot.timestamp);
      }

      console.log(`📝 Updated existing ECS entity ${entity.id}`);
    }

    // If this is the local player, update camera position temporarily
    if (isLocalPlayer) {
      (window as any).localPlayerPosition = snapshot.position;
      console.log(`📷 CAMERA: Local player spawn at (${snapshot.position.x.toFixed(1)}, ${snapshot.position.y.toFixed(1)})`);
    }
  }

  private async fetchBaseResources(): Promise<void> {
    try {
      const response = await fetch('/BaseResources.json');
      const resources = await response.json();
      (window as any).baseResources = resources;
      console.log('Loaded base resources:', Object.keys(resources));
    } catch (error) {
      console.error('Failed to fetch base resources:', error);
    }
  }

  async connect(playerName: string): Promise<boolean> {
    this.playerName = playerName;

    // Load base resources first
    await this.fetchBaseResources();

    return new Promise((resolve) => {
      this.ws = new WebSocket(this.config.serverUrl);
      this.ws.binaryType = 'arraybuffer';

      this.ws.onopen = () => {
        this.connected = true;
        console.log('Connected to server');

        // Send login request
        this.send(PacketHandler.createLoginRequest(playerName, ''));
        resolve(true);
      };

      this.ws.onmessage = (event) => {
        this.bytesReceived += event.data.byteLength;
        this.packetsReceived++;
        this.packetHandler.processPacket(event.data);
      };

      this.ws.onerror = (error) => {
        console.error('WebSocket error:', error);
        resolve(false);
      };

      this.ws.onclose = () => {
        this.connected = false;
        console.log('Disconnected from server');
      };
    });
  }

  sendInput(inputState: any, mouseX: number, mouseY: number): void {
    if (!this.connected || !this.ws) return;

    const sequenceNumber = this.predictionBuffer.getCurrentSequence();
    const isThrusting = inputState.thrust;

    const packet = PacketHandler.createMovementPacket(
      this.localPlayerId || 0,  // Pass actual player ID
      sequenceNumber,
      inputState,
      mouseX,
      mouseY,
      this.wasThrusting  // Pass previous thrust state
    );

    // Update thrust state for next frame
    this.wasThrusting = isThrusting;

    this.send(packet);

    // Add to prediction buffer
    this.predictionBuffer.addInput({
      timestamp: Date.now(),
      moveX: inputState.moveX,
      moveY: inputState.moveY,
      mouseX,
      mouseY,
      buttons: inputState.fire ? 1 : 0
    });
  }

  sendChat(message: string): void {
    if (!this.connected || !this.ws || !this.localPlayerId) return;

    const packet = PacketHandler.createChatPacket(this.localPlayerId, message);
    this.send(packet);
  }

  private send(data: ArrayBuffer): void {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(data);
      this.bytesSent += data.byteLength;
      this.packetsSent++;
    }
  }

  update(deltaTime: number): void {
    this.clientTime += deltaTime * 1000;

    // Send ping periodically
    if (this.clientTime - this.lastPingTime > this.pingInterval) {
      this.sendPing();
      this.lastPingTime = this.clientTime;
    }

    // Update network system
    this.networkSystem.update(deltaTime);
  }

  private sendPing(): void {
    if (!this.connected || !this.ws) return;

    const buffer = new ArrayBuffer(12);
    const view = new DataView(buffer);

    view.setInt16(0, 12, true);
    view.setInt16(2, PacketId.Ping, true);
    view.setFloat32(4, this.serverTime, true);
    view.setFloat32(8, Date.now(), true);

    this.send(buffer);
  }

  getStats() {
    return {
      connected: this.connected,
      latency: this.latency,
      bytesReceived: this.bytesReceived,
      bytesSent: this.bytesSent,
      packetsReceived: this.packetsReceived,
      packetsSent: this.packetsSent
    };
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

  setLocalPlayerCallback(callback: (playerId: number) => void): void {
    this.onLocalPlayerIdSet = callback;
  }

  getLocalPlayerId(): number | null {
    return this.localPlayerId;
  }
}