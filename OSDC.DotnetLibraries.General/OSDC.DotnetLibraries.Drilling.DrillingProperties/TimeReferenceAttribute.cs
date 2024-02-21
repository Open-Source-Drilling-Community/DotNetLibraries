using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TimeReferenceAttribute : ReferenceAttribute
    {
        public CommonProperty.TimeReferenceType ReferenceType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public TimeReferenceAttribute(CommonProperty.TimeReferenceType val)
        {
            this.ReferenceType = val;
        }
    }
}
