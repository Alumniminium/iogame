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
            LeaderBoard.Add(new LeaderBoard.LeaderBoardEntry { Name = ntt.Get<NameTagComponent>().Name, Score = lvl.Experience });
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

            vwp.Viewport.Width *= 1.03f;
            vwp.Viewport.Height *= 1.03f;
            eng.MaxPropulsion += 25;
            wep.BulletDamage += 10;
            wep.BulletSpeed += 1;
            wep.Frequency -= System.TimeSpan.FromMilliseconds(10);
            shi.Radius+=2;
            phy.Size += 0.5f;
            phy.ChangedTick = Game.CurrentTick;
        }
    }
}
