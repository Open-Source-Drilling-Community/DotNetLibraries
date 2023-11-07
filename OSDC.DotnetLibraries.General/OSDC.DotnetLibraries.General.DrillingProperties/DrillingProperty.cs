using OSDC.DotnetLibraries.General.Statistics;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public abstract class DrillingProperty
    {
        /// <summary>
        /// the probability distribution for the property
        /// </summary>
        public virtual ContinuousDistribution Value { get; set; } = null;
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
        public CommonProperty.PositionReferenceType PositionReference { get; set;} = CommonProperty.PositionReferenceType.None;
        /// <summary>
        /// a possible reference for an azimuth
        /// </summary>
        public CommonProperty.AzimuthReferenceType AzimuthReference { get; set; }= CommonProperty.AzimuthReferenceType.None;
        /// <summary>
        /// describe whether the property is mandatory in one or more perspectives
        /// </summary>
        public CommonProperty.MandatoryType Mandatory { get; set; } = CommonProperty.MandatoryType.None;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DrillingProperty() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public DrillingProperty(DrillingProperty src)
        {
            if (src != null)
            {
                if (src.Value != null)
                {
                    Value = src.Value.Clone();
                }
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