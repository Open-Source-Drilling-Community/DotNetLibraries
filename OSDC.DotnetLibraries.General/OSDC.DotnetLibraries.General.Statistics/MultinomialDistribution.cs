using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Common;
using System.ComponentModel.Design;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class MultinomialDistribution : DiscreteDistribution
    {
        protected uint? numberTrials_ = null;
        protected uint? numberOfStates_ = null;
        protected double[]? probabilities_ = null;
        /// <summary>
        /// 
        /// </summary>
        public MultinomialDistribution() : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">Number of trials</param>
        /// <param name="k">Number of states</param>
        /// <param name="p">probabilities of each state</param>
        public MultinomialDistribution(uint? n, uint? k, double[]? p) : base()
        {
            numberTrials_ = n;
            numberOfStates_ = k;
            if (k != null)
            {
                probabilities_ = new double[k.Value];
                if (p != null)
                {
                    double sum = 0;
                    for (int i = 0; i < System.Math.Min(p.Length, probabilities_.Length); i++)
                    {
                        probabilities_[i] = p[i];
                        sum += p[i];
                    }
                    if (sum > 0)
                    {
                        for (int i = 0; i < probabilities_.Length; i++)
                        {
                            p[i] /= sum;
                        }
                    }
                }
            }
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
        public uint? NumberOfStates
        {
            get { return numberOfStates_; }
            set
            {
                numberOfStates_ = value;
                if (numberOfStates_ == null)
                {
                    Probabilities = null;
                }
                else
                {
                    if (Probabilities == null || Probabilities.Length != numberOfStates_)
                    {
                        Probabilities = new double[(int)numberOfStates_];
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public double[]? Probabilities
        {
            get { return probabilities_; }
            private set {  probabilities_ = value; }
        }

        /// <summary>
        /// Return the probability of m success of event 0 within numberOfTrials.
        /// </summary>
        /// <param name="nSuccess"></param>
        /// <returns></returns>
        public override double? GetProbability(int m)
        {
            return GetProbability(m, 0);
        }
        /// <summary>
        /// Return the probability of m success of event i wihtin numberOfTrials
        /// </summary>
        /// <param name="m"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public double? GetProbability(int m, uint i)
        {
            if (IsValid() && numberTrials_ != null && Probabilities != null && i < Probabilities.Length)
            {
                int n = (int)numberTrials_.Value;
                double p = Probabilities[i];
                return SpecialFunctions.BinomialCoefficient(n, m) * System.Math.Pow(p, m) * System.Math.Pow(1 - p, n - m);
            }
            else return null;
        }

        /// <summary>
        /// Return the probability of ms success for each of the events within numberOfTrials
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public double? GetProbability(int[] ms)
        {
            if (IsValid() && numberTrials_ != null && Probabilities != null && ms != null && ms.Length == Probabilities.Length)
            {
                int n = (int)numberTrials_.Value;
                int nFact = Factorial(n);
                int denom = 1;
                for (int i = 0; i < ms.Length; i++)
                {
                    denom *= Factorial(ms[i]);
                }
                int coef = nFact / denom;
                double prod = 1.0;
                for (int i = 0; i < ms.Length; i++)
                {
                    prod *= System.Math.Pow(Probabilities[i], ms[i]);
                }
                return coef * prod;
            }
            else return null;
        }

        private int Factorial(int n)
        {
            int fact = 1;
            for (int i = 2; i <= n; i++)
            {
                fact *= i;
            }
            return fact;
        }

        /// <summary>
        /// return the cumulative probability to obtain m successes for first state within numberOfTrials
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetCumulativeProbability(int m)
        {
            return GetCumulativeProbability(m, 0);
        }

        /// <summary>
        /// return the cumulative probability to obtain m success of the i state within numberOfTrials
        /// </summary>
        /// <param name="m"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public double? GetCumulativeProbability(int m, int i)
        {
            if (IsValid() && numberTrials_ != null && Probabilities != null && Probabilities.Length > 0 && i < Probabilities.Length)
            {
                int n = (int)numberTrials_.Value;
                double p = Probabilities[0];
                int nFact = Factorial(n);
                double sum = 0;
                for (int k = 0; k < m; k++)
                {
                    sum += SpecialFunctions.BinomialCoefficient(n, m) * System.Math.Pow(Probabilities[i], k) * System.Math.Pow(1 - Probabilities[i], n - k);
                }
                return sum;
            }
            else return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int? Realize()
        {
            if (IsValid() && Probabilities != null)
            {
                double rnd = RandomGenerator.Instance.NextDouble();
                double cumulativeProbability = 0;

                for (int k = 0; k < Probabilities.Length; k++)
                {
                    cumulativeProbability += Probabilities[k];
                    if (rnd <= cumulativeProbability)
                    {
                        return k;
                    }
                }
                return null;
            }
            else
            {
                 return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public override void Copy(DiscreteDistribution from)
        {
            if (from is MultinomialDistribution)
            {
                MultinomialDistribution other = (MultinomialDistribution)from;
                numberTrials_ = other.numberTrials_;
                if (Probabilities != null && other.Probabilities != null && Probabilities.Length == other.Probabilities.Length != null)
                {
                    for (int i = 0; i < Probabilities.Length; i++)
                    {
                        Probabilities[i] = other.Probabilities[i];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DiscreteDistribution Clone()
        {
            return new MultinomialDistribution(numberTrials_, numberOfStates_, Probabilities);
        }

        /// <summary>
        /// Return the mean value of the first event
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            return GetMean(0);
        }
        /// <summary>
        /// Return the mean value of the i event
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double? GetMean(int i)
        {
            if (IsValid() && Probabilities != null && Probabilities.Length > 0 && i < Probabilities.Length)
            {
                return numberTrials_ * Probabilities[i];
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// return the standard deviation of the first event
        /// </summary>
        /// <returns></returns>
        public override double? GetStandardDeviation()
        {
            return GetStandardDeviation(0);
        }
        /// <summary>
        /// return the standard deviation of the i event
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double? GetStandardDeviation(int i)
        {
            if (IsValid() && numberTrials_ != null && Probabilities != null && Probabilities.Length > 0 && i < Probabilities.Length)
            {
                return System.Math.Sqrt(numberTrials_.Value * Probabilities[i] * (1 - Probabilities[i]));
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
            bool isValid = numberTrials_ != null;
            isValid &= Probabilities != null && Probabilities.Length >= 2;
            if (Probabilities != null) {
                double sum = 0;
                for (int i = 0; i< Probabilities.Length; i++)
                {
                    isValid &= Probabilities[i] >= 0 && Probabilities[i] <= 1;
                    sum += Probabilities[i];
                }
                isValid &= Numeric.EQ(sum, 1);
            }
            return isValid;
        }
    }
}
