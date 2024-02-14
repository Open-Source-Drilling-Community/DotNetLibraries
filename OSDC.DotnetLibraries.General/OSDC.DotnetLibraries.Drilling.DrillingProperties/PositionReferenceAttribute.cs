using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PositionReferenceAttribute : ReferenceAttribute
    {
        public CommonProperty.PositionReferenceType ReferenceType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public PositionReferenceAttribute(CommonProperty.PositionReferenceType val)
        {
            this.ReferenceType = val;
        }

    }
}
