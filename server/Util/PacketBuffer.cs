using System;
using iogame.Net;
using iogame.Simulation.Entities;

namespace iogame.Util
{
    public class PacketBuffer
    {
        public Dictionary<Player, Queue<byte[]>> Packets = new();

        public void Add(Player player, byte[] packet)
        {
            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<byte[]>();
                Packets.Add(player, queue);
            }
            queue.Enqueue(packet);
        }

        public async Task SendAll()
        {
            foreach (var kvp in Packets)
            {
                while (kvp.Value.Count > 0)
                {
                    var bigPacketIndex = 0;
                    var bigPacket = new byte[4096]; 

                    while(kvp.Value.Count != 0 && bigPacketIndex + kvp.Value.Peek().Length < bigPacket.Length)
                    { 
                        var packet = kvp.Value.Dequeue();
                        Array.Copy(packet,0,bigPacket,bigPacketIndex,packet.Length);
                        bigPacketIndex += packet.Length;
                    }
                    await kvp.Key.ForceSendAsync(bigPacket,bigPacketIndex);
                }
            }
        }

        public void ProcessAll()
        {
            foreach (var kvp in Packets)
                while (kvp.Value.Count > 0)
                    PacketHandler.Process(kvp.Key, kvp.Value.Dequeue());
        }
    }
}