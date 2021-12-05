using System;
using System.Runtime.CompilerServices;

namespace server.Helpers
{
    public class RefList<T>
    {
        private T[] _array;
        private int _index;
        private int _capacity = 4;

        public int Count => _index;

        public RefList(int capacity) => _array = new T[this._capacity = capacity];
        public RefList() => _array = new T[_capacity];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T value)
        {
            if (_index >= _array.Length)
                Expand();

            _array[_index++] = value;
        }

        public T Get(int index) => _array[index];
        public void Set(int index, T value) => _array[index] = value;

        internal void Remove(int offset)
        {
            var newArr = new T[_array.Length];
            if(offset == 0)
                Array.Copy(_array,1,newArr,0,_array.Length-1);
            else
            {
                Array.Copy(_array,0,newArr,0,offset);
                Array.Copy(_array,offset+1,newArr, offset,_array.Length - offset-1);
            }
            _index--;
        }
        public void Expand()
        {
            var newCapacity = _array.Length * 2;

            T[] newArray = new T[newCapacity];
            Array.Copy(_array, newArray, _array.Length);
            _array = newArray;

            _capacity = newCapacity;
        }

        public ref T this[int index]
        {
            get => ref _array[index];
        }
    }
} 