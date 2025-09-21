import { EvPacketReader } from '../EvPacketReader';
import { PacketHeader } from '../PacketHeader';
import { World } from '../../ecs/core/World';
import { EntityType } from '../../ecs/core/types';
import { PhysicsComponent } from '../../ecs/components/PhysicsComponent';
import { NetworkComponent } from '../../ecs/components/NetworkComponent';
import { RenderComponent } from '../../ecs/components/RenderComponent';

export class PresetSpawnPacket {
    header: PacketHeader
    entityId: string
    resourceId: number
    direction: number
    x: number
    y: number

    constructor(
        header: PacketHeader,
        entityId: string,
        resourceId: number,
        direction: number,
        x: number,
        y: number
    ) {
        this.header = header
        this.entityId = entityId
        this.resourceId = resourceId
        this.direction = direction
        this.x = x
        this.y = y
    }

    static fromBuffer(buffer: ArrayBuffer): PresetSpawnPacket {
        const reader = new EvPacketReader(buffer);
        const header = reader.Header();
        const entityId = reader.Guid();
        const resourceId = reader.u16();
        const direction = reader.f32();
        const x = reader.f32();
        const y = reader.f32();

        return new PresetSpawnPacket(
            header,
            entityId,
            resourceId,
            direction,
            x,
            y
        );
    }

    static handle(buffer: ArrayBuffer, baseResources: Record<string, any>): void {
        const packet = PresetSpawnPacket.fromBuffer(buffer);

        let entity = World.getEntity(packet.entityId);
        if (!entity) {
            entity = World.createEntity(EntityType.Pickable, packet.entityId);

            const resourceData = baseResources[packet.resourceId.toString()];
            if (!resourceData) {
                console.warn(`Unknown resourceId: ${packet.resourceId}`);
                World.destroyEntity(entity);
                return;
            }

            // Use resourceData for physics and render properties
            const physics = new PhysicsComponent(entity.id, {
                position: { x: packet.x, y: packet.y },
                velocity: { x: 0, y: 0 },
                size: resourceData.Size, // Use size from resource data
                width: resourceData.Width || resourceData.Size,
                height: resourceData.Height || resourceData.Size,
                shapeType: resourceData.ShapeType, // Use shapeType from resource data
                density: resourceData.Density || 1,
                elasticity: resourceData.Elasticity || 0.8,
            });
            physics.setRotation(packet.direction);
            entity.set(physics);

            const network = new NetworkComponent(entity.id, {
                serverId: packet.entityId,
                isLocallyControlled: false,
                serverPosition: { x: packet.x, y: packet.y },
                serverVelocity: { x: 0, y: 0 },
                serverRotation: packet.direction,
            });
            entity.set(network);

            const render = new RenderComponent(entity.id, {
                sides: resourceData.Sides,
                shapeType: resourceData.ShapeType,
                color: resourceData.Color,
            });
            entity.set(render);
        }
    }
}