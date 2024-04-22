using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class CategoricalDistribution : MultinomialDistribution
    {
        /// <summary>
        /// 
        /// </summary>
        public CategoricalDistribution() : base()
        {
            NumberTrials = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prob"></param>
        public CategoricalDistribution(uint? k, double[]? prob) : base(1, k, prob)
        {
        }
    }
}
