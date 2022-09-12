using System.Runtime.CompilerServices;

namespace server.Helpers
{
    public class RefList<T>
    {
        private T[] _array;
        private int _capacity = 64;

        public int Count { get; private set; }

        public RefList(int capacity)
        {
            _array = new T[_capacity = capacity];
        }

        public RefList()
        {
            _array = new T[_capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(in T value)
        {
            if (Count >= _array.Length)
                Expand();

            _array[Count++] = value;
            return Count;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(T value)
        {
            if (Count >= _array.Length)
                Expand();

            _array[Count++] = value;
            return Count;
        }
        public void Set(int index, T value)
        {
            _array[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Remove(int offset)
        {
            var newArr = new T[_array.Length];
            if (offset == 0)
                Array.Copy(_array, 1, newArr, 0, _array.Length - 1);
            else
            {
                Array.Copy(_array, 0, newArr, 0, offset - 1);
                Array.Copy(_array, offset, newArr, offset, _array.Length - offset - 1);
            }
            Count--;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Remove(in T item)
        {
            var index = IndexOf(in item);
            if (index != -1)
                Remove(index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item)
        {
            return IndexOf(in item) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item)
        {
            var c = EqualityComparer<T>.Default;
            for (var i = 0; i < Count; i++)
            {
                if (c.Equals(_array[i], item))
                    return i;
            }
            return -1;
        }

        private void Expand()
        {
            FConsole.WriteLine("Expanding");
            var newCapacity = _array.Length * 2;

            var newArray = new T[newCapacity];
            Array.Copy(_array, newArray, _array.Length);
            _array = newArray;

            _capacity = newCapacity;
        }

        public ref T this[int index] => ref _array[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (Count > 50)
                Array.Clear(_array, 0, Count);
            else
                for (var i = 0; i < Count; i++)
                    _array[i] = default;

            Count = 0;
        }

        public void AddRange(RefList<T> items)
        {
            for (var i = 0; i < items.Count; i++)
                Add(items[i]);
        }
        public void AddRange(List<T> items)
        {
            for (var i = 0; i < items.Count; i++)
                Add(items[i]);
        }
    }
}