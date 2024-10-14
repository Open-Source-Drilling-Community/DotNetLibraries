using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DefaultProbabilityAttribute : Attribute
    {
        public double Probability { get; } = 0.0;

        public DefaultProbabilityAttribute(double probability)
        {
            Probability = probability;
        }
    }
}
