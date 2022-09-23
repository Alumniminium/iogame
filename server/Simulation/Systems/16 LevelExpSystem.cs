using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class LevelExpSystem : PixelSystem<LevelComponent, ExpRewardComponent>
    {
        public LevelExpSystem() : base("Level & Exp System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref LevelComponent lvl, ref ExpRewardComponent exp)
        {
            lvl.Experience += exp.Experience;
            lvl.ChangedTick = Game.CurrentTick;
            ntt.Remove<ExpRewardComponent>();

            if (lvl.Experience < lvl.ExperienceToNextLevel)
                return;

            lvl.Level++;
            lvl.Experience = 0;
            lvl.ExperienceToNextLevel = (int)(lvl.ExperienceToNextLevel * 1.25f);

            ref var phy = ref ntt.Get<PhysicsComponent>();
            ref var shi = ref ntt.Get<ShieldComponent>();
            ref var wep = ref ntt.Get<WeaponComponent>();
            ref var eng = ref ntt.Get<EngineComponent>();
            ref var vwp = ref ntt.Get<ViewportComponent>();

            vwp.Viewport.Width = vwp.Viewport.Width * 1.03f;
            vwp.Viewport.Height = vwp.Viewport.Height * 1.03f;
            eng.MaxPropulsion += 50;
            wep.BulletDamage += 10;
            // wep.BulletSpeed += 10;
            wep.Frequency -= System.TimeSpan.FromMilliseconds(10);
            shi.Radius+=2;
            phy.Size += 0.5f;
            phy.ChangedTick = Game.CurrentTick;
        }
    }
}
