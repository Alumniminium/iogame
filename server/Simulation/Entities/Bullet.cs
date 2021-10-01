namespace iogame.Simulation.Entities
{
    public class Bullet : Entity
    {
        public Player Owner;
        public uint SpawnTime;
        
        public Bullet()
        {
            Size = 100;
            Sides = 4;
            FillColor = Convert.ToUInt32("ffe869", 16);
            BorderColor = Convert.ToUInt32("bfae4e", 16);
        }


        public override void Update(float deltaTime)
        {
            if(SpawnTime + 100 < Game.TickCount)
                Game.RemoveEntity(this);
            base.Update(deltaTime);
        }
    }
}