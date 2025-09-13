namespace server.Enums;

public enum PacketId : ushort
{
    LoginRequest = 1,
    LoginResponse = 2,
    AssociateId = 3,
    StatusPacket = 4,

    ChatPacket = 10,

    MovePacket = 20,
    InputPacket = 21,

    PresetSpawnPacket = 30,
    CustomSpawnPacket = 31,
    LineSpawnPacket = 33,
    RequestSpawnPacket = 39,


    Ping = 90,
}