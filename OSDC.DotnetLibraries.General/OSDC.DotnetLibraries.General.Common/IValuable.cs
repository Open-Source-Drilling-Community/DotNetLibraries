
namespace OSDC.DotnetLibraries.General.Common
{
    public interface IValuable
    {
        /// <summary>
        /// Interface for objects which can behave like a function f: R --> R
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        double Eval(double x);
    }
}
