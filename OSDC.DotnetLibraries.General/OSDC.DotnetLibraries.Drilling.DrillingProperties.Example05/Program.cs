using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;
using System.Reflection;

namespace DrillingProperties
{
    public class TestClass
    {
        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [SemanticDiracVariable("FluidDensitySetPoint")]
        [SemanticFact("FluidDensitySetPoint", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensitySetPoint#01", Nouns.Enum.SetPoint)]
        [SemanticFact("FluidDensitySetPoint#01", Verbs.Enum.HasDynamicValue, "FluidDensitySetPoint")]
        [SemanticFact("FluidDensitySetPoint#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        public ScalarDrillingProperty FluidDensitySetPoint { get; set; } = new ScalarDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [SemanticUniformVariable("FluidDensityMin", "FluidDensityMax")]
        [SemanticFact("FluidDensityMin", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensityMax", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensityUniform#01", Nouns.Enum.ComputedData)]
        [SemanticFact("FluidDensityUniform#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        [SemanticFact("FDEUncertainty#01", Nouns.Enum.MinMaxUncertainty)]
        [SemanticFact("FluidDensityUniform#01", Verbs.Enum.HasUncertainty, "FDEUncertainty#01")]
        [SemanticFact("FluidDensityMin#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("FluidDensityMax#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("FluidDensityMin#01", Verbs.Enum.HasDynamicValue, "FluidDensityMin")]
        [SemanticFact("FluidDensityMax#01", Verbs.Enum.HasDynamicValue, "FluidDensityMax")]
        [SemanticFact("FDEUncertainty#01", Verbs.Enum.HasUncertaintyMin, "FluidDensityMin#01")]
        [SemanticFact("FDEUncertainty#01", Verbs.Enum.HasUncertaintyMax, "FluidDensityMax#01")]
        public UniformDrillingProperty FluidDensityMargin { get; set; } = new UniformDrillingProperty();

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

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [SemanticGaussianVariable("CuttingsDensityMeasured", "sigma_CuttingsDensityMeasured")]
        [SemanticSensorVariable("CuttingsDensityMeasured", "CuttingsDensityMeasured_prec", "CuttingsDensityMeasured_acc")]
        [SemanticFullScaleVariable("CuttingsDensityMeasured", "CuttingsDensityMeasured_fs", "CuttingsDensityMeasured_prop")]
        [SemanticExclusiveOr(1, 2, 3)]
        [SemanticFact("CuttingsDensityMeasured", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("CuttingsDensityMeasured#01", Nouns.Enum.PhysicalData)]
        [SemanticFact("CuttingsDensityMeasured#01", Verbs.Enum.HasDynamicValue, "CuttingsDensityMeasured")]
        [SemanticFact("CuttingsDensityMeasured#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        [SemanticFact("tos#01", Nouns.Enum.TopOfStringReferenceLocation)]
        [SemanticFact("CuttingsDensityMeasured#01", Verbs.Enum.IsPhysicallyLocatedAt, "tos#01")]
        [SemanticFact("MovingAverage", Nouns.Enum.MovingAverage)]
        [SemanticFact("CuttingsDensityMeasured#01", Verbs.Enum.IsTransformationOutput, "MovingAverage")]
        [SemanticFact("LiquidComponent#01", Nouns.Enum.LiquidComponent)]
        [SemanticFact("CuttingsComponent#01", Nouns.Enum.CuttingsComponent)]
        [SemanticFact("GasComponent#01", Nouns.Enum.GasComponent)]
        [SemanticFact("CuttingsDensityMeasured#01", Verbs.Enum.ConcernsAFluidComponent, "CuttingsComponent#01")]
        [ExcludeFact("CuttingsDensityMeasured#01", Verbs.Enum.ConcernsAFluidComponent, "LiquidComponent#01")]
        [ExcludeFact("CuttingsDensityMeasured#01", Verbs.Enum.ConcernsAFluidComponent, "GasComponent#01")]
        [OptionalFact(1, "sigma_CuttingsDensityMeasured", Nouns.Enum.DrillingSignal)]
        [OptionalFact(1, "sigma_CuttingsDensityMeasured#01", Nouns.Enum.DrillingDataPoint)]
        [OptionalFact(1, "sigma_CuttingsDensityMeasured#01", Verbs.Enum.HasValue, "sigma_CuttingsDensityMeasured")]
        [OptionalFact(1, "GaussianUncertainty#01", Nouns.Enum.GaussianUncertainty)]
        [OptionalFact(1, "CuttingsDensityMeasured#01", Verbs.Enum.HasUncertainty, "GaussianUncertainty#01")]
        [OptionalFact(1, "GaussianUncertainty#01", Verbs.Enum.HasUncertaintyStandardDeviation, "sigma_CuttingsDensityMeasured#01")]
        [OptionalFact(1, 11, "GaussianUncertainty#01", Verbs.Enum.HasUncertaintyMean, "CuttingsDensityMeasured#01")]
        [OptionalFact(2, "CuttingsDensityMeasured_prec", Nouns.Enum.DrillingSignal)]
        [OptionalFact(2, "CuttingsDensityMeasured_prec#01", Nouns.Enum.DrillingDataPoint)]
        [OptionalFact(2, "CuttingsDensityMeasured_prec#01", Verbs.Enum.HasValue, "CuttingsDensityMeasured_prec")]
        [OptionalFact(2, "CuttingsDensityMeasured_acc", Nouns.Enum.DrillingSignal)]
        [OptionalFact(2, "CuttingsDensityMeasured_acc#01", Nouns.Enum.DrillingDataPoint)]
        [OptionalFact(2, "CuttingsDensityMeasured_acc#01", Verbs.Enum.HasValue, "CuttingsDensityMeasured_acc")]
        [OptionalFact(2, "SensorUncertainty#01", Nouns.Enum.SensorUncertainty)]
        [OptionalFact(2, "SensorUncertainty#01", Verbs.Enum.HasUncertaintyPrecision, "CuttingsDensityMeasured_prec#01")]
        [OptionalFact(2, "SensorUncertainty#01", Verbs.Enum.HasUncertaintyAccuracy, "CuttingsDensityMeasured_acc#01")]
        [OptionalFact(2, "CuttingsDensityMeasured#01", Verbs.Enum.HasUncertainty, "SensorUncertainty#01")]
        [OptionalFact(2, 21, "SensorUncertainty#01", Verbs.Enum.HasUncertaintyMean, "CuttingsDensityMeasured#01")]
        [OptionalFact(3, "CuttingsDensityMeasured_fs", Nouns.Enum.DrillingSignal)]
        [OptionalFact(3, "CuttingsDensityMeasured_fs#01", Nouns.Enum.DrillingDataPoint)]
        [OptionalFact(3, "CuttingsDensityMeasured_fs#01", Verbs.Enum.HasValue, "CuttingsDensityMeasured_fs")]
        [OptionalFact(3, "CuttingsDensityMeasured_prop", Nouns.Enum.DrillingSignal)]
        [OptionalFact(3, "CuttingsDensityMeasured_prop#01", Nouns.Enum.DrillingDataPoint)]
        [OptionalFact(3, "CuttingsDensityMeasured_prop#01", Verbs.Enum.HasValue, "CuttingsDensityMeasured_prop")]
        [OptionalFact(3, "FullScaleUncertainty#01", Nouns.Enum.FullScaleUncertainty)]
        [OptionalFact(3, "FullScaleUncertainty#01", Verbs.Enum.HasFullScale, "CuttingsDensityMeasured_fs#01")]
        [OptionalFact(3, "FullScaleUncertainty#01", Verbs.Enum.HasProportionError, "CuttingsDensityMeasured_prop#01")]
        [OptionalFact(3, "CuttingsDensityMeasured#01", Verbs.Enum.HasUncertainty, "FullScaleUncertainty#01")]
        [OptionalFact(3, 31, "FullScaleUncertainty#01", Verbs.Enum.HasUncertaintyMean, "CuttingsDensityMeasured#01")]
        public SensorDrillingProperty CuttingsDensityMeasured { get; set; } = new SensorDrillingProperty();
    }
    class Example
    {
        static void GenerateSparQLForMD(StreamWriter writer, string propertyName, Dictionary<string, Tuple<int, string>>? queries)
        {
            if (writer != null && !string.IsNullOrEmpty(propertyName) && queries != null)
            {
                writer.WriteLine("# Semantic Queries for `" + propertyName + "`");
                foreach (var query in queries)
                {
                    if (query.Value != null)
                    {
                        writer.WriteLine("## " + query.Key);
                        writer.WriteLine("```sparql");
                        writer.WriteLine(query.Value.Item2);
                        writer.WriteLine("```");
                    }
                }
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
                    string tempFile = Path.Combine(dir.FullName, "Example05.md");
                    using (StreamWriter writer = new StreamWriter(tempFile))
                    {
                        var queries1 = GeneratorSparQLManifestFile.GetSparQLQueries(assembly, typeof(TestClass).FullName, "FluidDensitySetPoint");
                        GenerateSparQLForMD(writer, "FluidDensitySetPoint", queries1);
                        var queries2 = GeneratorSparQLManifestFile.GetSparQLQueries(assembly, typeof(TestClass).FullName, "FluidDensityMargin");
                        GenerateSparQLForMD(writer, "FluidDensityMargin", queries2);
                        var queries3 = GeneratorSparQLManifestFile.GetSparQLQueries(assembly, typeof(TestClass).FullName, "FluidDensityEstimated");
                        GenerateSparQLForMD(writer, "FluidDensityEstimated", queries3);
                        var queries4 = GeneratorSparQLManifestFile.GetSparQLQueries(assembly, typeof(TestClass).FullName, "FluidDensityMeasured");
                        GenerateSparQLForMD(writer, "FluidDensityMeasured", queries4);
                        var queries5 = GeneratorSparQLManifestFile.GetSparQLQueries(assembly, typeof(TestClass).FullName, "CuttingsDensityMeasured");
                        GenerateSparQLForMD(writer, "CuttingsDensityMeasured", queries5);
                    }
                }
            }
        }
    }
}
