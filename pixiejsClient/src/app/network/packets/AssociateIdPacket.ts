import { EvPacketReader } from '../EvPacketReader'
import { PacketHeader } from '../PacketHeader'

export class AssociateIdPacket {
    header: PacketHeader
    entityId: string
    name: string

    constructor(header: PacketHeader, entityId: string, name: string) {
        this.header = header
        this.entityId = entityId
        this.name = name
    }

    static handle(buffer: ArrayBuffer) {
        const packet = AssociateIdPacket.fromBuffer(buffer)
        console.log(`📝 Entity ${packet.entityId} associated with name: "${packet.name}"`)
        // Could store this mapping for UI display if needed
    }

    static fromBuffer(buffer: ArrayBuffer): AssociateIdPacket {
        const reader = new EvPacketReader(buffer)
        const header = reader.Header()
        const entityId = reader.Guid()

        // AssociateId packet uses fixed 17-byte name format where first byte is length
        const name = reader.StringWith8bitLengthPrefix()

        return new AssociateIdPacket(header, entityId, name)
    }
}