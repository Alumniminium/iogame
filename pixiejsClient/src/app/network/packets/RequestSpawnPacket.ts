import { EvPacketWriter } from '../EvPacketWriter';
import { EvPacketReader } from '../EvPacketReader';
import { PacketHeader } from '../PacketHeader';
import { PacketId } from '../PacketHandler';

export class RequestSpawnPacket {
    header: PacketHeader
    requester: string
    target: string

    constructor(header: PacketHeader, requester: string, target: string) {
        this.header = header
        this.requester = requester
        this.target = target
    }

    static create(requesterId: string, targetId: string): ArrayBuffer {
        const writer = new EvPacketWriter(PacketId.RequestSpawnPacket);
        writer
            .Guid(requesterId) // Requester NTT
            .Guid(targetId)    // Target NTT
            .FinishPacket();
        return writer.ToArray();
    }

    static fromBuffer(buffer: ArrayBuffer): RequestSpawnPacket {
        const reader = new EvPacketReader(buffer);
        const header = reader.Header();
        const requester = reader.Guid();
        const target = reader.Guid();
        return new RequestSpawnPacket(header, requester, target);
    }
}