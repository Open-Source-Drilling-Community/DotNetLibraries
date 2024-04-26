using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticDeterministicCategoricalVariableAttribute : Attribute
    {
        public string? Variable { get; } = null;
        public uint? NumberOfStates { get; } = null;
        public SemanticDeterministicCategoricalVariableAttribute(string variable, uint numberOfStates)
        {
            Variable = variable;
            NumberOfStates = numberOfStates;
        }
    }
}
