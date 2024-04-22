using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class BinomialDistribution : MultinomialDistribution
    {

        public BinomialDistribution() : base()
        {
            numberOfStates_ = 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">Number of trials</param>
        /// <param name="p">probability of success</param>
        public BinomialDistribution(uint? n, double? p) : base()
        {
            numberOfStates_ = 2;
            numberTrials_ = n;
            if (p != null)
            {
                probabilities_ = new double[2];
                probabilities_[0] = p.Value;
                probabilities_[1] = 1 - p.Value;
            }
            Range = numberTrials_ + 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public double? Probability
        {
            get { return (probabilities_ != null) ? probabilities_[0] : null; }
            set
            {
                if (value == null)
                {
                    probabilities_ = null;
                }
                else
                {
                    if (probabilities_ == null) 
                    {
                        probabilities_ = new double[2];
                    }
                    probabilities_[0] = value.Value;
                    probabilities_[1] = 1.0 - value.Value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetProbability(int target)
        {
            if (IsValid() && numberTrials_ != null && Probability != null)
            {
                int k = target;
                int n = (int)numberTrials_.Value;
                double p = Probability.Value;
                return SpecialFunctions.BinomialCoefficient(n, k) * System.Math.Pow(p, k) * System.Math.Pow(1 - p, n - k);
            }
            else return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetCumulativeProbability(int target)
        {
            if (IsValid() && numberTrials_ != null && Probability != null)
            {
                double k = target;
                double n = numberTrials_.Value;
                double p = Probability.Value;
                return 1 - SpecialFunctions.IncompleteBetaRegularized(k + 1, n - k, p);
            }
            else return null;
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
                if (RandomGenerator.Instance.NextDouble() < Probability) successes++;
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
                Probability = ((BinomialDistribution)from).Probability;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DiscreteDistribution Clone()
        {
            return new BinomialDistribution(numberTrials_, Probability);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            return numberTrials_ * Probability;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetStandardDeviation()
        {
            if (IsValid() && numberTrials_ != null && Probability != null)
            {
                return System.Math.Sqrt(numberTrials_.Value * Probability.Value * (1 - Probability.Value));
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
            return numberTrials_ != null && Numeric.IsDefined(Probability) && Probability >= 0 && Probability <= 1;
        }
    }
}
