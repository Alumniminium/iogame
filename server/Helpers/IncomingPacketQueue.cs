using System;
using System.Collections.Generic;
using server.ECS;
using server.Simulation.Net;

namespace server.Helpers;

public static class IncomingPacketQueue
{
    private static readonly Dictionary<NTT, Queue<Memory<byte>>> Packets = [];

    public static void Add(in NTT player, in Memory<byte> packet)
    {
        if (!Packets.TryGetValue(player, out var queue))
        {
            queue = new Queue<Memory<byte>>();
            Packets.Add(player, queue);
        }
        if (packet.IsEmpty)
            return;
        queue.Enqueue(packet);
    }

    public static void Remove(in NTT player) => Packets.Remove(player);

    public static void ProcessAll()
    {
        foreach (var (ntt, queue) in Packets)
            while (queue.Count > 0)
            {
                var packet = queue.Dequeue();
                if (!NttWorld.EntityExists(ntt))
                {
                    queue.Clear();
                    continue;
                }
                PacketHandler.Process(in ntt, in packet);
            }
    }
}