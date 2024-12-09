using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class SemanticTypeVariableAttribute : Attribute
    {
        public string? ValueVariable { get; } = null;

        public SemanticTypeVariableAttribute(string? value)
        {
            ValueVariable = value;
        }
    }
}
