using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class WeaponSystem : PixelSystem<InputComponent, PhysicsComponent, WeaponComponent, ShapeComponent>
    {
        public WeaponSystem() : base("Weapon System", threads: 12) { }

        public override void Update(in PixelEntity ntt, ref InputComponent inp, ref PhysicsComponent phy, ref WeaponComponent wep, ref ShapeComponent shp)
        {
            if (!inp.ButtonStates.HasFlags(ButtonState.Fire))
                return;
            if (wep.LastShot + 10 > Game.CurrentTick)
                return;

            wep.LastShot = Game.CurrentTick;

            var direction = (float)Math.Atan2(inp.MousePositionWorld.Y, inp.MousePositionWorld.X);
            var bulletCount = 51;
            var d = bulletCount > 1 ? (float)Math.PI * 2 / bulletCount : 0;
            direction -= bulletCount > 1 ? d * bulletCount / 2 : 0;
            for (int x = 0; x < bulletCount; x++)
            {
                var dx = (float)Math.Cos(direction + d * x);
                var dy = (float)Math.Sin(direction + d * x);

                var bulletX = -dx + phy.Position.X;
                var bulletY = -dy + phy.Position.Y;
                var bulletPos = new Vector2(bulletX, bulletY);
                var bulletSize = 10;
                var bulletSpeed = 64;

                var dist = phy.Position - bulletPos;
                var penDepth = shp.Radius + bulletSize - dist.Length();
                var penRes = Vector2.Normalize(dist) * penDepth * 1.125f;
                bulletPos += penRes;
                var velocity = new Vector2(dx, dy) * bulletSpeed;
                SpawnManager.SpawnBullets(in ntt, ref bulletPos, ref velocity);
            }
        }
    }
}