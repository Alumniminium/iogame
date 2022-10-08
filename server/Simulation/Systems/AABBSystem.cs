using Packets.Enums;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public unsafe sealed class AABBSystem : PixelSystem<AABBComponent, PhysicsComponent>
    {
        public AABBSystem() : base("AABB System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Pickable && ntt.Type != EntityType.Static && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref AABBComponent aabb, ref PhysicsComponent phy)
        {
            if(phy.LastPosition == phy.Position)
                return;
                
            aabb.AABB.X = phy.Position.X - aabb.AABB.Width / 2;
            aabb.AABB.Y = phy.Position.Y - aabb.AABB.Height / 2;
            aabb.PotentialCollisions.Clear();
            Game.Grid.GetPotentialCollisions(ref aabb);
        }
    }
}