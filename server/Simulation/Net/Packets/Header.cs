namespace server.Simulation.Net.Packets
{
    public struct Header
    {
        public ushort Length;
        public ushort Id;

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