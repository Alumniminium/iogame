import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";

export class LoginResponsePacket {
  header: PacketHeader;
  playerId: string;
  tickCounter: number;
  posX: number;
  posY: number;
  mapWidth: number;
  mapHeight: number;
  viewDistance: number;
  playerColor: number;

  constructor(
    header: PacketHeader,
    playerId: string,
    tickCounter: number,
    posX: number,
    posY: number,
    mapWidth: number,
    mapHeight: number,
    viewDistance: number,
    playerColor: number,
  ) {
    this.header = header;
    this.playerId = playerId;
    this.tickCounter = tickCounter;
    this.posX = posX;
    this.posY = posY;
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
    const posX = reader.f32();
    const posY = reader.f32();
    const mapWidth = reader.i32();
    const mapHeight = reader.i32();
    const viewDistance = reader.i16();
    const playerColor = reader.u32();

    return new LoginResponsePacket(
      header,
      playerId,
      tickCounter,
      posX,
      posY,
      mapWidth,
      mapHeight,
      viewDistance,
      playerColor,
    );
  }
}
