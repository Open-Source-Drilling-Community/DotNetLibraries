namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// One cell of an octree, with its address and its geometry resolved. Handed to the predicates that
    /// decide whether a cell should be refined or whether it is blocked.
    /// </summary>
    public readonly struct OctreeCell
    {
        /// <summary>
        /// the address of the cell
        /// </summary>
        public OctreeKey Key { get; }

        /// <summary>
        /// the lowest corner of the cell
        /// </summary>
        public double MinimumNorth { get; }

        /// <summary>
        /// the lowest corner of the cell
        /// </summary>
        public double MinimumEast { get; }

        /// <summary>
        /// the lowest corner of the cell, vertical positive downward
        /// </summary>
        public double MinimumVertical { get; }

        /// <summary>
        /// the edge of the cell
        /// </summary>
        public double Size { get; }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        public OctreeCell(OctreeKey key, double minimumNorth, double minimumEast,
                          double minimumVertical, double size)
        {
            Key = key;
            MinimumNorth = minimumNorth;
            MinimumEast = minimumEast;
            MinimumVertical = minimumVertical;
            Size = size;
        }

        /// <summary>
        /// the depth of the cell in the octree
        /// </summary>
        public int Depth
        {
            get
            {
                return Key.Depth;
            }
        }

        /// <summary>
        /// the middle of the cell
        /// </summary>
        public double CentreNorth
        {
            get
            {
                return MinimumNorth + 0.5 * Size;
            }
        }

        /// <summary>
        /// the middle of the cell
        /// </summary>
        public double CentreEast
        {
            get
            {
                return MinimumEast + 0.5 * Size;
            }
        }

        /// <summary>
        /// the middle of the cell
        /// </summary>
        public double CentreVertical
        {
            get
            {
                return MinimumVertical + 0.5 * Size;
            }
        }

        /// <summary>
        /// the radius of the sphere through the corners of the cell. A cell touches a solid whenever its
        /// centre is within this of the solid, which is what makes a conservative classification possible
        /// from the centre alone.
        /// </summary>
        public double CircumscribedRadius
        {
            get
            {
                return 0.5 * System.Math.Sqrt(3.0) * Size;
            }
        }

        /// <summary>
        /// whether the cell overlaps the given box
        /// </summary>
        public bool Intersects(double[] minimum, double[] maximum)
        {
            return MinimumNorth < maximum[0] && MinimumNorth + Size > minimum[0]
                && MinimumEast < maximum[1] && MinimumEast + Size > minimum[1]
                && MinimumVertical < maximum[2] && MinimumVertical + Size > minimum[2];
        }

        /// <summary>
        /// whether the given position lies in the cell
        /// </summary>
        public bool Contains(double north, double east, double vertical)
        {
            return north >= MinimumNorth && north < MinimumNorth + Size
                && east >= MinimumEast && east < MinimumEast + Size
                && vertical >= MinimumVertical && vertical < MinimumVertical + Size;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Key} size {Size:G4} at ({CentreNorth:F2},{CentreEast:F2},{CentreVertical:F2})";
        }
    }
}
