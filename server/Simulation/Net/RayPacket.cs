using System;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct RayPacket
{
    public Header Header;
    public NTT UniqueId;
    public NTT TargetUniqueId;
    public Vector2 Origin;
    public Vector2 Hit;

    public static RayPacket Create(NTT uniqueId, NTT targetUniqueId, Vector2 origin, Vector2 hit)
    {
        return new RayPacket
        {
            Header = new Header(sizeof(RayPacket), PacketId.LineSpawnPacket),
            UniqueId = uniqueId,
            TargetUniqueId = targetUniqueId,
            Origin = origin,
            Hit = hit
        };
    }

    public static implicit operator Memory<byte>(RayPacket msg)
    {
        var buffer = new byte[sizeof(RayPacket)];
        fixed (byte* p = buffer)
            *(RayPacket*)p = *&msg;
        return buffer;
    }
    public static implicit operator RayPacket(Memory<byte> buffer)
    {
        fixed (byte* p = buffer.Span)
            return *(RayPacket*)p;
    }
}