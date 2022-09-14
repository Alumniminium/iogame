using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using server.Helpers;

namespace server.ECS
{
    public abstract class PixelSystem<T> : PixelSystem where T : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T>();

        public override void WaitLoop(object _)
        {
            var idx = (int)_;
            while (true)
            {
                Interlocked.Increment(ref _readyThreads);
                _block.WaitOne();

                var amount = _entities.Count;
                var chunkSize = amount / _threads.Length;

                var start = chunkSize * idx;

                if (_entities.Count < _threads.Length)
                {
                    chunkSize = amount;

                    if (idx != 0)
                        continue;
                }

                if (idx == _threads.Length - 1)
                    chunkSize = amount - start;

                for (var i = start; i < start + chunkSize; i++)
                {
                    ref readonly var ntt = ref _entitiesArr[i];
                    ref var c1 = ref ntt.Get<T>();
                    Update(in ntt, ref c1);
                }
            }
        }
        public abstract void Update(in PixelEntity ntt, ref T c1);
    }
    public abstract class PixelSystem<T, T2> : PixelSystem where T : struct where T2 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId)
        {
            return nttId.Has<T, T2>();
        }

        public override void WaitLoop(object _)
        {
            var idx = (int)_;
            while (true)
            {
                Interlocked.Increment(ref _readyThreads);
                _block.WaitOne();

                var amount = _entitiesArr.Length;
                var chunkSize = amount / _threads.Length;

                if (chunkSize == 0)
                {
                    if (idx != 0)
                        continue;
                    chunkSize = amount;
                }

                var start = chunkSize * idx;

                if (idx == _threads.Length - 1)
                    chunkSize = _entitiesArr.Length - start;

                for (var i = start; i < start + chunkSize; i++)
                {
                    ref readonly var ntt = ref _entitiesArr[i];
                    ref var c1 = ref ntt.Get<T>();
                    ref var c2 = ref ntt.Get<T2>();
                    Update(in ntt, ref c1, ref c2);
                }
            }
        }
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2);
    }
    public abstract class PixelSystem<T, T2, T3> : PixelSystem where T : struct where T2 : struct where T3 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId)
        {
            return nttId.Has<T, T2, T3>();
        }

        public override void WaitLoop(object _)
        {
            var idx = (int)_;
            while (true)
            {
                Interlocked.Increment(ref _readyThreads);
                _block.WaitOne();

                var amount = _entities.Count;
                var chunkSize = amount / _threads.Length;

                var start = chunkSize * idx;

                if (_entities.Count < _threads.Length)
                {
                    chunkSize = amount;

                    if (idx != 0)
                        continue;
                }

                if (idx == _threads.Length - 1)
                    chunkSize = amount - start;

                for (var i = start; i < start + chunkSize; i++)
                {
                    ref readonly var ntt = ref _entitiesArr[i];
                    ref var c1 = ref ntt.Get<T>();
                    ref var c2 = ref ntt.Get<T2>();
                    ref var c3 = ref ntt.Get<T3>();
                    Update(in ntt, ref c1, ref c2, ref c3);
                }
            }
        }
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2, ref T3 c3);
    }
    public abstract class PixelSystem<T, T2, T3, T4> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId)
        {
            return nttId.Has<T, T2, T3, T4>();
        }

        public override void WaitLoop(object _)
        {
            var idx = (int)_;
            while (true)
            {
                Interlocked.Increment(ref _readyThreads);
                _block.WaitOne();

                var amount = _entities.Count;
                var chunkSize = amount / _threads.Length;

                var start = chunkSize * idx;

                if (_entities.Count < _threads.Length)
                {
                    chunkSize = amount;

                    if (idx != 0)
                        continue;
                }

                if (idx == _threads.Length - 1)
                    chunkSize = amount - start;

                for (var i = start; i < start + chunkSize; i++)
                {
                    ref readonly var ntt = ref _entitiesArr[i];
                    ref var c1 = ref ntt.Get<T>();
                    ref var c2 = ref ntt.Get<T2>();
                    ref var c3 = ref ntt.Get<T3>();
                    ref var c4 = ref ntt.Get<T4>();
                    Update(in ntt, ref c1, ref c2, ref c3, ref c4);
                }
            }
        }
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4);
    }
    public abstract class PixelSystem<T, T2, T3, T4, T5> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId)
        {
            return nttId.Has<T, T2, T3, T4, T5>();
        }

        public override void WaitLoop(object _)
        {
            var idx = (int)_;
            while (true)
            {
                Interlocked.Increment(ref _readyThreads);
                _block.WaitOne();

                var amount = _entities.Count;
                var chunkSize = amount / _threads.Length;

                var start = chunkSize * idx;

                if (_entities.Count < _threads.Length)
                {
                    chunkSize = amount;

                    if (idx != 0)
                        continue;
                }

                if (idx == _threads.Length - 1)
                    chunkSize = amount - start;

                for (var i = start; i < start + chunkSize; i++)
                {
                    ref readonly var ntt = ref _entitiesArr[i];
                    ref var c1 = ref ntt.Get<T>();
                    ref var c2 = ref ntt.Get<T2>();
                    ref var c3 = ref ntt.Get<T3>();
                    ref var c4 = ref ntt.Get<T4>();
                    ref var c5 = ref ntt.Get<T5>();
                    Update(in ntt, ref c1, ref c2, ref c3, ref c4, ref c5);
                }
            }
        }
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4, ref T5 c5);
    }
    public abstract class PixelSystem : IDisposable
    {
        public string Name;
        internal int _readyThreads;
        internal readonly Dictionary<int, PixelEntity> _entities = new();
        internal PixelEntity[] _entitiesArr = Array.Empty<PixelEntity>();
        internal readonly Thread[] _threads;
        internal readonly Semaphore _block;
        internal float deltaTime;

        protected PixelSystem(string name, int threads = 1)
        {
            Name = name;
            PerformanceMetrics.RegisterSystem(this);
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

        public abstract void WaitLoop(object _);

        public void Update(float deltaTime)
        {
            if (_entities.Count != _entitiesArr.Length)
                _entitiesArr = _entities.Values.ToArray();

            this.deltaTime = deltaTime;

            _readyThreads = 0;
            PreUpdate();
            _block.Release(_threads.Length);
            while (_readyThreads < _threads.Length)
                Thread.Yield();
            PostUpdate();
        }
        protected virtual void PostUpdate() { }
        protected virtual void Update() { }
        protected virtual void PreUpdate() { }
        protected abstract bool MatchesFilter(in PixelEntity nttId);
        internal void EntityChanged(in PixelEntity ntt)
        {
            var isMatch = MatchesFilter(in ntt);
            if (!isMatch)
            {
                _entities.Remove(ntt.Id);
                return;
            }

            if (!_entities.ContainsKey(ntt.Id))
                _entities.Add(ntt.Id, ntt);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _block.Release(_threads.Length);
            foreach (var thread in _threads)
                thread.Join();
            _block.Dispose();
        }
    }
}