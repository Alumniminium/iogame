namespace iogame.Net.Packets;

struct Header
{
    public ushort Length;
    public ushort Id;
}

unsafe struct LoginRequestPacket
{
    public Header Header;
    public fixed byte Username[32];
    public fixed byte Password[32];

    public LoginRequestPacket Create(string user, string pass)
    {
        
    }
}

struct LoginResponsePacket
{

}