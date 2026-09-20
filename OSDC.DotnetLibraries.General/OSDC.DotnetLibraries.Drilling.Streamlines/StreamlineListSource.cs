using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// An <see cref="IStreamlineSource"/> backed by an in-memory list of <see cref="Streamline"/>.
    /// <para>
    /// Convenient for tests and for moderate problem sizes. At the upper end of the intended range a
    /// <see cref="Point3D"/> per position costs of the order of 70 bytes, so a source reading directly
    /// from files should be preferred once the total number of positions exceeds a few tens of millions.
    /// </para>
    /// </summary>
    public class StreamlineListSource : IStreamlineSource
    {
        private readonly IReadOnlyList<Streamline> streamlines_;

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="streamlines"></param>
        public StreamlineListSource(IReadOnlyList<Streamline> streamlines)
        {
            streamlines_ = streamlines ?? throw new ArgumentNullException(nameof(streamlines));
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                return streamlines_.Count;
            }
        }

        /// <inheritdoc/>
        public Guid? GetID(int index)
        {
            return streamlines_[index].ID;
        }

        /// <inheritdoc/>
        public string? GetName(int index)
        {
            return streamlines_[index].Name;
        }

        /// <inheritdoc/>
        public IEnumerable<(double X, double Y, double Z)> GetPositions(int index)
        {
            List<Point3D>? positions = streamlines_[index].Positions;
            if (positions == null)
            {
                yield break;
            }
            foreach (Point3D pt in positions)
            {
                if (pt != null && pt.X != null && pt.Y != null && pt.Z != null)
                {
                    yield return (pt.X.Value, pt.Y.Value, pt.Z.Value);
                }
            }
        }
    }
}
