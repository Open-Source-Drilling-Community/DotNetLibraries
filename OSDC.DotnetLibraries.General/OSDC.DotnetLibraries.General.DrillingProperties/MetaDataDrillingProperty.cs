using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class MetaDataDrillingProperty
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommonProperty.AbscissaReferenceType? AbscissaReferenceType { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommonProperty.AzimuthReferenceType?  AzimuthReferenceType { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommonProperty.DepthReferenceType? DepthReferenceType { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommonProperty.MandatoryType? MandatoryType { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommonProperty.PositionReferenceType? PositionReferenceType { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommonProperty.PressureReferenceType? PressureReferenceType { get; set;} = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PhysicalQuantity.QuantityEnum? PhysicalQuantity { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DrillingPhysicalQuantity.QuantityEnum? DrillingPhysicalQuantity { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SemanticFact>? SemanticFacts { get; set; } = null;
    }
}
