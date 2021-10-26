using System;
using iogame.Simulation.Entities;

namespace iogame.Util
{
    public static class OutgoingPacketQueue
    {
        public static Dictionary<Player, Queue<byte[]>> Packets = new();

        public static void Add(Player player, byte[] packet)
        {
            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<byte[]>();
                Packets.Add(player, queue);
            }
            queue.Enqueue(packet);
        }

        public static void Remove(Player player) => Packets.Remove(player);
        public static async Task SendAll()
        {
            foreach (var kvp in Packets)
            {
                try
                {
                    while (kvp.Value.Count > 0 && kvp.Key.Socket.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        var bigPacketIndex = 0;
                        var bigPacket = new byte[800];

                        while (kvp.Value.Count != 0 && bigPacketIndex + kvp.Value.Peek().Length < bigPacket.Length)
                        {
                            var packet = kvp.Value.Dequeue();
                            Array.Copy(packet, 0, bigPacket, bigPacketIndex, packet.Length);
                            bigPacketIndex += packet.Length;
                        }
                        await kvp.Key.ForceSendAsync(bigPacket, bigPacketIndex);
                    }
                }
                catch (Exception e)
                {
                    FConsole.WriteLine(e.Message + " " + e.StackTrace);
                    Remove(kvp.Key);
                }
            }
        }
    }
}