using OSDC.DotnetLibraries.General.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class GaussianDrillingProperty : DrillingProperty
    {
        [JsonIgnore]
        public override ContinuousDistribution Value
        {
            get
            {
                return GaussianValue;
            }
            set
            {
                if (value is GaussianDistribution)
                {
                    GaussianValue = (GaussianDistribution)value;
                }
            }
        }
        public GaussianDistribution GaussianValue { get; set; } = new GaussianDistribution();
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
