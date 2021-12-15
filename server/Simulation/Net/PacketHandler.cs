using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Entities;
using server.Simulation.Managers;
using server.Simulation.Net.Packets;

namespace server.Simulation.Net
{
    public static class PacketHandler
    {
        public static void Process(PixelEntity player, byte[] buffer)
        {
            var id = BitConverter.ToUInt16(buffer, 2);
            FConsole.WriteLine($"Processing {id}");

            switch (id)
            {
                case 1:
                    {
                        var packet = (LoginRequestPacket)buffer;
                        // player.Name = packet.GetUsername();
                        // player.Password = packet.GetPassword();
                        var shpPlayer = new Player
                        {
                            Entity = player
                        };
                        player.AttachTo(shpPlayer);
                        // auth

                        var inp =new InputComponent();
                        var vel =new VelocityComponent();
                        var pos =new PositionComponent(SpawnManager.GetPlayerSpawnPoint());
                        var spd =new SpeedComponent(350);
                        var shp =new ShapeComponent(32,10,Convert.ToUInt32("00bbf9", 16));
                        var hlt =new HealthComponent(100,100,10);
                        var phy =new PhysicsComponent((float)Math.Pow(shp.Size, 3), 1f, 0.05f);
                        var vwp =new ViewportComponent(750);

                        PixelWorld.Players.TryAdd(player, shpPlayer);
                        shpPlayer.Rect = new System.Drawing.RectangleF(pos.Position.X - shp.Radius, pos.Position.Y - shp.Radius, shp.Size, shp.Size);
                        
                        player.Add(ref inp);
                        player.Add(ref vel);
                        player.Add(ref pos);
                        player.Add(ref spd);
                        player.Add(ref shp);
                        player.Add(ref hlt);
                        player.Add(ref phy);
                        player.Add(ref vwp);

                        lock (Game.Tree)
                            Game.Tree.Add(shpPlayer);

                        player.NetSync(LoginResponsePacket.Create(player));
                        Game.Broadcast(ChatPacket.Create("Server", $"{packet.GetUsername()} joined!"));
                        FConsole.WriteLine($"Login Request for User: {packet.GetUsername()}, Pass: {packet.GetPassword()}");
                        break;
                    }
                case 1004:
                    {
                        var packet = (ChatPacket)buffer;
                        var user = packet.GetUsername();
                        var message = packet.GetText();

                        Game.Broadcast(packet);
                        FConsole.WriteLine($"ChatPacket from {user}: {message}");
                        break;
                    }
                case 1005:
                    {
                        var packet = (PlayerMovementPacket)buffer;

                        if (packet.UniqueId != player.EntityId)
                            return; // hax

                        var ticks = packet.TickCounter;
                        ref readonly var oldInp = ref player.Get<InputComponent>();
                        var inp = new InputComponent(new Vector2(packet.Left ? -1 : packet.Right ? 1 : 0,packet.Up ? -1 : packet.Down ? 1 : 0), new Vector2(packet.X,packet.Y), packet.Fire, oldInp.LastShot);
                        player.Replace(ref inp);

                        FConsole.WriteLine($"Movement Packet from Player {player.EntityId}: {(packet.Up ? "Up" : "")} {(packet.Down ? "Down" : "")} {(packet.Left ? "Left" : "")} {(packet.Right ? "Right" : "")} X: ${packet.X},Y: ${packet.Y}");
                        break;
                    }
                case 1016:
                    {
                        var packet = (RequestSpawnPacket)buffer;
                        FConsole.WriteLine($"RequestSpawnPacket from {packet.UniqueId} for {packet.EntityId}");

                        if (player.EntityId != packet.UniqueId)
                            return; //hax

                        if (!PixelWorld.EntityExists(packet.EntityId))
                            return;

                        ref var entity = ref PixelWorld.GetEntity(packet.EntityId);

                        if (entity.IsPlayer() || entity.IsBullet() || entity.IsNpc())
                            player.NetSync(SpawnPacket.Create(in entity));
                        else if (entity.IsFood())
                            player.NetSync(ResourceSpawnPacket.Create(in entity));

                        FConsole.WriteLine($"Spawnpacket sent for {packet.EntityId}");
                        break;
                    }
                case 9000:
                    {
                        var packet = (PingPacket)buffer;
                        var delta = DateTime.UtcNow.Ticks - packet.TickCounter;

                        packet.Ping = (ushort)(delta / 10000);

                        player.NetSync(packet);
                        break;
                    }
            }
        }
    }
}