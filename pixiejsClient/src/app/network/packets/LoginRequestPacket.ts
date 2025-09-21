import { EvPacketWriter } from "../EvPacketWriter";
import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";
import { PacketId } from "../PacketHandler";

export class LoginRequestPacket {
  header: PacketHeader;
  name: string;
  password: string;

  constructor(header: PacketHeader, name: string, password: string) {
    this.header = header;
    this.name = name;
    this.password = password;
  }

  static create(name: string, password: string): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.LoginRequest);
    writer
      .StringWith8bitLengthPrefix(name.substring(0, 16)) // Max 16 chars to fit in 17-byte array
      .Goto(17 + 4)
      .StringWith8bitLengthPrefix(password.substring(0, 16)) // Max 16 chars to fit in 17-byte array
      .FinishPacket();
    return writer.ToArray();
  }

  static fromBuffer(buffer: ArrayBuffer): LoginRequestPacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const name = reader.StringWith16bitLengthPrefix();
    const password = reader.StringWith16bitLengthPrefix();
    return new LoginRequestPacket(header, name, password);
  }
}
