using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class ScalarProperty : DrillingProperty
    {
        /// <summary>
        /// the value of the property
        /// </summary>
        [JsonIgnore]
        public override ContinuousDistribution Value
        {
            get
            {
                return DiracDistribution;
            }
            set
            {
                if (value != null && value is DiracDistribution distribution)
                {
                    DiracDistribution = distribution;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public DiracDistribution DiracDistribution { get; set; } = new DiracDistribution();

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ScalarProperty() :base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public ScalarProperty(ScalarProperty src) :base(src)
        {

        }
    }
}
