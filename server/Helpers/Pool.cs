using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server.Helpers
{
    public class Pool<T>
    {
        public int Count => _queue.Count;
        private readonly Queue<T> _queue;

        private readonly Func<T> _onCreate;
        private readonly Action<T> _onReturn;
        public Pool(Func<T> createInstruction, Action<T> returnAction, int amount)
        {
            _onCreate = createInstruction;
            _onReturn = returnAction;
            _queue = new(amount);

            for (var i = 0; i < amount; i++)
                _queue.Enqueue(createInstruction());
        }

        public T Get() => _queue.Count == 0 ? _onCreate() : _queue.Dequeue();

        public void Return(T obj)
        {
            Task.Run(() =>
            {
                _onReturn.Invoke(obj);
                _queue.Enqueue(obj);
            });
        }
    }
}