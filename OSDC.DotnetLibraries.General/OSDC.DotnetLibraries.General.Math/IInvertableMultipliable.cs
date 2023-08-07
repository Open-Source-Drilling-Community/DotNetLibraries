using System;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IInvertableMultipliable<T> : IMultipliable<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        T Invert();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        void Invert(ref T a);

        /// <summary>
        /// 
        /// </summary>
        void InvertAssign();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        T Divide(T a);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void Divide(T a, ref T b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        void DivideAssign(T a);

    }
}
