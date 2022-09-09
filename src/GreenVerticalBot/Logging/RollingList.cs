using System.Collections;

namespace GreenVerticalBot.Logging
{
    internal class RollingList<T> : IEnumerable<T>
    {
        private readonly LinkedList<T> innerList = new LinkedList<T>();

        public RollingList(int maximumCount)
        {
            if (maximumCount <= 0)
                throw new ArgumentException(null, nameof(maximumCount));

            MaximumCount = maximumCount;
        }

        public int MaximumCount { get; }
        public int Count => this.innerList.Count;

        public void Add(T value)
        {
            lock (this.innerList)
            {
                if (this.innerList.Count == MaximumCount)
                {
                    this.innerList.RemoveFirst();
                }
                this.innerList.AddLast(value);
            }
        }

        public T this[int index]
        {
            get
            {
                lock (this.innerList)
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException();

                    return this.innerList.Skip(index).First();
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (this.innerList)
            {
                return this.innerList.GetEnumerator();
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
