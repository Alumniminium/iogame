using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using server.ECS;
using server.Simulation.Components;

namespace server.Helpers;

/// <summary>
/// Batches multiple packets per client into a single WebSocket send for reduced latency.
/// Automatically creates a queue per client and batches all accumulated packets at flush time.
/// </summary>
public static class PacketQueue
{
    private static readonly ConcurrentDictionary<NTT, Queue<Memory<byte>>> Packets = new();

    /// <summary>
    /// Enqueues a packet for the specified client. Creates a queue automatically if needed.
    /// </summary>
    public static void Enqueue(in NTT player, in Memory<byte> packet)
    {
        if (!Packets.TryGetValue(player, out var queue))
        {
            queue = new Queue<Memory<byte>>();
            Packets.TryAdd(player, queue);
        }
        queue.Enqueue(packet);
    }

    /// <summary>
    /// Removes a player's queue (called on disconnect).
    /// </summary>
    public static void Remove(in NTT player) => Packets.TryRemove(player, out _);

    /// <summary>
    /// Flushes all queued packets by batching them per client into a single send.
    /// This reduces TCP/WebSocket overhead significantly by sending one frame instead of many.
    /// </summary>
    public static void FlushAll()
    {
        foreach (var (ntt, queue) in Packets)
        {
            if (queue.Count == 0) continue;

            try
            {
                var net = ntt.Get<NetworkComponent>();

                // Fast path: single packet, no batching needed
                if (queue.Count == 1)
                {
                    var packet = queue.Dequeue();
                    _ = net.Socket.SendAsync(packet, System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
                    continue;
                }

                // Batch multiple packets into a single buffer
                var totalSize = 0;
                foreach (var packet in queue)
                    totalSize += packet.Length;

                // Rent buffer from pool for zero allocation
                var buffer = ArrayPool<byte>.Shared.Rent(totalSize);
                try
                {
                    var offset = 0;
                    while (queue.Count > 0)
                    {
                        var packet = queue.Dequeue();
                        packet.Span.CopyTo(buffer.AsSpan(offset));
                        offset += packet.Length;
                    }

                    // Send batched packets as a single WebSocket frame
                    _ = net.Socket.SendAsync(new Memory<byte>(buffer, 0, totalSize), System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            catch (Exception)
            {
                // Client disconnected or error, queue will be removed elsewhere
                queue.Clear();
            }
        }
    }
}
