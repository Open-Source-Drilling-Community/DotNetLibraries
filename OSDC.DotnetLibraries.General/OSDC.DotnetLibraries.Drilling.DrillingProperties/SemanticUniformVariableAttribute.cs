using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticUniformVariableAttribute : Attribute
    {
        public string? MinValue { get; } = null;
        public string? MaxValue { get; } = null;

        public SemanticUniformVariableAttribute(string? minValue, string? maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
