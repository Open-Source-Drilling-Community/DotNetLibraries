using OSDC.DotnetLibraries.General.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class GaussianDrillingProperty : DrillingProperty
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
        public GaussianDrillingProperty() :base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public GaussianDrillingProperty(GaussianDrillingProperty src) : base(src) 
        {
        }

    }
}
