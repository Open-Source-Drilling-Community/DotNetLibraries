using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AxialPositionReferenceAttribute : ReferenceAttribute
    {
        public CommonProperty.AxialPositionReferenceType ReferenceType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public AxialPositionReferenceAttribute(CommonProperty.AxialPositionReferenceType val)
        {
            this.ReferenceType = val;
        }
    }
}
