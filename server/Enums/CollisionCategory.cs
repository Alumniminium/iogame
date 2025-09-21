using System;

namespace server.Enums;

[Flags]
public enum CollisionCategory : uint
{
    None = 0x0000,
    Player = 0x0001,
    Bullet = 0x0002,
    Environment = 0x0004,
    Pickup = 0x0008,
    All = 0xFFFF
}