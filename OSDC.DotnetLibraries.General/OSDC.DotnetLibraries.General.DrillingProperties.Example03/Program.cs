using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;
using System.Reflection;
using System.Text.Json;

namespace DrillingProperties
{
    public class TestClass
    {
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticFact("BitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.BitDepth)]
        [SemanticFact("BitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DerivedMeasurement)]
        [SemanticFact("BitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("Bit#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Bit)]
        public GaussianDrillingProperty MeasuredBitDepth { get; set; } = new GaussianDrillingProperty();

        [PhysicalQuantity(PhysicalQuantity.QuantityEnum.StandardLength)]
        [DepthReference(CommonProperty.DepthReferenceType.DrillFloor)]
        [Mandatory(CommonProperty.MandatoryType.PipeHandling | CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.BelongsToClass, Nouns.Enum.HookPosition)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.BelongsToClass, Nouns.Enum.SetPoint)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.IsMechanicallyLocatedAt, "Elevator#01")]
        [SemanticFact("Elevator#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Elevator)]
        public ScalarDrillingProperty BlockPositionSetPoint { get; set; } = new ScalarDrillingProperty();

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.BlockVelocity)]
        [Mandatory(CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic | CommonProperty.MandatoryType.MaterialTransport)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.BelongsToClass, Nouns.Enum.HookVelocity)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Limit)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsMechanicallyLocatedAt, Nouns.Enum.DrillString)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsPhysicallyLocatedAt, Nouns.Enum.TopOfStringReferenceLocation)]
        public UniformDrillingProperty TopOfStringSpeedUpwardLimit { get; set; } = new UniformDrillingProperty();

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        [Mandatory(CommonProperty.MandatoryType.None)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.BitDepth)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.ComputedData)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("Bit#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Bit)]
        public GeneralDistributionDrillingProperty EstimatedBitDepth { get; set; } = new GeneralDistributionDrillingProperty();
    }
    class Example
    {
        static void Main()
        {          
            var dict = GenerateDrillingPropertyMetaData.GetDrillingPropertyMetaData(Assembly.GetExecutingAssembly());
            if (dict != null)
            {
                foreach (var keyValue in dict)
                {
                    Console.WriteLine("(" + keyValue.Key.Item1 + ", " + keyValue.Key.Item2 + ") " + "=" + JsonSerializer.Serialize(keyValue.Value));
                }
            }
        }

    }
}