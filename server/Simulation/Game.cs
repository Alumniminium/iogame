using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using iogame.Net.Packets;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public static class Game
    {
        public const int TARGET_TPS = 1000;

        public const int PHYSICS_TPS = 120;
        public const int UPDATE_RATE_MS = 66;

        public static Random Random = new();
        public static SpawnManager SpawnManager = new();
        public static uint TickCount;
        public const int MAP_WIDTH = 90000;
        public const int MAP_HEIGHT = 30000;
        public const float DRAG = 0.99997f;
        private static Thread worker;


        private static DateTime lastSync = DateTime.UtcNow;
        private static DateTime lastTpsCheck = DateTime.UtcNow;
        private static uint tpsCounter = 0;

        public static void Start()
        {
            SpawnManager.Spawn();
            worker = new Thread(GameLoop) { IsBackground = true };
            worker.Start();
        }

        public static void AddPlayer(Player player)
        {
            var id = 1_000_000 + Collections.Players.Count;
            player.UniqueId = (uint)id;
            Collections.Players.TryAdd(player.UniqueId, player);
            Collections.Entities.TryAdd(player.UniqueId, player);
            Collections.Grid.Insert(player);
            Collections.EntitiesArray = Collections.Entities.Values.ToArray();
        }
        public static async Task AddEntity(Entity entity)
        {
            Collections.Entities.TryAdd(entity.UniqueId, entity);
            Collections.Grid.Insert(entity);
            Collections.EntitiesArray = Collections.Entities.Values.ToArray();

            foreach (var kvp in Collections.Players)
            {
                if (kvp.Value.CanSee(entity))
                    await kvp.Value.Send(SpawnPacket.Create(entity));
            }
        }
        public static async Task RemoveEntity(Entity entity)
        {
            Collections.Players.TryRemove(entity.UniqueId, out _);
            Collections.Entities.TryRemove(entity.UniqueId, out _);
            Collections.Grid.Remove(entity);
            Collections.EntitiesArray = Collections.Entities.Values.ToArray();

            foreach (var kvp in Collections.Players)
                await kvp.Value.Send(StatusPacket.Create(entity.UniqueId, (ulong)Math.Max(0, entity.Health), StatusType.Health));
        }

        public static async void GameLoop()
        {
            Console.WriteLine("Vectors Hw Acceleration: " + Vector.IsHardwareAccelerated);
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
                while (fixedUpdateAcc >= fixedUpdateTime)
                {
                    await FixedUpdate(fixedUpdateTime, now);
                    fixedUpdateAcc -= fixedUpdateTime;
                    tpsCounter++;
                }

                if (TARGET_TPS != 1000)
                {
                    var tickTIme = stopwatch.ElapsedMilliseconds;
                    Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme)));
                }
            }
        }
        private static async Task FixedUpdate(float dt, DateTime now)
        {
            foreach (var kvp in Collections.Entities)
            {
                var entity = kvp.Value;
                var pos = entity.Position;

                await entity.Update(dt);

                Collections.Grid.Move(pos, entity);

                if (entity.Health <= 0)
                    await RemoveEntity(entity);
            }
            CheckCollisions();

            if (lastSync.AddMilliseconds(UPDATE_RATE_MS) <= now)
            {
                lastSync = now;

                foreach (var pkvp in Collections.Players)
                {
                    var player = pkvp.Value;
                    var list = Collections.Grid.GetEntitiesSameAndSurroundingCells(pkvp.Value);
                    foreach (var entity in list)
                    {
                        await pkvp.Value.Send(MovementPacket.Create(entity.UniqueId, entity.Position, entity.Velocity));
                    }
                }
            }
            if (lastTpsCheck.AddSeconds(1) <= now)
            {
                lastTpsCheck = now;

                foreach (var pkvp in Collections.Players)
                {
                    await pkvp.Value.Send(PingPacket.Create());
                }
                Console.WriteLine($"TPS: {tpsCounter}");
                tpsCounter = 0;
            }
            TickCount++;
        }
        private static void CheckCollisions()
        {
            Parallel.For(0, Collections.EntitiesArray.Length, (i) =>
             {
                 try
                 {
                     var a = Collections.EntitiesArray[i];
                     var visible = Collections.Grid.GetEntitiesSameAndDirection(a).ToArray();
                     for (int j = 0; j < visible.Length; j++)
                     {
                         var b = visible[j];

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