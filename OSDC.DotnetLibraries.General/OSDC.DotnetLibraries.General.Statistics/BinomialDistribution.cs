using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class BinomialDistribution : DiscreteDistribution
    {
        protected uint? numberTrials_;
        protected double? probability_;

        public BinomialDistribution() : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">Number of trials</param>
        /// <param name="p">probability of success</param>
        public BinomialDistribution(uint? n, double? p) : base()
        {
            numberTrials_ = n;
            probability_ = p;
            Range = numberTrials_ + 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public uint? NumberTrials
        {
            get { return numberTrials_; }
            set
            {
                numberTrials_ = value;
                Range = numberTrials_ + 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double? Probability
        {
            get { return probability_; }
            set { probability_ = value; }
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
                int n = (int)numberTrials_.Value;
                double p = probability_.Value;
                return SpecialFunctions.BinomialCoefficient(n, k) * System.Math.Pow(p, k) * System.Math.Pow(1 - p, n - k);
            }
            else return null;
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
                double k = target;
                double n = numberTrials_.Value;
                double p = probability_.Value;
                return 1 - SpecialFunctions.IncompleteBetaRegularized(k + 1, n - k, p);
            }
            else return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int? Realize()
        {
            int successes = 0;
            for (int i = 0; i < numberTrials_; i++)
            {
                if (RandomGenerator.Instance.NextDouble() < probability_) successes++;
            }
            return successes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public override void Copy(DiscreteDistribution from)
        {
            if (from is BinomialDistribution)
            {
                numberTrials_ = ((BinomialDistribution)from).numberTrials_;
                probability_ = ((BinomialDistribution)from).probability_;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DiscreteDistribution Clone()
        {
            return new BinomialDistribution(numberTrials_, probability_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            return numberTrials_ * probability_;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetStandardDeviation()
        {
            if (IsValid() && numberTrials_ != null && probability_ != null)
            {
                return System.Math.Sqrt(numberTrials_.Value * probability_.Value * (1 - probability_.Value));
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
        public override bool IsValid()
        {
            return numberTrials_ != null && Numeric.IsDefined(probability_) && probability_ >= 0 && probability_ <= 1;
        }
    }
}
