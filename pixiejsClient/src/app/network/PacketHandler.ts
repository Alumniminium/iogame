
import { LoginResponsePacket } from './packets/LoginResponsePacket';
import { MovementPacket } from './packets/MovementPacket';
import { StatusPacket } from './packets/StatusPacket';
import { AssociateIdPacket } from './packets/AssociateIdPacket';
import { ChatPacket } from './packets/ChatPacket';
import { PingPacket } from './packets/PingPacket';
import { SpawnPacket } from './packets/SpawnPacket';
import { LineSpawnPacket } from './packets/LineSpawnPacket';

export enum PacketId {
  LoginRequest = 1,
  LoginResponse = 2,
  AssociateId = 3,
  StatusPacket = 4,
  ChatPacket = 10,
  MovePacket = 20,
  InputPacket = 21,
  SpawnPacket = 29,
  PresetSpawnPacket = 30,
  CustomSpawnPacket = 31,
  LineSpawnPacket = 33,
  RequestSpawnPacket = 39,
  Ping = 90,
}

export class PacketHandler {
  private packetStats = new Map<PacketId, { count: number; lastSeen: number }>();
  private unknownPackets = new Map<number, { count: number; lastSeen: number }>();

  private handleLoginResponse(packet: LoginResponsePacket): void {
    console.log(`🎮 Login successful! Player ID: ${packet.playerId}`);

    // Store local player ID globally for other packets to access
    (window as any).localPlayerId = packet.playerId;

    // Dispatch event for GameScreen to handle
    const event = new CustomEvent('login-response', {
      detail: {
        playerId: packet.playerId,
        tickCounter: packet.tickCounter,
        position: { x: packet.posX, y: packet.posY },
        mapSize: { width: packet.mapWidth, height: packet.mapHeight },
        viewDistance: packet.viewDistance,
        playerColor: packet.playerColor
      }
    });
    window.dispatchEvent(event);
  }

  processPacket(data: ArrayBuffer): void {
    const view = new DataView(data);
    let offset = 0;
    let packetsProcessed = 0;

    while (offset < data.byteLength) {
      // Check if we have enough bytes for header
      if (offset + 4 > data.byteLength) {
        break;
      }

      // If we're exactly at the end, we're done
      if (offset === data.byteLength) {
        break;
      }

      const packetLength = view.getUint16(offset, true);
      const packetId = view.getUint16(offset + 2, true) as PacketId;

      // Validate packet length
      if (packetLength < 4 || packetLength > 65535) {
        console.error(`Invalid packet length: ${packetLength} at offset ${offset}, packet ID: ${packetId}`);
        console.error(`Buffer context:`, new Uint8Array(data.slice(Math.max(0, offset - 8), offset + 16)));

        // Try to find next valid packet header by looking for reasonable length values
        let recoveryOffset = offset + 1;
        let foundValid = false;

        while (recoveryOffset < data.byteLength - 4) {
          const testLength = view.getUint16(recoveryOffset, true);
          const testId = view.getUint16(recoveryOffset + 2, true);

          // Check if this looks like a valid packet header (reasonable length and known packet ID)
          const validPacketIds = Object.values(PacketId).filter(id => typeof id === 'number') as number[];
          if (testLength >= 4 && testLength <= 65535 &&
            recoveryOffset + testLength <= data.byteLength &&
            validPacketIds.includes(testId)) {
            offset = recoveryOffset;
            foundValid = true;
            break;
          }
          recoveryOffset++;
        }

        if (!foundValid) {
          console.error(`Could not recover from invalid packet, dropping remaining ${data.byteLength - offset} bytes`);
          break;
        }
        continue;
      }

      if (offset + packetLength > data.byteLength) {
        break;
      }

      try {
        const data = view.buffer.slice(offset) as ArrayBuffer;
        let processed = false;

        switch (packetId) {
          case PacketId.LoginResponse:
            const loginPacket = LoginResponsePacket.fromBuffer(data);
            this.handleLoginResponse(loginPacket);
            processed = true;
            break;

          case PacketId.MovePacket:
            MovementPacket.handle(data);
            processed = true;
            break;

          case PacketId.StatusPacket:
            StatusPacket.handle(data);
            processed = true;
            break;

          case PacketId.AssociateId:
            AssociateIdPacket.handle(data);
            processed = true;
            break;

          case PacketId.ChatPacket:
            ChatPacket.handle(data);
            processed = true;
            break;

          case PacketId.Ping:
            const ping = PingPacket.handle(data);
            console.log(`🎮 Ping: ${ping}ms`);
            processed = true;
            break;

          case PacketId.SpawnPacket:
            SpawnPacket.handle(data, (window as any).localPlayerId || '');
            processed = true;
            break;

          case PacketId.LineSpawnPacket:
            LineSpawnPacket.handle(data);
            processed = true;
            break;

          case PacketId.InputPacket:
            processed = true;
            break;

          default:
            console.warn(`Unknown packet ID: ${packetId}`);
            break;
        }

        if (processed) {
          packetsProcessed++;

          // Update packet statistics
          const stats = this.packetStats.get(packetId) || { count: 0, lastSeen: 0 };
          stats.count++;
          stats.lastSeen = Date.now();
          this.packetStats.set(packetId, stats);
        } else {
          // Track unknown packets
          const unknownStats = this.unknownPackets.get(packetId) || { count: 0, lastSeen: 0 };
          unknownStats.count++;
          unknownStats.lastSeen = Date.now();
          this.unknownPackets.set(packetId, unknownStats);

          if (unknownStats.count <= 3) {
            console.warn(`Unknown packet ID: ${packetId} length: ${packetLength} (seen ${unknownStats.count} times)`);
          }
        }
      } catch (error) {
        console.error(`Error processing packet ${packetId} (${PacketId[packetId] || 'Unknown'}) at offset ${offset}:`, error);
      }

      offset += packetLength;
    }

    if (packetsProcessed === 0 && data.byteLength > 0) {
      console.error(`No packets processed from ${data.byteLength} bytes of data`);
    }
  }
}
