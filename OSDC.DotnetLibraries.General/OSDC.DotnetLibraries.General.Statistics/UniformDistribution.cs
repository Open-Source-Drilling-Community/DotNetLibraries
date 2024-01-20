using System;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class UniformDistribution : ContinuousDistribution
    {
        /// <summary>
        /// 
        /// </summary>
        public UniformDistribution()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public UniformDistribution(double? min, double? max)
            : base()
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// 
        /// </summary>
        public double? Min { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Max { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            if (isValid())
            {
                return (Min + Max) / 2;
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
        public override double? GetStandardDeviation()
        {
            if (isValid())
            {
                return (Max - Min) / (2 * System.Math.Sqrt(3));
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
        public override double? Realize()
        {
            if (isValid() && Min != null && Max != null)
            {
                double unif = RandomGenerator.Instance.NextDouble();
                double U;
                U = Min.Value + (Max.Value - Min.Value) * unif;
                return U;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cumulative"></param>
        /// <returns></returns>
        public override double? Quantile(double cumulative)
        {
            if (isValid() && Min != null && Max != null)
            {
                double U;
                U = Min.Value + (Max.Value - Min.Value) * cumulative;
                return U;
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
        public override double? InverseMean()
        {
            if (GetMean() > 0 && Min != null && Max != null)
            {
                return System.Math.Log(Max.Value / Min.Value) / (Max.Value - Min.Value);
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
        public override Tuple<double, double>[]? GetCurve()
        {
            if (isValid() && Min != null && Max != null)
            {
                double min = Min.Value - (Max.Value - Min.Value) * 0.05;
                double max = Max.Value + (Max.Value - Min.Value) * 0.05;
                double value = 1 / (Max.Value - Min.Value);
                Tuple<double, double>[] result = new Tuple<double, double>[6];
                result[0] = new Tuple<double, double>(min, 0);
                result[1] = new Tuple<double, double>(Min.Value, 0);
                result[2] = new Tuple<double, double>(Min.Value, value);
                result[3] = new Tuple<double, double>(Max.Value, value);
                result[4] = new Tuple<double, double>(Max.Value, 0);
                result[5] = new Tuple<double, double>(max, 0);
                return result;
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
        public override double? GetProbability(double target)
        {
            if (isValid() && Min != null && Max != null)
            {
                if (target < Min.Value || target > Max.Value)
                {
                    return 0;
                }
                else
                {
                    return 1 / (Max.Value - Min.Value);
                }
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
        public override double? GetCumulativeProbability(double target)
        {
            if (isValid() && Min != null && Max != null)
            {
                if (target <= Max.Value)
                {
                    return System.Math.Max((target - Min.Value) / (Max.Value - Min.Value), 0);
                }
                else
                {
                    return 1;
                }
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
        public override void Copy(ContinuousDistribution from)
        {
            if (from != null)
            {
                if (from is UniformDistribution)
                {
                    Min = ((UniformDistribution)from).Min;
                    Max = ((UniformDistribution)from).Max;
                }
                CopyExtraData(from);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool isValid()
        {
            return (Max > Min) && Numeric.GE(Min, minValue_) && Numeric.LE(Max, maxValue_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetInfo()
        {
            return GetDistributionTypeName() + " [" + Min + "," + Max + "]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetDistributionTypeName()
        {
            string s = base.GetDistributionTypeName();
            return s.Remove(0, s.IndexOf(" ") + 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetDataMax()
        {
            return Max;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetDataMin()
        {
            return Min;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetPercentile(double target)
        {
            if (Min != null && Max != null)
            {
                return Min.Value + (Max.Value - Min.Value) * target / 100.0;
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
        public override double? GetP10()
        {
            if (Min != null && Max != null)
            {
                return Min.Value + (Max.Value - Min.Value) * 0.1;
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
        public override double? GetP50()
        {
            if (Min != null && Max != null)
            {
                return Min.Value + (Max.Value - Min.Value) / 2;
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
        public override double? GetP90()
        {
            if (Min != null && Max != null)
            {
                return Min.Value + (Max.Value - Min.Value) * 0.9;
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
        public override ContinuousDistribution Clone()
        {
            UniformDistribution result = new UniformDistribution(Min, Max);
            CloneExtraData(result);
            return result;
        }
    }
}
