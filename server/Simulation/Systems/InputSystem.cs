using System;
using System.Numerics;
using Packets.Enums;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public sealed class InputSystem : PixelSystem<InputComponent>
    {

        public InputSystem() : base("InputSystem System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref InputComponent c1)
        {
            if (ntt.Has<EngineComponent>())
                ConfigureEngine(in ntt, ref c1);
            if (ntt.Has<WeaponComponent>())
                ConfigureWeapons(in ntt, ref c1);
            if (ntt.Has<InventoryComponent>())
                ConfigureInventory(in ntt, ref c1);
            if (ntt.Has<ShieldComponent>())
                ConfigureShield(in ntt, ref c1);
        }

        private static void ConfigureShield(in PixelEntity ntt, ref InputComponent c1)
        {
            ref var shield = ref ntt.Get<ShieldComponent>();
            shield.PowerOn = c1.ButtonStates.HasFlag(PlayerInput.Shield);
            if (shield.LastPowerOn == shield.PowerOn)
                return;
            shield.LastPowerOn = shield.PowerOn;
            shield.ChangedTick = Game.CurrentTick;
        }

        private static void ConfigureWeapons(in PixelEntity ntt, ref InputComponent inp)
        {
            if (!inp.ButtonStates.HasFlags(PlayerInput.Fire))
                return;

            ref var wep = ref ntt.Get<WeaponComponent>();
            wep.Fire = true;
        }
        private static void ConfigureInventory(in PixelEntity ntt, ref InputComponent inp)
        {
            if (!inp.ButtonStates.HasFlags(PlayerInput.Drop))
                return;

            ref var phy = ref ntt.Get<PhysicsComponent>();
            ref var wep = ref ntt.Get<WeaponComponent>();
            var halfPi = MathF.PI / 2;
            var behind = -phy.Forward.ToRadians();

            behind += (Random.Shared.NextSingle() + -Random.Shared.NextSingle()) * halfPi;

            var dx = MathF.Cos(behind);
            var dy = MathF.Sin(behind);

            var dropX = -dx + phy.Position.X;
            var dropY = -dy + phy.Position.Y;
            var dropPos = new Vector2(dropX, dropY);

            var dist = phy.Position - dropPos;
            var penDepth = phy.Radius + 1 - dist.Length();
            var penRes = Vector2.Normalize(dist) * penDepth * 1.25f;
            dropPos += penRes;

            if (dropPos.X + 1 > Game.MapSize.X || dropPos.X - 1 < 0 || dropPos.Y + 1 > Game.MapSize.Y || dropPos.Y - 1 < 0)
                return;

            var velocity = new Vector2(dx, dy) * 10;

            ref var inv = ref ntt.Get<InventoryComponent>();
            if (inv.Triangles != 0)
            {
                inv.ChangedTick = Game.CurrentTick;
                inv.Triangles--;
                SpawnManager.SpawnDrop(Database.Db.BaseResources[3], dropPos, 1, Database.Db.BaseResources[3].Color, TimeSpan.FromSeconds(15), velocity);
            }
            if (inv.Squares != 0)
            {
                inv.ChangedTick = Game.CurrentTick;
                inv.Squares--;
                SpawnManager.SpawnDrop(Database.Db.BaseResources[4], dropPos, 1, Database.Db.BaseResources[4].Color, TimeSpan.FromSeconds(15), velocity);
            }
            if (inv.Pentagons != 0)
            {
                inv.ChangedTick = Game.CurrentTick;
                inv.Pentagons--;
                SpawnManager.SpawnDrop(Database.Db.BaseResources[5], dropPos, 1, Database.Db.BaseResources[5].Color, TimeSpan.FromSeconds(15), velocity);
            }

        }

        private void ConfigureEngine(in PixelEntity ntt, ref InputComponent inp)
        {
            ref var eng = ref ntt.Get<EngineComponent>();
            eng.RCS = inp.ButtonStates.HasFlag(PlayerInput.RCS);

            if (inp.DidBoostLastFrame)
            {
                eng.ChangedTick = Game.CurrentTick;
                eng.Throttle = 0;
                inp.DidBoostLastFrame = false;
            }

            eng.Rotation = inp.ButtonStates.HasFlag(PlayerInput.Left) ? -1f : inp.ButtonStates.HasFlag(PlayerInput.Right) ? 1f : 0f;

            if (inp.ButtonStates.HasFlag(PlayerInput.Boost))
            {
                eng.ChangedTick = Game.CurrentTick;
                eng.Throttle = 1;
                inp.DidBoostLastFrame = true;
            }
            else if (inp.ButtonStates.HasFlags(PlayerInput.Thrust))
            {
                eng.ChangedTick = Game.CurrentTick;
                eng.Throttle = Math.Clamp(eng.Throttle + (1f * deltaTime), 0, 1);
            }
            else if (inp.ButtonStates.HasFlags(PlayerInput.InvThrust))
            {
                eng.ChangedTick = Game.CurrentTick;
                eng.Throttle = Math.Clamp(eng.Throttle - (1f * deltaTime), 0, 1);
            }

            if (inp.MovementAxis != Vector2.Zero)
                eng.Rotation = MathF.Atan2(inp.MovementAxis.X, inp.MovementAxis.Y);
        }
    }
}