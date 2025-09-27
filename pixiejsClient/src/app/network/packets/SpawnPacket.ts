import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";
import { World } from "../../ecs/core/World";
import { EntityType } from "../../ecs/core/types";
import { PhysicsComponent } from "../../ecs/components/PhysicsComponent";
import { NetworkComponent } from "../../ecs/components/NetworkComponent";
import { RenderComponent } from "../../ecs/components/RenderComponent";
import { ParticleSystemComponent } from "../../ecs/components/ParticleSystemComponent";
import { EngineComponent } from "../../ecs/components/EngineComponent";

interface ShipPart {
  gridX: number;
  gridY: number;
  type: number; // 0=hull, 1=shield, 2=engine
  shape: number; // 1=triangle, 2=square
  rotation: number; // 0=0°, 1=90°, 2=180°, 3=270°
}

export class SpawnPacket {
  header: PacketHeader;
  uid: string;
  shapeType: number;
  rotation: number;
  x: number;
  y: number;
  color: number;
  parts: ShipPart[];

  constructor(
    header: PacketHeader,
    uid: string,
    shapeType: number,
    rotation: number,
    x: number,
    y: number,
    color: number,
    parts: ShipPart[] = [],
  ) {
    this.header = header;
    this.uid = uid;
    this.shapeType = shapeType;
    this.rotation = rotation;
    this.x = x;
    this.y = y;
    this.color = color;
    this.parts = parts;
  }

  static handle(buffer: ArrayBuffer, localPlayerId: string) {
    const packet = SpawnPacket.fromBuffer(buffer);

    let validColor = packet.color;
    if (packet.color > 0xffffff || packet.color < 0) {
      validColor = 0xffffff;
    }

    const existingEntity = World.getEntity(packet.uid);
    const entity = World.createEntity(EntityType.Player, packet.uid);
    const isNewEntity = !existingEntity;

    const physics = new PhysicsComponent(entity.id, {
      position: { x: packet.x, y: packet.y },
      velocity: { x: 0, y: 0 },
      acceleration: { x: 0, y: 0 },
      size: 1.0,
      width: 1.0,
      height: 1.0,
      drag: 0.002,
      density: 1,
      elasticity: 0.8,
    });
    physics.setRotation(packet.rotation);
    entity.set(physics);

    if (isNewEntity) {
      const network = new NetworkComponent(entity.id, {
        serverId: packet.uid,
        isLocallyControlled: packet.uid === localPlayerId,
        serverPosition: { x: packet.x, y: packet.y },
        serverVelocity: { x: 0, y: 0 },
        serverRotation: packet.rotation,
      });
      entity.set(network);
    } else if (entity.has(NetworkComponent)) {
      const network = entity.get(NetworkComponent);
      if (network) {
        network.serverPosition = { x: packet.x, y: packet.y };
        network.serverRotation = packet.rotation;
      }
    }

    const isLocalPlayer = packet.uid === localPlayerId;

    if (isLocalPlayer && !isNewEntity && entity.has(RenderComponent)) {
      const existingRender = entity.get(RenderComponent);
      if (existingRender) {
        existingRender.shipParts = packet.parts;
      }

      // Add engine component and particle system if not already present but ship has engines
      const hasEngines = packet.parts.some((part) => part.type === 2);
      if (hasEngines && !entity.has(EngineComponent)) {
        const engineComponent = new EngineComponent(entity.id, {
          maxPropulsion: 100,
          powerUse: 200,
          throttle: 0,
          rcs: false,
          rotation: 0,
        });
        entity.set(engineComponent);
      }

      if (hasEngines && !entity.has(ParticleSystemComponent)) {
        const particleSystem = new ParticleSystemComponent(entity.id, {
          maxParticles: 80,
          emissionRate: 60,
          particleLifetime: 0.6,
          startSize: 0.25,
          endSize: 0.05,
          startColor: 0xffa500,
          endColor: 0xff4500,
          velocityVariance: 3.0,
          spread: Math.PI / 4,
        });
        entity.set(particleSystem);
      }
    } else {
      const render = new RenderComponent(entity.id, {
        sides: 0, // Not used for compound shapes
        shapeType: packet.shapeType,
        color: validColor,
        shipParts: packet.parts,
      });
      entity.set(render);

      // Add engine component and particle system if the entity has engine parts
      const hasEngines = packet.parts.some((part) => part.type === 2);
      if (hasEngines) {
        // Create engine component for entities with engines
        const engineComponent = new EngineComponent(entity.id, {
          maxPropulsion: 100, // Default values - server will override
          powerUse: 200,
          throttle: 0,
          rcs: false,
          rotation: 0,
        });
        entity.set(engineComponent);

        const particleSystem = new ParticleSystemComponent(entity.id, {
          maxParticles: 80,
          emissionRate: 60,
          particleLifetime: 0.6,
          startSize: 0.25,
          endSize: 0.05,
          startColor: 0xffa500, // Orange
          endColor: 0xff4500, // Red-orange
          velocityVariance: 3.0,
          spread: Math.PI / 4, // 45 degree spread
        });
        entity.set(particleSystem);
      }
    }
  }

  static fromBuffer(buffer: ArrayBuffer): SpawnPacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const uid = reader.Guid();
    const shapeType = reader.i32();
    const rotation = reader.f32();
    const x = reader.f32();
    const y = reader.f32();
    const color = reader.u32();

    const partCount = reader.i16();

    const parts: ShipPart[] = [];
    for (let i = 0; i < partCount; i++) {
      const part: ShipPart = {
        gridX: reader.i8(),
        gridY: reader.i8(),
        type: reader.i8(),
        shape: reader.i8(),
        rotation: reader.i8(),
      };
      parts.push(part);
    }

    return new SpawnPacket(
      header,
      uid,
      shapeType,
      rotation,
      x,
      y,
      color,
      parts,
    );
  }
}
