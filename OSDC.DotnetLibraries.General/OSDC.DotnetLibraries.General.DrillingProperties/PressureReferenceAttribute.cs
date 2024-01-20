using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class PressureReferenceAttribute : ReferenceAttribute
    {
        public CommonProperty.PressureReferenceType ReferenceType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public PressureReferenceAttribute(CommonProperty.PressureReferenceType val)
        {
            this.ReferenceType = val;
        }
    }
}
