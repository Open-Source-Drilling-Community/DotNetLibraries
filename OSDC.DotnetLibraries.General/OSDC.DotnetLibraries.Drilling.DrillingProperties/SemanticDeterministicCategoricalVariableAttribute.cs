﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticDeterministicCategoricalVariableAttribute : SemanticOneVariableAttribute
    {
        public uint? NumberOfStates { get; } = null;
        public SemanticDeterministicCategoricalVariableAttribute(string variable, uint numberOfStates) : base(variable)
        {
            NumberOfStates = numberOfStates;
        }
    }
}
