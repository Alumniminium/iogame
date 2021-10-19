namespace iogame.Simulation.Entities
{
    public class Bullet : Entity
    {
        public Entity Owner;
        public float LifeTimeSeconds;

        public Bullet(uint uniqueId, Entity owner)
        {
            UniqueId = uniqueId;
            Owner = owner;
            Size = 25;
            FillColor = Convert.ToUInt32("ffe869", 16);
            BorderColor = Convert.ToUInt32("bfae4e", 16);
            BodyDamage = 120f;
            Health = 100;
            MaxHealth = 100;
        }


        internal void Hit(Entity b)
        {
            Health -= b.BodyDamage;
            Health -= BodyDamage;
            b.Health -= BodyDamage;
        }

        public override void Update(float deltaTime)
        {
            LifeTimeSeconds -= deltaTime;

            if (LifeTimeSeconds <= 0)
                Game.RemoveEntity(this);
            base.Update(deltaTime);
        }
    }
}