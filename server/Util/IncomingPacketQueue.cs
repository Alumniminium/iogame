using System.Buffers;
using iogame.Net;
using iogame.Simulation.Entities;

namespace iogame.Util
{
    public static class IncomingPacketQueue
    {
        static readonly Dictionary<Player, Queue<byte[]>> _packets = new();

        static IncomingPacketQueue() => PerformanceMetrics.RegisterSystem(nameof(IncomingPacketQueue));

        public static void Add(Player player, byte[] packet)
        {
            if (!_packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<byte[]>();
                _packets.Add(player, queue);
            }
            queue.Enqueue(packet);
        }

        public static void Remove(Player player) => _packets.Remove(player);
        public static void ProcessAll()
        {
            foreach (var kvp in _packets)
                while (kvp.Value.Count > 0)
                {
                    var packet = kvp.Value.Dequeue();
                    PacketHandler.Process(kvp.Key, packet);
                    ArrayPool<byte>.Shared.Return(packet);
                }
        }
    }
}