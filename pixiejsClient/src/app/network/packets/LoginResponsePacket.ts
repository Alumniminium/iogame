import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";

export class LoginResponsePacket {
  header: PacketHeader;
  playerId: string;
  tickCounter: number;
  positionX: number;
  positionY: number;
  mapWidth: number;
  mapHeight: number;
  viewDistance: number;
  playerColor: number;

  constructor(
    header: PacketHeader,
    playerId: string,
    tickCounter: number,
    positionX: number,
    positionY: number,
    mapWidth: number,
    mapHeight: number,
    viewDistance: number,
    playerColor: number,
  ) {
    this.header = header;
    this.playerId = playerId;
    this.tickCounter = tickCounter;
    this.positionX = positionX;
    this.positionY = positionY;
    this.mapWidth = mapWidth;
    this.mapHeight = mapHeight;
    this.viewDistance = viewDistance;
    this.playerColor = playerColor;
  }

  static fromBuffer(buffer: ArrayBuffer): LoginResponsePacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const playerId = reader.Guid();
    const tickCounter = reader.u32();
    // Server sends Vector2 position (8 bytes: float X, float Y)
    const positionX = reader.f32();
    const positionY = reader.f32();
    const mapWidth = reader.i32();
    const mapHeight = reader.i32();
    const viewDistance = reader.u16();
    const playerColor = reader.u32();

    return new LoginResponsePacket(
      header,
      playerId,
      tickCounter,
      positionX,
      positionY,
      mapWidth,
      mapHeight,
      viewDistance,
      playerColor,
    );
  }
}
