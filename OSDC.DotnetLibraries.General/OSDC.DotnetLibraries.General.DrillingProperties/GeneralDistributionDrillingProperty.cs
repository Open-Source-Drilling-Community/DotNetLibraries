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

namespace OSDC.DotnetLibraries.General.DrillingProperties
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
        /// Initialization with meta data id
        /// </summary>
        /// <param name="id"></param>
        public GeneralDistributionDrillingProperty(string id) : base()
        {
            Guid guid;
            if (Guid.TryParse(id, out guid))
            {
                MetaDataID = guid;
            }
        }
        /// <summary>
        /// Initialize with meta data ID
        /// </summary>
        /// <param name="id"></param>
        public GeneralDistributionDrillingProperty(Guid id) : base()
        {
            MetaDataID = id;
        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public GeneralDistributionDrillingProperty(UniformDrillingProperty src) : base(src)
        {
        }
    }
}
