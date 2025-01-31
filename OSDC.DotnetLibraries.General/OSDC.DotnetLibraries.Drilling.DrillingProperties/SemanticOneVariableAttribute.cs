using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticOneVariableAttribute : Attribute
    {
        public virtual string? ValueVariable { get; } = null;

        public SemanticOneVariableAttribute(string? value) : base()
        {
            ValueVariable = value;
        }
    }
}
