using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using iogame.Net.Packets;
using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation
{
    public static class Game
    {
        public const int TARGET_TPS = 1000;
        public const int PHYSICS_TPS = 60;
        public const int UPDATE_RATE_MS = 66;

        public const int MAP_WIDTH = 90_000;
        public const int MAP_HEIGHT = 30_000;
        public const float DRAG = 0.99997f;

        public static Random Random = new();
        public static SpawnManager SpawnManager = new();
        private static Thread worker;

        public static uint CurrentTick;
        private static uint TicksPerSecond = 0;

        private static DateTime lastSync = DateTime.UtcNow;
        private static DateTime lastTpsCheck = DateTime.UtcNow;

        public static async Task StartAsync()
        {
            await SpawnManager.SpawnAsync();
            worker = new Thread(GameLoopAsync) { IsBackground = true };
            worker.Start();
        }
        public static void AddEntity(Entity entity) => Collections.EntitiesToAdd.Add(entity);
        public static void RemoveEntity(Entity entity) => Collections.EntitiesToRemove.Add(entity);

        private static async Task AddEntity_Internal()
        {
            foreach (var entity in Collections.EntitiesToAdd)
            {
                if(entity is Player)
                    Collections.Players.TryAdd(entity.UniqueId, (Player)entity);

                Collections.Entities.TryAdd(entity.UniqueId, entity);
                Collections.Grid.Insert(entity);
                Collections.EntitiesArray = Collections.Entities.Values.ToArray();

                foreach (var kvp in Collections.Players)
                {
                    if (kvp.Value.CanSee(entity))
                        await kvp.Value.SendAsync(SpawnPacket.Create(entity));
                }
            }
            Collections.EntitiesToAdd.Clear();
        }
        private static async Task RemoveEntity_Internal()
        {
            foreach (var entity in Collections.EntitiesToRemove)
            {
                Collections.Players.TryRemove(entity.UniqueId, out _);
                Collections.Entities.TryRemove(entity.UniqueId, out _);
                Collections.Grid.Remove(entity);
                Collections.EntitiesArray = Collections.Entities.Values.ToArray();

                foreach (var kvp in Collections.Players)
                    await kvp.Value.SendAsync(StatusPacket.Create(entity.UniqueId, (ulong)Math.Max(0, entity.Health), StatusType.Health));
            }
            Collections.EntitiesToRemove.Clear();
        }
        public static async Task BroadcastAsync(byte[] packet)
        {
            foreach (var kvp in Collections.Players)
                await kvp.Value.SendAsync(packet);
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
                // while (fixedUpdateAcc >= fixedUpdateTime)
                // {
                //     await FixedUpdate(fixedUpdateTime, now);
                //     fixedUpdateAcc -= fixedUpdateTime;
                //     tpsCounter++;
                // }

                await RemoveEntity_Internal();
                await AddEntity_Internal();
                await FixedUpdateAsync(dt, now);

                var tickTIme = stopwatch.ElapsedMilliseconds;
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme)));
                TicksPerSecond++;
            }
        }
        private static async Task FixedUpdateAsync(float dt, DateTime now)
        {
            for(int i = 0; i < Collections.EntitiesArray.Length; i++)
            {
                var entity = Collections.EntitiesArray[i];
                var pos = entity.Position;

                entity.Update(dt);

                if (entity.Health <= 0)
                    RemoveEntity(entity);
                else
                    Collections.Grid.Move(pos, entity);
            }
            CheckCollisions();

            if (lastSync.AddMilliseconds(UPDATE_RATE_MS) <= now)
            {
                lastSync = now;
                // FConsole.WriteLine($"sync pos " + now);
                foreach (var pkvp in Collections.Players)
                {
                    var player = pkvp.Value;
                    var list = Collections.Grid.GetEntitiesSameAndSurroundingCells(pkvp.Value);
                    foreach (var entity in list)
                        await pkvp.Value.SendAsync(MovementPacket.Create(entity.UniqueId, entity.Position, entity.Velocity));
                }
            }
            if (lastTpsCheck.AddSeconds(1) <= now)
            {
                lastTpsCheck = now;

                foreach (var pkvp in Collections.Players)
                    await pkvp.Value.SendAsync(PingPacket.Create());

                FConsole.WriteLine($"TPS: {TicksPerSecond}");
                TicksPerSecond = 0;
            }
            CurrentTick++;
        }
        private static void CheckCollisions()
        {
            Parallel.For(0, Collections.EntitiesArray.Length, (i) =>
             {
                 try
                 {
                     var a = Collections.EntitiesArray[i];
                     var visible = Collections.Grid.GetEntitiesSameAndDirection(a);
                     foreach (var b in visible)
                     {
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
                         }
                     }
                 }
                 catch { }
             });
        }
    }
}