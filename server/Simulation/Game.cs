using System.Diagnostics;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Database;
using iogame.Simulation.Entities;
using iogame.Simulation.Systems;
using iogame.Util;

namespace iogame.Simulation
{
    public static class Game
    {
        public const int TARGET_TPS = 1000;
        public const int PHYSICS_TPS = 30;
        public const int UPDATE_RATE_MS = 33;

        public const int MAP_WIDTH = 90_000;
        public const int MAP_HEIGHT = 30_000;
        public const float DRAG = 0.99997f;

        public static uint CurrentTick;
        private static uint TicksPerSecond = 0;
        private static uint PhysicsTicksPerSecond = 0;

        public static Random Random = new();
        public static SpawnManager SpawnManager = new();
        private static readonly Thread worker;
        public static PacketBuffer OutgoingPacketBuffer = new();
        public static PacketBuffer IncommingPacketBuffer = new();

        public static TimedThing[] TimedThings = new TimedThing[]
        {
            new TimedThing(TimeSpan.FromMilliseconds(UPDATE_RATE_MS), ()=>
            {
                foreach (var pkvp in Collections.Players)
                {
                    var player = pkvp.Value;
                    player.Viewport.Update();
                }
            }),
            new TimedThing(TimeSpan.FromSeconds(1), ()=> {
                foreach (var pkvp in Collections.Players)
                {
                    pkvp.Value.Send(PingPacket.Create());
                    pkvp.Value.Send(ChatPacket.Create("Server", $"Tickrate: {TicksPerSecond} (Physics: {PhysicsTicksPerSecond}/{PHYSICS_TPS}) | Entities: {Collections.Entities.Count}"));
                }

                FConsole.WriteLine($"Tickrate: {TicksPerSecond}/{TARGET_TPS} (Physics: {PhysicsTicksPerSecond}/{PHYSICS_TPS})");
                TicksPerSecond = 0;
                PhysicsTicksPerSecond = 0;
            })
        };

         static Game()
        {
            Db.LoadBaseResources();
            SpawnManager.Respawn();
            worker = new Thread(GameLoopAsync) { IsBackground = true };
            worker.Start();
        }
        public static void AddEntity(Entity entity) => Collections.EntitiesToAdd.Add(entity);
        public static void RemoveEntity(Entity entity) => Collections.EntitiesToRemove.Add(entity);

        private static void AddEntity_Internal()
        {
            foreach (var entity in Collections.EntitiesToAdd)
            {
                if (entity is Player player)
                    Collections.Players.TryAdd(entity.UniqueId, player);

                Collections.Entities.TryAdd(entity.UniqueId, entity);
                Collections.Grid.Insert(entity);

                foreach (var kvp in Collections.Players)
                {
                    if (kvp.Value.CanSee(entity))
                        kvp.Value.Send(SpawnPacket.Create(entity));
                }
            }
            Collections.EntitiesToAdd.Clear();
        }
        private static void RemoveEntity_Internal()
        {
            foreach (var entity in Collections.EntitiesToRemove)
            {
                if (entity is Player player)
                {
                    OutgoingPacketBuffer.Remove(player);
                    Collections.Players.Remove(player.UniqueId, out _);
                }
                Collections.Entities.Remove(entity.UniqueId, out _);
                Collections.Grid.Remove(entity);

                entity.Viewport.Send(StatusPacket.Create(entity.UniqueId, 0, StatusType.Health));
                entity.Viewport.Send(StatusPacket.Create(entity.UniqueId, 0, StatusType.Alive));
                entity.Viewport.Clear();
            }
            Collections.EntitiesToRemove.Clear();
        }
        public static void Broadcast(byte[] packet)
        {
            foreach (var kvp in Collections.Players)
                kvp.Value.Send(packet);
        }

        public static async void GameLoopAsync()
        {
            FConsole.WriteLine("Vectors Hw Acceleration: " + Vector.IsHardwareAccelerated);
            var stopwatch = new Stopwatch();
            var sleepTime = 1000 / TARGET_TPS;
            var prevTime = DateTime.UtcNow;
            var fixedUpdateAcc = 0f;
            var fixedUpdateTime = 1f / PHYSICS_TPS;

            while (true)
            {
                stopwatch.Restart();
                var now = DateTime.UtcNow;
                var dt = (float)(now - prevTime).TotalSeconds;
                fixedUpdateAcc += dt;
                prevTime = now;

                IncommingPacketBuffer.ProcessAll();
                RemoveEntity_Internal();
                AddEntity_Internal();

                while (fixedUpdateAcc >= fixedUpdateTime)
                {
                    FixedUpdate(fixedUpdateTime);
                    fixedUpdateAcc -= fixedUpdateTime;
                    PhysicsTicksPerSecond++;
                }
                await OutgoingPacketBuffer.SendAll();

                var tickTIme = stopwatch.ElapsedMilliseconds;
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme)));
                TicksPerSecond++;
            }
        }
        private static void FixedUpdate(float dt)
        {
            Collections.Grid.Clear();
            foreach (var kvp in Collections.Entities)
            {
                var entity = kvp.Value;

                LifetimeSystem.Update(dt, entity);
                MoveSystem.Update(dt, entity);
                HealthSystem.Update(dt, entity);

                Collections.Grid.Insert(entity);
            }
            CheckCollisions();

            for (int i = 0; i < TimedThings.Length; i++)
                TimedThings[i].Update(dt);

            CurrentTick++;
        }
        private static unsafe void CheckCollisions()
        {
            Parallel.ForEach(Collections.Entities, kvp =>
            {
                var a = kvp.Value;
                var visible = Collections.Grid.GetEntitiesSameAndSurroundingCells(a);
                foreach (var b in visible)
                {
                    if (a.UniqueId == b.UniqueId)
                        continue;
                    if (a is Bullet ba)
                    {
                        if (ba.Owner == b)
                            continue;
                    }
                    if (b is Bullet bb)
                    {
                        if (bb.Owner == a)
                            continue;
                    }
                    if(a is Bullet ab && b is Bullet bbb)
                    {
                        if(ab.Owner.UniqueId == bbb.Owner.UniqueId)
                            continue;
                    }

                    if (a.CheckCollision(b))
                    {
                        var aPos = a.PositionComponent.Position;
                        var bPos = b.PositionComponent.Position;

                        var (aVel,_, _) = a.VelocityComponent;
                        var (bVel,_, _) = b.VelocityComponent;

                        var dist = aPos - bPos;
                        var penDepth = a.ShapeComponent.Radius + b.ShapeComponent.Radius - dist.Magnitude();
                        var penRes = dist.Unit() * (penDepth / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass));
                        a.PositionComponent.Position += penRes * a.PhysicsComponent.InverseMass;
                        b.PositionComponent.Position += penRes * -b.PhysicsComponent.InverseMass;

                        var normal = (aPos - bPos).Unit();
                        var relVel = aVel- bVel;
                        var sepVel = Vector2.Dot(relVel, normal);
                        var new_sepVel = -sepVel * Math.Min(a.PhysicsComponent.Elasticity, b.PhysicsComponent.Elasticity);
                        var vsep_diff = new_sepVel - sepVel;

                        var impulse = vsep_diff / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass);
                        var impulseVec = normal * impulse;

                        if (a is Bullet bullet && b is not Bullet)
                        {
                            bullet.Hit(b);
                            b.VelocityComponent.Movement += 10 * impulseVec * -b.PhysicsComponent.InverseMass;
                        }
                        else if (b is Bullet bullet2 && a is not Bullet)
                        {
                            bullet2.Hit(a);
                            a.VelocityComponent.Movement += 10 * impulseVec * a.PhysicsComponent.InverseMass;
                        }
                        else
                        {
                            a.VelocityComponent.Movement += impulseVec * a.PhysicsComponent.InverseMass;
                            b.VelocityComponent.Movement += impulseVec * -b.PhysicsComponent.InverseMass;
                        }

                        a.GetHitBy(b);
                        b.GetHitBy(a);
                    }
                }
            });
        }
    }
}