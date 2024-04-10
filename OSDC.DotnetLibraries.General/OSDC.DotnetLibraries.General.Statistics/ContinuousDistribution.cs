using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public abstract class ContinuousDistribution : IDistribution
    {
        /// <summary>
        /// 
        /// </summary>
        protected double minValue_ = Numeric.MIN_DOUBLE;
        /// <summary>
        /// 
        /// </summary>
        protected double maxValue_ = Numeric.MAX_DOUBLE;
        /// <summary>
        /// 
        /// </summary>
        public ContinuousDistribution()
        {
        }

        /// <summary>
        /// Gets or sets the lower limit allowed for the distribution. Does not influence mean or realizations etc.
        /// </summary>
        public double MinValue
        {
            get { return minValue_; }
            set { minValue_ = value; }
        }
        /// <summary>
        /// Gets or sets the upper limit allowed for the distribution.
        /// </summary>
        public double MaxValue
        {
            get { return maxValue_; }
            set { maxValue_ = value; }
        }

        /// <summary>
        /// Copies name and parameter values if the type is correct to an exisiting continuousdistribution. Does not copy limits.
        /// </summary>
        /// <param name="from"></param>
        public virtual void Copy(ContinuousDistribution from)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public void CopyExtraData(ContinuousDistribution from)
        {
            if (from != null)
            {
                minValue_ = from.minValue_;
                maxValue_ = from.maxValue_;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual double? GetProbability(double target)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? GetDataMin()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? GetDataMax()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">0-100%</param>
        /// <returns></returns>
        public virtual double? GetPercentile(double target)
        {
            return Quantile(target / 100);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual double? GetCumulativeProbability(double target)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Tuple<double, double>[]? GetHistogram()
        {
            return GetCurve();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract double? Realize();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? InverseMean()
        {
            return null;
        }

        #region IDistribution Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Tuple<double, double>[]? GetCurve()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Tuple<double, double>[]? GetCDFCurve()
        {
            double? min = GetDataMin();
            double? max = GetDataMax();
            if (min != null && max != null)
            {
                double range = max.Value - min.Value;
                int n = 50;
                Tuple<double, double>[] curve = new Tuple<double, double>[n];
                for (int i = 0; i < n; i++)
                {
                    curve[i] = new Tuple<double, double>(min.Value + i * (range / ((double)(n - 1))), 0);
                    double? res = GetCumulativeProbability(curve[i].Item1);
                    if (res != null)
                    {
                        curve[i] = new Tuple<double, double>(curve[i].Item1, res.Value);
                    }
                    else
                    {
                        curve[i] = new Tuple<double, double>(curve[i].Item1, Numeric.UNDEF_DOUBLE);
                    }
                }
                return curve;
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
        public virtual double? GetMean()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? GetStandardDeviation()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? GetMostLikely()
        {
            return null;
        }
        #endregion

        #region IDistribution Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            return false;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string GetInfo()
        {
            return GetDistributionTypeName();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FromSI"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public virtual string GetInfo(Func<double, string, double> FromSI, string quantity)
        {
            return GetInfo();
        }

        public virtual string GetInfo(Func<double, string, double> FromSI, string quantity, string unitLabel)
        {
            return GetInfo(FromSI, quantity) + " " + unitLabel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string GetDistributionTypeName()
        {
            return GetSplitUpperCase(this.GetType().Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cumulative"></param>
        /// <returns></returns>
        public virtual double? Quantile(double cumulative)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? GetP10()
        {
            return GetPercentile(10);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? GetP50()
        {
            return GetPercentile(50);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? GetP90()
        {
            return GetPercentile(90);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="distribution"></param>
        public void CloneExtraData(ContinuousDistribution distribution)
        {
            if (distribution != null)
            {
                distribution.minValue_ = minValue_;
                distribution.maxValue_ = maxValue_;
            }
        }

        #region ICloneable Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual ContinuousDistribution Clone()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        protected string GetSplitUpperCase(string s)
        {
            char[] charTable = s.ToCharArray();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < charTable.GetLength(0); i++)
            {
                if (i > 0 && char.IsUpper(charTable[i])) sb.Append(" ");
                sb.Append(charTable[i]);
            }
            return sb.ToString();
        }
    }
}
