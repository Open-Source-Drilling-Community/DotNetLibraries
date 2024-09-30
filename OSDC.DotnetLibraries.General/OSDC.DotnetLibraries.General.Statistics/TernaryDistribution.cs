using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class TernaryDistribution : MultinomialDistribution
    {

        public TernaryDistribution() : base()
        {
            numberOfStates_ = 3;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">Number of trials</param>
        /// <param name="p1">probability of success</param>
        /// <param name="p2">probability of success</param>
        public TernaryDistribution(uint? n, double? p1, double? p2) : base()
        {
            numberOfStates_ = 3;
            numberTrials_ = n;
            if (p1 != null && p2 != null)
            {
                probabilities_ = new double[3];
                probabilities_[0] = p1.Value;
                probabilities_[1] = p2.Value;
                probabilities_[2] = 1 - p1.Value - p2.Value;
            }
            NumberOfStates = numberTrials_ + 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public double? Probability1
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
                        probabilities_ = new double[3];
                    }
                    probabilities_[0] = value.Value;
                    probabilities_[2] = 1.0 - value.Value - probabilities_[1];
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public double? Probability2
        {
            get { return (probabilities_ != null) ? probabilities_[1] : null; }
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
                        probabilities_ = new double[3];
                    }
                    probabilities_[1] = value.Value;
                    probabilities_[2] = 1.0 - probabilities_[0] - value.Value ;
                }
            }
        }
    }
}
