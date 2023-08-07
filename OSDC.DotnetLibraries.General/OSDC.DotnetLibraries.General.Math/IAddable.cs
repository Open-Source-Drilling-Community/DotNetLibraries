using System;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IAddable<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        T Add(T a);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        void AddAssign(T a);
   
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        T Substract(T a);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        void SubstractAssign(T a);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        T Negate();

        /// <summary>
        /// 
        /// </summary>
        void NegateAssign();

    }
}
