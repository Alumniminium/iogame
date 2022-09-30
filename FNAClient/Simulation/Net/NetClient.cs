using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using Packets;
using server.Helpers;

namespace RG351MP.Simulation.Net
{
    public static class NetClient
    {
        public static ClientWebSocket Socket;
        public static Memory<byte> Buffer;
        internal static bool LoggedIn;

        public static async void Connect(string ip)
        {
            Buffer = new byte[2];
            Socket = new ClientWebSocket();
            await Socket.ConnectAsync(new Uri($"ws://{ip}/chat"), CancellationToken.None);
            await Socket.SendAsync((Memory<byte>)LoginRequestPacket.Create("FNA"), WebSocketMessageType.Binary, true, CancellationToken.None);
            while (true)
                {
                    try
                    {
                        Buffer = new byte[2];
                        var result = await Socket.ReceiveAsync(Buffer, CancellationToken.None);
                        if(result.Count < 2)
                            Debugger.Break();
                        var recvCount = result.Count;
                        var packetSize = MemoryMarshal.Read<ushort>(Buffer.Span[..2]);
                        var remainingBytes = packetSize - recvCount; 
                        
                        var packet = new byte[packetSize];
                        Buffer.CopyTo(packet);

                        while(remainingBytes > 0)
                        {
                            Buffer = new byte[remainingBytes];
                            result = await Socket.ReceiveAsync(Buffer, CancellationToken.None);
                            Buffer.CopyTo(packet.AsMemory(recvCount));
                            remainingBytes -= result.Count;
                            recvCount += result.Count;
                            if(packetSize == recvCount)
                                break;
                        }

                        packetSize = MemoryMarshal.Read<ushort>(packet);
                        if(packetSize!= packet.Length)
                            Debugger.Break();
                        IncomingPacketQueue.Add(packet);
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteLine("Error: " + e.Message); // something went wrong, stop and disconnect client
                        FConsole.WriteLine("Error: " + e.StackTrace); // something went wrong, stop and disconnect client
                        // break;
                    }
                }
        }

        public static async void Send(Memory<byte> buffer)
        {
            FConsole.WriteLine("Sending packet " + buffer.Length);
            await Socket.SendAsync(buffer, WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);
        }
    }
}