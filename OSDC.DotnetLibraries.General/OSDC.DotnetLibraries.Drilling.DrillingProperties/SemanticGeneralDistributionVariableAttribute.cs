using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticGeneralDistributionVariableAttribute : Attribute
    {
        public string? Histogram { get; }

        public SemanticGeneralDistributionVariableAttribute(string? histogram)
        {
            Histogram = histogram;
        }
    }
}
