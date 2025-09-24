import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";
import { World } from "../../ecs/core/World";
import { EntityType } from "../../ecs/core/types";
import { PhysicsComponent } from "../../ecs/components/PhysicsComponent";
import { NetworkComponent } from "../../ecs/components/NetworkComponent";
import { RenderComponent } from "../../ecs/components/RenderComponent";

interface ShipPart {
  gridX: number;
  gridY: number;
  type: number; // 0=hull, 1=shield, 2=engine
  shape: number; // 0=triangle, 1=square
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
  centerX: number;
  centerY: number;

  constructor(
    header: PacketHeader,
    uid: string,
    shapeType: number,
    rotation: number,
    x: number,
    y: number,
    color: number,
    parts: ShipPart[] = [],
    centerX: number = 0,
    centerY: number = 0,
  ) {
    this.header = header;
    this.uid = uid;
    this.shapeType = shapeType;
    this.rotation = rotation;
    this.x = x;
    this.y = y;
    this.color = color;
    this.parts = parts;
    this.centerX = centerX;
    this.centerY = centerY;
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
        existingRender.centerX = packet.centerX;
        existingRender.centerY = packet.centerY;
      }
    } else {
      const render = new RenderComponent(entity.id, {
        sides: 0, // Not used for compound shapes
        shapeType: packet.shapeType,
        color: validColor,
        shipParts: packet.parts,
        centerX: packet.centerX,
        centerY: packet.centerY,
      });
      entity.set(render);
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

    const centerX = reader.i8();
    const centerY = reader.i8();

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
      centerX,
      centerY,
    );
  }
}
