import { EvPacketWriter } from "../EvPacketWriter";
import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";
import { PacketId } from "../PacketHandler";

export interface InputState {
  thrust: boolean;
  invThrust: boolean;
  left: boolean;
  right: boolean;
  boost: boolean;
  rcs: boolean;
  fire: boolean;
  drop: boolean;
  shield: boolean;
}

export class PlayerMovementPacket {
  header: PacketHeader;
  playerId: string;
  sequenceNumber: number;
  playerInputFlags: number;
  mouseX: number;
  mouseY: number;

  constructor(
    header: PacketHeader,
    playerId: string,
    sequenceNumber: number,
    playerInputFlags: number,
    mouseX: number,
    mouseY: number,
  ) {
    this.header = header;
    this.playerId = playerId;
    this.sequenceNumber = sequenceNumber;
    this.playerInputFlags = playerInputFlags;
    this.mouseX = mouseX;
    this.mouseY = mouseY;
  }

  static create(
    playerId: string,
    sequenceNumber: number,
    inputState: InputState,
    mouseX: number,
    mouseY: number,
    wasThrusting: boolean = false,
  ): ArrayBuffer {
    let playerInputFlags = 0;

    if (inputState.thrust) {
      playerInputFlags |= 1; // Thrust
    } else if (inputState.invThrust) {
      playerInputFlags |= 2; // InvThrust
    } else if (wasThrusting && !inputState.thrust) {
      playerInputFlags |= 2; // InvThrust (automatic decay)
    }

    if (inputState.left) playerInputFlags |= 4; // Left
    if (inputState.right) playerInputFlags |= 8; // Right
    if (inputState.boost) playerInputFlags |= 16; // Boost
    if (inputState.rcs) playerInputFlags |= 32; // RCS
    if (inputState.fire) playerInputFlags |= 64; // Fire
    if (inputState.drop) playerInputFlags |= 128; // Drop
    if (inputState.shield) playerInputFlags |= 256; // Shield

    const writer = new EvPacketWriter(PacketId.InputPacket);
    writer
      .Guid(playerId) // UniqueId (16 bytes)
      .i32(sequenceNumber) // TickCounter (4 bytes)
      .i16(playerInputFlags) // PlayerInput flags (2 bytes)
      .f32(mouseX) // MousePosition.X (4 bytes)
      .f32(mouseY) // MousePosition.Y (4 bytes)
      .FinishPacket();
    return writer.ToArray();
  }

  static fromBuffer(buffer: ArrayBuffer): PlayerMovementPacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const playerId = reader.Guid();
    const sequenceNumber = reader.i32();
    const playerInputFlags = reader.i16();
    const mouseX = reader.f32();
    const mouseY = reader.f32();

    return new PlayerMovementPacket(
      header,
      playerId,
      sequenceNumber,
      playerInputFlags,
      mouseX,
      mouseY,
    );
  }
}
