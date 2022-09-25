using System;
using System.Collections.Generic;
using server.Helpers;
using server.Simulation.Net;

namespace RG351MP.Simulation.Net
{
    public static class IncomingPacketQueue
    {
        private static readonly Queue<Memory<byte>> Packets = new();

        public static void Add(in Memory<byte> packet)
        {
            if (packet.IsEmpty)
                return;
            Packets.Enqueue(packet);
        }
        public static void ProcessAll()
        {
            while (Packets.Count > 0)
            {
                var packet = Packets.Dequeue();
                try
                {
                    PacketHandler.Process(in packet);
                }
                catch (Exception e)
                {
                    FConsole.WriteLine("PacketHandler Error: " + e.Message);
                }
            }
        }
    }
}