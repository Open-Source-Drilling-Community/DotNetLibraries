using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using OSDC.DotnetLibraries.General.Statistics;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public abstract class DiscreteDrillingProperty : DrillingProperty
    {
        /// <summary>
        /// the probability distribution for the property
        /// </summary>
        public virtual DiscreteDistribution? Value { get; set; } = null;
        [JsonIgnore]
        public abstract uint? NumberOfStates { get; }
        [JsonIgnore]
        public abstract double[]? Probabilities { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DiscreteDrillingProperty() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public DiscreteDrillingProperty(DiscreteDrillingProperty src)
        {
            if (src != null)
            {
                if (src.Value != null)
                {
                    Value = src.Value.Clone();
                }
            }
        }

        /// <summary>
        /// Draw a value according to the probability distribution defined in Value
        /// </summary>
        /// <returns></returns>
        public virtual int? Realize()
        {
            if (Value == null) return null;
            return Value.Realize();
        }
    }
}
