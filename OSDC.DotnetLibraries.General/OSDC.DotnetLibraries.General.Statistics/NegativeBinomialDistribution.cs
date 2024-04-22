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
            if (IsValid() && numberTrials_ != null && Probabilities != null && Probabilities.Length > 0)
            {
                int k = target;
                int r = (int)numberTrials_.Value;
                double p = Probabilities[0];
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
        public override double? GetCumulativeProbability(int target)
        {
            if (IsValid() && numberTrials_ != null && Probabilities != null && Probabilities.Length > 0)
            {
                int k = target;
                int r = (int)numberTrials_.Value;
                double p = Probabilities[0];
                return 1 - SpecialFunctions.IncompleteBetaRegularized(k + 1, r, p);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int? Realize()
        {
            if (numberTrials_ == null || Probabilities == null || Probabilities.Length == 0)
            {
                return null;
            }
            if (Numeric.EQ(Probabilities[0], 1)) return 0;
            if (Numeric.EQ(Probabilities[0], 0)) return (int)numberTrials_.Value;
            int failureCount = 0;
            int r = (int)numberTrials_.Value;
            int trials = 0;
            while (failureCount < r)
            {
                if (RandomGenerator.Instance.NextDouble() < (1 - Probabilities[0])) failureCount++;
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
            if (from is NegativeBinomialDistribution and not null)
            {
                NegativeBinomialDistribution other = (NegativeBinomialDistribution)from;
                numberTrials_ = other.numberTrials_;
                Probability = other.Probability;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DiscreteDistribution Clone()
        {
            return new NegativeBinomialDistribution(numberTrials_, Probability);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            return Probability * numberTrials_ / (1 - Probability);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetStandardDeviation()
        {
            if (Probability == null || numberTrials_ == null)
            {
                return null;
            }
            else
            {
                return System.Math.Sqrt((Probability.Value * numberTrials_.Value) / System.Math.Pow(1 - Probability.Value, 2));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return numberTrials_ != null && 
                Probability != null && 
                numberTrials_ > 0 && 
                Numeric.IsDefined(Probability) && 
                Probability >= 0 && 
                Probability <= 1;
        }
    }
}
