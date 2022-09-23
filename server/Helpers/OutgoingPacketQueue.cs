using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using server.ECS;
using server.Simulation;
using server.Simulation.Components;

namespace server.Helpers
{
    public static class OutgoingPacketQueue
    {
        private static readonly object SyncRoot = new();
        private const int MAX_PACKET_SIZE = 1024 * 16;
        private static readonly ConcurrentDictionary<PixelEntity, Queue<Memory<byte>>> Packets = new();

        public static void Add(in PixelEntity player, in Memory<byte> packet)
        {
            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<Memory<byte>>();
                Packets.TryAdd(player, queue);
            }
            lock (SyncRoot)
                queue.Enqueue(packet);
        }

        public static void Remove(in PixelEntity player) => Packets.TryRemove(player, out _);

        public static async ValueTask SendAll()
        {
            try
            {
                foreach (var (ntt, queue) in Packets)
                {
                    try
                    {
                        var net = ntt.Get<NetworkComponent>();
                        while (queue.Count > 0)
                        {
                            try
                            {
                                var bigPacketIndex = 0;
                                var bigPacket = ArrayPool<byte>.Shared.Rent(MAX_PACKET_SIZE);

                                while (queue.Count != 0 && bigPacketIndex + MemoryMarshal.Read<ushort>(queue.Peek().Span) < MAX_PACKET_SIZE)
                                {
                                    try
                                    {
                                        var packet = queue.Dequeue();
                                        var size = MemoryMarshal.Read<ushort>(packet.Span);
                                        packet.Span[..size].CopyTo(bigPacket.AsSpan(bigPacketIndex));
                                        bigPacketIndex += size;
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                }

                                try
                                {
                                    await net.Socket.SendAsync(new ArraySegment<byte>(bigPacket, 0, bigPacketIndex), System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(false);
                                }
                                catch (Exception e)
                                {
            Game.Grid.Remove(in ntt);
            PixelWorld.Destroy(in ntt);
                                    FConsole.WriteLine(e.Message);
                                }
                                finally { ArrayPool<byte>.Shared.Return(bigPacket); }

                                if (net.Socket.State == System.Net.WebSockets.WebSocketState.Closed || net.Socket.State == System.Net.WebSockets.WebSocketState.Aborted)
                                    break;
                            }
                            catch (Exception e)
                            {
                                FConsole.WriteLine(e.Message);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                FConsole.WriteLine(e.Message);
            }
        }
    }
}
