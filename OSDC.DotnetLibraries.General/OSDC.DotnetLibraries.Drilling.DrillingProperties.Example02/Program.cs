using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;

namespace DrillingProperties
{
    public class TestClass
    {
        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticGaussianVariable("BitDepthValue#01", "BitDepthStandardDeviationValue#01")]
        [SemanticFact("BitDepthValue#01", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("BitDepth#01", Nouns.Enum.BitDepth)]
        [SemanticFact("BitDepth#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [SemanticFact("BitDepth#01", Nouns.Enum.DerivedMeasurement)]
        [SemanticFact("BitDepth#01", Verbs.Enum.HasDynamicValue, "BitDepthValue#01")]
        [SemanticFact("BitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("WGS84VerticalDatum", Nouns.Enum.WGS84VerticalLocation)]
        [SemanticFact("VerticalDepthFrame", Nouns.Enum.VerticalDepthFrame)]
        [SemanticFact("VerticalDepthFrame", Verbs.Enum.HasReferenceFrameOrigin, "WGS84VerticalDatum")]
        [SemanticFact("BitDepth#01", Verbs.Enum.HasReferenceFrame, "VerticalDepthFrame")]
        [SemanticFact("Bit#01", Nouns.Enum.DrillingBit)]
        [SemanticFact("BitDepthStandardDeviation#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("BitDepthStandardDeviationValue#01", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("BitDepthStandardDeviation#01", Verbs.Enum.HasDynamicValue, "BitDepthStandardDeviationValue#01")]
        [SemanticFact("GaussianUncertainty#01", Nouns.Enum.GaussianUncertainty)]
        [SemanticFact("BitDepth#01", Verbs.Enum.HasUncertainty, "GaussianUncertainty")]
        [SemanticFact("GaussianUncertainty#01", Verbs.Enum.HasUncertaintyStandardDeviation, "BitDepthStandardDeviation#01")]
        public GaussianDrillingProperty MeasuredBitDepth { get; set; } = new GaussianDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [Mandatory(CommonProperty.MandatoryType.PipeHandling| CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic)]
        [SemanticDiracVariable("BlockPositionSPValue#01")]
        [SemanticFact("BlockPositionSPValue#01", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("BlockPositionSPValue#01", Verbs.Enum.IsOfMeasurableQuantity, PhysicalQuantity.QuantityEnum.StandardLength)]
        [SemanticFact("BlockPositionSP#01", Nouns.Enum.HookPosition)]
        [SemanticFact("BlockPositionSP#01", Nouns.Enum.SetPoint)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.HasDynamicValue, "BlockPositionSPValue#01")]
        [SemanticFact("DrillFloorDatum", Nouns.Enum.DerrickFloorVerticalLocation)]
        [SemanticFact("VerticalDepthFrame", Nouns.Enum.VerticalDepthFrame)]
        [SemanticFact("VerticalDepthFrame", Verbs.Enum.HasReferenceFrameOrigin, "DrillFloorDatum")]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.HasReferenceFrame, "VerticalDepthFrame")]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.IsMechanicallyLocatedAt, "Elevator#01")]
        [SemanticFact("Elevator#01", Nouns.Enum.Elevator)]
        public ScalarDrillingProperty BlockPositionSetPoint { get; set;} = new ScalarDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [Mandatory(CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic | CommonProperty.MandatoryType.MaterialTransport)]
        [SemanticUniformVariable("TopOfStringVelocityUpwardMinValue#01", "TopOfStringVelocityUpwardMaxValue#01")]
        [SemanticFact("TopOfStringVelocityUpward#01", Nouns.Enum.HookVelocity)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.BlockVelocity)]
        [SemanticFact("TopOfStringVelocityUpwardMin#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("TopOfStringVelocityUpwardMax#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("TopOfStringVelocityUpwardMinValue#01", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("TopOfStringVelocityUpwardMaxValue#01", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("TopOfStringVelocityUpwardMin#01", Verbs.Enum.HasDynamicValue, "TopOfStringVelocityUpwardMinValue#01")]
        [SemanticFact("TopOfStringVelocityUpwardMax#01", Verbs.Enum.HasDynamicValue, "TopOfStringVelocityUpwardMaxValue#01")]
        [SemanticFact("TopOfStringVelocityUpward#01",  Nouns.Enum.Limit)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsMechanicallyLocatedAt, Nouns.Enum.DrillString)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsPhysicallyLocatedAt, Nouns.Enum.TopOfStringReferenceLocation)]
        [SemanticFact("UniformUncertainty#01", Nouns.Enum.MinMaxUncertainty)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.HasUncertainty, "UniformUncertainty#01")]
        [SemanticFact("UniformUncertainty#01", Verbs.Enum.HasUncertaintyMin, "TopOfStringVelocityUpwardMin#01")]
        [SemanticFact("UniformUncertainty#01", Verbs.Enum.HasUncertaintyMax, "TopOfStringVelocityUpwardMax#01")]
        public UniformDrillingProperty TopOfStringSpeedUpwardLimit { get; set;} = new UniformDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [Mandatory(CommonProperty.MandatoryType.None)]
        [SemanticGeneralDistributionVariable("EstimatedBitDepthHistogramValue#01")]
        [SemanticFact("EstimatedBitDepth#01", Nouns.Enum.BitDepth)]
        [SemanticFact("EstimatedBitDepth#01", Nouns.Enum.ComputedData)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [SemanticFact("WGS84VerticalDatum", Nouns.Enum.WGS84VerticalLocation)]
        [SemanticFact("VerticalDepthFrame", Nouns.Enum.VerticalDepthFrame)]
        [SemanticFact("VerticalDepthFrame", Verbs.Enum.HasReferenceFrameOrigin, "WGS84VerticalDatum")]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.HasReferenceFrame, "VerticalDepthFrame")]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("Bit#01", Nouns.Enum.DrillingBit)]
        [SemanticFact("TransientT&D#01", Nouns.Enum.ComputationUnit)]
        [OptionalFact(0, "TransientT&D#01", Nouns.Enum.ModelledDegreeOfFreedom, "DegreeOfFreedom", "4")]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.IsTransformationOutput, "TransientT&D#01")]
        [SemanticFact("EstimatedBitDepthHistogramValue#01", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("EstimatedBitDepthHistogram#01", Nouns.Enum.GenericUncertainty)]
        [SemanticFact("GeneralUncertaintyDistribution#01", Nouns.Enum.GenericUncertainty)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.HasUncertainty, "GeneralUncertaintyDistribution#01")]
        [SemanticFact("GeneralUncertaintyDistribution#01", Verbs.Enum.HasUncertaintyHistogram, "EstimatedBitDepthHistogram#01")]
        public GeneralDistributionDrillingProperty EstimatedBitDepth { get; set; } = new GeneralDistributionDrillingProperty();
    }
    class Example
    {
        static void Main()
        {
            TestClass testClass = new TestClass();
            testClass.MeasuredBitDepth.Mean = 1000.0;
            testClass.MeasuredBitDepth.StandardDeviation = 0.1;
            testClass.BlockPositionSetPoint.ScalarValue = 10.0;
            testClass.TopOfStringSpeedUpwardLimit.Min = 0.10;
            testClass.TopOfStringSpeedUpwardLimit.Max = 0.11;
            testClass.EstimatedBitDepth.Histogram = new Tuple<double, double>[]
            {
                new Tuple<double, double>(999.8, 0.05),
                new Tuple<double, double>(999.9, 0.10),
                new Tuple<double, double>(1000.0, 0.25),
                new Tuple<double, double>(1000.1, 0.50),
                new Tuple<double, double>(1000.2, 0.08),
                new Tuple<double, double>(1000.3, 0.02)
            };
        }

    }
}