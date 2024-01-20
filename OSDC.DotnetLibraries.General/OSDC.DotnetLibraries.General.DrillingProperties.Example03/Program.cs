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
        [MetaDataID("1532f187-99d3-42d2-a99d-d579b94cb55e")]
        public GaussianDrillingProperty MeasuredBitDepth { get; set; } = new GaussianDrillingProperty("1532f187-99d3-42d2-a99d-d579b94cb55e");

        [PhysicalQuantity(PhysicalQuantity.QuantityEnum.StandardLength)]
        [DepthReference(CommonProperty.DepthReferenceType.DrillFloor)]
        [Mandatory(CommonProperty.MandatoryType.PipeHandling | CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.BelongsToClass, Nouns.Enum.HookPosition)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.BelongsToClass, Nouns.Enum.SetPoint)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.IsMechanicallyLocatedAt, "Elevator#01")]
        [SemanticFact("Elevator#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Elevator)]
        [MetaDataID("fe1e95a1-fa56-4d7f-9db3-98719edfd485")]
        public ScalarDrillingProperty BlockPositionSetPoint { get; set; } = new ScalarDrillingProperty("fe1e95a1-fa56-4d7f-9db3-98719edfd485");

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.BlockVelocity)]
        [Mandatory(CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic | CommonProperty.MandatoryType.MaterialTransport)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.BelongsToClass, Nouns.Enum.HookVelocity)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Limit)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsMechanicallyLocatedAt, Nouns.Enum.DrillString)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsPhysicallyLocatedAt, Nouns.Enum.TopOfStringReferenceLocation)]
        [MetaDataID("a7378c62-c17b-4031-a711-e9f36d44ee3f")]
        public UniformDrillingProperty TopOfStringSpeedUpwardLimit { get; set; } = new UniformDrillingProperty("a7378c62-c17b-4031-a711-e9f36d44ee3f");

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        [Mandatory(CommonProperty.MandatoryType.None)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.BitDepth)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.ComputedData)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("Bit#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Bit)]
        [MetaDataID("1d30f759-6979-4996-a3bd-d42f991d2392")]
        public GeneralDistributionDrillingProperty EstimatedBitDepth { get; set; } = new GeneralDistributionDrillingProperty("1d30f759-6979-4996-a3bd-d42f991d2392");
    }
    class Example
    {
        static void Main()
        {
            TestClass testClass = new TestClass();
            testClass.MeasuredBitDepth.GaussianValue.Mean = 1000.0;
            testClass.MeasuredBitDepth.GaussianValue.StandardDeviation = 0.1;
            testClass.BlockPositionSetPoint.DiracDistributionValue.Value = 10.0;
            testClass.TopOfStringSpeedUpwardLimit.UniformValue.Min = 0.10;
            testClass.TopOfStringSpeedUpwardLimit.UniformValue.Max = 0.11;
            testClass.EstimatedBitDepth.GeneralDistributionValue.Function = new Tuple<double, double>[]
            {
                new Tuple<double, double>(999.8, 0.05),
                new Tuple<double, double>(999.9, 0.10),
                new Tuple<double, double>(1000.0, 0.25),
                new Tuple<double, double>(1000.1, 0.50),
                new Tuple<double, double>(1000.2, 0.08),
                new Tuple<double, double>(1000.3, 0.02)
            };

            var dict = GenerateDrillingPropertyMetaData.GetDrillingPropertyMetaData(Assembly.GetExecutingAssembly());

            string json = JsonSerializer.Serialize(testClass);
            Console.WriteLine("Serialization of instance in json:");
            Console.WriteLine(json);
            Console.WriteLine();
            if (dict != null)
            {
                Console.WriteLine("Drilling Property Dictionary");
                foreach (var keyValue in dict)
                {
                    Console.WriteLine(keyValue.Key + "=" + JsonSerializer.Serialize(keyValue.Value));
                }
            }
        }

    }
}