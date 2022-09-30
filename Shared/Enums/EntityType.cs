namespace Packets.Enums
{
    [Flags]
    public enum EntityType
    {
        Static = 1,
        Passive = 2,
        Pickable = 4,
        Projectile = 8,
        Npc = 16,
        Player = 32,
    }
}