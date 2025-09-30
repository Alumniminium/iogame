namespace server.Enums;

public enum PacketId : ushort
{
    LoginRequest = 1,
    LoginResponse = 2,
    DespawnPacket = 4,

    ChatPacket = 10,

    InputPacket = 21,

    PresetSpawnPacket = 30,
    CustomSpawnPacket = 31,
    LineSpawnPacket = 33,
    RequestSpawnPacket = 39,

    ComponentState = 50,

    Ping = 90,
}