using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public interface IDistribution
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Tuple<double, double>[]? GetCurve();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double? GetMean();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double? GetStandardDeviation();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IsValid();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double? GetMostLikely();
    }
}
