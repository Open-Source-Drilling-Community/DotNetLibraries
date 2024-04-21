using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;
using System.Reflection;
using DWIS.API.DTO;

namespace DrillingProperties
{
    public class TestClass
    {
        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [SemanticGaussianVariable("FluidDensityEstimated", "FluidDensityEstimatedStdDev")]
        [SemanticFact("FluidDensityEstimated", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensityEstimated#01", Nouns.Enum.ComputedData)]
        [SemanticFact("FluidDensityEstimated#01", Verbs.Enum.HasDynamicValue, "FluidDensityEstimated")]
        [SemanticFact("FluidDensityEstimated#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        [SemanticFact("FDEUncertainty#01", Nouns.Enum.GaussianUncertainty)]
        [SemanticFact("FluidDensityEstimated#01", Verbs.Enum.HasUncertainty, "FDEUncertainty#01")]
        [SemanticFact("FluidDensityEstimatedStdDev", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensityEstimatedStdDev#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("FluidDensityEstimatedStdDev#01", Verbs.Enum.HasStaticValue, "FluidDensityEstimatedStdDev")]
        [SemanticFact("FDEUncertainty#01", Verbs.Enum.HasUncertaintyStandardDeviation, "FluidDensityEstimatedStdDev#01")]
        [OptionalFact(1, "FDEUncertainty#01", Verbs.Enum.HasUncertaintyMean, "FluidDensityEstimated#01")]
        public GaussianDrillingProperty FluidDensityEstimated { get; set; } = new GaussianDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [SemanticGaussianVariable("FluidDensityMeasured", "sigma_FluidDensityMeasured")]
        [SemanticSensorVariable("FluidDensityMeasured", "FluidDensityMeasured_prec", "FluidDensityMeasured_acc")]
        [SemanticFullScaleVariable("FluidDensityMeasured", "FluidDensityMeasured_fs", "FluidDensityMeasured_prop")]
        [SemanticExclusiveOr(1, 2, 3)]
        [SemanticManifestOptions(2, 21)]
        [SemanticFact("FluidDensityMeasured", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensityMeasured#01", Nouns.Enum.PhysicalData)]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.HasDynamicValue, "FluidDensityMeasured")]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        [SemanticFact("tos#01", Nouns.Enum.TopOfStringReferenceLocation)]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.IsPhysicallyLocatedAt, "tos#01")]
        [SemanticFact("MovingAverage", Nouns.Enum.MovingAverage)]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.IsTransformationOutput, "MovingAverage")]
        [OptionalFact(1, "sigma_FluidDensityMeasured", Nouns.Enum.DrillingSignal)]
        [OptionalFact(1, "sigma_FluidDensityMeasured#01", Nouns.Enum.DrillingDataPoint)]
        [OptionalFact(1, "sigma_FluidDensityMeasured#01", Verbs.Enum.HasValue, "sigma_FluidDensityMeasured")]
        [OptionalFact(1, "GaussianUncertainty#01", Nouns.Enum.GaussianUncertainty)]
        [OptionalFact(1, "FluidDensityMeasured#01", Verbs.Enum.HasUncertainty, "GaussianUncertainty#01")]
        [OptionalFact(1, "GaussianUncertainty#01", Verbs.Enum.HasUncertaintyStandardDeviation, "sigma_FluidDensityMeasured#01")]
        [OptionalFact(1, 11, "GaussianUncertainty#01", Verbs.Enum.HasUncertaintyMean, "FluidDensityMeasured#01")]
        [OptionalFact(2, "FluidDensityMeasured_prec", Nouns.Enum.DrillingSignal)]
        [OptionalFact(2, "FluidDensityMeasured_prec#01", Nouns.Enum.DrillingDataPoint)]
        [OptionalFact(2, "FluidDensityMeasured_prec#01", Verbs.Enum.HasValue, "FluidDensityMeasured_prec")]
        [OptionalFact(2, "FluidDensityMeasured_acc", Nouns.Enum.DrillingSignal)]
        [OptionalFact(2, "FluidDensityMeasured_acc#01", Nouns.Enum.DrillingDataPoint)]
        [OptionalFact(2, "FluidDensityMeasured_acc#01", Verbs.Enum.HasValue, "FluidDensityMeasured_acc")]
        [OptionalFact(2, "SensorUncertainty#01", Nouns.Enum.SensorUncertainty)]
        [OptionalFact(2, "SensorUncertainty#01", Verbs.Enum.HasUncertaintyPrecision, "FluidDensityMeasured_prec#01")]
        [OptionalFact(2, "SensorUncertainty#01", Verbs.Enum.HasUncertaintyAccuracy, "FluidDensityMeasured_acc#01")]
        [OptionalFact(2, "FluidDensityMeasured#01", Verbs.Enum.HasUncertainty, "SensorUncertainty#01")]
        [OptionalFact(2, 21, "SensorUncertainty#01", Verbs.Enum.HasUncertaintyMean, "FluidDensityMeasured#01")]
        [OptionalFact(3, "FluidDensityMeasured_fs", Nouns.Enum.DrillingSignal)]
        [OptionalFact(3, "FluidDensityMeasured_fs#01", Nouns.Enum.DrillingDataPoint)]
        [OptionalFact(3, "FluidDensityMeasured_fs#01", Verbs.Enum.HasValue, "FluidDensityMeasured_fs")]
        [OptionalFact(3, "FluidDensityMeasured_prop", Nouns.Enum.DrillingSignal)]
        [OptionalFact(3, "FluidDensityMeasured_prop#01", Nouns.Enum.DrillingDataPoint)]
        [OptionalFact(3, "FluidDensityMeasured_prop#01", Verbs.Enum.HasValue, "FluidDensityMeasured_prop")]
        [OptionalFact(3, "FullScaleUncertainty#01", Nouns.Enum.FullScaleUncertainty)]
        [OptionalFact(3, "FullScaleUncertainty#01", Verbs.Enum.HasFullScale, "FluidDensityMeasured_fs#01")]
        [OptionalFact(3, "FullScaleUncertainty#01", Verbs.Enum.HasProportionError, "FluidDensityMeasured_prop#01")]
        [OptionalFact(3, "FluidDensityMeasured#01", Verbs.Enum.HasUncertainty, "FullScaleUncertainty#01")]
        [OptionalFact(3, 31, "FullScaleUncertainty#01", Verbs.Enum.HasUncertaintyMean, "FluidDensityMeasured#01")]
        public SensorDrillingProperty FluidDensityMeasured { get; set; } = new SensorDrillingProperty();
    }
    class Example
    {
        static void GenerateMermaidForMD(StreamWriter writer, string propertyName, string? mermaid)
        {
            if (writer != null && !string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(mermaid))
            {
                writer.WriteLine("# Semantic Graph for `" + propertyName + "`");
                writer.WriteLine(mermaid);
            }
        }
 
        static void Main()
        {
            TestClass testClass = new TestClass();
            Assembly? assembly = Assembly.GetAssembly(typeof(TestClass));
            if (assembly != null)
            {
                string tempPath = Directory.GetCurrentDirectory();
                DirectoryInfo? dir = new DirectoryInfo(tempPath);
                dir = dir?.Parent?.Parent?.Parent;
                if (dir != null)
                {
                    string tempFile = Path.Combine(dir.FullName, "Example06.md");
                    using (StreamWriter writer = new StreamWriter(tempFile))
                    {
                        var manifestFile1 = GeneratorSparQLManifestFile.GetManifestFile(assembly, typeof(TestClass).FullName, "FluidDensityEstimated", "SampleManifest", "ExampleCompany", "Test:");
                        if (manifestFile1 != null)
                        {
                            GenerateMermaidForMD(writer, "FluidDensityEstimated", GeneratorSparQLManifestFile.GetMermaid(manifestFile1));
                        }
                        var manifestFile2 = GeneratorSparQLManifestFile.GetManifestFile(assembly, typeof(TestClass).FullName, "FluidDensityMeasured", "SampleManifest", "ExampleCompany", "Test:");
                        if (manifestFile2 != null)
                        {
                            GenerateMermaidForMD(writer, "FluidDensityMeasured", GeneratorSparQLManifestFile.GetMermaid(manifestFile2));
                        }
                    }
                }
            }
        }
    }
}
