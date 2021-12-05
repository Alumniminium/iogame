using System.Buffers;
using iogame.ECS;
using iogame.Net;
using iogame.Simulation.Entities;

namespace iogame.Util
{
    public static class IncomingPacketQueue
    {
        static readonly Dictionary<PixelEntity, Queue<byte[]>> _packets = new();

        static IncomingPacketQueue() => PerformanceMetrics.RegisterSystem(nameof(IncomingPacketQueue));

        public static void Add(PixelEntity player, byte[] packet)
        {
            if (!_packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<byte[]>();
                _packets.Add(player, queue);
            }
            queue.Enqueue(packet);
        }

        public static void Remove(PixelEntity player) => _packets.Remove(player);
        public static void ProcessAll()
        {
            foreach (var kvp in _packets)
                while (kvp.Value.Count > 0)
                {
                    var packet = kvp.Value.Dequeue();
                    if(packet == null)
                        continue;
                    PacketHandler.Process(kvp.Key, packet);
                    ArrayPool<byte>.Shared.Return(packet);
                }
        }
    }
}