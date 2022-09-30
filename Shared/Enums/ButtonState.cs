namespace Packets.Enums
{
    [Flags]
    public enum PlayerInput : ushort
    {
        None = 0,
        Thrust = 1,
        InvThrust = 2,
        Left = 4,
        Right = 8,
        Boost = 16,
        RCS = 32,
        Fire = 64,
        Drop = 128,
        Shield = 256,
    }
}