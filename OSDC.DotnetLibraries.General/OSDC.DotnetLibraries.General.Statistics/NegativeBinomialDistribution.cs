using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Statistics
{
    /// <summary>
    /// Implementation of the Negative Binomial Distribution. Number of trials is here understood as number of failures
    /// until the experiment is stopped. Probability is the success probability
    /// </summary>
    public class NegativeBinomialDistribution : BinomialDistribution
    {
        /// <summary>
        /// 
        /// </summary>
        public NegativeBinomialDistribution() : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r">Number of failures until experiment is stopped</param>
        /// <param name="p">Success probability</param>
        public NegativeBinomialDistribution(uint? r, double? p) : base(r, p)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetProbability(int target)
        {
            if (IsValid() && numberTrials_ != null && probability_ != null)
            {
                int k = target;
                int r = (int)numberTrials_.Value;
                double p = probability_.Value;
                return SpecialFunctions.BinomialCoefficient(k + r - 1, k) * System.Math.Pow(1 - p, r) * System.Math.Pow(p, k);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double GetCumulativeProbability(int target)
        {
            if (IsValid() && numberTrials_ != null && probability_ != null)
            {
                int k = target;
                int r = (int)numberTrials_.Value;
                double p = probability_.Value;
                return 1 - SpecialFunctions.IncompleteBetaRegularized(k + 1, r, p);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int? Realize()
        {
            if (numberTrials_ == null || probability_ == null)
            {
                return null;
            }
            if (Numeric.EQ(probability_, 1)) return 0;
            if (Numeric.EQ(probability_, 0)) return (int)numberTrials_.Value;
            int failureCount = 0;
            int r = (int)numberTrials_.Value;
            int trials = 0;
            while (failureCount < r)
            {
                if (RandomGenerator.Instance.NextDouble() < (1 - probability_)) failureCount++;
                trials++;
            }
            return trials;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public override void Copy(DiscreteDistribution from)
        {
            if (from is NegativeBinomialDistribution)
            {
                numberTrials_ = ((NegativeBinomialDistribution)from).numberTrials_;
                probability_ = ((NegativeBinomialDistribution)from).probability_;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DiscreteDistribution Clone()
        {
            return new NegativeBinomialDistribution(numberTrials_, probability_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            return probability_ * numberTrials_ / (1 - probability_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetStandardDeviation()
        {
            if (probability_ == null || numberTrials_ == null)
            {
                return null;
            }
            else
            {
                return System.Math.Sqrt((probability_.Value * numberTrials_.Value) / System.Math.Pow(1 - probability_.Value, 2));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return numberTrials_ != null && 
                probability_ != null && 
                numberTrials_ > 0 && 
                Numeric.IsDefined(probability_) && 
                probability_ >= 0 && 
                probability_ <= 1;
        }
    }
}
