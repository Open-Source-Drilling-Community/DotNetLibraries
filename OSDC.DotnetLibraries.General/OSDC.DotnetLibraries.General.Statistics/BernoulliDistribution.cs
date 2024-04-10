using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Statistics
{
    /// <summary>
    /// Implements a Bernoulli distribution.
    /// Such a distribution returns 0 with probability probability_
    /// and 1 with probability 1-probability_ (it is thus nothing else
    ///  than flipping a biased coin).
    public class BernoulliDistribution : BinomialDistribution
    {
        /// <summary>
        /// 
        /// </summary>
        public BernoulliDistribution() : base()
        {
            NumberTrials = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prob"></param>
        public BernoulliDistribution(double? prob) : base(1, prob)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int? Realize()
        {
            double uniform = RandomGenerator.Instance.NextDouble();
            if (uniform < probability_)
            {
                return 0;
            }
            else
            { 
                return 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DiscreteDistribution Clone()
        {
            return new BernoulliDistribution(probability_);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public override void Copy(DiscreteDistribution from)
        {
            if (from is BernoulliDistribution)
            {
                probability_ = ((BernoulliDistribution)from).probability_;
            }
        }
    }
}
