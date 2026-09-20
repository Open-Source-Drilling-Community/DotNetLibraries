namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// Several media laid over one another, multiplied face by face.
    /// <para>
    /// A channel and a guide do different jobs and the medium wants both: the channel says where the
    /// cheap route is and leaves the flow isotropic within it, while a guide says which way is cheap
    /// somewhere small and particular. Multiplying is what keeps them separable — the channel raises the
    /// level and the guide biases the direction within whatever level it finds, so neither has to know
    /// about the other.
    /// </para>
    /// </summary>
    public class CombinedMobilityField : IFaceMobility
    {
        private readonly IFaceMobility[] parts_;
        private readonly double[] scratch_ = new double[3];

        /// <summary>
        /// how many media are laid over one another
        /// </summary>
        public int Count
        {
            get
            {
                return parts_.Length;
            }
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="parts">the media to lay over one another; nulls are left out</param>
        public CombinedMobilityField(IEnumerable<IFaceMobility?> parts)
        {
            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }
            List<IFaceMobility> kept = new List<IFaceMobility>();
            foreach (IFaceMobility? part in parts)
            {
                if (part != null)
                {
                    kept.Add(part);
                }
            }
            if (kept.Count == 0)
            {
                throw new ArgumentException("nothing to combine", nameof(parts));
            }
            parts_ = kept.ToArray();
        }

        /// <summary>
        /// the three face values at a position
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <param name="values"></param>
        public void GetValues(double north, double east, double vertical, double[] values)
        {
            if (values == null || values.Length < 3)
            {
                throw new ArgumentException("three values are needed", nameof(values));
            }
            values[0] = 1.0;
            values[1] = 1.0;
            values[2] = 1.0;
            foreach (IFaceMobility part in parts_)
            {
                part.GetValues(north, east, vertical, scratch_);
                values[0] *= scratch_[0];
                values[1] *= scratch_[1];
                values[2] *= scratch_[2];
            }
        }
    }
}
