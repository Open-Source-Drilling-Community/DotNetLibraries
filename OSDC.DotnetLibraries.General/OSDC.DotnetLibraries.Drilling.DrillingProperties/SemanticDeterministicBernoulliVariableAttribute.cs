using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticDeterministicBernoulliVariableAttribute : Attribute
    {
        public string? Variable { get; } = null;
        public SemanticDeterministicBernoulliVariableAttribute(string variable)
        {
            Variable = variable;
        }
    }
}
