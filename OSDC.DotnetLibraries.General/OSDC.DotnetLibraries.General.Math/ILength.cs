using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface ILength
    {
        /// <summary>
        /// Get length (it is more efficient to use GetLength2() for comparison of lengths)
        /// </summary>
        /// <returns></returns>
        double? GetLength();

        /// <summary>
        /// Get squared length, efficient to use for comparison of lengths when true length is not needed.
        /// </summary>
        /// <returns></returns>
        double? GetLength2();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        void SetLength(double a);
    }
}
