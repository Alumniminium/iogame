namespace server.Simulation.Net.Packets
{
    public readonly ref struct Header
    {
        public readonly ushort Length;
        public readonly ushort Id;

        public Header(ushort length, ushort id)
        {
            Length = length;
            Id = id;
        }
        public Header(int length, ushort id)
        {
            Length = (ushort)length;
            Id = id;
        }
    }
}