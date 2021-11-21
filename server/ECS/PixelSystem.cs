
using System.Diagnostics;
using iogame.ECS;
using iogame.Util;

namespace iogame.Simulation.Managers
{
    public class PixelSystem<T> : PixelSystem where T : struct 
    {
        public PixelSystem(int threads = 1) : base(threads) { }
        public override bool MatchesFilter(ref PixelEntity entity) => entity.Has<T>();
    }
    public class PixelSystem<T,T2> : PixelSystem where T : struct where T2 : struct
    {
        public PixelSystem(int threads = 1) : base(threads) { }
        public override bool MatchesFilter(ref PixelEntity entity) => entity.Has<T,T2>(); 
    }
    public class PixelSystem<T,T2,T3> : PixelSystem where T : struct where T2 : struct where T3 : struct
    {
        public PixelSystem(int threads = 1) : base(threads) { }
        public override bool MatchesFilter(ref PixelEntity entity) => entity.Has<T,T2,T3>();
    }
    public class PixelSystem<T,T2,T3,T4> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct 
    {
        public PixelSystem(int threads = 1) : base(threads) { }
        public override bool MatchesFilter(ref PixelEntity entity) => entity.Has<T,T2,T3,T4>();
    }
    public class PixelSystem<T,T2,T3,T4,T5> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct  where T5: struct
    {
        public PixelSystem(int threads = 1) : base(threads) { }
        public override bool MatchesFilter(ref PixelEntity entity) => entity.Has<T,T2,T3,T4,T5>();
    }
    public abstract class PixelSystem
    {
        public bool IsActive { get; set; }
        public string Name { get; set; } = "Unnamed System";
        
        private int readyThreads;
        private int _counter;
        public List<PixelEntity>[] Entities;
        public Thread[] Threads;
        public SemaphoreSlim Block;
        private float CurrentDeltaTime;

        public PixelSystem(int threads=1)
        {
            Entities = new List<PixelEntity>[threads];
            Threads=new Thread[threads];
            Block = new SemaphoreSlim(0);

            for (int i = 0; i < Threads.Length; i++)
            {
                Entities[i] = new List<PixelEntity>();
                Threads[i] = new Thread(WaitLoop)
                {
                    Name = Name + " Thread #" + i,
                    IsBackground = true
                };
                Threads[i].Start(i);
            }
        }

        private void WaitLoop(object ido)
        {
            var idx = (int)ido;
            var sw = Stopwatch.StartNew();
            while(true)
            {
                Interlocked.Increment(ref readyThreads);
                Block.Wait();

                var last = sw.Elapsed.TotalMilliseconds;
                Update(CurrentDeltaTime, Entities[idx]);
                PerformanceMetrics.AddSample(Name, sw.Elapsed.TotalMilliseconds - last);
            }
        }

        public void Update(float deltaTime) 
        {
            CurrentDeltaTime= deltaTime;
            readyThreads=0;

            Block.Release(Threads.Length);
            while(readyThreads < Threads.Length)
                Thread.Yield(); // wait for threads to finish
        }
        public virtual void Update(float deltaTime, List<PixelEntity> entities){}
        public virtual bool MatchesFilter(ref PixelEntity entityId)=>false;
        private bool ContainsEntity(ref PixelEntity entity)
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
        private void AddEntity(ref PixelEntity entity)
        {
            Entities[_counter++].Add(entity);
            
            if(_counter==Threads.Length)
                _counter=0;
        }
        public void RemoveEntity(ref PixelEntity entity)
        {
            foreach(var list in Entities)
                list.Remove(entity);
        }

        internal void EntityChanged(ref PixelEntity entity)
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