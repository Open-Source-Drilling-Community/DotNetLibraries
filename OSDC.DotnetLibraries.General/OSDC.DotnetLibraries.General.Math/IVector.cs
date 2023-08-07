using System;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IVector : ILength
    {
        /// <summary>
        /// 
        /// </summary>
        int Dim { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        double? this[int index] { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        void CopyTo(double?[] a);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="v"></param>
        void CopyTo(int start, IVector v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        void CopyFrom(double[] a);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="v"></param>
        void CopyFrom(int start, IVector v);
    }
}
