﻿using OSDC.DotnetLibraries.General.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class UniformDrillingProperty : DrillingProperty
    {
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

        public UniformDistribution UniformValue { get; set; } = new UniformDistribution();

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UniformDrillingProperty() : base() { }
        /// <summary>
        /// initialization with meta data ID
        /// </summary>
        /// <param name="id"></param>
        public UniformDrillingProperty(string id) :base()
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
        public UniformDrillingProperty(Guid id) : base()
        {
            MetaDataID = id;
        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public UniformDrillingProperty(UniformDrillingProperty src) : base(src)
        {
        }
    }
}
