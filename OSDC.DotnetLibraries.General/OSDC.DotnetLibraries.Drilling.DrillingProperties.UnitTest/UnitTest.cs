using DWIS.Vocabulary.Schemas;
using OSDC.DotnetLibraries.General.Statistics;
using System.Reflection;
using System.Text.Json;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties.UnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var scalarDrillingProperty = new ScalarDrillingProperty();
            scalarDrillingProperty.ScalarValue = 1;

            string json = JsonSerializer.Serialize(scalarDrillingProperty);
            ScalarDrillingProperty? deserialized = JsonSerializer.Deserialize<ScalarDrillingProperty>(json);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.DiracDistributionValue.Value);
            Assert.AreEqual((double)scalarDrillingProperty.DiracDistributionValue.Value, (double)deserialized.DiracDistributionValue.Value, 1e-6);
        }

        [Test]
        public void Test2()
        {
            var uniformDrillingProperty = new UniformDrillingProperty();
            uniformDrillingProperty.Min = 0;
            uniformDrillingProperty.Max = 1;

            string json = JsonSerializer.Serialize(uniformDrillingProperty);
            UniformDrillingProperty? deserialized = JsonSerializer.Deserialize<UniformDrillingProperty>(json);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.Min);
            Assert.NotNull(deserialized.Max);
            Assert.AreEqual((double)uniformDrillingProperty.Min, (double)deserialized.Min, 1e-6);
            Assert.AreEqual((double)uniformDrillingProperty.Max, (double)deserialized.Max, 1e-6);
        }
        [Test]
        public void Test3()
        {
            var gaussianDrillingProperty = new GaussianDrillingProperty();
            gaussianDrillingProperty.Mean = 1;
            gaussianDrillingProperty.StandardDeviation = 0.1;

            string json = JsonSerializer.Serialize(gaussianDrillingProperty);
            GaussianDrillingProperty? deserialized = JsonSerializer.Deserialize<GaussianDrillingProperty>(json);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.Mean);
            Assert.NotNull(deserialized.StandardDeviation);
            Assert.AreEqual((double)gaussianDrillingProperty.Mean, (double)deserialized.Mean, 1e-6);
            Assert.AreEqual((double)gaussianDrillingProperty.StandardDeviation, (double)deserialized.StandardDeviation, 1e-6);
        }

        [Test]
        public void Test4()
        {
            var generalDistribution = new GeneralDistributionDrillingProperty();
            generalDistribution.GeneralDistributionValue.Data.Add(0);
            generalDistribution.GeneralDistributionValue.Data.Add(1);
            generalDistribution.GeneralDistributionValue.Data.Add(2);
            generalDistribution.GeneralDistributionValue.Data.Add(3);

            string json = JsonSerializer.Serialize(generalDistribution);
            GeneralDistributionDrillingProperty? deserialized = JsonSerializer.Deserialize<GeneralDistributionDrillingProperty>(json);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.GeneralDistributionValue.Data);
            Assert.AreEqual(deserialized.GeneralDistributionValue.Data.Count, generalDistribution.GeneralDistributionValue.Data.Count);
            for (int i = 0; i < generalDistribution.GeneralDistributionValue.Data.Count; i++)
            {
                Assert.AreEqual((double)generalDistribution.GeneralDistributionValue.Data[i], (double)deserialized.GeneralDistributionValue.Data[i], 1e-6);
            }
        }


        [Test]
        public void Test5()
        {
            TestClass test = new TestClass();
            test.ScalarValue.ScalarValue = 1.0;
            test.UniformValue.Min = 0;
            test.UniformValue.Max = 2;
            test.GaussianValue.Mean = 10;
            test.GaussianValue.StandardDeviation = 3;
            test.GeneralDistributionValue.GeneralDistributionValue.Data = new List<double>() { 2, 4, 7, 1, 2, 3 };

            string json = JsonSerializer.Serialize(test);
            TestClass? deserialized = JsonSerializer.Deserialize<TestClass>(json);
            Assert.IsNotNull(deserialized);
            Assert.IsNotNull(deserialized.ScalarValue);
            Assert.IsNotNull(deserialized.UniformValue);
            Assert.IsNotNull(deserialized.GaussianValue);
            Assert.IsNotNull(deserialized.GeneralDistributionValue);
            Assert.NotNull(deserialized.ScalarValue.ScalarValue);
            Assert.AreEqual((double)test.ScalarValue.ScalarValue, (double)deserialized.ScalarValue.ScalarValue, 1e-6);
            Assert.NotNull(deserialized.UniformValue.Min);
            Assert.NotNull(deserialized.UniformValue.Max);
            Assert.AreEqual((double)test.UniformValue.Min, (double)deserialized.UniformValue.Min, 1e-6);
            Assert.AreEqual((double)test.UniformValue.Max, (double)deserialized.UniformValue.Max, 1e-6);
            Assert.NotNull(deserialized.GaussianValue.Mean);
            Assert.NotNull(deserialized.GaussianValue.StandardDeviation);
            Assert.AreEqual((double)test.GaussianValue.Mean, (double)deserialized.GaussianValue.Mean, 1e-6);
            Assert.AreEqual((double)test.GaussianValue.StandardDeviation, (double)deserialized.GaussianValue.StandardDeviation, 1e-6);
            Assert.NotNull(deserialized.GeneralDistributionValue.GeneralDistributionValue.Data);
            Assert.AreEqual(deserialized.GeneralDistributionValue.GeneralDistributionValue.Data.Count, test.GeneralDistributionValue.GeneralDistributionValue.Data.Count);
            for (int i = 0; i < test.GeneralDistributionValue.GeneralDistributionValue.Data.Count; i++)
            {
                Assert.AreEqual((double)test.GeneralDistributionValue.GeneralDistributionValue.Data[i], (double)deserialized.GeneralDistributionValue.GeneralDistributionValue.Data[i], 1e-6);
            }
        }

        [Test]
        public void Test6()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var results = GenerateDrillingPropertyMetaData.GetDrillingPropertyMetaData(assembly);
            Assert.IsNotNull(results);
            Assert.AreEqual(4, results.Count);
        }
    }
}