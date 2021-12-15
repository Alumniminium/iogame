using System.Buffers;
using server.ECS;
using server.Simulation.Net;

namespace server.Helpers
{
    public static class IncomingPacketQueue
    {
        static readonly Dictionary<PixelEntity, Queue<byte[]>> Packets = new();

        static IncomingPacketQueue() => PerformanceMetrics.RegisterSystem(nameof(IncomingPacketQueue));

        public static void Add(PixelEntity player, byte[] packet)
        {
            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<byte[]>();
                Packets.Add(player, queue);
            }
            queue.Enqueue(packet);
        }

        public static void Remove(PixelEntity player) => Packets.Remove(player);
        public static void ProcessAll()
        {
            foreach (var (entity, queue) in Packets)
                while (queue.Count > 0)
                {
                    var packet = queue.Dequeue();
                    if(!PixelWorld.EntityExists(entity.EntityId))
                    {
                        queue.Clear();
                        continue;
                    }
                    PacketHandler.Process(entity, packet);
                    ArrayPool<byte>.Shared.Return(packet);
                }
        }
    }
}