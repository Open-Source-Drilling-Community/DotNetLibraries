using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// An ordered suite of positions describing the path of a single streamline.
    /// The positions are interpreted as the vertices of a piecewise linear curve, in flow order.
    /// </summary>
    [Serializable]
    public class Streamline
    {
        /// <summary>
        /// optional unique identifier of the streamline
        /// </summary>
        public Guid? ID { get; set; } = null;

        /// <summary>
        /// optional name of the streamline
        /// </summary>
        public string? Name { get; set; } = null;

        /// <summary>
        /// the ordered positions of the streamline, from its origin to its termination
        /// </summary>
        public List<Point3D>? Positions { get; set; } = null;

        /// <summary>
        /// default constructor
        /// </summary>
        public Streamline()
        {
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="cmp"></param>
        public Streamline(Streamline cmp)
        {
            if (cmp != null)
            {
                ID = cmp.ID;
                Name = cmp.Name;
                if (cmp.Positions != null)
                {
                    Positions = new List<Point3D>(cmp.Positions.Count);
                    foreach (Point3D pt in cmp.Positions)
                    {
                        Positions.Add(new Point3D(pt));
                    }
                }
            }
        }

        /// <summary>
        /// constructor with initialization from an ordered set of positions
        /// </summary>
        /// <param name="positions"></param>
        public Streamline(IEnumerable<Point3D> positions)
        {
            if (positions != null)
            {
                Positions = new List<Point3D>(positions);
            }
        }

        /// <summary>
        /// the number of positions in the streamline
        /// </summary>
        public int Count
        {
            get
            {
                return Positions == null ? 0 : Positions.Count;
            }
        }

        /// <summary>
        /// the curvilinear length of the streamline, or null if it cannot be determined
        /// </summary>
        /// <returns></returns>
        public double? GetLength()
        {
            if (Positions == null || Positions.Count < 2)
            {
                return null;
            }
            double length = 0;
            Point3D? previous = null;
            foreach (Point3D pt in Positions)
            {
                if (pt == null || pt.X == null || pt.Y == null || pt.Z == null)
                {
                    continue;
                }
                if (previous != null)
                {
                    double dx = pt.X.Value - previous.X!.Value;
                    double dy = pt.Y.Value - previous.Y!.Value;
                    double dz = pt.Z.Value - previous.Z!.Value;
                    length += System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
                }
                previous = pt;
            }
            return previous == null ? null : length;
        }
    }
}
