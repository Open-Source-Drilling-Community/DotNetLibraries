namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// A disjoint set forest with union by size and path halving.
    /// </summary>
    internal sealed class UnionFind
    {
        private readonly int[] parent_;
        private readonly int[] size_;

        /// <summary>
        /// the number of disjoint sets currently held
        /// </summary>
        public int SetCount { get; private set; }

        /// <summary>
        /// constructor with initialization to the given number of singletons
        /// </summary>
        /// <param name="count"></param>
        public UnionFind(int count)
        {
            parent_ = new int[count];
            size_ = new int[count];
            Reset(count);
        }

        /// <summary>
        /// resets the forest to the given number of singletons, reusing the storage
        /// </summary>
        /// <param name="count"></param>
        public void Reset(int count)
        {
            for (int i = 0; i < count; i++)
            {
                parent_[i] = i;
                size_[i] = 1;
            }
            SetCount = count;
        }

        /// <summary>
        /// the capacity of the forest
        /// </summary>
        public int Capacity
        {
            get
            {
                return parent_.Length;
            }
        }

        /// <summary>
        /// the representative of the set the given element belongs to
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public int Find(int element)
        {
            int root = element;
            while (parent_[root] != root)
            {
                parent_[root] = parent_[parent_[root]];
                root = parent_[root];
            }
            return root;
        }

        /// <summary>
        /// merges the sets of the two given elements. Returns false when they already were in the same
        /// set, in which case nothing was done.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public bool Union(int first, int second)
        {
            int a = Find(first);
            int b = Find(second);
            if (a == b)
            {
                return false;
            }
            if (size_[a] < size_[b])
            {
                (a, b) = (b, a);
            }
            parent_[b] = a;
            size_[a] += size_[b];
            SetCount--;
            return true;
        }
    }
}
