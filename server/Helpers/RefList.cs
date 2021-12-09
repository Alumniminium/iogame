using System.Runtime.CompilerServices;

namespace server.Helpers
{
    public class RefList<T>
    {
        private T[] _array;
        private int _index;
        private int _capacity = 4;

        public int Count => _index;

        public RefList(int capacity) => _array = new T[_capacity = capacity];
        public RefList() => _array = new T[_capacity];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(T value)
        {
            if (_index >= _array.Length)
                Expand();

            _array[_index++] = value;
            return _index;
        }

        public ref T Get(int index) => ref _array[index];
        public void Set(int index, T value) => _array[index] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Remove(int offset)
        {
            var newArr = new T[_array.Length];
            if(offset == 0)
                Array.Copy(_array,1,newArr,0,_array.Length-1);
            else
            {
                Array.Copy(_array,0,newArr,0,offset-1);
                Array.Copy(_array,offset,newArr, offset,_array.Length - offset-1);
            }
            _index--;
        }

        private void Expand()
        {
            var newCapacity = _array.Length * 2;

            T[] newArray = new T[newCapacity];
            Array.Copy(_array, newArray, _array.Length);
            _array = newArray;

            _capacity = newCapacity;
        }

        public ref T this[int index] => ref _array[index];
    }
} 