using OSDC.DotnetLibraries.General.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class GeneralDistributionDrillingProperty : ContinuousDrillingProperty
    {
        /// <summary>
        /// redefined property to utilize the synonym one with the correct type
        /// </summary>
        [JsonIgnore]
        public override ContinuousDistribution? Value
        {
            get
            {
                return GeneralDistributionValue;
            }
            set
            {
                if (value != null && value is GeneralContinuousDistribution generalDistribution)
                {
                    GeneralDistributionValue = generalDistribution;
                }
            }
        }
        /// <summary>
        /// synonym property to Value but with the correct type
        /// </summary>
        public GeneralContinuousDistribution GeneralDistributionValue { get; set; } = new GeneralContinuousDistribution();
        /// <summary>
        /// convenience property to access directly the histogram of the GeneralDistributionValue
        /// </summary>
        [JsonIgnore]
        public Tuple<double, double>[]? Histogram
        {
            get
            {
                if (GeneralDistributionValue != null)
                {
                    return GeneralDistributionValue.Function;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (GeneralDistributionValue != null)
                {
                    GeneralDistributionValue.Function = value;
                }
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public GeneralDistributionDrillingProperty() : base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public GeneralDistributionDrillingProperty(UniformDrillingProperty src) : base(src)
        {
        }

        public override bool Equals(DrillingProperty? cmp)
        {
            if (cmp is not null and GeneralDistributionDrillingProperty drillProp)
            {
                if (GeneralDistributionValue != null && drillProp.GeneralDistributionValue != null)
                {
                    return GeneralDistributionValue.Equals(drillProp.GeneralDistributionValue);
                }
                else
                {
                    return GeneralDistributionValue == null && drillProp.GeneralDistributionValue == null;
                }
            }
            return false;
        }

        public override void CopyTo(DrillingProperty? dest)
        {
            if (GeneralDistributionValue != null && dest is not null and GeneralDistributionDrillingProperty drillProp && drillProp.GeneralDistributionValue != null)
            {
                GeneralDistributionValue.CopyTo(drillProp.GeneralDistributionValue);
            }
        }
    }
}
