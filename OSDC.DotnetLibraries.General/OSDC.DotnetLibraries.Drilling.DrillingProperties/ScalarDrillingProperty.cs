using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class ScalarDrillingProperty : DrillingProperty
    {
        /// <summary>
        /// the value of the property
        /// </summary>
        [JsonIgnore]
        public override ContinuousDistribution? Value
        {
            get
            {
                return DiracDistributionValue;
            }
            set
            {
                if (value != null && value is DiracDistribution distribution)
                {
                    DiracDistributionValue = distribution;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public DiracDistribution DiracDistributionValue { get; set; } = new DiracDistribution();

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ScalarDrillingProperty() :base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public ScalarDrillingProperty(ScalarDrillingProperty src) :base(src)
        {

        }
    }
}
