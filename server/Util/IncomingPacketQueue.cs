using iogame.Net;
using iogame.Simulation.Entities;

namespace iogame.Util
{
    public static class IncomingPacketQueue
    {
        public static Dictionary<Player, Queue<byte[]>> Packets = new();

        public static void Add(Player player, byte[] packet)
        {
            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<byte[]>();
                Packets.Add(player, queue);
            }
            queue.Enqueue(packet);
        }

        public static void Remove(Player player) => Packets.Remove(player);
        public static void ProcessAll()
        {
            foreach (var kvp in Packets)
                while (kvp.Value.Count > 0)
                    PacketHandler.Process(kvp.Key, kvp.Value.Dequeue());
        }
    }
}