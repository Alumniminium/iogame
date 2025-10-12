import { LoginResponsePacket } from "./packets/LoginResponsePacket";
import { ChatPacket } from "./packets/ChatPacket";
import { PingPacket } from "./packets/PingPacket";
import { LineSpawnPacket } from "./packets/LineSpawnPacket";
import { ComponentStatePacket } from "./packets/ComponentStatePacket";
import { World } from "../ecs/core/World";

/**
 * Packet type identifiers matching server protocol
 */
export enum PacketId {
  LoginRequest = 1,
  LoginResponse = 2,
  ChatPacket = 10,
  InputPacket = 21,
  PresetSpawnPacket = 30,
  CustomSpawnPacket = 31,
  LineSpawnPacket = 33,
  ComponentState = 50,
  Ping = 90,
}

/**
 * Processes incoming binary packets from server.
 * Handles packet parsing, routing to handlers, and error recovery.
 */
export class PacketHandler {
  private packetStats = new Map<PacketId, { count: number; lastSeen: number }>();
  private packetQueue: ArrayBuffer[] = [];

  private handleLoginResponse(packet: LoginResponsePacket): void {
    (window as any).localPlayerId = packet.playerId;

    // Sync client tick to server tick on login
    World.currentTick = BigInt(packet.tickCounter);

    const event = new CustomEvent("login-response", {
      detail: {
        playerId: packet.playerId,
        tickCounter: packet.tickCounter,
        mapSize: { width: packet.mapWidth, height: packet.mapHeight },
        viewDistance: packet.viewDistance,
      },
    });
    window.dispatchEvent(event);
  }

  /**
   * Queue a binary packet buffer for processing on next tick
   */
  queuePacket(data: ArrayBuffer): void {
    this.packetQueue.push(data);
  }

  /**
   * Process all queued packets. Should be called at the beginning of each tick.
   */
  processQueuedPackets(): void {
    const packetsToProcess = [...this.packetQueue];
    this.packetQueue.length = 0;

    for (const data of packetsToProcess) {
      this.processPacket(data);
    }
  }

  /**
   * Process a binary packet buffer, handling multiple packets in one message
   */
  private processPacket(data: ArrayBuffer): void {
    const view = new DataView(data);
    let offset = 0;
    let packetsProcessed = 0;

    while (offset < data.byteLength) {
      if (offset + 4 > data.byteLength) break;

      if (offset === data.byteLength) break;

      const packetLength = view.getUint16(offset, true);
      const packetId = view.getUint16(offset + 2, true) as PacketId;

      if (packetLength < 4 || packetLength > 65535) {
        let recoveryOffset = offset + 1;
        let foundValid = false;

        while (recoveryOffset < data.byteLength - 4) {
          const testLength = view.getUint16(recoveryOffset, true);
          const testId = view.getUint16(recoveryOffset + 2, true);

          const validPacketIds = Object.values(PacketId).filter((id) => typeof id === "number") as number[];
          if (testLength >= 4 && testLength <= 65535 && recoveryOffset + testLength <= data.byteLength && validPacketIds.includes(testId)) {
            offset = recoveryOffset;
            foundValid = true;
            break;
          }
          recoveryOffset++;
        }

        if (!foundValid) break;
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

          case PacketId.ChatPacket:
            ChatPacket.handle(data);
            processed = true;
            break;

          case PacketId.Ping:
            PingPacket.handle(data);
            processed = true;
            break;

          case PacketId.LineSpawnPacket:
            LineSpawnPacket.handle(data);
            processed = true;
            break;

          case PacketId.InputPacket:
            processed = true;
            break;

          case PacketId.ComponentState:
            ComponentStatePacket.handle(data);
            processed = true;
            break;

          default:
            break;
        }

        if (processed) {
          packetsProcessed++;

          const stats = this.packetStats.get(packetId) || {
            count: 0,
            lastSeen: 0,
          };
          stats.count++;
          stats.lastSeen = Date.now();
          this.packetStats.set(packetId, stats);
        } else {
          console.log("Unhandled packet", packetId);
        }
      } catch (error) {
        console.error(`[PacketHandler] Error processing packet ${packetId}:`, error);
      }

      offset += packetLength;
    }
  }
}
