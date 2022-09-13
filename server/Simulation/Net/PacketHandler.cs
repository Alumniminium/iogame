using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
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

                        var inp = new InputComponent();
                        var eng = new EngineComponent(10);
                        var shp = new ShapeComponent(16, 10, Convert.ToUInt32("00bbf9", 16));
                        var hlt = new HealthComponent(20000, 20000, 10);
                        var phy = new PhysicsComponent(SpawnManager.GetPlayerSpawnPoint(), MathF.Pow(shp.Size, 3), elasticity: 0.2f, drag: 0.0003f);
                        var vwp = new ViewportComponent(500);
                        var syn = new NetSyncComponent(SyncThings.All);
                        var wep = new WeaponComponent(0f);
                        var inv = new InventoryComponent(100);

                        player.Add(ref inv);
                        player.Add(ref inp);
                        player.Add(ref eng);
                        player.Add(ref shp);
                        player.Add(ref hlt);
                        player.Add(ref phy);
                        player.Add(ref vwp);
                        player.Add(ref wep);
                        player.Add(ref syn);

                        lock (Game.Grid)
                            Game.Grid.Add(player);

                        player.NetSync(LoginResponsePacket.Create(player));
                        PixelWorld.Players.Add(player);
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

                        if (packet.UniqueId != player.Id)
                            return; // hax

                        // var ticks = packet.TickCounter;
                        ref var inp = ref player.Get<InputComponent>();

                        var movement = Vector2.Zero;

                        if (packet.Inputs.HasFlags(ButtonState.Thrust) || packet.Inputs.HasFlags(ButtonState.Boost) || packet.Inputs.HasFlags(ButtonState.InvThrust))
                        {
                            ref readonly var phy = ref player.Get<PhysicsComponent>();
                            movement = phy.Forward;
                        }

                        inp.MovementAxis = movement;
                        inp.ButtonStates = packet.Inputs;
                        inp.MouseDir = packet.MousePosition;
                        break;
                    }
                case 1016:
                    {
                        var packet = (RequestSpawnPacket)buffer;
                        FConsole.WriteLine($"RequestSpawnPacket from {packet.UniqueId} for {packet.EntityId}");

                        if (player.Id != packet.UniqueId)
                            return; //hax

                        if (!PixelWorld.EntityExists(packet.EntityId))
                            return;

                        ref var ntt = ref PixelWorld.GetEntity(packet.EntityId);

                        if (ntt.Type != EntityType.Food && ntt.Type != EntityType.Asteroid)
                            player.NetSync(SpawnPacket.Create(in ntt));
                        else if (ntt.Type == EntityType.Food)
                            player.NetSync(ResourceSpawnPacket.Create(in ntt));
                        else if (ntt.Type == EntityType.Asteroid)
                            player.NetSync(AsteroidSpawnPacket.Create(in ntt));

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