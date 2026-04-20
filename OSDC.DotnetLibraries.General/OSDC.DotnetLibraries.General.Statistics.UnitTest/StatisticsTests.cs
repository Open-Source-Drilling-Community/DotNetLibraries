using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Statistics.UnitTest
{
    public class StatisticsTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_GetChiSquare3D()
        {
            var chiSquare = Statistics.GetChiSquare3D(0.95);

            Assert.That(chiSquare, Is.EqualTo(7.82));
        }

        [Test]
        public void Test_GetConfidenceFactor()
        {
            var confidenceFactor = Statistics.GetConfidenceFactor(0.95);

            Assert.That(confidenceFactor, Is.EqualTo(0.18604651162790697));

            var confidenceFactor095 = Statistics.GetConfidenceFactor(7.82);

            Assert.That(confidenceFactor095, Is.EqualTo(0.95));
        }
    }
}