import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";
import { PlayerNameManager } from "../../managers/PlayerNameManager";

export class AssociateIdPacket {
  header: PacketHeader;
  entityId: string;
  name: string;

  constructor(header: PacketHeader, entityId: string, name: string) {
    this.header = header;
    this.entityId = entityId;
    this.name = name;
  }

  static handle(buffer: ArrayBuffer) {
    const packet = AssociateIdPacket.fromBuffer(buffer);

    const nameManager = PlayerNameManager.getInstance();
    nameManager.setPlayerName(packet.entityId, packet.name);
  }

  static fromBuffer(buffer: ArrayBuffer): AssociateIdPacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const entityId = reader.Guid();

    const name = reader.StringWith8bitLengthPrefix();

    return new AssociateIdPacket(header, entityId, name);
  }
}
