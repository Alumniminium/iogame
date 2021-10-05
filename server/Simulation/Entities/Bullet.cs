namespace iogame.Simulation.Entities
{
    public class Bullet : Entity
    {
        public Entity Owner;
        public uint SpawnTime;
        
        public Bullet(uint uniqueId, Entity owner)
        {
            UniqueId=  uniqueId;
            Owner=owner;
            Size = 50;
            FillColor = Convert.ToUInt32("ffe869", 16);
            BorderColor = Convert.ToUInt32("bfae4e", 16);
            BodyDamage = 12f;
            Health = 10; 
        }


        internal void Hit(Entity b)
        {
            Health -= b.BodyDamage;
            Health -= BodyDamage;
            b.Health -= BodyDamage;
        }

        public override async Task Update(float deltaTime)
        {
            if(SpawnTime + 100 < Game.TickCount)
                await Game.RemoveEntity(this);
            await base.Update(deltaTime);
        }
    }
}