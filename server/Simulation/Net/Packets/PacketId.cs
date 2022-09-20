namespace server.Simulation.Net.Packets
{
    public enum PacketId : ushort
    {
        LoginRequest = 1,
        LoginResponse = 2,
        AssociateId = 3,
        StatusPacket = 4,

        ChatPacket = 10,

        MovePacket = 20,
        PlayerMovePacket = 21,

        PresetSpawnPacket = 30,
        BoxSpawnPacket = 31,
        SphereSpawnPacket = 32,
        LineSpawnPacket = 33,
        RequestSpawnPacket = 39,


        Ping = 90,
    }
}