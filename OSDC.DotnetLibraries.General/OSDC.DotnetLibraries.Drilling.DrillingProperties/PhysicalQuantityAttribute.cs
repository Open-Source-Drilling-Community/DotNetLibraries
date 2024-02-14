using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.UnitConversion.Conversion;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PhysicalQuantityAttribute : Attribute
    {
        public PhysicalQuantity.QuantityEnum PhysicalQuantity { get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public PhysicalQuantityAttribute(PhysicalQuantity.QuantityEnum val) 
        { 
            PhysicalQuantity = val;
        }
    }
}
