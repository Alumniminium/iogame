using System.Buffers;
using System.Collections.Concurrent;
using server.ECS;
using server.Simulation.Components;

namespace server.Helpers
{
    public static class OutgoingPacketQueue
    {
        private static readonly object SyncRoot = new();
        private const int MAX_PACKET_SIZE = 1024 * 16;
        private static readonly ConcurrentDictionary<PixelEntity, Queue<byte[]>> Packets = new();
        static OutgoingPacketQueue() => PerformanceMetrics.RegisterSystem(nameof(OutgoingPacketQueue));

        public static void Add(in PixelEntity player, in byte[] packet)
        {
#if DEBUG
            if (!player.IsPlayer())
                throw new ArgumentException("Only players can send packets.");
#endif

            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<byte[]>();
                Packets.TryAdd(player, queue);
            }
            lock (SyncRoot)
                queue.Enqueue(packet);
        }

        public static void Remove(in PixelEntity player) => Packets.TryRemove(player, out _);
        public static async void SendAll()
        {
            foreach (var (ntt, queue) in Packets)
            {
                var net = ntt.Get<NetworkComponent>();
                while (queue.Count > 0)
                {
                    var bigPacketIndex = 0;
                    var bigPacket = ArrayPool<byte>.Shared.Rent(MAX_PACKET_SIZE);

                    while (queue.Count != 0 && bigPacketIndex + BitConverter.ToUInt16(queue.Peek(), 0) < MAX_PACKET_SIZE)
                    {
                        var packet = queue.Dequeue();
                        var size = BitConverter.ToUInt16(packet, 0);
                        Array.Copy(packet, 0, bigPacket, bigPacketIndex, size);
                        ArrayPool<byte>.Shared.Return(packet);
                        bigPacketIndex += size;
                    }
                    try { await net.Socket.SendAsync(new ArraySegment<byte>(bigPacket, 0, bigPacketIndex), System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None); }
                    catch { Remove(in ntt); }
                    finally { ArrayPool<byte>.Shared.Return(bigPacket); }
                }
            }
        }
    }
}