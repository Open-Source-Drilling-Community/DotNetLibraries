using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DefaultStandardDeviationAttribute : Attribute
    {
        public double StandardDeviation { get; } = 0.0;

        public DefaultStandardDeviationAttribute(double standardDeviation)
        {
            StandardDeviation = standardDeviation;
        }
    }
}
