import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";

export class LineSpawnPacket {
  header: PacketHeader;
  uniqueId: string;
  targetUniqueId: string;
  origin: { x: number; y: number };
  hit: { x: number; y: number };

  constructor(
    header: PacketHeader,
    uniqueId: string,
    targetUniqueId: string,
    origin: { x: number; y: number },
    hit: { x: number; y: number },
  ) {
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

    return new LineSpawnPacket(
      header,
      uniqueId,
      targetUniqueId,
      { x: originX, y: originY },
      { x: hitX, y: hitY },
    );
  }

  static handle(buffer: ArrayBuffer): void {
    const packet = LineSpawnPacket.fromBuffer(buffer);

    // Dispatch event for ECS systems to handle
    const event = new CustomEvent("line-spawn", {
      detail: {
        uniqueId: packet.uniqueId,
        targetUniqueId: packet.targetUniqueId,
        origin: packet.origin,
        hit: packet.hit,
      },
    });
    window.dispatchEvent(event);
  }
}
