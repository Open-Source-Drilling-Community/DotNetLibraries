using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Statistics;

namespace OSDC.DotnetLibraries.Drilling.Surveying.UnitTest
{
    public class UncertaintyEllipsoidTests
    {
        [TestCase(0.0)]
        [TestCase(2.0)]
        public void CalculateScalesUncertaintyButNotBoreholeRadius(double scalingFactor)
        {
            const double confidenceFactor = 0.95;
            const double boreholeRadius = 0.25;
            SurveyStation station = new()
            {
                BoreholeRadius = boreholeRadius,
                EigenValues = new Vector3D { X = 1.0, Y = 4.0, Z = 9.0 }
            };
            UncertaintyEllipsoid ellipsoid = new()
            {
                EllipsoidSurveyStation = station,
                ConfidenceFactor = confidenceFactor,
                ScalingFactor = scalingFactor,
                CalculateHorizontalEllipse = false,
                CalculateVerticalEllipse = false,
                CalculatePerpendicularEllipse = false
            };

            bool calculated = ellipsoid.Calculate();

            Assert.That(calculated, Is.True);
            Assert.That(ellipsoid.EllipsoidRadii, Is.Not.Null);
            double confidenceScale = System.Math.Sqrt(Statistics.GetChiSquare3D(confidenceFactor));
            Assert.That(ellipsoid.EllipsoidRadii!.X, Is.EqualTo(scalingFactor * confidenceScale + boreholeRadius).Within(1e-12));
            Assert.That(ellipsoid.EllipsoidRadii.Y, Is.EqualTo(scalingFactor * confidenceScale * 2.0 + boreholeRadius).Within(1e-12));
            Assert.That(ellipsoid.EllipsoidRadii.Z, Is.EqualTo(scalingFactor * confidenceScale * 3.0 + boreholeRadius).Within(1e-12));
        }
    }
}
