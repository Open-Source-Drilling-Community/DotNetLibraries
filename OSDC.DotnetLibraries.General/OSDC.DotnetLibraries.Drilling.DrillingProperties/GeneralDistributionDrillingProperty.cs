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
    public class GeneralDistributionDrillingProperty : DrillingProperty
    {
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
        public GeneralContinuousDistribution GeneralDistributionValue { get; set; } = new GeneralContinuousDistribution();
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
    }
}
