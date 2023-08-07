using System;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IMultipliable<T> : IProductable<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        void TimeAssign(T a);
    }
}
