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
    public class UniformDrillingProperty : DrillingProperty
    {
        /// <summary>
        /// synonym property to UniformValue
        /// </summary>
        [JsonIgnore]
        public override ContinuousDistribution? Value
        {
            get
            {
                return UniformValue;
            }
            set
            {
                if (value != null && value is UniformDistribution uniformDistribution)
                {
                    UniformValue = uniformDistribution;
                }
            }
        }
        /// <summary>
        /// synonym property to the Value but with the correct type
        /// </summary>
        public UniformDistribution UniformValue { get; set; } = new UniformDistribution();
        /// <summary>
        /// convenience property to access directly the min value of UniformValue
        /// </summary>
        [JsonIgnore]
        public double? Min
        {
            get
            {
                if (UniformValue != null)
                {
                    return UniformValue.Min;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (UniformValue != null)
                {
                    UniformValue.Min = value;
                }
            }
        }
        /// <summary>
        /// Convenience property to directly access the max value of UniformValue
        /// </summary>
        [JsonIgnore]
        public double? Max
        {
            get
            {
                if (UniformValue != null)
                {
                    return UniformValue.Max;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (UniformValue != null)
                {
                    UniformValue.Max = value;
                }
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UniformDrillingProperty() : base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public UniformDrillingProperty(UniformDrillingProperty src) : base(src)
        {
        }
    }
}
