using DWIS.Vocabulary.Schemas;
using OSDC.DotnetLibraries.General.Statistics;
using System.Reflection;
using System.Text.Json;

namespace OSDC.DotnetLibraries.General.DrillingProperties.UnitTest
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
            Assert.AreEqual(new Guid("bc51b872-cf4d-4474-86a9-9f4a83efe905"), deserialized.ScalarValue.MetaDataID);
            Assert.AreEqual(new Guid("52539cdd-1918-4f7d-8cdd-b9bc9bb818c3"), deserialized.UniformValue.MetaDataID);
            Assert.AreEqual(new Guid("cc46f945-c9f5-4605-b1b3-c391239416fb"), deserialized.GaussianValue.MetaDataID);
            Assert.AreEqual(new Guid("ae4ed40a-e7d2-486d-8368-61528bc95cee"), deserialized.GeneralDistributionValue.MetaDataID);
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
            Assert.IsNotNull(results[new Guid("bc51b872-cf4d-4474-86a9-9f4a83efe905")]);
            Assert.IsNotNull(results[new Guid("52539cdd-1918-4f7d-8cdd-b9bc9bb818c3")]);
            Assert.IsNotNull(results[new Guid("cc46f945-c9f5-4605-b1b3-c391239416fb")]);
            Assert.IsNotNull(results[new Guid("ae4ed40a-e7d2-486d-8368-61528bc95cee")]);
        }
    }
}