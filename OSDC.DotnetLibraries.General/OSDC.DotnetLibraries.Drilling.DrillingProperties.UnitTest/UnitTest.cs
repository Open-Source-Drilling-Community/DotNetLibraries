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
            scalarDrillingProperty.DiracDistributionValue.Value = 1;

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
            uniformDrillingProperty.UniformValue.Min = 0;
            uniformDrillingProperty.UniformValue.Max = 1;

            string json = JsonSerializer.Serialize(uniformDrillingProperty);
            UniformDrillingProperty? deserialized = JsonSerializer.Deserialize<UniformDrillingProperty>(json);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.UniformValue.Min);
            Assert.NotNull(deserialized.UniformValue.Max);
            Assert.AreEqual((double)uniformDrillingProperty.UniformValue.Min, (double)deserialized.UniformValue.Min, 1e-6);
            Assert.AreEqual((double)uniformDrillingProperty.UniformValue.Max, (double)deserialized.UniformValue.Max, 1e-6);
        }
        [Test]
        public void Test3()
        {
            var gaussianDrillingProperty = new GaussianDrillingProperty();
            gaussianDrillingProperty.GaussianValue.Mean = 1;
            gaussianDrillingProperty.GaussianValue.StandardDeviation = 0.1;

            string json = JsonSerializer.Serialize(gaussianDrillingProperty);
            GaussianDrillingProperty? deserialized = JsonSerializer.Deserialize<GaussianDrillingProperty>(json);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.GaussianValue.Mean);
            Assert.NotNull(deserialized.GaussianValue.StandardDeviation);
            Assert.AreEqual((double)gaussianDrillingProperty.GaussianValue.Mean, (double)deserialized.GaussianValue.Mean, 1e-6);
            Assert.AreEqual((double)gaussianDrillingProperty.GaussianValue.StandardDeviation, (double)deserialized.GaussianValue.StandardDeviation, 1e-6);
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
            test.ScalarValue.DiracDistributionValue.Value = 1.0;
            test.UniformValue.UniformValue.Min = 0;
            test.UniformValue.UniformValue.Max = 2;
            test.GaussianValue.GaussianValue.Mean = 10;
            test.GaussianValue.GaussianValue.StandardDeviation = 3;
            test.GeneralDistributionValue.GeneralDistributionValue.Data = new List<double>() { 2, 4, 7, 1, 2, 3 };

            string json = JsonSerializer.Serialize(test);
            TestClass? deserialized = JsonSerializer.Deserialize<TestClass>(json);
            Assert.IsNotNull(deserialized);
            Assert.IsNotNull(deserialized.ScalarValue);
            Assert.IsNotNull(deserialized.UniformValue);
            Assert.IsNotNull(deserialized.GaussianValue);
            Assert.IsNotNull(deserialized.GeneralDistributionValue);
            Assert.NotNull(deserialized.ScalarValue.DiracDistributionValue.Value);
            Assert.AreEqual((double)test.ScalarValue.DiracDistributionValue.Value, (double)deserialized.ScalarValue.DiracDistributionValue.Value, 1e-6);
            Assert.NotNull(deserialized.UniformValue.UniformValue.Min);
            Assert.NotNull(deserialized.UniformValue.UniformValue.Max);
            Assert.AreEqual((double)test.UniformValue.UniformValue.Min, (double)deserialized.UniformValue.UniformValue.Min, 1e-6);
            Assert.AreEqual((double)test.UniformValue.UniformValue.Max, (double)deserialized.UniformValue.UniformValue.Max, 1e-6);
            Assert.NotNull(deserialized.GaussianValue.GaussianValue.Mean);
            Assert.NotNull(deserialized.GaussianValue.GaussianValue.StandardDeviation);
            Assert.AreEqual((double)test.GaussianValue.GaussianValue.Mean, (double)deserialized.GaussianValue.GaussianValue.Mean, 1e-6);
            Assert.AreEqual((double)test.GaussianValue.GaussianValue.StandardDeviation, (double)deserialized.GaussianValue.GaussianValue.StandardDeviation, 1e-6);
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