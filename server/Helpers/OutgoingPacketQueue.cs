using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        public static void Add(PixelEntity player, byte[] packet)
        {
            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<byte[]>();
                Packets.TryAdd(player, queue);
            }
            lock (SyncRoot)
                queue.Enqueue(packet);
        }

        public static void Remove(PixelEntity player) => Packets.TryRemove(player, out _);
        public static async Task SendAll()
        {
            foreach (var (key, value) in Packets)
            {
                try
                {
                    while (value.Count > 0)
                    {
                        var bigPacketIndex = 0;
                        var bigPacket = ArrayPool<byte>.Shared.Rent(MAX_PACKET_SIZE);

                        while (value.Count != 0 && bigPacketIndex + BitConverter.ToUInt16(value.Peek(), 0) < MAX_PACKET_SIZE)
                        {
                            lock (SyncRoot)
                            {
                                var packet = value.Dequeue();
                                var size = BitConverter.ToUInt16(packet, 0);
                                Array.Copy(packet, 0, bigPacket, bigPacketIndex, size);
                                ArrayPool<byte>.Shared.Return(packet);
                                bigPacketIndex += size;
                            }
                        }
                        await key.Get<NetworkComponent>().Socket.SendAsync(new ArraySegment<byte>(bigPacket, 0, bigPacketIndex), System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
                        ArrayPool<byte>.Shared.Return(bigPacket);
                    }
                }
                catch (Exception e)
                {
                    FConsole.WriteLine($"{e.Message} {e.StackTrace}");
                    Remove(key);
                }
            }
        }
    }
}