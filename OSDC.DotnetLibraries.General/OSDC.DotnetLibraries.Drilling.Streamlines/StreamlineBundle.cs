namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// A set of streamlines that travel together: at every cross-section where two of them are both
    /// present, they belong to the same locally connected group of streamlines.
    /// <para>
    /// A streamline that travels with no other one is a bundle of one.
    /// </para>
    /// </summary>
    public class StreamlineBundle
    {
        /// <summary>
        /// the rank of the bundle in <see cref="StreamlineBundlingResult.Bundles"/>
        /// </summary>
        public int Index { get; internal set; } = -1;

        /// <summary>
        /// the indices, in the source, of the streamlines belonging to the bundle, in increasing order
        /// </summary>
        public List<int> StreamlineIndices { get; internal set; } = new List<int>();

        /// <summary>
        /// the identifiers of the streamlines belonging to the bundle, in the same order as
        /// <see cref="StreamlineIndices"/>. An entry is null when the source gave no identifier.
        /// </summary>
        public List<Guid?> StreamlineIDs { get; internal set; } = new List<Guid?>();

        /// <summary>
        /// the number of streamlines in the bundle
        /// </summary>
        public int Count
        {
            get
            {
                return StreamlineIndices.Count;
            }
        }

        /// <summary>
        /// whether the bundle holds a single streamline
        /// </summary>
        public bool IsSingleton
        {
            get
            {
                return StreamlineIndices.Count == 1;
            }
        }
    }
}
