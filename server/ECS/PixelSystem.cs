
using iogame.ECS;

namespace iogame.Simulation.Managers
{
    public class PixelSystem<T> : PixelSystem where T : struct 
    {
        public PixelSystem(int threads = 1) : base(threads) { }
        public override bool MatchesFilter(ref Entity entity) => Pattern<T>.Match(entity);
    }
    public class PixelSystem<T,T2> : PixelSystem where T : struct where T2 : struct
    {
        public PixelSystem(int threads = 1) : base(threads) { }
        public override bool MatchesFilter(ref Entity entity) => Pattern<T,T2>.Match(entity); 
    }
    public class PixelSystem<T,T2,T3> : PixelSystem where T : struct where T2 : struct where T3 : struct
    {
        public PixelSystem(int threads = 1) : base(threads) { }
        public override bool MatchesFilter(ref Entity entity) => Pattern<T,T2,T3>.Match(entity);
    }
    public class PixelSystem<T,T2,T3,T4> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct 
    {
        public PixelSystem(int threads = 1) : base(threads) { }
        public override bool MatchesFilter(ref Entity entity) => Pattern<T,T2,T3,T4>.Match(entity);
    }
    public abstract class PixelSystem
    {
        public bool IsActive { get; set; }
        public string Name { get; set; } = "Unnamed System";
        
        private int readyThreads;
        private int _counter;
        public List<Entity>[] Entities;
        public Thread[] Threads;
        public SemaphoreSlim Block;
        private float CurrentDeltaTime;

        public PixelSystem(int threads=1)
        {
            Entities = new List<Entity>[threads];
            Threads=new Thread[threads];
            Block = new SemaphoreSlim(0);

            for (int i = 0; i < Threads.Length; i++)
            {
                Entities[i] = new List<Entity>(10_000);
                Threads[i] = new Thread(WaitLoop);
                Threads[i].Name = Name + " Thread #" + i;
                Threads[i].IsBackground=true;
                Threads[i].Start(i);
            }
        }

        private void WaitLoop(object ido)
        {
            var idx = (int)ido;
            while(true)
            {
                Interlocked.Increment(ref readyThreads);
                Block.Wait();
                Update(CurrentDeltaTime, Entities[idx]);
            }
        }

        public virtual void Initialize(){ IsActive = true;}

        public void Update(float deltaTime) 
        {
            CurrentDeltaTime= deltaTime;
            readyThreads=0;

            Block.Release(Threads.Length);
            while(readyThreads < Threads.Length)
                Thread.Yield(); // wait for threads to finish
        }
        public virtual void Update(float deltaTime, List<Entity> entities){}
        public virtual bool MatchesFilter(ref Entity entityId)=>false;
        private bool ContainsEntity(ref Entity entity)
        {
            for(int i =0;i<Threads.Length;i++)
            {
                if (Entities[i].Contains(entity))
                {
                    return true;
                }
            }
            return false;
        }
        private void AddEntity(ref Entity entity)
        {
            Entities[_counter++].Add(entity);
            
            if(_counter==Threads.Length)
                _counter=0;
        }
        private void RemoveEntity(ref Entity entity)
        {
            foreach(var list in Entities)
                list.Remove(entity);
        }

        internal void EntityChanged(ref Entity entity)
        {
            var isMatch = MatchesFilter(ref entity);
            var isNew = !ContainsEntity(ref entity);

            if(isMatch && isNew)
                AddEntity(ref entity);
            else if(!isMatch && !isNew)   
                RemoveEntity(ref entity);
        }
    }
}