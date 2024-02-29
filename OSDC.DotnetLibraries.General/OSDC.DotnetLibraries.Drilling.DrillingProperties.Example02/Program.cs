using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;

namespace DrillingProperties
{
    public class TestClass
    {
        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticGaussianVariables("BitDepthValue#01", "BitDepthStandardDeviationValue#01")]
        [SemanticFact("BitDepthValue#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("BitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.BitDepth)]
        [SemanticFact("BitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DerivedMeasurement)]
        [SemanticFact("BitDepth#01", Verbs.Enum.HasDynamicValue, "BitDepthValue#01")]
        [SemanticFact("BitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("Bit#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Bit)]
        [SemanticFact("BitDepthStandardDeviation#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("BitDepthStandardDeviationValue#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("BitDepthStandardDeviation#01", Verbs.Enum.HasDynamicValue, "BitDepthStandardDeviationValue#01")]
        [SemanticFact("GaussianUncertainty#01", Verbs.Enum.BelongsToClass, Nouns.Enum.GaussianUncertainty)]
        [SemanticFact("BitDepth#01", Verbs.Enum.HasUncertainty, "GaussianUncertainty")]
        [SemanticFact("GaussianUncertainty#01", Verbs.Enum.HasUncertaintyStandardDeviation, "BitDepthStandardDeviation#01")]
        public GaussianDrillingProperty MeasuredBitDepth { get; set; } = new GaussianDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [PhysicalQuantity(PhysicalQuantity.QuantityEnum.StandardLength)]
        [DepthReference(CommonProperty.DepthReferenceType.DerrickFloor)]
        [Mandatory(CommonProperty.MandatoryType.PipeHandling| CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic)]
        [SemanticDiracVariable("BlockPositionSPValue#01")]
        [SemanticFact("BlockPositionSPValue#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.BelongsToClass, Nouns.Enum.HookPosition)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.BelongsToClass, Nouns.Enum.SetPoint)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.HasDynamicValue, "BlockPositionSPValue#01")]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.IsMechanicallyLocatedAt, "Elevator#01")]
        [SemanticFact("Elevator#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Elevator)]
        public ScalarDrillingProperty BlockPositionSetPoint { get; set;} = new ScalarDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.BlockVelocity)]
        [Mandatory(CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic | CommonProperty.MandatoryType.MaterialTransport)]
        [SemanticUniformVariable("TopOfStringVelocityUpwardMinValue#01", "TopOfStringVelocityUpwardMaxValue#01")]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.BelongsToClass, Nouns.Enum.HookVelocity)]
        [SemanticFact("TopOfStringVelocityUpwardMin#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("TopOfStringVelocityUpwardMax#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("TopOfStringVelocityUpwardMinValue#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("TopOfStringVelocityUpwardMaxValue#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("TopOfStringVelocityUpwardMin#01", Verbs.Enum.HasDynamicValue, "TopOfStringVelocityUpwardMinValue#01")]
        [SemanticFact("TopOfStringVelocityUpwardMax#01", Verbs.Enum.HasDynamicValue, "TopOfStringVelocityUpwardMaxValue#01")]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Limit)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsMechanicallyLocatedAt, Nouns.Enum.DrillString)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsPhysicallyLocatedAt, Nouns.Enum.TopOfStringReferenceLocation)]
        [SemanticFact("UniformUncertainty#01", Verbs.Enum.BelongsToClass, Nouns.Enum.MinMaxUncertainty)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.HasUncertainty, "UniformUncertainty#01")]
        [SemanticFact("UniformUncertainty#01", Verbs.Enum.HasUncertaintyMin, "TopOfStringVelocityUpwardMin#01")]
        [SemanticFact("UniformUncertainty#01", Verbs.Enum.HasUncertaintyMax, "TopOfStringVelocityUpwardMax#01")]
        public UniformDrillingProperty TopOfStringSpeedUpwardLimit { get; set;} = new UniformDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        [Mandatory(CommonProperty.MandatoryType.None)]
        [SemanticGeneralDistributionVariable("EstimatedBitDepthHistogramValue#01")]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.BitDepth)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.ComputedData)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("Bit#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Bit)]
        [SemanticFact("TransientT&D#01", Verbs.Enum.BelongsToClass, Nouns.Enum.ComputationUnit)]
        [OptionalFact(0, "TransientT&D#01", Verbs.Enum.BelongsToClass, Nouns.Enum.ModelledDegreeOfFreedom, "DegreeOfFreedom", "4")]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.IsTransformationOutput, "TransientT&D#01")]
        [SemanticFact("EstimatedBitDepthHistogramValue#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("EstimatedBitDepthHistogram#01", Verbs.Enum.BelongsToClass, Nouns.Enum.GenericUncertainty)]
        [SemanticFact("GeneralUncertaintyDistribution#01", Verbs.Enum.BelongsToClass, Nouns.Enum.GenericUncertainty)]
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