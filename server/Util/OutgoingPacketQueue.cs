using System;
using System.Buffers;
using iogame.Simulation.Entities;

namespace iogame.Util
{
    public static class OutgoingPacketQueue
    {
        public const int MAX_PACKET_SIZE = 1400;
        static readonly Dictionary<Player, Queue<byte[]>> Packets = new();
        static OutgoingPacketQueue() => PerformanceMetrics.RegisterSystem(nameof(OutgoingPacketQueue));

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
                    while (kvp.Value.Count > 0 && kvp.Key.NetworkComponent.Socket.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        var bigPacketIndex = 0;
                        var bigPacket = ArrayPool<byte>.Shared.Rent(MAX_PACKET_SIZE);

                        while (kvp.Value.Count != 0 && bigPacketIndex + BitConverter.ToUInt16(kvp.Value.Peek(),0) < MAX_PACKET_SIZE)
                        {
                            var packet = kvp.Value.Dequeue();
                            var size = BitConverter.ToUInt16(packet,0);
                            Array.Copy(packet, 0, bigPacket, bigPacketIndex, size);
                            ArrayPool<byte>.Shared.Return(packet);
                            bigPacketIndex += size;
                        }
                        await kvp.Key.ForceSendAsync(bigPacket, bigPacketIndex);
                        ArrayPool<byte>.Shared.Return(bigPacket);
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