using System;

namespace server.Helpers;

[Flags]
public enum SyncThings : ushort
{
    None = 0,
    Position = 1,
    Health = 2,
    Generator = 4,
    Viewport = 8,
    Invenory = 16,
    Throttle = 32,
    Battery = 64,
    Shield = 128,
    Level = 256,
    Experience = 512,

    All = 0b1111111111111111,
}