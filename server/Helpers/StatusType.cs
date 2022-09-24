namespace server.Simulation.Net.Packets
{
    public enum StatusType : byte
    {
        Alive = 0,
        Health = 1,
        MaxHealth = 2,
        Size = 3,
        Direction = 4,
        Throttle = 5,
        BatteryCapacity = 10,
        BatteryCharge = 11,
        BatteryChargeRate = 12,
        BatteryDischargeRate = 13,
        EnginePowerDraw = 14,
        ShieldPowerDraw = 15,
        WeaponPowerDraw = 16,

        ShieldCharge = 20,
        ShieldMaxCharge = 21,
        ShieldRechargeRate = 22,
        ShieldPowerUse = 23,
        ShieldPowerUseRecharge = 24,
        ShieldRadius = 25,

        InventoryCapacity = 100,
        InventoryTriangles = 101,
        InventorySquares = 102,
        InventoryPentagons = 103,

        Level = 200,
        Experience = 201,
        ExperienceToNextLevel = 202,
    }
}