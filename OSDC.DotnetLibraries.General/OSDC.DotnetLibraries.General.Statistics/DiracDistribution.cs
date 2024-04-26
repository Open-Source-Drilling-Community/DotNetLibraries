using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class DiracDistribution : ContinuousDistribution
    {
        /// <summary>
        /// 
        /// </summary>
        public double? Value { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public DiracDistribution()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public DiracDistribution(double? value)
            : base()
        {
            Value = value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(DiracDistribution cmp)
        {
            bool eq = base.Equals(cmp);
            if (cmp != null)
            {
                eq &= Numeric.EQ(Value, cmp.Value);
            }
            return eq;
        }
        public void CopyTo(DiracDistribution dest)
        {
            base.CopyTo(dest);
            if (dest != null)
            {
                dest.Value = Value;
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
                if (from is DiracDistribution)
                {
                    Value = ((DiracDistribution)from).Value;
                }
                CopyExtraData(from);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Tuple<double, double>[]? GetCurve()
        {
            if (Value == null)
            {
                return null;
            }
            else
            {
                double min;
                double max;
                if (!Numeric.EQ(Value.Value, 0))
                {
                    min = Value.Value * 0.95;
                    max = Value.Value * 1.05;
                }
                else
                {
                    min = -0.05;
                    max = 0.05;
                }
                Tuple<double, double>[] result = new Tuple<double, double>[5];

                result[0] = new Tuple<double, double>(min, 0);
                result[1] = new Tuple<double, double>(Value.Value,0);
                result[2] = new Tuple<double, double>(Value.Value, 0);
                result[3] = new Tuple<double, double>(Value.Value, 0);
                result[4] = new Tuple<double, double>(max, 0);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetProbability(double target)
        {
            if (Value == null)
            {
                return null;
            }
            else
            {
                return (Numeric.EQ(target, Value)) ? 1 : 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetCumulativeProbability(double target)
        {
            if (Value == null)
            {
                return null;
            }
            else
            {
                return (Numeric.GE(target, Value)) ? 1 : 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return !Numeric.IsUndefined(Value) && Numeric.GE(Value, minValue_) && Numeric.LE(Value, maxValue_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            return Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetDataMin()
        {
            return Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetDataMax()
        {
            return Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetStandardDeviation()
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? Realize()
        {
            return Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cumulative"></param>
        /// <returns></returns>
        public override double? Quantile(double cumulative)
        {
            return Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? InverseMean()
        {
            if (GetMean() > 0)
            {
                return 1 / GetMean();
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
        public override string GetInfo()
        {
            return GetDistributionTypeName() + " [" + Value + "]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetDistributionTypeName()
        {
            return "Single Value Distribution";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ContinuousDistribution Clone()
        {
            ContinuousDistribution result = new DiracDistribution(Value);
            CloneExtraData(result);
            return result;
        }
    }
}
