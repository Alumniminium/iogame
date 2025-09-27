import { EvPacketWriter } from "../EvPacketWriter";
import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";
import { PacketId } from "../PacketHandler";

export interface ShipPart {
  gridX: number;
  gridY: number;
  type: number; // 0=hull, 1=shield, 2=engine
  shape: number; // 1=triangle, 2=square
  rotation: number; // 0=0°, 1=90°, 2=180°, 3=270°
}

export class ShipConfigurationPacket {
  header: PacketHeader;
  playerId: string;
  parts: ShipPart[];

  constructor(header: PacketHeader, playerId: string, parts: ShipPart[]) {
    this.header = header;
    this.playerId = playerId;
    this.parts = parts;
  }

  static create(playerId: string, parts: ShipPart[]): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.ShipConfiguration);
    writer.Guid(playerId);
    writer.i16(parts.length);

    for (const part of parts) {
      writer.i8(part.gridX);
      writer.i8(part.gridY);
      writer.i8(part.type);
      writer.i8(part.shape);
      writer.i8(part.rotation);
    }

    writer.FinishPacket();
    return writer.ToArray();
  }

  static fromBuffer(buffer: ArrayBuffer): ShipConfigurationPacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const playerId = reader.Guid();
    const partCount = reader.i16();

    const parts: ShipPart[] = [];
    for (let i = 0; i < partCount; i++) {
      parts.push({
        gridX: reader.i8(),
        gridY: reader.i8(),
        type: reader.i8(),
        shape: reader.i8(),
        rotation: reader.i8(),
      });
    }

    return new ShipConfigurationPacket(header, playerId, parts);
  }
}
