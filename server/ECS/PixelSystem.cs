using System.Collections.Generic;
using server.Helpers;

namespace server.ECS
{
    public abstract class PixelSystem<T> : PixelSystem where T : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T>();
        protected override void Update()
        {
            for (var i = 0; i < _entities.Count; i++)
            {
                var ntt = _entities[i];
                ref var c1 = ref ntt.Get<T>();
                Update(in ntt, ref c1);
            }
        }
        public abstract void Update(in PixelEntity ntt, ref T c1);
    }
    public abstract class PixelSystem<T, T2> : PixelSystem where T : struct where T2 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T, T2>();
        protected override void Update()
        {
            for (var i = 0; i < _entities.Count; i++)
            {
                var ntt = _entities[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                Update(in ntt, ref c1, ref c2);
            }
        }
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2);
    }
    public abstract class PixelSystem<T, T2, T3> : PixelSystem where T : struct where T2 : struct where T3 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T, T2, T3>();
        protected override void Update()
        {
            for (var i = 0; i < _entities.Count; i++)
            {
                var ntt = _entities[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                Update(in ntt, ref c1, ref c2, ref c3);
            }
        }
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2, ref T3 c3);
    }
    public abstract class PixelSystem<T, T2, T3, T4> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T, T2, T3, T4>();
        protected override void Update()
        {
            for (var i = 0; i < _entities.Count; i++)
            {
                var ntt = _entities[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                ref var c4 = ref ntt.Get<T4>();
                Update(in ntt, ref c1, ref c2, ref c3, ref c4);
            }
        }
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4);
    }
    public abstract class PixelSystem<T, T2, T3, T4, T5> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T, T2, T3, T4, T5>();
        protected override void Update()
        {
            for (var i = 0; i < _entities.Count; i++)
            {
                var ntt = _entities[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                ref var c4 = ref ntt.Get<T4>();
                ref var c5 = ref ntt.Get<T5>();
                Update(in ntt, ref c1, ref c2, ref c3, ref c4, ref c5);
            }
        }
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4, ref T5 c5);
    }
    public abstract class PixelSystem
    {
        public string Name;
        internal readonly List<PixelEntity> _entities = new();
        internal float deltaTime;

        protected PixelSystem(string name, int threads = 1)
        {
            Name = name;
            PerformanceMetrics.RegisterSystem(this);
        }

        public void Update(float deltaTime)
        {
            this.deltaTime = deltaTime;
            PreUpdate();
            Update();
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
                _entities.Remove(ntt);
            else if (!_entities.Contains(ntt))
                _entities.Add(ntt);
        }
    }
}