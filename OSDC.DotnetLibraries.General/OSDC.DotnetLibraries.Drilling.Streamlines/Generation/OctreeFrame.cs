namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// The cube an octree subdivides, and the mapping between positions and cell addresses.
    /// <para>
    /// Positions are north, east and vertical with the vertical positive downward, as everywhere in
    /// OSDC. The frame itself is axis aligned: no rotation is applied, so the octree axes are the
    /// coordinate axes.
    /// </para>
    /// <para>
    /// The root is a cube whose edge is a power of two multiple of the finest cell wanted, so every
    /// level of the tree lands on an exact binary fraction of it and no rounding accumulates down the
    /// levels.
    /// </para>
    /// </summary>
    public readonly struct OctreeFrame
    {
        /// <summary>
        /// the lowest corner of the root cube
        /// </summary>
        public double OriginNorth { get; }

        /// <summary>
        /// the lowest corner of the root cube
        /// </summary>
        public double OriginEast { get; }

        /// <summary>
        /// the lowest corner of the root cube, vertical positive downward
        /// </summary>
        public double OriginVertical { get; }

        /// <summary>
        /// the edge of the root cube
        /// </summary>
        public double RootSize { get; }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="originNorth"></param>
        /// <param name="originEast"></param>
        /// <param name="originVertical"></param>
        /// <param name="rootSize"></param>
        public OctreeFrame(double originNorth, double originEast, double originVertical, double rootSize)
        {
            if (!(rootSize > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(rootSize));
            }
            OriginNorth = originNorth;
            OriginEast = originEast;
            OriginVertical = originVertical;
            RootSize = rootSize;
        }

        /// <summary>
        /// A frame whose root cube covers the given box and whose cells at
        /// <paramref name="depth"/> are exactly <paramref name="finestCellSize"/>.
        /// <para>
        /// The cube is centred on the box so that the padding, which the octree will never refine
        /// because nothing of interest is in it, is spread evenly rather than piled on one side.
        /// </para>
        /// </summary>
        /// <param name="minimum">the lowest corner of the box to cover</param>
        /// <param name="maximum">the highest corner of the box to cover</param>
        /// <param name="finestCellSize">the size wanted at the deepest level that will be used</param>
        /// <param name="depth">receives the depth at which cells have that size</param>
        /// <returns></returns>
        public static OctreeFrame Covering(double[] minimum, double[] maximum,
                                           double finestCellSize, out int depth)
        {
            if (minimum == null || minimum.Length < 3)
            {
                throw new ArgumentException("three coordinates are needed", nameof(minimum));
            }
            if (maximum == null || maximum.Length < 3)
            {
                throw new ArgumentException("three coordinates are needed", nameof(maximum));
            }
            if (!(finestCellSize > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(finestCellSize));
            }
            double extent = 0;
            for (int a = 0; a < 3; a++)
            {
                double span = maximum[a] - minimum[a];
                if (span > extent)
                {
                    extent = span;
                }
            }
            if (!(extent > 0))
            {
                extent = finestCellSize;
            }
            depth = 0;
            double size = finestCellSize;
            while (size < extent && depth < OctreeKey.MaximumDepth)
            {
                size *= 2.0;
                depth++;
            }
            return new OctreeFrame(0.5 * (minimum[0] + maximum[0]) - 0.5 * size,
                                   0.5 * (minimum[1] + maximum[1]) - 0.5 * size,
                                   0.5 * (minimum[2] + maximum[2]) - 0.5 * size,
                                   size);
        }

        /// <summary>
        /// the edge of a cell at the given depth
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public double GetCellSize(int depth)
        {
            return RootSize / (1 << depth);
        }

        /// <summary>
        /// the address of the cell of the given depth holding the given position, or null when the
        /// position falls outside the root cube
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public OctreeKey? GetKey(double north, double east, double vertical, int depth)
        {
            double size = GetCellSize(depth);
            int extent = 1 << depth;
            int i = (int)System.Math.Floor((north - OriginNorth) / size);
            int j = (int)System.Math.Floor((east - OriginEast) / size);
            int k = (int)System.Math.Floor((vertical - OriginVertical) / size);
            if (i < 0 || j < 0 || k < 0 || i >= extent || j >= extent || k >= extent)
            {
                return null;
            }
            return OctreeKey.Create(i, j, k, depth);
        }

        /// <summary>
        /// the cell addressed by the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public OctreeCell GetCell(OctreeKey key)
        {
            key.GetCoordinates(out int i, out int j, out int k);
            double size = GetCellSize(key.Depth);
            return new OctreeCell(key, OriginNorth + i * size, OriginEast + j * size,
                                  OriginVertical + k * size, size);
        }
    }
}
