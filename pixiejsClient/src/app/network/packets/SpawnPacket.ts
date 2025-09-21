import { EvPacketReader } from '../EvPacketReader'
import { PacketHeader } from '../PacketHeader'
import { World } from '../../ecs/core/World'
import { EntityType } from '../../ecs/core/types'
import { PhysicsComponent } from '../../ecs/components/PhysicsComponent'
import { NetworkComponent } from '../../ecs/components/NetworkComponent'
import { RenderComponent } from '../../ecs/components/RenderComponent'

export class SpawnPacket {
    header: PacketHeader
    uid: string
    shapeType: number
    rotation: number
    x: number
    y: number
    color: number

    constructor(
        header: PacketHeader,
        uid: string,
        shapeType: number,
        rotation: number,
        x: number,
        y: number,
        color: number
    ) {
        this.header = header
        this.uid = uid
        this.shapeType = shapeType
        this.rotation = rotation
        this.x = x
        this.y = y
        this.color = color
    }

    static handle(buffer: ArrayBuffer, localPlayerId: string) {
        const packet = SpawnPacket.fromBuffer(buffer)

        // Only log non-square shapes to debug missing triangles and pentagons
        if (packet.shapeType !== 2) {
            console.log(`🟢 SPAWN: shapeType=${packet.shapeType} (${packet.shapeType === 0 ? 'CIRCLE' : packet.shapeType === 1 ? 'TRIANGLE' : packet.shapeType === 2 ? 'BOX' : 'POLYGON'}), size=1x1, pos=(${packet.x},${packet.y}), color=0x${packet.color.toString(16)}`)
        }

        // Validate color value
        let validColor = packet.color
        if (packet.color > 0xFFFFFF || packet.color < 0) {
            console.warn(`Invalid color value: ${packet.color} (0x${packet.color.toString(16)}), using default`)
            validColor = 0xffffff
        }

        // Create entity
        const entity = World.createEntity(EntityType.Player, packet.uid)

        // Add physics component - all shapes are now 1x1
        let sides = 4 // Default for boxes

        if (packet.shapeType === 0) {
            sides = 0 // Circle
        } else if (packet.shapeType === 1) {
            sides = 3 // Triangle
        } else if (packet.shapeType === 2) {
            sides = 4 // Box/Rectangle
        } else {
            sides = Math.max(packet.shapeType, 3) // For other polygon shapes
        }

        const physics = new PhysicsComponent(entity.id, {
            position: { x: packet.x, y: packet.y },
            velocity: { x: 0, y: 0 },
            acceleration: { x: 0, y: 0 },
            size: 1.0,
            width: 1.0,
            height: 1.0,
            drag: 0.002,
            density: 1,
            elasticity: 0.8,
        })
        physics.setRotation(packet.rotation)
        entity.set(physics)

        // Add network component
        const network = new NetworkComponent(entity.id, {
            serverId: packet.uid,
            isLocallyControlled: packet.uid === localPlayerId,
            serverPosition: { x: packet.x, y: packet.y },
            serverVelocity: { x: 0, y: 0 },
            serverRotation: packet.rotation,
        })
        entity.set(network)

        // Add render component with proper sides
        const render = new RenderComponent(entity.id, {
            sides: sides,
            shapeType: packet.shapeType,
            color: validColor,
        })
        entity.set(render)
    }

    static fromBuffer(buffer: ArrayBuffer): SpawnPacket {
        const reader = new EvPacketReader(buffer)
        const header = reader.Header()
        const uid = reader.Guid()
        const shapeType = reader.i32()
        const rotation = reader.f32()
        const x = reader.f32()
        const y = reader.f32()
        const color = reader.u32()

        return new SpawnPacket(header, uid, shapeType, rotation, x, y, color)
    }
}