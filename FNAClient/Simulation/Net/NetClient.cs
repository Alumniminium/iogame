using System;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using server.Helpers;
using server.Simulation.Net;
using server.Simulation.Net.Packets;

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
            await Socket.ConnectAsync(new Uri($"wss://{ip}/chat"), CancellationToken.None);
            await Socket.SendAsync((Memory<byte>)LoginRequestPacket.Create("FNA"), WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);
            while (true)
                {
                    try
                    {
                        var result = await Socket.ReceiveAsync(Buffer, CancellationToken.None);
            
                        var recvCount = result.Count;    
                        var packetSize = MemoryMarshal.Read<ushort>(Buffer.Span);
                        var remainingBytes = packetSize - recvCount; 
                        
                        var packet = new byte[packetSize];
                        Buffer.CopyTo(packet);

                        Buffer = new byte[remainingBytes];
                        await Socket.ReceiveAsync(Buffer, CancellationToken.None);
                        Array.Copy(Buffer.ToArray(), 0, packet, recvCount, remainingBytes);

                        IncomingPacketQueue.Add(packet);
                        Buffer = new byte[2];
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteLine("Error: " + e.Message); // something went wrong, stop and disconnect client
                        break;
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