using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
    public class AccessToVariableAttribute : Attribute
    {
        public CommonProperty.VariableAccessType AccessType { get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public AccessToVariableAttribute(CommonProperty.VariableAccessType val)
        {
            AccessType = val;
        }
    }
}
