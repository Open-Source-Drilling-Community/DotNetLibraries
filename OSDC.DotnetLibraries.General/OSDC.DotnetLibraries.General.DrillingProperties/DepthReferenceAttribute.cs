using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DepthReferenceAttribute : ReferenceAttribute
    {
        public CommonProperty.DepthReferenceType ReferenceType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public DepthReferenceAttribute(CommonProperty.DepthReferenceType val)
        {
            this.ReferenceType = val;
        }
    }
}
