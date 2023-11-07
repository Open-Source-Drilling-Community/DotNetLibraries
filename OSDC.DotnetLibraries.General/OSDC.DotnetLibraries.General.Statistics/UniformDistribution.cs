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
        public override double[,]? GetCurve()
        {
            if (isValid() && Min != null && Max != null)
            {
                double min = Min.Value - (Max.Value - Min.Value) * 0.05;
                double max = Max.Value + (Max.Value - Min.Value) * 0.05;
                double value = 1 / (Max.Value - Min.Value);
                double[,] result = new double[6, 2];
                result[0, 0] = min;
                result[0, 1] = 0;
                result[1, 0] = Min.Value;
                result[1, 1] = 0;
                result[2, 0] = Min.Value;
                result[2, 1] = value;
                result[3, 0] = Max.Value;
                result[3, 1] = value;
                result[4, 0] = Max.Value;
                result[4, 1] = 0;
                result[5, 0] = max;
                result[5, 1] = 0;
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
