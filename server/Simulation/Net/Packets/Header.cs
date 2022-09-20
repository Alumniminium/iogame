namespace server.Simulation.Net.Packets
{
    public readonly ref struct Header
    {
        public readonly ushort Length;
        public readonly ushort Id;

        public Header(ushort length, PacketId id)
        {
            Length = length;
            Id = (ushort)id;
        }
        public Header(int length, PacketId id)
        {
            Length = (ushort)length;
            Id = (ushort)id;
        }
    }
}