import { EvPacketReader } from '../EvPacketReader';
import { PacketHeader } from '../PacketHeader';

export class MovementPacket {
    header: PacketHeader
    entityId: string
    tickCounter: number
    x: number
    y: number
    velocityX: number
    velocityY: number
    rotation: number

    constructor(
        header: PacketHeader,
        entityId: string,
        tickCounter: number,
        x: number,
        y: number,
        velocityX: number,
        velocityY: number,
        rotation: number
    ) {
        this.header = header
        this.entityId = entityId
        this.tickCounter = tickCounter
        this.x = x
        this.y = y
        this.velocityX = velocityX
        this.velocityY = velocityY
        this.rotation = rotation
    }

    static fromBuffer(buffer: ArrayBuffer): MovementPacket {
        const reader = new EvPacketReader(buffer);
        const header = reader.Header();
        const entityId = reader.Guid();
        const tickCounter = reader.u32();
        const x = reader.f32();
        const y = reader.f32();
        const velocityX = reader.f32();
        const velocityY = reader.f32();
        const rotation = reader.f32();

        return new MovementPacket(
            header,
            entityId,
            tickCounter,
            x,
            y,
            velocityX,
            velocityY,
            rotation
        );
    }

    static handle(buffer: ArrayBuffer): MovementPacket {
        const packet = MovementPacket.fromBuffer(buffer);

        // Dispatch a custom event that the NetworkSystem can listen to
        const event = new CustomEvent('server-movement-update', {
            detail: {
                entityId: packet.entityId,
                position: { x: packet.x, y: packet.y },
                velocity: { x: packet.velocityX, y: packet.velocityY },
                rotation: packet.rotation,
                timestamp: Date.now(),
                tickCounter: packet.tickCounter
            }
        });

        window.dispatchEvent(event);

        return packet;
    }
}