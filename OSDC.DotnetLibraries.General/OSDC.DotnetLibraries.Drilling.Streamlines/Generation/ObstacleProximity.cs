namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// What the obstacles look like from one position: how far outside the nearest two of them it lies,
    /// and how wide the passage between those two is.
    /// </summary>
    public readonly struct ObstacleProximity
    {
        /// <summary>
        /// how far outside the nearest obstacle the position lies, negative when inside it, positive
        /// infinity when there is none within reach
        /// </summary>
        public double NearestExcess { get; }

        /// <summary>
        /// which well the nearest obstacle belongs to, or -1
        /// </summary>
        public int NearestWell { get; }

        /// <summary>
        /// the semi-minor axis of the nearest obstacle where it was met. The scale on which that solid
        /// changes shape, and so how finely its surface is worth resolving.
        /// </summary>
        public double NearestSemiMinorAxis { get; }

        /// <summary>
        /// how far outside the nearest obstacle of a different well the position lies
        /// </summary>
        public double SecondExcess { get; }

        /// <summary>
        /// which well that is, or -1
        /// </summary>
        public int SecondWell { get; }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        public ObstacleProximity(double nearestExcess, int nearestWell, double nearestSemiMinorAxis,
                                 double secondExcess, int secondWell)
        {
            NearestExcess = nearestExcess;
            NearestWell = nearestWell;
            NearestSemiMinorAxis = nearestSemiMinorAxis;
            SecondExcess = secondExcess;
            SecondWell = secondWell;
        }

        /// <summary>
        /// whether any obstacle was found within reach
        /// </summary>
        public bool HasObstacle
        {
            get
            {
                return NearestWell >= 0 && !double.IsPositiveInfinity(NearestExcess);
            }
        }

        /// <summary>
        /// The width of the passage here: the room outside the nearest obstacle plus the room outside the
        /// next nearest one belonging to a different well. This is what the refinement of the octree is
        /// driven by, because it is the gap a planned well has to be threaded through.
        /// </summary>
        public double Clearance
        {
            get
            {
                double first = NearestExcess > 0 ? NearestExcess : 0;
                double second = SecondExcess > 0 ? SecondExcess : 0;
                return first + second;
            }
        }
    }
}
