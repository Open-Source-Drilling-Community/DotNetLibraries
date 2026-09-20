namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// The address of one cell of an octree, packed into a single unsigned long.
    /// <para>
    /// The cell coordinates are interleaved into a Morton code taken at <see cref="MaximumDepth"/>
    /// whatever the cell's own depth, and the depth is carried in the low five bits. Normalising the
    /// code to the deepest level like this is what makes the ordering useful: sorting the keys puts
    /// every cell immediately before its own descendants and puts spatially close cells close together,
    /// so a whole subtree is a contiguous range and the leaf containing a position is found by one
    /// binary search.
    /// </para>
    /// <para>
    /// Nineteen levels of three bits each leave fifty seven bits of code and five of depth, so the key
    /// is 62 bits. A root cube of a kilometre therefore addresses cells down to two millimetres.
    /// </para>
    /// </summary>
    public readonly struct OctreeKey : IEquatable<OctreeKey>, IComparable<OctreeKey>
    {
        /// <summary>
        /// the deepest level the packing can address
        /// </summary>
        public const int MaximumDepth = 19;

        private const int DepthBits = 5;
        private const ulong DepthMask = (1UL << DepthBits) - 1UL;

        /// <summary>
        /// the packed value, ordered so that sorting gives depth first order within a subtree
        /// </summary>
        public ulong Value { get; }

        /// <summary>
        /// constructor from an already packed value
        /// </summary>
        /// <param name="value"></param>
        public OctreeKey(ulong value)
        {
            Value = value;
        }

        /// <summary>
        /// the cell of the given depth holding the given cell coordinates
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static OctreeKey Create(int i, int j, int k, int depth)
        {
            if (depth < 0 || depth > MaximumDepth)
            {
                throw new ArgumentOutOfRangeException(nameof(depth));
            }
            int extent = 1 << depth;
            if (i < 0 || j < 0 || k < 0 || i >= extent || j >= extent || k >= extent)
            {
                throw new ArgumentOutOfRangeException(nameof(i), $"({i},{j},{k}) is outside depth {depth}");
            }
            int shift = MaximumDepth - depth;
            ulong morton = Spread((ulong)(uint)i << shift)
                           | (Spread((ulong)(uint)j << shift) << 1)
                           | (Spread((ulong)(uint)k << shift) << 2);
            return new OctreeKey((morton << DepthBits) | (uint)depth);
        }

        /// <summary>
        /// the depth of the cell
        /// </summary>
        public int Depth
        {
            get
            {
                return (int)(Value & DepthMask);
            }
        }

        /// <summary>
        /// the Morton code of the cell, taken at <see cref="MaximumDepth"/>
        /// </summary>
        public ulong NormalizedCode
        {
            get
            {
                return Value >> DepthBits;
            }
        }

        /// <summary>
        /// the cell coordinates at the cell's own depth
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        public void GetCoordinates(out int i, out int j, out int k)
        {
            ulong morton = NormalizedCode;
            int shift = MaximumDepth - Depth;
            i = (int)(Compact(morton) >> shift);
            j = (int)(Compact(morton >> 1) >> shift);
            k = (int)(Compact(morton >> 2) >> shift);
        }

        /// <summary>
        /// the number of cells along one edge of the octree at the cell's depth
        /// </summary>
        public int Extent
        {
            get
            {
                return 1 << Depth;
            }
        }

        /// <summary>
        /// how much of the normalized code one cell of this depth spans
        /// </summary>
        public ulong CodeSpan
        {
            get
            {
                return 1UL << (3 * (MaximumDepth - Depth));
            }
        }

        /// <summary>
        /// the cell one level up, or itself when already at the root
        /// </summary>
        public OctreeKey Parent
        {
            get
            {
                int depth = Depth;
                if (depth == 0)
                {
                    return this;
                }
                GetCoordinates(out int i, out int j, out int k);
                return Create(i >> 1, j >> 1, k >> 1, depth - 1);
            }
        }

        /// <summary>
        /// the ancestor at the given depth
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public OctreeKey GetAncestor(int depth)
        {
            int own = Depth;
            if (depth >= own)
            {
                return this;
            }
            GetCoordinates(out int i, out int j, out int k);
            int shift = own - depth;
            return Create(i >> shift, j >> shift, k >> shift, depth);
        }

        /// <summary>
        /// one of the eight children, indexed by the bits of <paramref name="child"/> as i, j then k
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public OctreeKey GetChild(int child)
        {
            GetCoordinates(out int i, out int j, out int k);
            return Create(2 * i + (child & 1), 2 * j + ((child >> 1) & 1), 2 * k + ((child >> 2) & 1),
                          Depth + 1);
        }

        /// <summary>
        /// the neighbour of the same depth one step along the given axis, or null when it would fall
        /// outside the octree
        /// </summary>
        /// <param name="axis">0 for i, 1 for j, 2 for k</param>
        /// <param name="step">either -1 or 1</param>
        /// <returns></returns>
        public OctreeKey? GetNeighbour(int axis, int step)
        {
            GetCoordinates(out int i, out int j, out int k);
            switch (axis)
            {
                case 0: i += step; break;
                case 1: j += step; break;
                default: k += step; break;
            }
            int extent = Extent;
            if (i < 0 || j < 0 || k < 0 || i >= extent || j >= extent || k >= extent)
            {
                return null;
            }
            return Create(i, j, k, Depth);
        }

        /// <summary>
        /// whether the given cell is this cell or lies inside it
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Contains(OctreeKey other)
        {
            if (other.Depth < Depth)
            {
                return false;
            }
            ulong start = NormalizedCode;
            return other.NormalizedCode >= start && other.NormalizedCode < start + CodeSpan;
        }

        /// <inheritdoc/>
        public bool Equals(OctreeKey other)
        {
            return Value == other.Value;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is OctreeKey other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <inheritdoc/>
        public int CompareTo(OctreeKey other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            GetCoordinates(out int i, out int j, out int k);
            return $"({i},{j},{k})@{Depth}";
        }

        /// <summary>
        /// spreads the low bits of a value so that they occupy every third bit
        /// </summary>
        private static ulong Spread(ulong value)
        {
            value &= 0x1fffffUL;
            value = (value | (value << 32)) & 0x1f00000000ffffUL;
            value = (value | (value << 16)) & 0x1f0000ff0000ffUL;
            value = (value | (value << 8)) & 0x100f00f00f00f00fUL;
            value = (value | (value << 4)) & 0x10c30c30c30c30c3UL;
            value = (value | (value << 2)) & 0x1249249249249249UL;
            return value;
        }

        /// <summary>
        /// gathers every third bit of a value back into contiguous low bits
        /// </summary>
        private static ulong Compact(ulong value)
        {
            value &= 0x1249249249249249UL;
            value = (value ^ (value >> 2)) & 0x10c30c30c30c30c3UL;
            value = (value ^ (value >> 4)) & 0x100f00f00f00f00fUL;
            value = (value ^ (value >> 8)) & 0x1f0000ff0000ffUL;
            value = (value ^ (value >> 16)) & 0x1f00000000ffffUL;
            value = (value ^ (value >> 32)) & 0x1fffffUL;
            return value;
        }
    }
}
