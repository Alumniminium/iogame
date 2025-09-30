import { EvPacketWriter } from "../EvPacketWriter";
import { PacketId } from "../PacketHandler";

export class LoginRequestPacket {
  static create(name: string, password: string): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.LoginRequest);
    writer
      .StringWith8bitLengthPrefix(name.substring(0, 16)) // Max 16 chars to fit in 17-byte array
      .Goto(17 + 4)
      .StringWith8bitLengthPrefix(password.substring(0, 16)) // Max 16 chars to fit in 17-byte array
      .FinishPacket();
    return writer.ToArray();
  }
}
