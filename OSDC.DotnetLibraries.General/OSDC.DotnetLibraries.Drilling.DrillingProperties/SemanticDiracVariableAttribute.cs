using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticDiracVariableAttribute : Attribute
    {
        public string? Value { get; } = null;

        public SemanticDiracVariableAttribute(string? value)
        {
            Value = value;
        }
    }
}
