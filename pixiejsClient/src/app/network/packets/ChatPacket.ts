import { EvPacketWriter } from "../EvPacketWriter";
import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";
import { PacketId } from "../PacketHandler";

export class ChatPacket {
  header: PacketHeader;
  playerId: string;
  channel: number;
  message: string;

  constructor(
    header: PacketHeader,
    playerId: string,
    channel: number,
    message: string,
  ) {
    this.header = header;
    this.playerId = playerId;
    this.channel = channel;
    this.message = message;
  }

  static create(
    playerId: string,
    message: string,
    channel: number = 0,
  ): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.ChatPacket);
    writer
      .Guid(playerId) // UserId (16 bytes)
      .i8(channel) // Channel (1 byte)
      .StringWith8bitLengthPrefix(message) // Message with 8-bit length prefix
      .FinishPacket();
    return writer.ToArray();
  }

  static fromBuffer(buffer: ArrayBuffer): ChatPacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const playerId = reader.Guid();
    const channel = reader.i8();
    const message = reader.StringWith8bitLengthPrefix();

    return new ChatPacket(header, playerId, channel, message);
  }

  static handle(buffer: ArrayBuffer): void {
    const packet = ChatPacket.fromBuffer(buffer);
    console.log(`[Chat] Player ${packet.playerId}: ${packet.message}`);

    // Dispatch event for UI to handle
    const event = new CustomEvent("chat-message", {
      detail: {
        playerId: packet.playerId,
        message: packet.message,
        channel: packet.channel,
      },
    });
    window.dispatchEvent(event);
  }
}
