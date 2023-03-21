namespace OSDC.DotnetLibraries.General.Common
{
    /// <summary>
    /// an interface for things that can be copied into another object of the same class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICopyable<T>
    {
        /// <summary>
        /// copy from this to target
        /// </summary>
        /// <param name="target"></param>
        void Copy(ref T target);
    }
}
