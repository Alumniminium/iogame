using server.Helpers;

namespace server.ECS
{
    public class PixelSystem<T> : PixelSystem where T : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity entity) => entity.Has<T>();
    }
    public class PixelSystem<T, T2> : PixelSystem where T : struct where T2 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity entity) => entity.Has<T, T2>();
    }
    public class PixelSystem<T, T2, T3> : PixelSystem where T : struct where T2 : struct where T3 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity entity) => entity.Has<T, T2, T3>();
    }
    public class PixelSystem<T, T2, T3, T4> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity entity) => entity.Has<T, T2, T3, T4>();
    }
    public class PixelSystem<T, T2, T3, T4, T5> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity entity) => entity.Has<T, T2, T3, T4, T5>();
    }
    public abstract class PixelSystem
    {
        public string Name;
        private int _readyThreads;
        private int _threadId;
        private readonly List<PixelEntity>[] _entities;
        private readonly Thread[] _threads;
        private readonly Semaphore _block;
        private float _currentDeltaTime;

        protected PixelSystem(string name, int threads = 2)
        {
            Name = name;
            PerformanceMetrics.RegisterSystem(Name);
            _entities = new List<PixelEntity>[threads];
            _threads = new Thread[threads];
            _block = new Semaphore(0, threads);

            for (var i = 0; i < _threads.Length; i++)
            {
                _entities[i] = new List<PixelEntity>();
                _threads[i] = new Thread(WaitLoop)
                {
                    Name = $"{Name} Thread #{i}",
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
                _block.WaitOne();
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

        protected virtual void Update(float deltaTime, List<PixelEntity> entities) { }
        protected abstract bool MatchesFilter(in PixelEntity entityId);
        private bool ContainsEntity(in PixelEntity entity)
        {
            for (var i = 0; i < _entities.Length; i++)
                for (var k = 0; k < _entities[i].Count; k++)
                    if (_entities[i][k].EntityId == entity.EntityId)
                        return true;
            return false;
        }
        private void AddEntity(in PixelEntity entity)
        {
            _entities[_threadId++].Add(entity);

            if (_threadId == _threads.Length)
                _threadId = 0;
        }

        private void RemoveEntity(in PixelEntity entity)
        {
            for (var i = 0; i < _entities.Length; i++)
                _entities[i].Remove(entity);
                // for(int k = 0; k < _entities[i].Count; k++)
                //     if (_entities[i][k].EntityId == entity.EntityId)
                //     {
                //         _entities[i].Remove(k);
                //         break;
                //     }
        }

        internal void EntityChanged(in PixelEntity entity)
        {
            var isMatch = MatchesFilter(in entity);
            var isNew = !ContainsEntity(in entity);

            switch (isMatch)
            {
                case true when isNew:
                    AddEntity(in entity);
                    break;
                case false when !isNew:
                    RemoveEntity(in entity);
                    break;
            }
        }
    }
}