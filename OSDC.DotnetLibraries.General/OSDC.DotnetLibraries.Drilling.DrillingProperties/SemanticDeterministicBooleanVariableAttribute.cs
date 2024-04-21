using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticDeterministicBooleanVariableAttribute : Attribute
    {
        public string? Variable { get; } = null;
        public SemanticDeterministicBooleanVariableAttribute(string variable)
        {
            Variable = variable;
        }
    }
}
