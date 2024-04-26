using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class DeterministicBernoulliDistribution : DeterministicCategoricalDistribution
    {
        /// <summary>
        /// 
        /// </summary>
        public DeterministicBernoulliDistribution()
        {
            NumberOfStates = 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return NumberOfStates == 2;
        }
    }
}
