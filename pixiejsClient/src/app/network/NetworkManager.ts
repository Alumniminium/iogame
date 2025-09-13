import { World } from "../ecs/core/World";
import { EntityType } from "../ecs/core/types";
import { NetworkComponent } from "../ecs/components/NetworkComponent";
import { PhysicsComponent } from "../ecs/components/PhysicsComponent";
import { RenderComponent } from "../ecs/components/RenderComponent";
import { HealthComponent } from "../ecs/components/HealthComponent";
import { EnergyComponent } from "../ecs/components/EnergyComponent";
import { BatteryComponent } from "../ecs/components/BatteryComponent";
import { ShieldComponent } from "../ecs/components/ShieldComponent";
import { EngineComponent } from "../ecs/components/EngineComponent";
import { DebugComponent } from "../ecs/components/DebugComponent";
import { AABBComponent } from "../ecs/components/AABBComponent";
import type { InputState } from "../ecs/systems/InputSystem";
import { NetworkSystem } from "../ecs/systems/NetworkSystem";
import { PacketHandler, PacketId } from "./PacketHandler";
import { TickSynchronizer } from "./TickSynchronizer";

export interface NetworkConfig {
  serverUrl?: string;
  interpolationDelay?: number;
  predictionEnabled?: boolean;
}

export interface NetworkStats {
  connected: boolean;
  latency: number;
  bytesReceived: number;
  bytesSent: number;
  packetsReceived: number;
  packetsSent: number;
}

export class NetworkManager {
  private ws: WebSocket | null = null;
  private connected = false;
  private localPlayerId: number | null = null;
  private config: Required<NetworkConfig>;
  private onLocalPlayerIdSet: ((playerId: number) => void) | null = null;
  private packetHandler = new PacketHandler();
  private baseResources: Record<string, any> = {};
  private viewDistance: number = 300; // Default view distance
  private onViewDistanceReceived: ((viewDistance: number) => void) | null =
    null;

  // Timing
  private clientTime = 0;
  private lastPingTime = 0;
  private latency = 0;
  private tickSynchronizer = TickSynchronizer.getInstance();

  // Stats
  private bytesReceived = 0;
  private bytesSent = 0;
  private packetsReceived = 0;
  private packetsSent = 0;

  constructor(config: NetworkConfig = {}) {
    this.config = {
      serverUrl: config.serverUrl || "ws://localhost:5000/ws",
      interpolationDelay: config.interpolationDelay || 100,
      predictionEnabled: config.predictionEnabled !== false,
    };
    this.setupPacketHandlers();
  }

  private setupPacketHandlers(): void {
    // Login response
    this.packetHandler.registerHandler(
      PacketId.LoginResponse,
      (view, offset) => {
        // LoginResponsePacket structure:
        // Header (4 bytes): length + id
        // UniqueId (4 bytes)
        // TickCounter (4 bytes)
        // Position (8 bytes): float32 x, float32 y
        // MapSize (8 bytes): int32 width, int32 height
        // ViewDistance (2 bytes): uint16
        // PlayerColor (4 bytes): uint32

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

        // Synchronize client tick counter with server
        this.tickSynchronizer.synchronizeWithServer(tickCounter, this.latency);

        console.log(`🎮 Login successful! Player ID: ${playerId}`);
        console.log(
          `📍 MAP INFO: ${mapWidth}x${mapHeight}, View Distance: ${viewDistance}, Color: 0x${playerColor.toString(16)}`,
        );
        console.log(`⏱️ Server tick sync: ${tickCounter} (latency: ${this.latency}ms)`);

        if (this.onLocalPlayerIdSet) {
          this.onLocalPlayerIdSet(playerId);
        }

        if (this.onViewDistanceReceived) {
          this.onViewDistanceReceived(viewDistance);
        }

        // Request spawn
        this.send(PacketHandler.createRequestSpawnPacket());
      },
    );

    // Associate ID (for entity name mapping)
    this.packetHandler.registerHandler(PacketId.AssociateId, (view, offset) => {
      const entityId = view.getInt32(offset + 4, true);
      const nameResult = PacketHandler.readString(view, offset + 8);

      console.log(
        `📝 Entity ${entityId} associated with name: "${nameResult.value}"`,
      );
      // Could store this mapping for UI display if needed
    });

    // Entity spawn
    this.packetHandler.registerHandler(PacketId.CustomSpawn, (view, offset) => {
      if (offset + 36 > view.byteLength) {
        console.warn("Incomplete spawn packet");
        return;
      }

      const id = view.getInt32(offset + 4, true);
      const shapeType = view.getInt32(offset + 8, true);
      const width = view.getFloat32(offset + 12, true);
      const height = view.getFloat32(offset + 16, true);
      const rotation = view.getFloat32(offset + 20, true);
      const x = view.getFloat32(offset + 24, true);
      const y = view.getFloat32(offset + 28, true);
      const color = view.getUint32(offset + 32, true);

      // console.log(`CustomSpawn: id=${id}, shape=${shapeType}, size=${width}x${height}, pos=(${x.toFixed(1)}, ${y.toFixed(1)}), rot=${(rotation * 180 / Math.PI).toFixed(1)}°`);

      // Create ECS entity
      let entity = World.getEntity(id);
      if (!entity) {
        entity = World.createEntity(EntityType.Player, id);

        // Add physics component with proper width/height
        const physics = new PhysicsComponent(entity.id, {
          position: { x, y },
          velocity: { x: 0, y: 0 },
          size: Math.max(width, height), // Keep for circle radius compatibility
          width: width,
          height: height,
          shapeType: shapeType as any,
        });
        physics.setRotation(rotation);
        entity.set(physics);

        // Add network component
        const network = new NetworkComponent(entity.id, {
          serverId: id,
          isLocallyControlled: id === this.localPlayerId,
          serverPosition: { x, y },
          serverVelocity: { x: 0, y: 0 },
          serverRotation: rotation,
        });
        entity.set(network);

        // Get shape data from BaseResources - but use server-sent size, not template size
        const resourceData = this.baseResources[shapeType.toString()];
        let sides, actualSize;

        // Always use server-sent dimensions for size (width/height from server)
        actualSize = Math.max(width, height);

        if (resourceData) {
          // Use shape info from BaseResources, but not the size
          sides = resourceData.sides || resourceData.Sides || shapeType; // Handle camelCase from API
        } else {
          // Fallback for unknown shapes - use shapeType directly
          if (shapeType === 0) {
            sides = 0; // Circle
          } else if (shapeType === 1) {
            sides = 3; // Triangle
          } else if (shapeType === 2) {
            sides = 4; // Box/Rectangle
          } else {
            sides = Math.max(shapeType, 3); // For other polygon shapes
          }
        }

        // Update physics component dimensions - preserve width/height for rectangles
        physics.size = actualSize;
        physics.width = width;
        physics.height = height;

        // Add render component with proper sides
        const render = new RenderComponent(entity.id, {
          sides: sides,
          shapeType: shapeType,
          color: color || 0xffffff,
        });
        entity.set(render);

        // Add default stat components
        entity.set(new HealthComponent(entity.id, { max: 100 }));
        entity.set(new EnergyComponent(entity.id, { batteryCapacity: 100 }));
        entity.set(new BatteryComponent(entity.id, { capacity: 100 }));
        entity.set(new ShieldComponent(entity.id, { maxCharge: 250, charge: 250, targetRadius: 45, minRadius: 2 }));

        // Add engine component for players (required for movement)
        entity.set(new EngineComponent(entity.id, { maxPropulsion: 500 }));

        // Add AABB component matching server logic but with proper rectangle support
        if (shapeType === 2 || shapeType === 4) { // Rectangle/Box (both type 2 and 4)
          // For rectangles, use actual width/height instead of radius
          const aabb = new AABBComponent(entity.id, {
            aabb: {
              x: x - width / 2,
              y: y - height / 2,
              width: width,
              height: height
            }
          });
          entity.set(aabb);
        } else {
          // For circles and other shapes, use server's original logic (square AABB from size)
          const size = Math.max(width, height);
          const aabb = new AABBComponent(entity.id, {
            aabb: {
              x: x - size / 2,
              y: y - size / 2,
              width: size,
              height: size
            }
          });
          entity.set(aabb);
        }

        // Create debug visualization entity for server position
        const debugEntityId = id + 100000; // Offset to avoid conflicts
        let debugEntity = World.getEntity(debugEntityId);
        if (!debugEntity) {
          debugEntity = World.createEntity(EntityType.Debug, debugEntityId);

          // Add physics component (for position tracking)
          const debugPhysics = new PhysicsComponent(debugEntity.id, {
            position: { x, y },
            velocity: { x: 0, y: 0 },
            size: actualSize * 0.8, // Slightly smaller than main entity
          });
          debugPhysics.setRotation(rotation);
          debugEntity.set(debugPhysics);

          // Add debug component to mark this as a debug entity
          const debugComp = new DebugComponent(debugEntity.id, {
            debugType: "serverPosition",
            targetEntityId: id,
          });
          debugEntity.set(debugComp);

          // Add render component - make it semi-transparent and different color
          const debugRender = new RenderComponent(debugEntity.id, {
            sides: sides,
            shapeType: shapeType,
            color: 0xff0000, // Red for server position
            alpha: 0.5, // Semi-transparent
          });
          debugEntity.set(debugRender);
        }
      }
    });

    // Status updates
    this.packetHandler.registerHandler(PacketId.Status, (view, offset) => {
      const entityId = view.getInt32(offset + 4, true);
      const value = view.getFloat64(offset + 8, true);
      const statusType = view.getUint8(offset + 16);

      // Update entity components based on status type
      const entity = World.getEntity(entityId);
      if (entity) {
        this.updateEntityStatus(entity, statusType, value);
      }
    });

    // Movement updates
    this.packetHandler.registerHandler(PacketId.Movement, (view, offset) => {
      const entityId = view.getInt32(offset + 4, true);
      const tickCounter = view.getUint32(offset + 8, true);
      const x = view.getFloat32(offset + 12, true);
      const y = view.getFloat32(offset + 16, true);
      const velocityX = view.getFloat32(offset + 20, true);
      const velocityY = view.getFloat32(offset + 24, true);
      const rotation = view.getFloat32(offset + 28, true);

      // Update through NetworkSystem for proper interpolation/prediction
      const systems = World.getSystems();
      const networkSystem = systems.find(s => s instanceof NetworkSystem) as NetworkSystem;
      if (networkSystem) {
        // Use velocity directly from server packet
        const serverVelocity = { x: velocityX, y: velocityY };

        // Pass the tick counter as the input sequence for reconciliation
        networkSystem.updateEntityFromServer(
          entityId,
          { x, y },
          serverVelocity, // Use actual velocity from server
          rotation,
          Date.now(),
          tickCounter
        );

        // Update debug visualization entity with exact server position (no prediction)
        const debugEntityId = entityId + 100000;
        const debugEntity = World.getEntity(debugEntityId);
        if (debugEntity) {
          const debugPhysics = debugEntity.get(PhysicsComponent);
          if (debugPhysics) {
            // Set exact server position and rotation
            debugPhysics.position.x = x;
            debugPhysics.position.y = y;
            debugPhysics.setRotation(rotation);
          }
        }
      }
    });

    // Chat messages
    this.packetHandler.registerHandler(PacketId.Chat, (view, offset) => {
      const playerId = view.getUint32(offset + 4, true);
      const channel = view.getUint8(offset + 8);
      const messageLength = view.getUint8(offset + 9);

      if (offset + 10 + messageLength > view.byteLength) {
        console.warn(`Chat packet message truncated`);
        return;
      }

      let message = "";
      for (let i = 0; i < messageLength; i++) {
        message += String.fromCharCode(view.getUint8(offset + 10 + i));
      }

      console.log(`[Chat] Player ${playerId}: ${message}`);
    });

    // Ping response
    this.packetHandler.registerHandler(PacketId.Ping, (view, offset) => {
      const serverTime = view.getFloat32(offset + 4, true);
      const clientTime = view.getFloat32(offset + 8, true);
      this.latency = Date.now() - clientTime;
    });
  }

  private updateEntityStatus(
    entity: any,
    statusType: number,
    value: number,
  ): void {
    switch (statusType) {
      case 1: // Health
        this.updateHealthComponent(entity, "current", value);
        break;
      case 2: // MaxHealth
        this.updateHealthComponent(entity, "max", value);
        break;
      case 6: // Energy
        this.updateEnergyComponent(entity, "current", value);
        break;
      case 7: // MaxEnergy
        this.updateEnergyComponent(entity, "max", value);
        break;
      case 11: // BatteryCharge
        this.updateBatteryComponent(entity, "currentCharge", value);
        break;
      case 10: // BatteryCapacity
        this.updateBatteryComponent(entity, "capacity", value);
        break;
      case 20: // ShieldCharge
        this.updateShieldComponent(entity, "currentCharge", value);
        break;
      case 21: // ShieldMaxCharge
        this.updateShieldComponent(entity, "maxCharge", value);
        break;
      case 25: // ShieldRadius
        this.updateShieldComponent(entity, "radius", value);
        break;
    }
  }

  private updateHealthComponent(
    entity: any,
    property: string,
    value: number,
  ): void {
    let health = entity.get(HealthComponent);
    if (!health) {
      health = new HealthComponent(entity.id, { max: 100, current: 100 });
      entity.add(health);
    }
    (health as any)[property] = value;
    health.isDead = health.current <= 0;
  }

  private updateEnergyComponent(
    entity: any,
    property: string,
    value: number,
  ): void {
    let energy = entity.get(EnergyComponent);
    if (!energy) {
      energy = new EnergyComponent(entity.id, { batteryCapacity: 100 });
      entity.add(energy);
    }

    // Map network property names to energy component properties
    if (property === "current") {
      energy.availableCharge = value;
    } else if (property === "max") {
      energy.batteryCapacity = value;
    } else {
      (energy as any)[property] = value;
    }

    energy.markChanged();
  }

  private updateBatteryComponent(
    entity: any,
    property: string,
    value: number,
  ): void {
    let battery = entity.get(BatteryComponent);
    if (!battery) {
      battery = new BatteryComponent(entity.id, {});
      entity.add(battery);
    }
    (battery as any)[property] = value;
  }

  private updateShieldComponent(
    entity: any,
    property: string,
    value: number,
  ): void {
    let shield = entity.get(ShieldComponent);
    if (!shield) {
      shield = new ShieldComponent(entity.id, { maxCharge: 100 });
      entity.add(shield);
    }

    // Map network property names to shield component properties
    if (property === "currentCharge") {
      shield.charge = value;
      // Also update radius based on charge (matching server calculation)
      const chargePercent = shield.charge / shield.maxCharge;
      shield.radius = Math.max(shield.minRadius, shield.targetRadius * chargePercent);
    } else if (property === "radius") {
      // Server-sent radius takes priority
      shield.radius = value;
    } else {
      (shield as any)[property] = value;
    }

    // Update shield power state based on charge
    shield.powerOn = shield.charge > 0;
    shield.markChanged();
  }

  private async fetchBaseResources(): Promise<void> {
    try {
      const response = await fetch("/api/baseresources");
      this.baseResources = await response.json();
      console.log("Loaded base resources from API:", Object.keys(this.baseResources));
    } catch (error) {
      console.warn("Failed to load base resources from API:", error);
      this.baseResources = {}; // Empty object as fallback
    }
  }

  async connect(playerName: string): Promise<boolean> {
    // Load base resources first
    await this.fetchBaseResources();

    return new Promise((resolve) => {
      try {
        this.ws = new WebSocket(this.config.serverUrl);
        this.ws.binaryType = "arraybuffer";

        this.ws.onopen = () => {
          console.log("Connected to server");
          this.connected = true;
          // Send binary login request instead of JSON
          this.send(PacketHandler.createLoginRequest(playerName, ""));
          resolve(true);
        };

        this.ws.onmessage = (event) => {
          this.bytesReceived += event.data.byteLength;
          this.packetsReceived++;
          this.packetHandler.processPacket(event.data);
        };

        this.ws.onerror = (error) => {
          console.error("WebSocket error:", error);
          this.connected = false;
          resolve(false);
        };

        this.ws.onclose = () => {
          console.log("Disconnected from server");
          this.connected = false;
        };
      } catch (error) {
        console.error("Failed to connect:", error);
        resolve(false);
      }
    });
  }

  disconnect(): void {
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
    this.connected = false;
  }

  isConnected(): boolean {
    return this.connected;
  }

  setLocalPlayerCallback(callback: (playerId: number) => void): void {
    this.onLocalPlayerIdSet = callback;
  }

  setViewDistanceCallback(callback: (viewDistance: number) => void): void {
    this.onViewDistanceReceived = callback;
  }

  getViewDistance(): number {
    return this.viewDistance;
  }

  getTickSynchronizer(): TickSynchronizer {
    return this.tickSynchronizer;
  }

  getLatency(): number {
    return this.latency;
  }

  update(deltaTime: number): void {
    this.clientTime += deltaTime * 1000;

    // Send periodic ping
    const now = Date.now();
    if (now - this.lastPingTime > 1000) {
      this.sendPing();
      this.lastPingTime = now;
    }
  }

  sendInput(input: InputState, mouseWorldX: number, mouseWorldY: number): void {
    if (!this.connected || !this.ws || !this.localPlayerId) return;

    // Use server tick instead of timestamp for proper prediction/reconciliation
    const serverTick = this.tickSynchronizer.getCurrentServerTick();

    const packet = PacketHandler.createMovementPacket(
      this.localPlayerId,
      serverTick,
      input,
      mouseWorldX,
      mouseWorldY,
    );

    this.send(packet);
  }

  sendChat(message: string): void {
    if (!this.connected || !this.ws || !this.localPlayerId) return;

    const packet = PacketHandler.createChatPacket(this.localPlayerId, message);
    this.send(packet);
  }

  getStats(): NetworkStats {
    return {
      connected: this.connected,
      latency: this.latency,
      bytesReceived: this.bytesReceived,
      bytesSent: this.bytesSent,
      packetsReceived: this.packetsReceived,
      packetsSent: this.packetsSent,
    };
  }

  private send(data: ArrayBuffer): void {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(data);
      this.bytesSent += data.byteLength;
      this.packetsSent++;
    }
  }

  private sendPing(): void {
    if (!this.connected || !this.ws) return;

    const buffer = new ArrayBuffer(12);
    const view = new DataView(buffer);

    view.setInt16(0, 12, true);
    view.setInt16(2, PacketId.Ping, true);
    view.setFloat32(4, 0, true); // server time
    view.setFloat32(8, Date.now(), true); // client time

    this.send(buffer);
  }
}
