using System;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IProductable<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        T Time(T a);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void Time(T a, ref T b);
    }
}
