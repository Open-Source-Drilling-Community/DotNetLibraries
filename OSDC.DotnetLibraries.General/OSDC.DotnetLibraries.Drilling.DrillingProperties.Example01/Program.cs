using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using System.Globalization;

namespace DrillingProperties
{
    class TestClass
    {
        public ScalarDrillingProperty Value1 { get; set; } = new ScalarDrillingProperty();
        public UniformDrillingProperty Value2 { get; set; } = new UniformDrillingProperty();
        public GaussianDrillingProperty Value3 { get; set; } = new GaussianDrillingProperty();
        public GeneralDistributionDrillingProperty Value4 { get; set; } = new GeneralDistributionDrillingProperty();
        public SensorDrillingProperty Value5 { get; set; } = new SensorDrillingProperty();
        public FullScaleDrillingProperty Value6 { get; set; }= new FullScaleDrillingProperty();
    }
    class Example
    {
        static void Main()
        {
            TestClass testClass = new TestClass();
            testClass.Value1.ScalarValue = 1.0;
            testClass.Value2.Min = -1.0;
            testClass.Value2.Max = 1.0;
            testClass.Value3.Mean = 10.0;
            testClass.Value3.StandardDeviation = 0.5;
            testClass.Value4.Histogram = new Tuple<double, double>[] {
                new Tuple<double, double>(0.0, 0.1),
                new Tuple<double, double>(1.0, 0.2),
                new Tuple<double, double>(2.0, 0.3),
                new Tuple<double, double>(3.0, 0.4)
            };
            testClass.Value5.Accuracy = 0.1;
            testClass.Value5.Precision = 0.01;
            testClass.Value5.Mean = 1.0;
            testClass.Value6.FullScale = 10.0;
            testClass.Value6.ProportionError = 0.001;
            testClass.Value6.Mean = 1.0;
            for (int i = 0; i < 10; i++)
            {
                Realize(testClass);
            }
        }

        static void Realize(TestClass testClass)
        {
            double? value1 = testClass.Value1.Realize();
            double? value2 = testClass.Value2.Realize();
            double? value3 = testClass.Value3.Realize();
            double? value4 = testClass.Value4.Realize();
            double? value5 = testClass.Value5.Realize();
            double? value6 = testClass.Value6.Realize();
            Console.WriteLine("Realized values:" +
                " value1 = " + value1?.ToString("F", CultureInfo.InvariantCulture) +
                " value2 = " + value2?.ToString("F", CultureInfo.InvariantCulture) +
                " value3 = " + value3?.ToString("F", CultureInfo.InvariantCulture) +
                " value4 = " + value4?.ToString("F", CultureInfo.InvariantCulture) +
                " value5 = " + value5?.ToString("F", CultureInfo.InvariantCulture) +
                " value6 = " + value6?.ToString("F", CultureInfo.InvariantCulture));
        }
    }
}