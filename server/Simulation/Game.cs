using System.Diagnostics;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation
{
    public class TimedThing
    {
        public float TotalSecondsSinceLastExecution = 0f;
        public float IntervalSeconds = 0f;
        public Action Action;

        public TimedThing(TimeSpan interval, Action action)
        {
            IntervalSeconds = (float)interval.TotalSeconds;
            Action = action;
        }

        public void Update(float dt)
        {
            TotalSecondsSinceLastExecution += dt;
            if (TotalSecondsSinceLastExecution >= IntervalSeconds)
            {
                TotalSecondsSinceLastExecution = 0;
                Action.Invoke();
            }
        }
    }
    public static class Game
    {
        public const int TARGET_TPS = 1000;
        public const int PHYSICS_TPS = 60;
        public const int UPDATE_RATE_MS = 30;

        public const int MAP_WIDTH = 90_000;
        public const int MAP_HEIGHT = 30_000;
        public const float DRAG = 0.99997f;

        public static uint CurrentTick;
        private static uint TicksPerSecond = 0;
        private static uint PhysicsTicksPerSecond = 0;

        public static Random Random = new();
        public static SpawnManager SpawnManager = new();
        private static Thread worker;

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
                    pkvp.Value.Send(ChatPacket.Create("Server", $"Tickrate: {TicksPerSecond}/{TARGET_TPS} (Physics: {PhysicsTicksPerSecond}/{PHYSICS_TPS})"));
                }

                FConsole.WriteLine($"Tickrate: {TicksPerSecond}/{TARGET_TPS} (Physics: {PhysicsTicksPerSecond}/{PHYSICS_TPS})");
                TicksPerSecond = 0;
                PhysicsTicksPerSecond = 0;
            })
        };

        public static async Task StartAsync()
        {
            await SpawnManager.SpawnAsync();
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
                Collections.Players.Remove(entity.UniqueId, out _);
                Collections.Entities.Remove(entity.UniqueId, out _);
                Collections.Grid.Remove(entity);

                foreach (var kvp in Collections.Players)
                    kvp.Value.Send(StatusPacket.Create(entity.UniqueId, 0, StatusType.Health));
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
                    FixedUpdate(fixedUpdateTime, now);
                    fixedUpdateAcc -= fixedUpdateTime;
                    PhysicsTicksPerSecond++;
                }
                await OutgoingPacketBuffer.SendAll();

                var tickTIme = stopwatch.ElapsedMilliseconds;
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme)));
                TicksPerSecond++;
            }
        }
        private static void FixedUpdate(float dt, DateTime now)
        {
            foreach (var kvp in Collections.Entities)
            {
                var entity = kvp.Value;
                var pos = entity.Position;

                entity.Update(dt);

                if (entity.Health <= 0)
                    RemoveEntity(entity);
                else
                    Collections.Grid.Move(pos, entity);
            }
            CheckCollisions();

            for(int i = 0; i < TimedThings.Length; i++)
                TimedThings[i].Update(dt);

            CurrentTick++;
        }
        private static void CheckCollisions()
        {
#if DEBUG
            foreach (var kvp in Collections.Entities)
#else
            Parallel.ForEach(Collections.Entities, kvp =>
#endif
            {
                var a = kvp.Value;
                var visible = Collections.Grid.GetEntitiesSameAndSurroundingCells(a);
                foreach (var b in visible)
                {
                    if(a.UniqueId == b.UniqueId)
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

                    if (a.CheckCollision(b))
                    {
                        var dist = a.Position - b.Position;
                        var penDepth = a.Radius + b.Radius - dist.Magnitude();
                        var penRes = dist.Unit() * (penDepth / (a.InverseMass + b.InverseMass));
                        a.Position += penRes * a.InverseMass;
                        b.Position += penRes * -b.InverseMass;

                        var normal = (a.Position - b.Position).Unit();
                        var relVel = a.Velocity - b.Velocity;
                        var sepVel = Vector2.Dot(relVel, normal);
                        var new_sepVel = -sepVel * Math.Min(a.Elasticity, b.Elasticity);
                        var vsep_diff = new_sepVel - sepVel;

                        var impulse = vsep_diff / (a.InverseMass + b.InverseMass);
                        var impulseVec = normal * impulse;

                        if (a is Bullet bullet && b is not Bullet)
                        {
                            bullet.Hit(b);
                            b.Velocity += 10 * impulseVec * -b.InverseMass;
                        }
                        else if (b is Bullet bullet2 && a is not Bullet)
                        {
                            bullet2.Hit(a);
                            a.Velocity += 10 * impulseVec * a.InverseMass;
                        }
                        else
                        {
                            a.Velocity += impulseVec * a.InverseMass;
                            b.Velocity += impulseVec * -b.InverseMass;
                        }

                        a.GetHitBy(b);
                        b.GetHitBy(a);
                    }
                }
#if DEBUG
            }
#else
            });
#endif
        }
    }
}