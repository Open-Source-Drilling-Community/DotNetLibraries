using OSDC.DotnetLibraries.Drilling.Streamlines;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// A large raft of vertical streamlines that parts into four quadrants below an obstruction, produced
    /// on the fly rather than stored.
    /// <para>
    /// Nothing is held in memory: the positions are recomputed on every pass. This is how a source
    /// reading from files is meant to behave, and it lets the bundler be exercised at sizes where holding
    /// the positions would need hundreds of gigabytes.
    /// </para>
    /// </summary>
    internal sealed class SyntheticStreamlineSource : IStreamlineSource
    {
        private readonly int side_;
        private readonly int samples_;
        private readonly double spacing_;
        private readonly double offset_;

        public SyntheticStreamlineSource(int streamlineCount, int samples,
                                         double spacing = 20.0, double offset = 120.0)
        {
            side_ = (int)System.Math.Ceiling(System.Math.Sqrt(streamlineCount));
            Count = streamlineCount;
            samples_ = samples;
            spacing_ = spacing;
            offset_ = offset;
        }

        public int Count { get; }

        public Guid? GetID(int index)
        {
            return null;
        }

        public string? GetName(int index)
        {
            return null;
        }

        /// <summary>
        /// which of the four quadrants the given streamline belongs to, and therefore which bundle it is
        /// expected to end up in
        /// </summary>
        public int GetExpectedQuadrant(int index)
        {
            int i = index / side_;
            int j = index % side_;
            return (i < side_ / 2 ? 0 : 1) * 2 + (j < side_ / 2 ? 0 : 1);
        }

        public IEnumerable<(double X, double Y, double Z)> GetPositions(int index)
        {
            int i = index / side_;
            int j = index % side_;
            double x = i * spacing_;
            double y = j * spacing_;
            double dx = (i < side_ / 2 ? -offset_ : offset_);
            double dy = (j < side_ / 2 ? -offset_ : offset_);
            for (int sample = 0; sample <= samples_; sample++)
            {
                double z = -2000.0 * sample / samples_;
                double ramp = z >= -800.0 ? 0.0 : (z <= -1200.0 ? 1.0 : (-800.0 - z) / 400.0);
                yield return (x + dx * ramp, y + dy * ramp, z);
            }
        }
    }
}
