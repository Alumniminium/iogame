using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using server.ECS;
using server.Simulation.Components;

namespace server.Helpers
{
    public static class OutgoingPacketQueue
    {
        public static object SyncRoot = new object();
        public const int MaxPacketSize = 1024 * 16;
        static readonly Dictionary<PixelEntity, Queue<byte[]>> Packets = new();
        static OutgoingPacketQueue() => PerformanceMetrics.RegisterSystem(nameof(OutgoingPacketQueue));

        public static void Add(PixelEntity player, byte[] packet)
        {
            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<byte[]>();
                Packets.Add(player, queue);
            }
            lock (SyncRoot)
                queue.Enqueue(packet);
        }

        public static void Remove(PixelEntity player) => Packets.Remove(player);
        public static async Task SendAll()
        {
            foreach (var kvp in Packets)
            {
                try
                {
                    while (kvp.Value.Count > 0)
                    {
                        var bigPacketIndex = 0;
                        var bigPacket = ArrayPool<byte>.Shared.Rent(MaxPacketSize);

                        while (kvp.Value.Count != 0 && bigPacketIndex + BitConverter.ToUInt16(kvp.Value.Peek(), 0) < MaxPacketSize)
                        {
                            lock (SyncRoot)
                            {
                                var packet = kvp.Value.Dequeue();
                                var size = BitConverter.ToUInt16(packet, 0);
                                Array.Copy(packet, 0, bigPacket, bigPacketIndex, size);
                                ArrayPool<byte>.Shared.Return(packet);
                                bigPacketIndex += size;
                            }
                        }
                        await kvp.Key.Get<NetworkComponent>().Socket.SendAsync(new ArraySegment<byte>(bigPacket, 0, bigPacketIndex), System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
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