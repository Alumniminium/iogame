using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using server.ECS;
using server.Simulation.Components;

namespace server.Helpers;

public static class OutgoingPacketQueue
{
    private static readonly ConcurrentDictionary<NTT, Queue<Memory<byte>>> Packets = new();

    public static void Add(in NTT player, in Memory<byte> packet)
    {
        if (!Packets.TryGetValue(player, out var queue))
        {
            queue = new Queue<Memory<byte>>();
            Packets.TryAdd(player, queue);
        }
        // lock (SyncRoot)
        queue.Enqueue(packet);
    }

    public static void Remove(in NTT player) => Packets.TryRemove(player, out _);

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
                        var packet = queue.Dequeue();
                        await net.Socket.SendAsync(packet, System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }
    }
}
