using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
    public class MandatoryAttribute : Attribute
    {
        public CommonProperty.MandatoryType Mandatory { get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public MandatoryAttribute(CommonProperty.MandatoryType val)
        {
            Mandatory = val;
        }

    }
}
