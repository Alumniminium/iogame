using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class WeaponSystem : PixelSystem<InputComponent, PhysicsComponent, WeaponComponent, ShapeComponent>
    {
        public WeaponSystem() : base("Weapon System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref InputComponent inp, ref PhysicsComponent phy, ref WeaponComponent wep, ref ShapeComponent shp)
        {
            if (inp.ButtonStates.HasFlags(ButtonState.Drop))
            {
                var halfPi = MathF.PI / 2;
                var behind = -phy.Forward.ToRadians();

                behind += (Random.Shared.NextSingle() + -Random.Shared.NextSingle()) * halfPi;

                var dx = MathF.Cos(behind);
                var dy = MathF.Sin(behind);

                var dropX = -dx + phy.Position.X;
                var dropY = -dy + phy.Position.Y;
                var dropPos = new Vector2(dropX, dropY);

                var dist = phy.Position - dropPos;
                var penDepth = shp.Radius + 1 - dist.Length();
                var penRes = Vector2.Normalize(dist) * penDepth * 1.125f;
                dropPos += penRes;

                if (dropPos.X + 1 <= Game.MapSize.X && dropPos.X - 1 >= 0 && dropPos.Y + 1 <= Game.MapSize.Y && dropPos.Y - 1 >= 0)
                {
                    var velocity = new Vector2(dx, dy) * 10;

                    ref var inv = ref ntt.Get<InventoryComponent>();
                    if (inv.Triangles != 0)
                    {
                        inv.ChangedTick = Game.CurrentTick;
                        inv.Triangles--;
                        SpawnManager.SpawnDrop(Database.Db.BaseResources[3], dropPos, 1, Database.Db.BaseResources[3].Color, TimeSpan.FromMinutes(5), velocity);
                    }
                    if (inv.Squares != 0)
                    {
                        inv.ChangedTick = Game.CurrentTick;
                        inv.Squares--;
                        SpawnManager.SpawnDrop(Database.Db.BaseResources[4], dropPos, 1, Database.Db.BaseResources[4].Color, TimeSpan.FromMinutes(5), velocity);
                    }
                    if (inv.Pentagons != 0)
                    {
                        inv.ChangedTick = Game.CurrentTick;
                        inv.Pentagons--;
                        SpawnManager.SpawnDrop(Database.Db.BaseResources[5], dropPos, 1, Database.Db.BaseResources[5].Color, TimeSpan.FromMinutes(5), velocity);
                    }
                }
            }

            if (!inp.ButtonStates.HasFlags(ButtonState.Fire))
                return;
            if (wep.LastShot + 5 > Game.CurrentTick)
                return;

            wep.LastShot = Game.CurrentTick;

            var direction = phy.Forward.ToRadians() + wep.Direction.ToRadians();
            var bulletCount = wep.BulletCount;
            var d = bulletCount > 1 ? MathF.PI * 2 / bulletCount : 0;
            direction -= bulletCount > 1 ? d * bulletCount / 2 : 0;
            for (int x = 0; x < bulletCount; x++)
            {
                var dx = MathF.Cos(direction + d * x);
                var dy = MathF.Sin(direction + d * x);

                var bulletX = -dx + phy.Position.X;
                var bulletY = -dy + phy.Position.Y;
                var bulletPos = new Vector2(bulletX, bulletY);

                var bulletSize = 10;
                var bulletSpeed = 250;

                var dist = phy.Position - bulletPos;
                var penDepth = shp.Radius + bulletSize - dist.Length();
                var penRes = Vector2.Normalize(dist) * penDepth * 1.125f;
                bulletPos += penRes;

                if (bulletPos.X + bulletSize > Game.MapSize.X || bulletPos.X - bulletSize < 0 || bulletPos.Y + bulletSize > Game.MapSize.Y || bulletPos.Y - bulletSize < 0)
                    continue;

                var velocity = new Vector2(dx, dy) * bulletSpeed;
                SpawnManager.SpawnBullets(in ntt, ref bulletPos, ref velocity);
            }
        }
    }
}