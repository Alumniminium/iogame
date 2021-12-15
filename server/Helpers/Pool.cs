using System.Collections.Concurrent;

namespace server.Helpers
{
    public class Pool<T> where T : new()
    {
        public static Pool<T> Shared = new(() => new T(), null, 50);
        public int Count => _queue.Count;
        private readonly ConcurrentQueue<T> _queue;

        private readonly Func<T> _onCreate;
        private readonly Action<T> _onReturn;
        public Pool(Func<T> createInstruction, Action<T> returnAction, int amount)
        {
            _onCreate = createInstruction;
            _onReturn = returnAction;
            _queue = new ConcurrentQueue<T>();

            for (var i = 0; i < amount; i++)
                _queue.Enqueue(createInstruction());
        }

        public T Get()
        {
            T found;
            
            while (!_queue.TryDequeue(out found))
                _onCreate();

            return found;
        }


        public void Return(ref T obj)
        {
            _onReturn?.Invoke(obj);
            _queue.Enqueue(obj);
        }
    }
}