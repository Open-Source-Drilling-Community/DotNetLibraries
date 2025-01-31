using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticThreeVariablesAttribute : SemanticTwoVariablesAttribute
    {
        public virtual string? ThirdValueVariable { get; }

        public SemanticThreeVariablesAttribute(string? valueVariable, string? secondValueVariable, string? thirdValueVariable) : base(valueVariable, secondValueVariable)
        {
            ThirdValueVariable = thirdValueVariable;
        }
    }
}
