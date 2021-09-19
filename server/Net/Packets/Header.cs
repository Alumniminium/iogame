namespace iogame.Net.Packets
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
    }
}