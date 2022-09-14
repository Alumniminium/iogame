using System.Buffers;
using System.Collections.Generic;
using server.ECS;
using server.Simulation.Net;

namespace server.Helpers
{
    public static class IncomingPacketQueue
    {
        private static readonly Dictionary<PixelEntity, Queue<byte[]>> Packets = new();

        static IncomingPacketQueue()
        {
            PerformanceMetrics.RegisterSystem(nameof(IncomingPacketQueue));
        }

        public static void Add(in PixelEntity player, in byte[] packet)
        {
            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<byte[]>();
                Packets.Add(player, queue);
            }
            queue.Enqueue(packet);
        }

        public static void Remove(in PixelEntity player)
        {
            Packets.Remove(player);
        }

        public static void ProcessAll()
        {
            foreach (var (ntt, queue) in Packets)
                while (queue.Count > 0)
                {
                    var packet = queue.Dequeue();
                    if (!PixelWorld.EntityExists(in ntt))
                    {
                        queue.Clear();
                        continue;
                    }
                    PacketHandler.Process(ntt, packet);
                    ArrayPool<byte>.Shared.Return(packet);
                }
        }
    }
}