namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// a generic interface for things that are equal with a numerical precision (IEquatable wording is avoided due to conflict with System.IEquatable<T>)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEquivalent<T>
    {
        bool EQ(T a);
        bool EQ(T a, double precision);
    }
}
