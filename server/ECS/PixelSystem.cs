using System.Threading;
using server.Helpers;

namespace server.ECS
{
    public class PixelSystem<T> : PixelSystem where T : struct
    {
        public PixelSystem(string name, int threads = 1) : base(name, threads) { }
        public override bool MatchesFilter(ref PixelEntity entity) => entity.Has<T>();
    }
    public class PixelSystem<T, T2> : PixelSystem where T : struct where T2 : struct
    {
        public PixelSystem(string name, int threads = 1) : base(name, threads) { }
        public override bool MatchesFilter(ref PixelEntity entity) => entity.Has<T, T2>();
    }
    public class PixelSystem<T, T2, T3> : PixelSystem where T : struct where T2 : struct where T3 : struct
    {
        public PixelSystem(string name, int threads = 1) : base(name, threads) { }
        public override bool MatchesFilter(ref PixelEntity entity) => entity.Has<T, T2, T3>();
    }
    public class PixelSystem<T, T2, T3, T4> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct
    {
        public PixelSystem(string name, int threads = 1) : base(name, threads) { }
        public override bool MatchesFilter(ref PixelEntity entity) => entity.Has<T, T2, T3, T4>();
    }
    public class PixelSystem<T, T2, T3, T4, T5> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        public PixelSystem(string name, int threads = 1) : base(name, threads) { }
        public override bool MatchesFilter(ref PixelEntity entity) => entity.Has<T, T2, T3, T4, T5>();
    }
    public abstract class PixelSystem
    {
        public bool IsActive { get; set; }
        public string Name { get; set; } = "Unnamed System";

        private int _readyThreads;
        private int _threadId;
        private readonly RefList<PixelEntity>[] _entities;
        private readonly Thread[] _threads;
        private readonly SemaphoreSlim _block;
        private float _currentDeltaTime;

        public PixelSystem(string name, int threads = 1)
        {
            Name = name;
            PerformanceMetrics.RegisterSystem(Name);
            _entities = new RefList<PixelEntity>[threads];
            _threads = new Thread[threads];
            _block = new SemaphoreSlim(0);

            for (int i = 0; i < _threads.Length; i++)
            {
                _entities[i] = new RefList<PixelEntity>();
                _threads[i] = new Thread(WaitLoop)
                {
                    Name = Name + " Thread #" + i,
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                _threads[i].Start(i);
            }
        }

        private void WaitLoop(object ido)
        {
            var idx = (int)ido;
            while (true)
            {
                Interlocked.Increment(ref _readyThreads);
                _block.Wait();
                Update(_currentDeltaTime, _entities[idx]);
            }
        }

        public void Update(float deltaTime)
        {
            _currentDeltaTime = deltaTime;
            _readyThreads = 0;

            _block.Release(_threads.Length);
            while (_readyThreads < _threads.Length)
                Thread.Yield();
        }
        public virtual void Update(float deltaTime, RefList<PixelEntity> entities) { }
        public abstract bool MatchesFilter(ref PixelEntity entityId);
        private bool ContainsEntity(ref PixelEntity entity)
        {
            for (int i = 0; i < _entities.Length; i++)
            {
                for (int k = 0; k < _entities[i].Count; k++)
                {
                    if (_entities[i][k].EntityId == entity.EntityId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void AddEntity(ref PixelEntity entity)
        {
            _entities[_threadId++].Add(entity);

            if (_threadId == _threads.Length)
                _threadId = 0;
        }

        public void RemoveEntity(ref PixelEntity entity)
        {
            for (int i = 0; i < _entities.Length; i++)
            {
                for (int k = 0; k < _entities[i].Count; k++)
                {
                    if (_entities[i][k].EntityId == entity.EntityId)
                    {
                        _entities[i].Remove(k);
                    }
                }
            }
        }

        internal void EntityChanged(ref PixelEntity entity)
        {
            var isMatch = MatchesFilter(ref entity);
            var isNew = !ContainsEntity(ref entity);

            if (isMatch && isNew)
                AddEntity(ref entity);
            else if (!isMatch && !isNew)
                RemoveEntity(ref entity);
        }
    }
}