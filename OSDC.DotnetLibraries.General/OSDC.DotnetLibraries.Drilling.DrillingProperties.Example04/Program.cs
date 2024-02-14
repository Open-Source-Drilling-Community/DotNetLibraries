using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;


namespace DrillingProperties
{
    public class TestClass
    {
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        public GaussianDrillingProperty FluidDensityEstimated { get; set; } = new GaussianDrillingProperty();

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        public double FluidDensityMeasured { get; set; }
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        public GaussianDrillingProperty DensitometerAccuracy { get; set; } = new GaussianDrillingProperty();
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        public GaussianDrillingProperty DensitometerPrecision { get; set; } = new GaussianDrillingProperty();

        public double? RealizeFluidDensityEstimated()
        {
            return FluidDensityEstimated.Realize();
        }

        public double? RealizeFluidDensityMeasured()
        {
            double? accuracy = DensitometerAccuracy.Realize();
            return RealizeFluidDensityMeasured(accuracy);
        }

        public double? RealizeFluidDensityMeasured(double? accuracy)
        {
            double? precision = DensitometerPrecision.Realize();
            return FluidDensityMeasured + accuracy + precision;
        }
    }
    class Example
    {
        static void Main()
        {
            TestClass testClass = new TestClass();
            testClass.FluidDensityEstimated.GaussianValue.Mean = 1000.0;
            testClass.FluidDensityEstimated.GaussianValue.StandardDeviation = 10.0;
            testClass.FluidDensityMeasured = 999.0;
            testClass.DensitometerAccuracy.GaussianValue.Mean = 0.0;
            testClass.DensitometerAccuracy.GaussianValue.StandardDeviation = 1.0; // Anton Paar L-Dens 3300
            testClass.DensitometerPrecision.GaussianValue.Mean = 0.0;
            testClass.DensitometerPrecision.GaussianValue.StandardDeviation = 0.1;

            double? densitometerAccuracy = testClass.DensitometerAccuracy.Realize();
            for (int i = 0; i < 10; i++)
            {
                double? fluidDensityEstimated = testClass.RealizeFluidDensityEstimated();
                double? fluidDensityMeasured1 = testClass.RealizeFluidDensityMeasured();
                double? fluidDensityMeasured2 = testClass.RealizeFluidDensityMeasured(densitometerAccuracy);
                Console.WriteLine(fluidDensityEstimated + "\t" + fluidDensityMeasured1 + "\t" + fluidDensityMeasured2);
            }
        }
    }
}
