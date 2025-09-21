import { EvPacketWriter } from '../EvPacketWriter';
import { EvPacketReader } from '../EvPacketReader';
import { PacketHeader } from '../PacketHeader';
import { PacketId } from '../PacketHandler';

export class PingPacket {
    header: PacketHeader
    ping: number
    time: bigint

    constructor(header: PacketHeader, ping: number, time: bigint) {
        this.header = header
        this.ping = ping
        this.time = time
    }

    static create(): ArrayBuffer {
        const writer = new EvPacketWriter(PacketId.Ping);
        writer
            .i16(0) // Ping field (ushort)
            .i64(BigInt(Date.now() * 10000)) // TickCounter (long) - convert milliseconds to .NET ticks
            .FinishPacket();
        return writer.ToArray();
    }

    static fromBuffer(buffer: ArrayBuffer): PingPacket {
        const reader = new EvPacketReader(buffer);
        const header = reader.Header();
        const ping = reader.i16();
        const time = reader.i64();
        return new PingPacket(header, ping, time);
    }

    static handle(buffer: ArrayBuffer): number {
        const packet = PingPacket.fromBuffer(buffer);
        const latency = Date.now() - Number(packet.time / 10000n); // Convert .NET ticks to milliseconds
        return latency;
    }
}