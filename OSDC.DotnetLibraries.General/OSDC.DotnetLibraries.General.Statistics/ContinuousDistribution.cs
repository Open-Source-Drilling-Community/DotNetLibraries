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
    public class ContinuousDistribution : IDistribution
    {
        /// <summary>
        /// 
        /// </summary>
        protected bool isCorrelated_ = false;
        /// <summary>
        /// 
        /// </summary>
        protected CorrelationGroup correlationGroup_ = null;
        /// <summary>
        /// 
        /// </summary>
        protected double correlationCoefficient_ = Numeric.UNDEF_DOUBLE;
        /// <summary>
        /// 
        /// </summary>
        protected CorrelationType correlationType_ = CorrelationType.Noise;
        /// <summary>
        /// 
        /// </summary>
        private string correlationGroupName_ = "";
        /// <summary>
        /// 
        /// </summary>
        private string name_ = "";
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
        /// Gets or sets the name of the distribution.
        /// </summary>
        public string Name
        {
            get { return name_; }
            set
            {
                name_ = value;
            }
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
        /// Gets or sets whether the distribution is correlated or not.
        /// </summary>
        public bool IsCorrelated
        {
            get { return isCorrelated_; }
            set { isCorrelated_ = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public CorrelationType CorrelationType
        {
            get { return correlationType_; }
            set { correlationType_ = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double CorrelationCoefficient
        {
            get { return correlationCoefficient_; }
            set { correlationCoefficient_ = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string CorrelationGroupName
        {
            get { return correlationGroupName_; }
            set
            {
                correlationGroupName_ = value;
                if (value != null && value != string.Empty)
                {
                    correlationGroup_ = CorrelationManager.Instance.GetGroup(correlationGroupName_);
                }
                if (correlationGroup_ != null)
                {
                    correlationGroup_.Add(this);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public CorrelationGroup CorrelationGroup
        {
            get
            {
                return correlationGroup_;
            }
            set
            {
                correlationGroup_ = value;
                if (correlationGroup_ != null)
                {
                    correlationGroupName_ = correlationGroup_.Name;
                }
                else
                {
                    correlationGroupName_ = "";
                }
            }
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
                correlationCoefficient_ = from.correlationCoefficient_;
                correlationGroup_ = from.correlationGroup_;
                CorrelationType = from.correlationType_;
                isCorrelated_ = from.isCorrelated_;
                name_ = from.name_;
                minValue_ = from.minValue_;
                maxValue_ = from.maxValue_;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual double GetProbability(double target)
        {
            return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double GetDataMin()
        {
            return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double GetDataMax()
        {
            return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">0-100%</param>
        /// <returns></returns>
        public virtual double GetPercentile(double target)
        {
            return Quantile(target / 100);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual double GetCumulativeProbability(double target)
        {
            return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double[,] GetHistogram()
        {
            return GetCurve();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double Realize()
        {
            if (isCorrelated_ && correlationGroup_ != null)
            {
                double baseValue = correlationCoefficient_ > 0 ? correlationGroup_.Value : 1 - correlationGroup_.Value;

                double cumulative = Numeric.UNDEF_DOUBLE;
                if (correlationType_ == CorrelationType.Mixing)
                {
                    double random = RandomGenerator.Instance.NextDouble();
                    cumulative = (1 - System.Math.Abs(correlationCoefficient_)) * random + System.Math.Abs(correlationCoefficient_) * baseValue;
                }
                else if (correlationType_ == CorrelationType.Noise)
                {
                    double c1 = baseValue * System.Math.Abs(correlationCoefficient_);
                    double c2 = c1 + 1 - System.Math.Abs(correlationCoefficient_);
                    if (Numeric.EQ(c1, c2))
                    {
                        cumulative = c1;
                    }
                    else
                    {
                        cumulative = RandomGenerator.Instance.NextDouble(c1, c2);
                    }
                }
                return Quantile(cumulative);
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public virtual double Realize(string argument)
        {
            return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double InverseMean()
        {
            return Numeric.UNDEF_DOUBLE;
        }

        #region IDistribution Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double[,] GetCurve()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double[,] GetCDFCurve()
        {
            double min = GetDataMin();
            double max = GetDataMax();
            double range = max - min;
            int n = 50;
            double[,] curve = new double[n, 2];
            for (int i = 0; i < n; i++)
            {
                curve[i, 0] = min + i * (range / ((double)(n - 1)));
                curve[i, 1] = GetCumulativeProbability(curve[i, 0]);
            }
            return curve;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double GetMean()
        {
            return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double GetStandardDeviation()
        {
            return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double GetMostLikely()
        {
            return Numeric.UNDEF_DOUBLE;
        }
        #endregion

        #region IDistribution Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool isValid()
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
        public virtual double Quantile(double cumulative)
        {
            return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double GetP10()
        {
            return GetPercentile(10);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double GetP50()
        {
            return GetPercentile(50);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double GetP90()
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
                distribution.correlationCoefficient_ = correlationCoefficient_;
                distribution.correlationGroup_ = correlationGroup_;
                distribution.CorrelationType = correlationType_;
                distribution.isCorrelated_ = isCorrelated_;
                distribution.name_ = name_;
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
