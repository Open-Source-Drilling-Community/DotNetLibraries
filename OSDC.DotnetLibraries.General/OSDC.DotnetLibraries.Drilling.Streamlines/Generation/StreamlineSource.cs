using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// Where a planned well starts and which way it leaves: a slot, or a tie-in on an existing trajectory
    /// that is about to be sidetracked.
    /// <para>
    /// The direction is a hard constraint rather than a preference. A tie-in also names the well it leaves
    /// from, because that well has to stop being an obstacle over a short interval around the window or
    /// the planned well would have nowhere to start.
    /// </para>
    /// </summary>
    public class StreamlineSource
    {
        /// <summary>
        /// optional name, carried through to the streamlines produced
        /// </summary>
        public string? Name { get; set; } = null;

        /// <summary>
        /// where the planned well starts
        /// </summary>
        public Point3D? Position { get; set; } = null;

        /// <summary>
        /// the direction it leaves in. Need not be a unit vector.
        /// </summary>
        public Vector3D? Direction { get; set; } = null;

        /// <summary>
        /// How far this source holds its direction, m, or null to use the generation's own setting.
        /// <para>
        /// A slot holds vertical over the conductor, while a sidetrack needs the two or three stands it
        /// takes to clear its parent, so the two do not share a length.
        /// </para>
        /// </summary>
        public double? ConduitLength { get; set; } = null;

        /// <summary>
        /// the index, in the obstacle field, of the well this is a tie-in on, or -1 for a slot that
        /// belongs to no existing well
        /// </summary>
        public int ParentWell { get; set; } = -1;

        /// <summary>
        /// the measured depth along the parent at which the sidetrack leaves it
        /// </summary>
        public double ParentMeasuredDepth { get; set; } = 0;

        /// <summary>
        /// How much of the parent, either side of the tie-in, stops being an obstacle.
        /// <para>
        /// This exists only so that the start of the planned well is not inside a solid. It is not the
        /// separation a sidetrack needs from its parent, which is an outcome of the generation and is
        /// measured afterwards rather than imposed here.
        /// </para>
        /// </summary>
        public double ParentMutedLength { get; set; } = 30.0;

        /// <summary>
        /// default constructor
        /// </summary>
        public StreamlineSource()
        {
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        public StreamlineSource(Point3D position, Vector3D direction)
        {
            Position = position;
            Direction = direction;
        }

        /// <summary>
        /// whether the source can be used
        /// </summary>
        public bool IsValid()
        {
            return Position != null && Position.X != null && Position.Y != null && Position.Z != null
                   && Direction != null && Direction.X != null && Direction.Y != null && Direction.Z != null
                   && (Direction.GetLength() ?? 0) > 0;
        }

        /// <summary>
        /// the direction as a unit vector
        /// </summary>
        public void GetUnitDirection(out double north, out double east, out double vertical)
        {
            north = Direction!.X!.Value;
            east = Direction.Y!.Value;
            vertical = Direction.Z!.Value;
            double length = System.Math.Sqrt(north * north + east * east + vertical * vertical);
            if (length > 0)
            {
                north /= length;
                east /= length;
                vertical /= length;
            }
        }
    }
}
