namespace iogame.Simulation.Entities
{
    public class Bullet : Entity
    {
        public Entity Owner;
        public uint SpawnTime;
        public float Damage;
        
        public Bullet(uint uniqueId, Entity owner)
        {
            UniqueId=  uniqueId;
            Owner=owner;
            Size = 20;
            Sides = 4;
            FillColor = Convert.ToUInt32("ffe869", 16);
            BorderColor = Convert.ToUInt32("bfae4e", 16);
            Damage = 120f;
        }


        internal void Hit(Entity b)
        {
            Health--;
            b.Health -= Damage;

            Console.WriteLine(b.Health);
        }

        public override void Update(float deltaTime)
        {
            if(SpawnTime + 100 < Game.TickCount)
                Game.RemoveEntity(this);
            base.Update(deltaTime);
        }
    }
}