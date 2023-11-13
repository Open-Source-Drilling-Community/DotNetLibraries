using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DrillingPhysicalQuantityAttribute : Attribute
    {
        public DrillingPhysicalQuantity.QuantityEnum PhysicalQuantity { get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public DrillingPhysicalQuantityAttribute(DrillingPhysicalQuantity.QuantityEnum val)
        {
            this.PhysicalQuantity = val;
        }
    }
}
