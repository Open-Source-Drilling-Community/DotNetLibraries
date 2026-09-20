namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// whether a cell should be divided further
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public delegate bool OctreeRefinementRule(in OctreeCell cell);

    /// <summary>
    /// An octree held as nothing but its leaves: a sorted array of <see cref="OctreeKey"/>, eight bytes
    /// each, with no nodes and no pointers.
    /// <para>
    /// A pointer tree costs eight child references per interior node before any payload, which at tens of
    /// millions of leaves is gigabytes of pointers alone. Here the structure is implicit in the keys:
    /// because they are sorted on a Morton code normalised to the deepest level, a subtree is a
    /// contiguous run and the leaf holding a position is one binary search away.
    /// </para>
    /// <para>
    /// Payload belongs to the caller, as arrays indexed by leaf index, so that a cell costs exactly what
    /// the caller needs it to and nothing more.
    /// </para>
    /// </summary>
    public sealed class LinearOctree
    {
        private readonly ulong[] keys_;

        /// <summary>
        /// the cube the octree subdivides
        /// </summary>
        public OctreeFrame Frame { get; }

        /// <summary>
        /// the depth of the coarsest cells
        /// </summary>
        public int BaseDepth { get; }

        /// <summary>
        /// the depth of the finest cells that were allowed
        /// </summary>
        public int DeepestDepth { get; }

        private LinearOctree(OctreeFrame frame, int baseDepth, int deepestDepth, ulong[] keys)
        {
            Frame = frame;
            BaseDepth = baseDepth;
            DeepestDepth = deepestDepth;
            keys_ = keys;
        }

        /// <summary>
        /// the number of leaves
        /// </summary>
        public int LeafCount
        {
            get
            {
                return keys_.Length;
            }
        }

        /// <summary>
        /// the address of the given leaf
        /// </summary>
        /// <param name="leaf"></param>
        /// <returns></returns>
        public OctreeKey GetKey(int leaf)
        {
            return new OctreeKey(keys_[leaf]);
        }

        /// <summary>
        /// the geometry of the given leaf
        /// </summary>
        /// <param name="leaf"></param>
        /// <returns></returns>
        public OctreeCell GetCell(int leaf)
        {
            return Frame.GetCell(new OctreeKey(keys_[leaf]));
        }

        /// <summary>
        /// the number of leaves at each depth
        /// </summary>
        /// <returns></returns>
        public int[] GetDepthHistogram()
        {
            int[] histogram = new int[OctreeKey.MaximumDepth + 1];
            for (int leaf = 0; leaf < keys_.Length; leaf++)
            {
                histogram[new OctreeKey(keys_[leaf]).Depth]++;
            }
            return histogram;
        }

        /// <summary>
        /// the leaf holding the given position, or -1 when no leaf does
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <returns></returns>
        public int FindLeaf(double north, double east, double vertical)
        {
            OctreeKey? deepest = Frame.GetKey(north, east, vertical, OctreeKey.MaximumDepth);
            if (deepest == null)
            {
                return -1;
            }
            return FindLeafHolding(deepest.Value.NormalizedCode);
        }

        /// <summary>
        /// the leaf whose cell holds the given address, or -1 when no leaf does
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int FindLeafHolding(OctreeKey key)
        {
            return FindLeafHolding(key.NormalizedCode);
        }

        private int FindLeafHolding(ulong code)
        {
            // the last leaf whose code does not exceed the target is the one holding it, if any does
            int low = 0;
            int high = keys_.Length;
            while (low < high)
            {
                int middle = (int)(((uint)low + (uint)high) >> 1);
                if ((keys_[middle] >> 5) <= code)
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle;
                }
            }
            if (low == 0)
            {
                return -1;
            }
            int candidate = low - 1;
            OctreeKey found = new OctreeKey(keys_[candidate]);
            ulong start = found.NormalizedCode;
            return code >= start && code < start + found.CodeSpan ? candidate : -1;
        }

        /// <summary>
        /// the index of the leaf with exactly this address, or -1 when it is not a leaf
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int IndexOf(OctreeKey key)
        {
            int index = Array.BinarySearch(keys_, key.Value);
            return index < 0 ? -1 : index;
        }

        /// <summary>
        /// The leaves sharing the face of the given leaf in the given direction.
        /// <para>
        /// With the tree balanced there is either one neighbour of the same depth, one coarser by a
        /// single level, or four finer by a single level.
        /// </para>
        /// </summary>
        /// <param name="leaf"></param>
        /// <param name="axis">0 for north, 1 for east, 2 for vertical</param>
        /// <param name="step">-1 or 1</param>
        /// <param name="neighbours">receives the leaf indices, appended</param>
        public void GetFaceNeighbours(int leaf, int axis, int step, List<int> neighbours)
        {
            OctreeKey key = new OctreeKey(keys_[leaf]);
            OctreeKey? across = key.GetNeighbour(axis, step);
            if (across == null)
            {
                return;
            }
            OctreeKey target = across.Value;

            // leaves inside the neighbouring cell, if it has been divided
            ulong start = target.NormalizedCode;
            ulong end = start + target.CodeSpan;
            int first = LowerBound(start);
            bool any = false;
            for (int index = first; index < keys_.Length; index++)
            {
                OctreeKey candidate = new OctreeKey(keys_[index]);
                if (candidate.NormalizedCode >= end)
                {
                    break;
                }
                if (candidate.Depth < target.Depth)
                {
                    continue;
                }
                any = true;
                if (candidate.Depth == target.Depth || TouchesFace(candidate, target, axis, step))
                {
                    neighbours.Add(index);
                }
            }
            if (any)
            {
                return;
            }
            // otherwise the neighbour is coarser: walk up until a leaf is found
            OctreeKey ancestor = target;
            while (ancestor.Depth > 0)
            {
                ancestor = ancestor.Parent;
                int index = IndexOf(ancestor);
                if (index >= 0)
                {
                    neighbours.Add(index);
                    return;
                }
            }
        }

        /// <summary>
        /// whether a cell inside <paramref name="parent"/> touches the face of the parent facing back
        /// toward the cell the neighbour was asked for
        /// </summary>
        private static bool TouchesFace(OctreeKey candidate, OctreeKey parent, int axis, int step)
        {
            candidate.GetCoordinates(out int i, out int j, out int k);
            parent.GetCoordinates(out int pi, out int pj, out int pk);
            int shift = candidate.Depth - parent.Depth;
            int c = axis == 0 ? i : axis == 1 ? j : k;
            int p = axis == 0 ? pi : axis == 1 ? pj : pk;
            // the face of the neighbour that is shared is the one on the side the step came from
            int wanted = step > 0 ? (p << shift) : (((p + 1) << shift) - 1);
            return c == wanted;
        }

        private int LowerBound(ulong code)
        {
            int low = 0;
            int high = keys_.Length;
            while (low < high)
            {
                int middle = (int)(((uint)low + (uint)high) >> 1);
                if ((keys_[middle] >> 5) < code)
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle;
                }
            }
            return low;
        }

        /// <summary>
        /// Builds an octree over the part of the frame covered by the given box, dividing a cell whenever
        /// <paramref name="refine"/> says so and never going past <paramref name="deepestDepth"/>.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="domainMinimum">the box actually of interest inside the root cube</param>
        /// <param name="domainMaximum"></param>
        /// <param name="baseDepth">depth of the coarsest cells</param>
        /// <param name="deepestDepth">depth beyond which no cell is divided</param>
        /// <param name="refine"></param>
        /// <param name="balance">whether to enforce that face neighbours differ by at most one level</param>
        /// <returns></returns>
        public static LinearOctree Build(OctreeFrame frame, double[] domainMinimum, double[] domainMaximum,
                                         int baseDepth, int deepestDepth, OctreeRefinementRule refine,
                                         bool balance = true)
        {
            if (baseDepth < 0 || baseDepth > OctreeKey.MaximumDepth)
            {
                throw new ArgumentOutOfRangeException(nameof(baseDepth));
            }
            if (deepestDepth < baseDepth || deepestDepth > OctreeKey.MaximumDepth)
            {
                throw new ArgumentOutOfRangeException(nameof(deepestDepth));
            }
            if (refine == null)
            {
                throw new ArgumentNullException(nameof(refine));
            }

            List<OctreeKey> current = new List<OctreeKey>();
            double baseSize = frame.GetCellSize(baseDepth);
            int extent = 1 << baseDepth;
            int[] from = new int[3];
            int[] to = new int[3];
            double[] origin = { frame.OriginNorth, frame.OriginEast, frame.OriginVertical };
            for (int a = 0; a < 3; a++)
            {
                from[a] = System.Math.Max(0, (int)System.Math.Floor((domainMinimum[a] - origin[a]) / baseSize));
                to[a] = System.Math.Min(extent - 1, (int)System.Math.Floor((domainMaximum[a] - origin[a]) / baseSize));
            }
            for (int i = from[0]; i <= to[0]; i++)
            {
                for (int j = from[1]; j <= to[1]; j++)
                {
                    for (int k = from[2]; k <= to[2]; k++)
                    {
                        current.Add(OctreeKey.Create(i, j, k, baseDepth));
                    }
                }
            }

            List<ulong> leaves = new List<ulong>(current.Count);
            for (int depth = baseDepth; depth <= deepestDepth && current.Count > 0; depth++)
            {
                List<OctreeKey> next = new List<OctreeKey>();
                foreach (OctreeKey key in current)
                {
                    OctreeCell cell = frame.GetCell(key);
                    if (depth < deepestDepth && refine(in cell))
                    {
                        for (int child = 0; child < 8; child++)
                        {
                            next.Add(key.GetChild(child));
                        }
                    }
                    else
                    {
                        leaves.Add(key.Value);
                    }
                }
                current = next;
            }

            ulong[] keys = leaves.ToArray();
            Array.Sort(keys);
            LinearOctree tree = new LinearOctree(frame, baseDepth, deepestDepth, keys);
            return balance ? tree.Balanced() : tree;
        }

        /// <summary>
        /// The same octree with no two face neighbours differing by more than one level.
        /// <para>
        /// Without that, a cell can face a neighbour several levels coarser, the flux stencil across the
        /// interface grows without bound and the tracer has no simple rule for which sub-face a
        /// streamline is heading into.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public LinearOctree Balanced()
        {
            HashSet<ulong> set = new HashSet<ulong>(keys_);
            Queue<ulong> pending = new Queue<ulong>(keys_);
            while (pending.Count > 0)
            {
                OctreeKey key = new OctreeKey(pending.Dequeue());
                if (!set.Contains(key.Value))
                {
                    continue;
                }
                int depth = key.Depth;
                if (depth < 2)
                {
                    continue;
                }
                for (int axis = 0; axis < 3; axis++)
                {
                    for (int step = -1; step <= 1; step += 2)
                    {
                        OctreeKey? across = key.GetNeighbour(axis, step);
                        if (across == null)
                        {
                            continue;
                        }
                        OctreeKey ancestor = across.Value;
                        bool found = set.Contains(ancestor.Value);
                        while (!found && ancestor.Depth > 0)
                        {
                            ancestor = ancestor.Parent;
                            found = set.Contains(ancestor.Value);
                        }
                        if (!found || ancestor.Depth >= depth - 1)
                        {
                            continue;
                        }
                        set.Remove(ancestor.Value);
                        for (int child = 0; child < 8; child++)
                        {
                            OctreeKey divided = ancestor.GetChild(child);
                            set.Add(divided.Value);
                            pending.Enqueue(divided.Value);
                        }
                    }
                }
            }
            ulong[] keys = new ulong[set.Count];
            set.CopyTo(keys);
            Array.Sort(keys);
            return new LinearOctree(Frame, BaseDepth, DeepestDepth, keys);
        }
    }
}
