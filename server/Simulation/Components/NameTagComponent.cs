using System;
using System.Runtime.InteropServices;
using System.Text;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.NameTag, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct NameTagComponent
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick;

    /// <summary>
    /// Fixed-size buffer for the name (64 bytes max)
    /// </summary>
    public fixed byte NameBytes[64];

    public NameTagComponent(string name)
    {
        ChangedTick = NttWorld.Tick;
        var bytes = Encoding.UTF8.GetBytes(name);
        var length = Math.Min(bytes.Length, 63); // Leave space for null terminator

        fixed (byte* ptr = NameBytes)
        {
            for (int i = 0; i < length; i++)
                ptr[i] = bytes[i];
            ptr[length] = 0; // Null terminator
        }
    }

    public readonly string Name
    {
        get
        {
            fixed (byte* ptr = NameBytes)
            {
                var span = new ReadOnlySpan<byte>(ptr, 64);
                var nullIndex = span.IndexOf((byte)0);
                if (nullIndex >= 0)
                    span = span[..nullIndex];
                return Encoding.UTF8.GetString(span);
            }
        }
    }
}