using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AzimuthReferenceAttribute : ReferenceAttribute
    {
        public CommonProperty.AzimuthReferenceType ReferenceType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public AzimuthReferenceAttribute(CommonProperty.AzimuthReferenceType val)
        {
            this.ReferenceType = val;
        }

    }
}
