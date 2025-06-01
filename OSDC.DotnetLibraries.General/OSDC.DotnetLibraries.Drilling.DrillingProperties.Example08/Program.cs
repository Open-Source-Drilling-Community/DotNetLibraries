using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;
using System.Reflection;
using DWIS.API.DTO;

namespace DrillingProperties
{

    [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
    [SemanticTypeVariable("MeasuredDensity")]
    [SemanticFact("MeasuredDensity", Nouns.Enum.DynamicDrillingSignal)]
    [SemanticFact("MeasuredDensity#01", Nouns.Enum.Measurement)]
    [SemanticFact("MeasuredDensity#01", Verbs.Enum.HasDynamicValue, "MeasuredDensity")]
    public class MeasuredFluidDensity
    {
        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [SemanticDiracVariable("TimeStamp")]
        [SemanticFact("TimeStamp", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("TimeStamp#01", Verbs.Enum.HasDynamicValue, "TimeStamp")]
        [SemanticFact("AbsoluteTimeRef", Nouns.Enum.AbsoluteTimeReference)]
        [SemanticFact("TimeStamp#01", Verbs.Enum.HasTimeReference, "AbsoluteTimeRef")]
        [SemanticFact("AcquisitionClock", Nouns.Enum.Clock, "Stratum", "3")]
        [SemanticFact("TimeStamp#01", Verbs.Enum.HasAcquisitionClock, "AcquisitionClock")]
        public DateTime TimeStamp { get; set; }

        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [SemanticGaussianVariable("FluidDensityMeasured", "sigma_FluidDensityMeasured")]
        [SemanticFact("FluidDensityMeasured", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensityMeasured#01", Nouns.Enum.PhysicalData)]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.HasDynamicValue, "FluidDensityMeasured")]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.MassDensityDrilling)]
        [SemanticFact("MovingAverage", Nouns.Enum.MovingAverage)]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.IsTransformationOutput, "MovingAverage")]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.HasSourceTime, "TimeStamp#01")]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.HasPressureReference, "pressure#01")]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.HasTemperatureReference, "temperature#01")]
        [SemanticFact("sigma_FluidDensityMeasured", Nouns.Enum.DrillingSignal)]
        [SemanticFact("sigma_FluidDensityMeasured#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("sigma_FluidDensityMeasured#01", Verbs.Enum.HasValue, "sigma_FluidDensityMeasured")]
        [SemanticFact("GaussianUncertainty#01", Nouns.Enum.GaussianUncertainty)]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.HasUncertainty, "GaussianUncertainty#01")]
        [SemanticFact("GaussianUncertainty#01", Verbs.Enum.HasUncertaintyStandardDeviation, "sigma_FluidDensityMeasured#01")]
        [SemanticFact("GaussianUncertainty#01", Verbs.Enum.HasUncertaintyMean, "FluidDensityMeasured#01")]
        public GaussianDrillingProperty MassDensity { get; set; } = new GaussianDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticGaussianVariable("temperature", "sigma_temperature")]
        [SemanticFact("temperature", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("temperature#01", Nouns.Enum.PhysicalData)]
        [SemanticFact("temperature#01", Verbs.Enum.HasDynamicValue, "temperature")]
        [SemanticFact("temperature#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.TemperatureDrilling)]
        [SemanticFact("MovingAverage", Nouns.Enum.MovingAverage)]
        [SemanticFact("temperature#01", Verbs.Enum.IsTransformationOutput, "MovingAverage")]
        [SemanticFact("sigma_temperature", Nouns.Enum.DrillingSignal)]
        [SemanticFact("sigma_temperature#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("sigma_temperature#01", Verbs.Enum.HasValue, "sigma_temperature")]
        [SemanticFact("GaussianUncertainty#01", Nouns.Enum.GaussianUncertainty)]
        [SemanticFact("temperature#01", Verbs.Enum.HasUncertainty, "GaussianUncertainty#01")]
        [SemanticFact("GaussianUncertainty#01", Verbs.Enum.HasUncertaintyStandardDeviation, "sigma_temperature#01")]
        [SemanticFact("GaussianUncertainty#01", Verbs.Enum.HasUncertaintyMean, "temperature#01")]
        public GaussianDrillingProperty Temperature { get; set; } = new GaussianDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticGaussianVariable("pressure", "sigma_pressure")]
        [SemanticFact("pressure", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("pressure#01", Nouns.Enum.PhysicalData)]
        [SemanticFact("pressure#01", Verbs.Enum.HasDynamicValue, "pressure")]
        [SemanticFact("pressure#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.PressureDrilling)]
        [SemanticFact("AbsolutePressure", Nouns.Enum.AbsolutePressureReference)]
        [SemanticFact("pressure#01", Verbs.Enum.HasPressureReferenceType, "AbsolutePressure")]
        [SemanticFact("MovingAverage", Nouns.Enum.MovingAverage)]
        [SemanticFact("pressure#01", Verbs.Enum.IsTransformationOutput, "MovingAverage")]
        [SemanticFact("sigma_pressure", Nouns.Enum.DrillingSignal)]
        [SemanticFact("sigma_pressure#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("sigma_pressure#01", Verbs.Enum.HasValue, "sigma_pressure")]
        [SemanticFact("GaussianUncertainty#01", Nouns.Enum.GaussianUncertainty)]
        [SemanticFact("pressure#01", Verbs.Enum.HasUncertainty, "GaussianUncertainty#01")]
        [SemanticFact("GaussianUncertainty#01", Verbs.Enum.HasUncertaintyStandardDeviation, "sigma_pressure#01")]
        [SemanticFact("GaussianUncertainty#01", Verbs.Enum.HasUncertaintyMean, "pressure#01")]
        public GaussianDrillingProperty Pressure { get; set; } = new GaussianDrillingProperty();


    }
    class Example
    {
        static void GenerateMermaidForMD(StreamWriter writer, string? typeName, string? mermaid)
        {
            if (writer != null && !string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(mermaid))
            {
                writer.WriteLine("# Semantic Graph for `" + typeName + "`");
                writer.WriteLine(mermaid);
            }
        }
        static void GenerateSparQLForMD(StreamWriter writer, string? typeName, Dictionary<string, QuerySpecification>? queries)
        {
            if (writer != null && !string.IsNullOrEmpty(typeName) && queries != null)
            {
                writer.WriteLine("# Semantic Queries for `" + typeName + "`");
                foreach (var query in queries)
                {
                    if (query.Value != null)
                    {
                        writer.WriteLine("## " + query.Key);
                        writer.WriteLine("```sparql");
                        writer.WriteLine(query.Value.SparQL);
                        writer.WriteLine("```");
                    }
                }
            }
        }
        static void Main()
        {
            MeasuredFluidDensity testClass = new MeasuredFluidDensity();
            Assembly? assembly = Assembly.GetAssembly(typeof(MeasuredFluidDensity));
            if (assembly != null)
            {
                string tempPath = Directory.GetCurrentDirectory();
                DirectoryInfo? dir = new DirectoryInfo(tempPath);
                dir = dir?.Parent?.Parent?.Parent;
                if (dir != null)
                {
                    string tempFile = Path.Combine(dir.FullName, "Example08.md");
                    using (StreamWriter writer = new StreamWriter(tempFile))
                    {
                        var manifestFileClass = GeneratorSparQLManifestFile.GetManifestFile(assembly, typeof(MeasuredFluidDensity).FullName, "SampleManifest", "ExampleCompany", "Test:");
                        if (manifestFileClass != null)
                        {
                            GenerateMermaidForMD(writer, "MeasuredFluidDensity", GeneratorSparQLManifestFile.GetMermaid(manifestFileClass));
                        }
                        var manifestFile0 = GeneratorSparQLManifestFile.GetManifestFile(assembly, typeof(MeasuredFluidDensity).FullName, "TimeStamp", "SampleManifest", "ExampleCompany", "Test:");
                        if (manifestFile0 != null)
                        {
                            GenerateMermaidForMD(writer, "TimeStamp", GeneratorSparQLManifestFile.GetMermaid(manifestFile0));
                        }
                        var manifestFile1 = GeneratorSparQLManifestFile.GetManifestFile(assembly, typeof(MeasuredFluidDensity).FullName, "MassDensity", "SampleManifest", "ExampleCompany", "Test:");
                        if (manifestFile1 != null)
                        {
                            GenerateMermaidForMD(writer, "MassDensity", GeneratorSparQLManifestFile.GetMermaid(manifestFile1));
                        }
                        var manifestFile2 = GeneratorSparQLManifestFile.GetManifestFile(assembly, typeof(MeasuredFluidDensity).FullName, "Temperature", "SampleManifest", "ExampleCompany", "Test:");
                        if (manifestFile2 != null)
                        {
                            GenerateMermaidForMD(writer, "Temperature", GeneratorSparQLManifestFile.GetMermaid(manifestFile2));
                        }
                        var manifestFile3 = GeneratorSparQLManifestFile.GetManifestFile(assembly, typeof(MeasuredFluidDensity).FullName, "Pressure", "SampleManifest", "ExampleCompany", "Test:");
                        if (manifestFile3 != null)
                        {
                            GenerateMermaidForMD(writer, "Pressure", GeneratorSparQLManifestFile.GetMermaid(manifestFile3));
                        }
                    }
                }
            }
        }
    }
}
