using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;
using MathNet.Numerics.Distributions;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class GaussianDrillingProperty : ContinuousDrillingProperty
    {
        /// <summary>
        /// redefined to use the synonym property that is of the correct type
        /// </summary>
        [JsonIgnore]
        public override ContinuousDistribution? Value
        {
            get
            {
                return GaussianValue;
            }
            set
            {
                if (value != null && value is GaussianDistribution gaussianDistribution)
                {
                    GaussianValue = gaussianDistribution;
                }
            }
        }
        /// <summary>
        /// synonym property to Value but with the correct type
        /// </summary>
        public GaussianDistribution GaussianValue { get; set; } = new GaussianDistribution();
        /// <summary>
        /// convenience property to access directly the mean value of the GaussianValue
        /// </summary>
        [JsonIgnore]
        public double? Mean
        {
            get
            {
                if (GaussianValue != null)
                {
                    return GaussianValue.Mean;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (GaussianValue != null)
                {
                    GaussianValue.Mean = value;
                }
            }
        }
        /// <summary>
        /// convenience property to access directly the standard deviation of the GaussianValue
        /// </summary>
        [JsonIgnore]
        public double? StandardDeviation
        {
            get
            {
                if (GaussianValue != null)
                {
                    return GaussianValue.StandardDeviation;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (GaussianValue != null)
                {
                    GaussianValue.StandardDeviation = value;
                }
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public GaussianDrillingProperty() : base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public GaussianDrillingProperty(GaussianDrillingProperty src) : base(src)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public override bool Equals(DrillingProperty? cmp)
        {
            if (cmp is not null and GaussianDrillingProperty)
            {
                GaussianDrillingProperty? gaussianDistribution = cmp as GaussianDrillingProperty;
                return Equals(gaussianDistribution);
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(GaussianDrillingProperty? cmp)
        {
            if (cmp != null)
            {
                if (GaussianValue != null)
                {
                    return GaussianValue.Equals(cmp.GaussianValue);
                }
                else
                {
                    return GaussianValue == null && cmp.GaussianValue == null;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void CopyTo(DrillingProperty? dest)
        {
            if (dest is not null and GaussianDrillingProperty)
            {
                GaussianDrillingProperty? gaussianDrillingProperty = dest as GaussianDrillingProperty;
                CopyTo(gaussianDrillingProperty);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        public void CopyTo(GaussianDrillingProperty? dest)
        {
            if (dest != null)
            {
                if (GaussianValue != null)
                {
                    if (dest.GaussianValue == null)
                    {
                        dest.GaussianValue = new GaussianDistribution();
                    }
                    GaussianValue.CopyTo(dest.GaussianValue);
                }
            }
        }
        public double? ProbabilityLT(double? cmp)
        {
            if (cmp == null || Mean == null || StandardDeviation == null)
            {
                return null;
            }
            else
            {
                return Normal.CDF(Mean.Value, StandardDeviation.Value, cmp.Value);
            }
        }

        public double? ProbabilityGT(double? cmp)
        {
            double? result = ProbabilityLT(cmp);
            if (result == null)
            {
                return null;
            }
            else
            {
                return 1.0 - result.Value;
            }
        }

        public double? ProbabilityLT(GaussianDrillingProperty? cmp)
        {
            if (cmp == null || cmp.Mean == null || cmp.StandardDeviation == null || Mean == null || StandardDeviation == null)
            {
                return null;
            }
            else
            {
                double mean = Mean.Value - cmp.Mean.Value;
                double stdDev = Math.Sqrt(StandardDeviation.Value*StandardDeviation.Value + cmp.StandardDeviation.Value * cmp.StandardDeviation.Value);
                return Normal.CDF(mean, stdDev, 0);
            }
        }

        public double? ProbabilityGT(GaussianDrillingProperty? cmp)
        {
            double? result = ProbabilityLT(cmp);
            if (result == null)
            {
                return null;
            }
            else
            {
                return 1.0 - result.Value;
            }
        }
    }
}
