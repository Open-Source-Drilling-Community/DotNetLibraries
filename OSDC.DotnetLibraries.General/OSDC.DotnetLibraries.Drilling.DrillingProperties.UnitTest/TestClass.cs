using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties.UnitTest
{
    internal class TestClass
    {
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticFact("signal", Verbs.Enum.BelongsToClass, Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("signal", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DepthDrilling)]
        [SemanticFact("SignalVerticalElevationFrame", Verbs.Enum.BelongsToClass, Nouns.Enum.VerticalDepthFrame)]
        [SemanticFact("signal", Verbs.Enum.HasReferenceFrame, "SignalVerticalElevationFrame")]
        [SemanticFact("WGS84DepthDatum", Verbs.Enum.BelongsToClass, Nouns.Enum.WGS84VerticalLocation)]
        [SemanticFact("SignalVerticalElevationFrame", Verbs.Enum.HasReferenceFrameOrigin, "WGS84DepthDatum")]
        public ScalarDrillingProperty ScalarValue { get; set; } = new ScalarDrillingProperty();

        [Mandatory(CommonProperty.MandatoryType.PipeHandling | CommonProperty.MandatoryType.Mechanical)]
        [SemanticFact("signal", Verbs.Enum.BelongsToClass, Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("signal", Verbs.Enum.IsOfMeasurableQuantity, BasePhysicalQuantity.QuantityEnum.LengthSmall)]
        [SemanticFact("SignalCurvilinearFrame", Verbs.Enum.BelongsToClass, Nouns.Enum.OneDimensionalCurviLinearReferenceFrame)]
        [SemanticFact("signal", Verbs.Enum.HasReferenceFrame, "SignalCurvilinearFrame")]
        [SemanticFact("TopOfStringDatum", Verbs.Enum.BelongsToClass, Nouns.Enum.TopOfStringReferenceLocation)]
        [SemanticFact("SignalCurvilinearFrame", Verbs.Enum.HasReferenceFrameOrigin, "TopOfStringDatum")]
        public UniformDrillingProperty UniformValue { get; set; } = new UniformDrillingProperty();

        [Mandatory(CommonProperty.MandatoryType.Hydraulic | CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.MaterialTransport)]
        [SemanticFact("signal", Verbs.Enum.BelongsToClass, Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("signal", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.MassDensityDrilling)]
        public GaussianDrillingProperty GaussianValue { get; set; } = new GaussianDrillingProperty();

        [Mandatory(CommonProperty.MandatoryType.Directional)]
        [SemanticFact("signal", Verbs.Enum.BelongsToClass, Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("signal", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.PlaneAngleDrilling)]
        [SemanticFact("SignalAzimuthFrame", Verbs.Enum.BelongsToClass, Nouns.Enum.AngleReferenceFrame)]
        [SemanticFact("signal", Verbs.Enum.HasReferenceFrame, "SignalAzimuthFrame")]
        [SemanticFact("TrueNorthDatum", Verbs.Enum.BelongsToClass, Nouns.Enum.TrueNorthAzimuthLocation)]
        [SemanticFact("SignalAzimuthFrame", Verbs.Enum.HasReferenceFrameOrigin, "TrueNorthDatum")]
        public GeneralDistributionDrillingProperty GeneralDistributionValue { get; set; } = new GeneralDistributionDrillingProperty();
    }
}
