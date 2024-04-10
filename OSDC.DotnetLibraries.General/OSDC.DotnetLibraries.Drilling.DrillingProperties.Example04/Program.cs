using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;

namespace DrillingProperties
{
    public class TestClass
    {
        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [SemanticGaussianVariable("FluidDensityEstimated", "FluidDensityEstimatedStdDev")]
        [SemanticFact("FluidDensityEstimated",  Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensityEstimated#01",  Nouns.Enum.ComputedData)]
        [SemanticFact("FluidDensityEstimated#01", Verbs.Enum.HasDynamicValue, "FluidDensityEstimated")]
        [SemanticFact("FluidDensityEstimated#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        [SemanticFact("FDEUncertainty#01",  Nouns.Enum.GaussianUncertainty)]
        [SemanticFact("FluidDensityEstimated#01", Verbs.Enum.HasUncertainty, "FDEUncertainty#01")]
        [SemanticFact("FluidDensityEstimatedStdDev",  Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensityEstimatedStdDev#01",  Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("FluidDensityEstimatedStdDev#01", Verbs.Enum.HasStaticValue, "FluidDensityEstimatedStdDev")]
        [SemanticFact("FDEUncertainty#01", Verbs.Enum.HasUncertaintyStandardDeviation, "FluidDensityEstimatedStdDev#01")]
        [SemanticFact("FDEUncertainty#01", Verbs.Enum.HasUncertaintyMean, "FluidDensityEstimated#01")]
        public GaussianDrillingProperty FluidDensityEstimated { get; set; } = new GaussianDrillingProperty();

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [SemanticSensorVariable("FluidDensityMeasured", "FluidDensitySensorPrecision", "FluidDensitySensorAccuracy")]
        [SemanticFact("FluidDensityMeasured",  Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensityMeasured#01",  Nouns.Enum.Measurement)]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.HasDynamicValue, "FluidDensityMeasured")]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        [SemanticFact("FDMUncertainty#01",  Nouns.Enum.SensorUncertainty)]
        [SemanticFact("FluidDensityMeasured#01", Verbs.Enum.HasUncertainty, "FDMUncertainty#01")]
        [SemanticFact("FluidDensitySensorPrecision",  Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensitySensorPrecision#01",  Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("FluidDensitySensorPrecision#01", Verbs.Enum.HasStaticValue, "FluidDensitySensorPrecision")]
        [SemanticFact("FDMUncertainty#01", Verbs.Enum.HasUncertaintyPrecision, "FluidDensitySensorPrecision#01")]
        [SemanticFact("FluidDensitySensorAccuracy",  Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("FluidDensitySensorAccuracy#01",  Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("FluidDensitySensorAccuracy#01", Verbs.Enum.HasStaticValue, "FluidDensitySensorAccuracy")]
        [SemanticFact("FDMUncertainty#01", Verbs.Enum.HasUncertaintyAccuracy, "FluidDensitySensorAccuracy#01")]
        [SemanticFact("FDMUncertainty#01", Verbs.Enum.HasUncertaintyMean, "FluidDensityMeasured#01")]
        public SensorDrillingProperty FluidDensityMeasured { get; set; } = new SensorDrillingProperty();

        public double? RealizeFluidDensityEstimated()
        {
            return FluidDensityEstimated.Realize();
        }

        public double? RealizeFluidDensityMeasured()
        {
            return FluidDensityMeasured.Realize();
        }

    }
    class Example
    {
        static void Main()
        {
            TestClass testClass = new TestClass();
            testClass.FluidDensityEstimated.Mean = 1000.0;
            testClass.FluidDensityEstimated.StandardDeviation = 10.0;
            testClass.FluidDensityMeasured.Mean = 999.0;
            testClass.FluidDensityMeasured.Accuracy = 1.0; // Anton Paar L-Dens 3300
            testClass.FluidDensityMeasured.Precision = 0.1; // Anton Paar L-Dens 3300

            for (int i = 0; i < 10; i++)
            {
                double? fluidDensityEstimated = testClass.RealizeFluidDensityEstimated();
                double? fluidDensityMeasured = testClass.RealizeFluidDensityMeasured();
                Console.WriteLine(fluidDensityEstimated + "\t" + fluidDensityMeasured);
            }
        }
    }
}
