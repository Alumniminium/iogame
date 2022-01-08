using System.Runtime.CompilerServices;

namespace server.Helpers
{
    public class RefList<T>
    {
        private T[] _array;
        private int _index;
        private int _capacity = 64;

        public int Count => _index;

        public RefList(int capacity) => _array = new T[_capacity = capacity];
        public RefList() => _array = new T[_capacity];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(in T value)
        {
            if (_index >= _array.Length)
                Expand();

            _array[_index++] = value;
            return _index;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(T value)
        {
            if (_index >= _array.Length)
                Expand();

            _array[_index++] = value;
            return _index;
        }
        public void Set(int index, T value) => _array[index] = value;

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
            _index--;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Remove(in T item)
        {
            var index = IndexOf(in item);
            if (index != -1)
                Remove(index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item) => IndexOf(in item) >= 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item)
        {
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            for (int i = 0; i < _index; i++)
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
            if (_index > 50)
                Array.Clear(_array, 0, _index);
            else
                for (int i = 0; i < _index; i++)
                    _array[i] = default;

            _index = 0;
        }

        public void AddRange(RefList<T> items)
        {
            for (int i = 0; i < items.Count; i++)
                Add(items[i]);
        }
        public void AddRange(List<T> items)
        {
            for (int i = 0; i < items.Count; i++)
                Add(items[i]);
        }
    }
}