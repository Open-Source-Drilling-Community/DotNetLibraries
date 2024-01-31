﻿using OSDC.DotnetLibraries.General.Statistics;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public abstract class DrillingProperty
    {
        /// <summary>
        /// the probability distribution for the property
        /// </summary>
        public virtual ContinuousDistribution? Value { get; set; } = null;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DrillingProperty() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public DrillingProperty(DrillingProperty src)
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
        public virtual double? Realize()
        {
            if (Value == null) return null;
            return Value.Realize();
        }
    }
}