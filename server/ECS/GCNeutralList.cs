namespace iogame.Simulation.Managers
{
    public class GCNeutralList<T>
    {
        private T[] items;
        private readonly Stack<int> availableIndicies;

        public GCNeutralList(int maxCountItems)
        {
            items = new T[maxCountItems];
            availableIndicies=new Stack<int>(maxCountItems);
            Clear();
        }

        public ref T this[int index] { get => ref items[index]; }

        public int Count => items.Length - availableIndicies.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if(availableIndicies.TryPop(out int arrayIdx))
                items[arrayIdx]=item;
            else
                throw new Exception("No more space in this list u fool!");
        }
        public bool Remove(T item)
        {
            for(int i = 0; i< items.Length; i++)
            {
                if(items[i].Equals(item))
                {
                    availableIndicies.Push(i);
                    return true;
                }
            }
            return false;
        }
        public bool Contains(T item)
        {
            // can be optimized, we know how many items are inside
            // however, that would require us to defrag the array i think?
            //for(int i = 0; i < (Items.Length-availableIndicies.Count); i++)
            for(int i = 0; i < items.Length; i++)
            {
                if(items[i].Equals(item))
                    return true;
            }
            return false;
        }

        public int IndexOf(T item)
        {
            for(int i = 0; i< items.Length; i++)
            {
                if(items[i].Equals(item))
                    return i;
            }
            return -1;
        }
        public void Clear()
        {
            foreach(var i in Enumerable.Range(0,items.Length).Reverse())
                availableIndicies.Push(i);
        }

    }
}