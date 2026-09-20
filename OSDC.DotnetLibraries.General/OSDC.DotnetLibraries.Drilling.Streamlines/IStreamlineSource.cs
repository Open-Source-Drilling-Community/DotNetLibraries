namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// A source of streamlines for the bundler.
    /// <para>
    /// The bundler never materializes the complete point set: it makes two sequential passes over the
    /// source and only retains, per streamline, the crossings of its own sweep planes. A source that
    /// reads from files should therefore stream, not cache. <see cref="GetPositions"/> may be called
    /// more than once for the same streamline and must yield the same positions each time.
    /// </para>
    /// </summary>
    public interface IStreamlineSource
    {
        /// <summary>
        /// the number of streamlines in the source
        /// </summary>
        int Count { get; }

        /// <summary>
        /// the optional identifier of the streamline at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Guid? GetID(int index);

        /// <summary>
        /// the optional name of the streamline at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string? GetName(int index);

        /// <summary>
        /// the ordered positions of the streamline at the given index, in flow order
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IEnumerable<(double X, double Y, double Z)> GetPositions(int index);
    }
}
