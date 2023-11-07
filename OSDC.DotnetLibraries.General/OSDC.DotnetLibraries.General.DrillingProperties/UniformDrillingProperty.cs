﻿using OSDC.DotnetLibraries.General.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class UniformDrillingProperty : DrillingProperty
    {
        [JsonIgnore]
        public override ContinuousDistribution Value
        {
            get
            {
                return UniformValue;
            }
            set
            {
                if (value is UniformDistribution)
                {
                    UniformValue = (UniformDistribution)value;
                }
            }
        }

        public UniformDistribution UniformValue { get; set; } = new UniformDistribution();

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
