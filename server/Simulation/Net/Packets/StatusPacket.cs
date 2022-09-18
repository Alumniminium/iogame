using System.Buffers;

namespace server.Simulation.Net.Packets
{
    public enum StatusType : byte
    {
        Alive = 0,
        Health = 1,
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
    }
    public unsafe struct StatusPacket
    {
        public Header Header;
        public int UniqueId;
        public uint Value;
        public StatusType Type;

        public static StatusPacket Create(int uid, uint val, StatusType type)
        {
            return new StatusPacket
            {
                Header = new Header(sizeof(StatusPacket), 1010),
                UniqueId = uid,
                Value = val,
                Type = type
            };
        }
        public static StatusPacket CreateDespawn(int nttId)
        {
            return new StatusPacket
            {
                Header = new Header(sizeof(StatusPacket), 1010),
                UniqueId = nttId,
                Type = StatusType.Alive,
                Value = 0
            };
        }


        public static implicit operator byte[](StatusPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(StatusPacket));
            fixed (byte* p = buffer)
                *(StatusPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator StatusPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
                return *(StatusPacket*)p;
        }
    }
}