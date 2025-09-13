export enum PacketId {
  LoginRequest = 1,
  LoginResponse = 2,
  AssociateId = 3,
  Status = 4,
  Chat = 10,
  Movement = 20,
  PlayerMovement = 21,
  PresetSpawn = 30,
  CustomSpawn = 31,
  LineSpawn = 33,
  RequestSpawn = 39,
  Ping = 90,
}

export class PacketHandler {
  private handlers = new Map<
    PacketId,
    (view: DataView, offset: number) => void
  >();

  registerHandler(
    id: PacketId,
    handler: (view: DataView, offset: number) => void,
  ): void {
    this.handlers.set(id, handler);
  }

  processPacket(data: ArrayBuffer): void {
    const view = new DataView(data);
    let offset = 0;

    while (offset < data.byteLength) {
      // Check if we have enough bytes for header
      if (offset + 4 > data.byteLength) {
        // Not enough data for complete header, ignore remaining bytes
        break;
      }

      const packetLength = view.getUint16(offset, true);
      const packetId = view.getUint16(offset + 2, true) as PacketId;

      // Validate packet length
      if (packetLength < 4 || packetLength > 65535) {
        // Invalid packet length, try to recover by skipping this byte
        offset += 1;
        continue;
      }

      if (offset + packetLength > data.byteLength) {
        // Incomplete packet, ignore remaining bytes
        break;
      }

      const handler = this.handlers.get(packetId);
      if (handler) {
        try {
          handler(view, offset);
        } catch (error) {
          console.error(`Error processing packet ${packetId}:`, error);
        }
      } else {
        console.warn(
          `Unknown packet ID: ${packetId} (length: ${packetLength})`,
        );
      }

      offset += packetLength;
    }
  }

  static createLoginRequest(name: string, password: string): ArrayBuffer {
    const nameBytes = new TextEncoder().encode(name);
    const passBytes = new TextEncoder().encode(password);
    const totalLength = 4 + 2 + nameBytes.length + 2 + passBytes.length;

    const buffer = new ArrayBuffer(totalLength);
    const view = new DataView(buffer);

    view.setInt16(0, totalLength, true);
    view.setInt16(2, PacketId.LoginRequest, true);
    view.setInt16(4, nameBytes.length, true);

    let offset = 6;
    for (let i = 0; i < nameBytes.length; i++) {
      view.setUint8(offset++, nameBytes[i]);
    }

    view.setInt16(offset, passBytes.length, true);
    offset += 2;

    for (let i = 0; i < passBytes.length; i++) {
      view.setUint8(offset++, passBytes[i]);
    }

    return buffer;
  }

  static createMovementPacket(
    playerId: number,
    sequenceNumber: number,
    inputState: {
      thrust: boolean;
      invThrust: boolean;
      left: boolean;
      right: boolean;
      boost: boolean;
      rcs: boolean;
      fire: boolean;
      drop: boolean;
      shield: boolean;
    },
    mouseX: number,
    mouseY: number,
    wasThrusting: boolean = false,
  ): ArrayBuffer {
    // Convert input state to PlayerInput flags (matching server enum)
    let playerInputFlags = 0;

    // Handle thrust with automatic decay logic
    if (inputState.thrust) {
      playerInputFlags |= 1; // Thrust
    } else if (inputState.invThrust) {
      playerInputFlags |= 2; // InvThrust
    } else if (wasThrusting && !inputState.thrust) {
      // If we were thrusting but now released W, send InvThrust to decay
      playerInputFlags |= 2; // InvThrust (automatic decay)
    }

    if (inputState.left) playerInputFlags |= 4; // Left
    if (inputState.right) playerInputFlags |= 8; // Right
    if (inputState.boost) playerInputFlags |= 16; // Boost
    if (inputState.rcs) playerInputFlags |= 32; // RCS
    if (inputState.fire) playerInputFlags |= 64; // Fire
    if (inputState.drop) playerInputFlags |= 128; // Drop
    if (inputState.shield) playerInputFlags |= 256; // Shield

    // PlayerMovementPacket structure: Header(4) + UniqueId(4) + TickCounter(4) + PlayerInput(2) + MousePosition(8) = 22 bytes
    const buffer = new ArrayBuffer(22);
    const view = new DataView(buffer);

    view.setInt16(0, 22, true); // Header length
    view.setInt16(2, PacketId.PlayerMovement, true); // Header packet ID
    view.setInt32(4, playerId, true); // UniqueId (must match player ID!)
    view.setUint32(8, sequenceNumber, true); // TickCounter
    view.setUint16(12, playerInputFlags, true); // PlayerInput flags
    view.setFloat32(14, mouseX, true); // MousePosition.X
    view.setFloat32(18, mouseY, true); // MousePosition.Y

    return buffer;
  }

  static createChatPacket(playerId: number, message: string): ArrayBuffer {
    const msgBytes = new TextEncoder().encode(message);
    const totalLength = 4 + 4 + 2 + msgBytes.length;

    const buffer = new ArrayBuffer(totalLength);
    const view = new DataView(buffer);

    view.setInt16(0, totalLength, true);
    view.setInt16(2, PacketId.Chat, true);
    view.setInt32(4, playerId, true);
    view.setInt16(8, msgBytes.length, true);

    let offset = 10;
    for (let i = 0; i < msgBytes.length; i++) {
      view.setUint8(offset++, msgBytes[i]);
    }

    return buffer;
  }

  static createRequestSpawnPacket(): ArrayBuffer {
    const buffer = new ArrayBuffer(4);
    const view = new DataView(buffer);

    view.setInt16(0, 4, true);
    view.setInt16(2, PacketId.RequestSpawn, true);

    return buffer;
  }

  static readString(
    view: DataView,
    offset: number,
  ): { value: string; bytesRead: number } {
    // Check if we have enough bytes to read the length
    if (offset + 2 > view.byteLength) {
      throw new RangeError(
        `Cannot read string length at offset ${offset}: buffer too small`,
      );
    }

    const length = view.getInt16(offset, true);

    // Check if we have enough bytes to read the string data
    if (offset + 2 + length > view.byteLength) {
      throw new RangeError(
        `Cannot read string of length ${length} at offset ${offset + 2}: buffer too small`,
      );
    }

    const bytes = new Uint8Array(length);

    for (let i = 0; i < length; i++) {
      bytes[i] = view.getUint8(offset + 2 + i);
    }

    return {
      value: new TextDecoder().decode(bytes),
      bytesRead: 2 + length,
    };
  }
}
