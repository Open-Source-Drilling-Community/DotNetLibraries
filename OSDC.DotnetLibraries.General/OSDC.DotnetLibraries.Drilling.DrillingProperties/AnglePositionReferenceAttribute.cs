using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AnglePositionReferenceAttribute : ReferenceAttribute
    {
        public CommonProperty.AnglePositionReferenceType ReferenceType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public AnglePositionReferenceAttribute(CommonProperty.AnglePositionReferenceType val)
        {
            this.ReferenceType = val;
        }
    }
}
