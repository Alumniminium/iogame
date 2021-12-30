using System.Numerics;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class InputSystem : PixelSystem<InputComponent, PhysicsComponent, ShapeComponent>
    {
        public InputSystem() : base("Input System", threads: 1) { }

        protected override void Update(float dt, Span<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                ref var inp = ref entity.Get<InputComponent>();
                ref var phy = ref entity.Get<PhysicsComponent>();
                ref var shp = ref entity.Get<ShapeComponent>();

                if (!inp.Fire)
                    continue;
                if (inp.LastShot + 10 > Game.CurrentTick)
                    continue;

                inp.LastShot = Game.CurrentTick;

                var direction = (float)Math.Atan2(inp.MousePositionWorld.Y, inp.MousePositionWorld.X);
                var bulletCount = 51;
                var d = bulletCount > 1 ? (float)Math.PI*2/bulletCount : 0;
                direction -= bulletCount > 1 ? d * bulletCount/2 : 0 ;
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
                    SpawnManager.SpawnBullets(ref entity, ref bulletPos, ref velocity);
                }
            }
        }
    }
}