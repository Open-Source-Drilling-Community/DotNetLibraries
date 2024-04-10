using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class PoissonDistribution : DiscreteDistribution
    {
        /// <summary>
        /// 
        /// </summary>
        public double? Rate { get; set; }

        public PoissonDistribution()
            : base()
        {
        }

        public PoissonDistribution(double? rate)
            : base()
        {
            Rate = rate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return Numeric.IsDefined(Rate) && Rate > 0;
        }

        public override void Copy(DiscreteDistribution from)
        {
            if (from != null)
            {
                if (from is PoissonDistribution)
                {
                    Rate = ((PoissonDistribution)from).Rate;
                }
            }
        }

        /// <summary>
        /// taken from Knuth(via wikipedia)
        /// </summary>
        /// <returns></returns>
        public override int? Realize()
        {
            if (Rate != null)
            {
                int result = 0;
                double p = 1;
                double L = System.Math.Exp(-Rate.Value);
                do
                {
                    result++;
                    double rand = RandomGenerator.Instance.NextDouble();
                    p *= rand;
                }
                while (Numeric.GT(p, L));
                return result - 1;
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
        public override double? GetMean()
        {
            return Rate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetStandardDeviation()
        {
            return (Rate != null && IsValid()) ? System.Math.Sqrt(Rate.Value) : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetProbability(int target)
        {
            if (IsValid() && Rate != null)
            {
                int k = target;
                return (System.Math.Pow(Rate.Value, k) * System.Math.Exp(-Rate.Value)) / Math.SpecialFunctions.Factorial(k);
            }
            else
            {
                return null;
            }
        }

        public override double GetCumulativeProbability(int target)
        {
            return base.GetCumulativeProbability(target);
        }

        /// <summary>
        /// Implementation based on https://github.com/haifengl/smile/blob/master/math/src/main/java/smile/stat/distribution/PoissonDistribution.java
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int Quantile(double p)
        {
            if (p < 0.0 || p > 1.0 || Rate == null)
            {
                return 0;
            }

            if (p < System.Math.Exp(-Rate.Value))
            {
                return 0;
            }

            int n = (int)System.Math.Max(System.Math.Sqrt(Rate.Value), 5.0);
            int nl, nu, inc = 1;

            if (p < GetCumulativeProbability(n))
            {
                do
                {
                    n = System.Math.Max(n - inc, 0);
                    inc *= 2;
                }
                while (p < GetCumulativeProbability(n) && n > 0);
                nl = n;
                nu = n + inc / 2;
            }
            else
            {
                do
                {
                    n += inc;
                    inc *= 2;
                }
                while (p > GetCumulativeProbability(n));
                nu = n;
                nl = n - inc / 2;
            }

            return Quantile(p, nl, nu);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Tuple<double, double>[]? GetCurve()
        {
            if (IsValid())
            {
                double min = Quantile(0);
                double max = Quantile(1 - 1e-5);
                double step = (max - min) / 100.0;
                Tuple<double, double>[] result = new Tuple<double, double>[100];
                double x = 0;
                for (int i = 0; i < 100; i++)
                {
                    int tempInt = Convert.ToInt32(min + x);
                    double? p1 = GetProbability(tempInt);
                    double? p2 = GetProbability(tempInt + 1);
                    if (p1 != null && p2 != null)
                    {
                        result[i] = new Tuple<double, double>(min + x, Numeric.Interpolate(tempInt, tempInt + 1, p1.Value, p2.Value, min + x));
                    }
                    x += step;
                }
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
