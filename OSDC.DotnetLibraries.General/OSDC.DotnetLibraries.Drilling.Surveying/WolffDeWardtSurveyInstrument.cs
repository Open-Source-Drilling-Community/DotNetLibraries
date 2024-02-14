using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public class WolffDeWardtSurveyInstrument
    {
        /// <summary>
        /// Relative Depth Error
        /// </summary>
        [PhysicalQuantity(PhysicalQuantity.QuantityEnum.SmallProportion)]
        [Mandatory(CommonProperty.MandatoryType.Directional)]
        public double? RelDepthError { get; set; } = null;
        /// <summary>
        /// Misalignment
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        [Mandatory(CommonProperty.MandatoryType.Directional)]
        public double? Misalignment { get; set; } = null;
        /// <summary>
        /// True Inclination
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        [Mandatory(CommonProperty.MandatoryType.Directional)]
        public double? TrueInclination { get; set; } = null;
        /// <summary>
        /// Reference Error
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        [Mandatory(CommonProperty.MandatoryType.Directional)]
        public double? ReferenceError { get; set; } = null;
        /// <summary>
        /// DrillString Magnetism
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        [Mandatory(CommonProperty.MandatoryType.Directional)]
        public double? DrillStringMag { get; set; } = null;
        /// <summary>
        /// Gyro Compass Error
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        [Mandatory(CommonProperty.MandatoryType.Directional)]
        public double? GyroCompassError { get; set; } = null;

    }
}
