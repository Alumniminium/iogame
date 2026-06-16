import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";
import { World } from "../../ecs/core/World";
import { LineComponent } from "../../ecs/components/LineComponent";
import { LifeTimeComponent } from "../../ecs/components/LifeTimeComponent";

export class LineSpawnPacket {
  header: PacketHeader;
  uniqueId: string;
  targetUniqueId: string;
  origin: { x: number; y: number };
  hit: { x: number; y: number };

  constructor(header: PacketHeader, uniqueId: string, targetUniqueId: string, origin: { x: number; y: number }, hit: { x: number; y: number }) {
    this.header = header;
    this.uniqueId = uniqueId;
    this.targetUniqueId = targetUniqueId;
    this.origin = origin;
    this.hit = hit;
  }

  static fromBuffer(buffer: ArrayBuffer): LineSpawnPacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const uniqueId = reader.Guid();
    const targetUniqueId = reader.Guid();
    const originX = reader.f32();
    const originY = reader.f32();
    const hitX = reader.f32();
    const hitY = reader.f32();

    return new LineSpawnPacket(header, uniqueId, targetUniqueId, { x: originX, y: originY }, { x: hitX, y: hitY });
  }

  static handle(buffer: ArrayBuffer): void {
    const packet = LineSpawnPacket.fromBuffer(buffer);

    // Create entity for the line with LineComponent and LifetimeComponent
    const lineEntity = World.createEntity(packet.uniqueId);

    const duration = 1000; // milliseconds
    const color = 0xff0000; // red

    lineEntity.set(new LineComponent(lineEntity, packet.origin, packet.hit, color, duration));
    lineEntity.set(new LifeTimeComponent(lineEntity, duration / 1000)); // LifeTimeComponent uses seconds
  }
}
