using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OSDC.DotnetLibraries.General.DrillingProperties.DrillingProperty;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class ScalarProperty
    {
        /// <summary>
        /// the value of the property
        /// </summary>
        public double? Value { get; set; }
        /// <summary>
        /// the drilling specific physical quantity. If it is defined then PhysicalQuantity is ignored
        /// </summary>
        public DrillingPhysicalQuantity.QuantityEnum? DrillingPhysicalQuantity { get; set; }
        /// <summary>
        /// the general physical quantity. Is used only if DrillingPhysicalQuantity is null
        /// </summary>
        public PhysicalQuantity.QuantityEnum? PhysicalQuantity { get; set; }
        /// <summary>
        /// a possible reference for an abscissa
        /// </summary>
        public CommonProperty.AbscissaReferenceType AbscissaReference { get; set; } = CommonProperty.AbscissaReferenceType.None;
        /// <summary>
        /// a possible reference for a drilling depth
        /// </summary>
        public CommonProperty.DepthReferenceType DepthReference { get; set; } = CommonProperty.DepthReferenceType.None;
        /// <summary>
        /// a possible reference for a position
        /// </summary>
        public CommonProperty.PositionReferenceType PositionReference { get; set; } = CommonProperty.PositionReferenceType.None;
        /// <summary>
        /// a possible reference for an azimuth
        /// </summary>
        public CommonProperty.AzimuthReferenceType AzimuthReference { get; set; } = CommonProperty.AzimuthReferenceType.None;
        /// <summary>
        /// describe whether the property is mandatory in one or more perspectives
        /// </summary>
        public CommonProperty.MandatoryType Mandatory { get; set; } = CommonProperty.MandatoryType.None;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ScalarProperty() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public ScalarProperty(ScalarProperty src)
        {
            if (src != null)
            {
                Value = src.Value;
                DrillingPhysicalQuantity = src.DrillingPhysicalQuantity;
                PhysicalQuantity = src.PhysicalQuantity;
                AbscissaReference = src.AbscissaReference;
                DepthReference = src.DepthReference;
                PositionReference = src.PositionReference;
                AzimuthReference = src.AzimuthReference;
                Mandatory = src.Mandatory;
            }
        }
    }
}
