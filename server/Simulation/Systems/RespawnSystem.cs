using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems;

public sealed class RespawnSystem : NttSystem<RespawnTagComponent, PhysicsComponent, LevelComponent, HealthComponent, EngineComponent>
{
    public RespawnSystem() : base("Respawn System", threads: 1) { }

    public override void Update(in NTT ntt, ref RespawnTagComponent rtc, ref PhysicsComponent phy, ref LevelComponent lvl, ref HealthComponent hlt, ref EngineComponent eng)
    {
        if (rtc.RespawnTimeTick > NttWorld.Tick)
            return;

        lvl.Experience -= rtc.ExpPenalty;
        while (lvl.Experience < 0)
        {
            if (lvl.Level == 1)
            {
                lvl.Experience = 0;
                lvl.ExperienceToNextLevel = 100;
                break;
            }
            lvl.Level--;
            lvl.Experience += lvl.Experience;
            lvl.ExperienceToNextLevel = (int)(100f * lvl.Level * 1.25f);
        }
        eng.Throttle = 0f;
        hlt.Health = hlt.MaxHealth;
        var spawn = SpawnManager.PlayerSpawnPoint;
        phy.RotationRadians = -90f.ToRadians();
        phy.Position = spawn;
        phy.LinearVelocity = Vector2.Zero;
        phy.AngularVelocity = 0f;
        phy.ChangedTick = NttWorld.Tick;
        ntt.Remove<RespawnTagComponent>();
        var inp = new InputComponent(ntt, default, default, default);
        ntt.Set(ref inp);
    }
}