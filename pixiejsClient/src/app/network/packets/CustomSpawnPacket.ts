import { EvPacketReader } from '../EvPacketReader';
import { PacketHeader } from '../PacketHeader';
import { World } from '../../ecs/core/World';
import { EntityType } from '../../ecs/core/types';
import { PhysicsComponent } from '../../ecs/components/PhysicsComponent';
import { NetworkComponent } from '../../ecs/components/NetworkComponent';
import { RenderComponent } from '../../ecs/components/RenderComponent';

export class CustomSpawnPacket {
    header: PacketHeader
    entityId: string
    shapeType: number
    width: number
    height: number
    rotation: number
    x: number
    y: number
    color: number

    constructor(
        header: PacketHeader,
        entityId: string,
        shapeType: number,
        width: number,
        height: number,
        rotation: number,
        x: number,
        y: number,
        color: number
    ) {
        this.header = header
        this.entityId = entityId
        this.shapeType = shapeType
        this.width = width
        this.height = height
        this.rotation = rotation
        this.x = x
        this.y = y
        this.color = color
    }

    static fromBuffer(buffer: ArrayBuffer): CustomSpawnPacket {
        const reader = new EvPacketReader(buffer);
        const header = reader.Header();
        const entityId = reader.Guid();
        const shapeType = reader.i32();
        const width = reader.f32();
        const height = reader.f32();
        const rotation = reader.f32();
        const x = reader.f32();
        const y = reader.f32();
        const color = reader.u32();

        return new CustomSpawnPacket(
            header,
            entityId,
            shapeType,
            width,
            height,
            rotation,
            x,
            y,
            color
        );
    }

    static handle(buffer: ArrayBuffer): void {
        const packet = CustomSpawnPacket.fromBuffer(buffer);

        // Validate color value
        let validColor = packet.color;
        if (packet.color > 0xFFFFFF || packet.color < 0) {
            console.warn(`Invalid color value: ${packet.color} (0x${packet.color.toString(16)}), using default`);
            validColor = 0xffffff;
        }

        // Create entity in World
        let entity = World.getEntity(packet.entityId);
        if (!entity) {
            entity = World.createEntity(EntityType.Player, packet.entityId);

            // Add physics component
            const actualSize = Math.max(packet.width, packet.height);
            const physics = new PhysicsComponent(entity.id, {
                position: { x: packet.x, y: packet.y },
                velocity: { x: 0, y: 0 },
                acceleration: { x: 0, y: 0 },
                size: actualSize,
                width: packet.width,
                height: packet.height,
                drag: 0.002,
                density: 1,
                elasticity: 0.8,
            });
            physics.setRotation(packet.rotation);
            entity.set(physics);

            // Check if this is the local player
            const isLocalPlayer = packet.entityId === (window as any).localPlayerId;

            // Add network component
            const network = new NetworkComponent(entity.id, {
                serverId: packet.entityId,
                isLocallyControlled: isLocalPlayer,
                serverPosition: { x: packet.x, y: packet.y },
                serverVelocity: { x: 0, y: 0 },
                serverRotation: packet.rotation,
            });
            entity.set(network);

            // Add render component with proper sides
            let sides = 4; // Default for boxes
            if (packet.shapeType === 0) {
                sides = 0; // Circle
            } else if (packet.shapeType === 1) {
                sides = 3; // Triangle
            } else if (packet.shapeType === 2) {
                sides = 4; // Box/Rectangle
            } else {
                sides = Math.max(packet.shapeType, 3); // For other polygon shapes
            }

            const render = new RenderComponent(entity.id, {
                sides: sides,
                shapeType: packet.shapeType,
                color: validColor,
            });
            entity.set(render);

            if (isLocalPlayer) {
                console.log(`🎮 Created LOCAL player entity ${packet.entityId}`);
            }
        }

        // Dispatch event for ECS systems to handle
        const event = new CustomEvent('custom-spawn', {
            detail: {
                entityId: packet.entityId,
                position: { x: packet.x, y: packet.y },
                shapeType: packet.shapeType,
                color: validColor
            }
        });
        window.dispatchEvent(event);
    }
}