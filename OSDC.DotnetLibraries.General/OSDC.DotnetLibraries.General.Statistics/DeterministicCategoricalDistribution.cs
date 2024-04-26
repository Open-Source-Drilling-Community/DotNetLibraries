using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class DeterministicCategoricalDistribution : DiscreteDistribution
    {
        public uint? NumberOfStates { get; protected set; }

        public uint? State { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DeterministicCategoricalDistribution()
        {
            NumberOfStates = 2;
        }

        public DeterministicCategoricalDistribution(uint? numberOfStates)
        {
            NumberOfStates = numberOfStates;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(DeterministicCategoricalDistribution? cmp)
        {
            bool eq = base.Equals(cmp);
            if (cmp != null)
            {
                eq &= NumberOfStates == cmp.NumberOfStates;
            }
            return eq;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        public void CopyTo(DeterministicCategoricalDistribution? dest)
        {
            base.CopyTo(dest);
            if (dest != null)
            {
                dest.NumberOfStates = NumberOfStates;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public override double? GetProbability(int state)
        {
            if (state == State)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetCumulativeProbability(int target)
        {
            return target < 0 ? 0 : 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int? Realize()
        {
            return (State == null) ? null : (int)State.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Tuple<double, double>[]? GetCurve()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            return State;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetStandardDeviation()
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return NumberOfStates != null && NumberOfStates > 0;
        }
    }
}
