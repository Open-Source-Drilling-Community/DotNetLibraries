using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticGaussianVariableAttribute : Attribute
    {
        public string? Mean { get; } = null;
        public string? StandardDeviation { get; } = null;

        public SemanticGaussianVariableAttribute(string? mean, string? standardDeviation)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
        }
    }
}
