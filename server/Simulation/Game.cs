using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public class Game
    {
        public static Random random = new();
        public static SpawnManager SpawnManager = new();
        public static uint TickCounter;
        public const int MAP_WIDTH =  300000;
        public const int MAP_HEIGHT = 100000;
        public const float DRAG = 0.99997f;
        private Thread worker;


        public DateTime lastSync = DateTime.UtcNow;
        public DateTime lastTpsCheck = DateTime.UtcNow;
        public uint tpsCounter = 0;

        public List<Entity> MovedThisTick = new List<Entity>();

        public void Start()
        {
            SpawnManager.Spawn();
            worker = new Thread(GameLoop) { IsBackground = true };
            worker.Start();
        }

        internal void AddPlayer(Player player)
        {
            var id = 1_000_000 + Collections.Players.Count;
            player.UniqueId = (uint)id;
            Collections.Players.TryAdd(player.UniqueId, player);
            Collections.Entities.TryAdd(player.UniqueId, player);
            Collections.Grid.Insert(player);
            Collections.EntitiesArray = Collections.Entities.Values.ToArray();
        }
        internal void RemoveEntity(Entity entity)
        {
            Collections.Players.TryRemove(entity.UniqueId, out _);
            Collections.Entities.TryRemove(entity.UniqueId, out _);
            Collections.Grid.Remove(entity);
            Collections.EntitiesArray = Collections.Entities.Values.ToArray();
        }

        public void GameLoop()
        {
            Console.WriteLine("Vectors Hw Acceleration: " + Vector.IsHardwareAccelerated);
            var stopwatch = new Stopwatch();
            var targetTps = 1000;
            var sleepTime = 1000 / targetTps;
            var prevTime = DateTime.UtcNow;

            while (true)
            {
                stopwatch.Restart();
                tpsCounter++;
                var now = DateTime.UtcNow;
                var dt = (float)(now - prevTime).TotalSeconds;
                prevTime = now;
                var curTps = Math.Round(1 / dt);

                Update(now, dt);

                var tickTIme = stopwatch.ElapsedMilliseconds;

                if (targetTps != 1000)
                    Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme)));
            }
        }

        private void Update(DateTime now, float dt)
        {
            MovedThisTick.Clear();
            foreach (var kvp in Collections.Entities)
            {
                var curPos = kvp.Value.Position;
                kvp.Value.Update(dt);
                if (curPos != kvp.Value.Position)
                {
                    Collections.Grid.Move(curPos, kvp.Value);
                    MovedThisTick.Add(kvp.Value);
                }
            }
            CheckCollisions();

            if (lastSync.AddMilliseconds(70) <= now)
            {
                lastSync = now;
                TickCounter++;

                foreach (var pkvp in Collections.Players)
                {
                    pkvp.Value.Send(PingPacket.Create(DateTime.UtcNow.Ticks, 0));
                    var vectorC = new Vector2(((int)pkvp.Value.Position.X) / Grid.W, ((int)pkvp.Value.Position.Y) / Grid.H);
                    var entityLists = Collections.Grid.GetEntitiesSameAndSurroundingCells(pkvp.Value);
                    pkvp.Value.Screen.Check(entityLists);
                    foreach (var list in entityLists)
                        foreach (var entity in list)
                            pkvp.Value.Send(MovementPacket.Create(entity.UniqueId, entity.Position, entity.Velocity));
                }
            }
            if (lastTpsCheck.AddSeconds(1) <= now)
            {
                lastTpsCheck = now;

                var info = GC.GetGCMemoryInfo();
                Console.WriteLine($"TPS: {tpsCounter} | Time Spent in GC: {info.PauseTimePercentage}%");
                tpsCounter = 0;
            }
        }

        private void CheckCollisions()
        {
            var movements = new List<(Vector2,Entity)>();

            foreach(var kvp in Collections.Entities)
            {
                var a = kvp.Value;

                var visible = Collections.Grid.GetEntitiesSameCell(a);

                foreach(var b in visible)
                {
                    if(a.CheckCollision(b))
                    {
                        movements.Add((a.Position,a));
                        movements.Add((b.Position,b));

                        var dist = a.Position - b.Position;
                        var penDepth = a.Radius + b.Radius - dist.Magnitude();
                        var penRes = dist.unit() * (penDepth / (a.InverseMass + b.InverseMass));
                        a.Position += penRes * a.InverseMass;
                        b.Position += penRes * -b.InverseMass;

                        var normal = (a.Position - b.Position).unit();
                        var relVel = a.Velocity-b.Velocity;
                        var sepVel = Vector2.Dot(relVel, normal);
                        var new_sepVel = -sepVel * Math.Min(a.Elasticity, b.Elasticity);
                        var vsep_diff = new_sepVel - sepVel;
                        
                        var impulse = vsep_diff / (a.InverseMass + b.InverseMass);
                        var impulseVec = normal * impulse;

                        a.Velocity += impulseVec * a.InverseMass;
                        b.Velocity += impulseVec * -b.InverseMass;
                    }
                }
            }

            // for(int i = 0; i < Collections.EntitiesArray.Length; i++)
            // {
            //     var a = Collections.EntitiesArray[i];

            //     for(int j = i+1; j<Collections.EntitiesArray.Length; j++)
            //     {
            //         var b = Collections.EntitiesArray[j];

            //         if(a.CheckCollision(b))
            //         {
            //             var dist = a.Position - b.Position;
            //             var penDepth = a.Radius + b.Radius - dist.Magnitude();
            //             var penRes = dist.unit() * (penDepth / 2);
            //             movements.Add((a.Position,a));
            //             movements.Add((b.Position,b));
            //             a.Position += penRes;
            //             b.Position += penRes * -1;

            //             var normal = (a.Position - b.Position).unit();
            //             var relVel = a.Velocity-b.Velocity;
            //             var sepVel = Vector2.Dot(relVel, normal);
            //             var new_sepVel = -sepVel;
            //             var sepVelVec = normal * new_sepVel;
                    
            //             a.Velocity += sepVelVec;
            //             b.Velocity += sepVelVec * -1;
            //         }
            //     }
            // }
            foreach(var movement in movements)
                Collections.Grid.Move(movement.Item1,movement.Item2);
        }
    }
}