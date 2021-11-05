using iogame.Simulation.Components;

namespace iogame.Simulation.Entities
{
    public unsafe class Bullet : ShapeEntity
    {
        public ShapeEntity Owner;
        public ref LifeTimeComponent LifeTimeComponent => ref Entity.Get<LifeTimeComponent>();

        public Bullet()
        {
            BodyDamage = 2f;
        }

        public void SetOwner(ShapeEntity owner) => Owner = owner;
    }
}