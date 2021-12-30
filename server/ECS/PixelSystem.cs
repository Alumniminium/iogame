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
        private readonly HashSet<PixelEntity> _entities= new ();
        private PixelEntity[] _entitiesArr = Array.Empty<PixelEntity>();
        private readonly Thread[] _threads;
        private readonly Semaphore _block;
        private float _currentDeltaTime;

        protected PixelSystem(string name, int threads = 1)
        {
            Name = name;
            PerformanceMetrics.RegisterSystem(Name);
            _threads = new Thread[threads];
            _block = new Semaphore(0, threads);

            for (var i = 0; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(WaitLoop)
                {
                    Name = $"{Name} #{i}",
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

                var amount = _entities.Count;
                var chunkSize = amount / _threads.Length;
                var start = chunkSize * idx;

                Update(_currentDeltaTime, _entitiesArr.AsSpan(start,chunkSize));
            }
        }

        public void Update(float deltaTime)
        {
            if(_entities.Count != _entitiesArr.Length)
                _entitiesArr = _entities.ToArray();

            _currentDeltaTime = deltaTime;
            _readyThreads = 0;

            _block.Release(_threads.Length);
            while (_readyThreads < _threads.Length)
                Thread.Yield();
        }

        protected virtual void Update(float deltaTime, Span<PixelEntity> entities) { }
        protected abstract bool MatchesFilter(in PixelEntity entityId);
        internal void EntityChanged(in PixelEntity entity)
        {
            var isMatch = MatchesFilter(in entity);
            var isNew = !_entities.Contains(entity);

            switch (isMatch)
            {
                case true when isNew:
                    _entities.Add(entity);
                    break;
                case false when !isNew:
                    _entities.Remove(entity);
                    break;
            }
        }
    }
}