namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// Whatever a well may not enter.
    /// <para>
    /// Today that is the position uncertainty of the wells already there, and the interface is deliberately
    /// narrower than the field that supplies it: a fault, a geobody or a lease boundary answers the same
    /// question, and a tolerance region is built from the answer rather than from what kind of thing gave
    /// it.
    /// </para>
    /// </summary>
    public interface IForbiddenZoneField
    {
        /// <summary>
        /// How far outside the nearest forbidden zone the given position lies: negative when the position
        /// is inside one, and positive infinity when none is within <paramref name="searchLimit"/>.
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <param name="searchLimit">how far to look before giving up, m</param>
        /// <returns></returns>
        double GetClearance(double north, double east, double vertical, double searchLimit);
    }
}
