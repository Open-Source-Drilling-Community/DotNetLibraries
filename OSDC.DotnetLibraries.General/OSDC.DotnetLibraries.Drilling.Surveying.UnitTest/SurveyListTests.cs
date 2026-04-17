using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.Surveying.UnitTest
{
    public class SurveyListTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            List<SurveyPoint> traj = new List<SurveyPoint>() {
               new SurveyPoint() { X = 6560503.255, Y = 635328.164, Z = 0, MD = 0, Inclination = 0, Azimuth = 0 },
               new SurveyPoint() { MD = 100, Inclination = 0, Azimuth = 0},
               new SurveyPoint() { MD = 200, Inclination = 0, Azimuth = 0}
             };
            bool ok = SurveyPoint.CompleteSurvey(traj);
            Assert.IsTrue(ok);
            Assert.NotNull(traj[0].X);
            Assert.NotNull(traj[0].Y);
            Assert.NotNull(traj[0].Z);
            Assert.NotNull(traj.Last<SurveyPoint>().X);
            Assert.NotNull(traj.Last<SurveyPoint>().Y);
            Assert.NotNull(traj.Last<SurveyPoint>().Z);
            Assert.AreEqual((double)traj[0].X, (double)traj.Last<SurveyPoint>().X, 1e-3);
            Assert.AreEqual((double)traj[0].Y, (double)traj.Last<SurveyPoint>().Y, 1e-3);
            Assert.AreEqual((double)traj.Last<SurveyPoint>().MD, (double)traj.Last<SurveyPoint>().Z, 1e-3);
        }

        [Test]
        public void Test2()
        {
            List<SurveyPoint> traj = new List<SurveyPoint>() {
               new SurveyPoint() { X = 6560503.255, Y = 635328.164, Z = 0, MD = 0, Inclination = 0, Azimuth = 0 },
               new SurveyPoint() { MD = 100, Inclination = 1.0 * Numeric.PI/180.0, Azimuth = 30.0 * Numeric.PI/180.0},
               new SurveyPoint() { MD = 200, Inclination = 3.0 * Numeric.PI/180.0, Azimuth = 31.0 * Numeric.PI/180.0}
             };
            bool ok = SurveyPoint.CompleteSurvey(traj);
            Assert.IsTrue(ok);
            SurveyPoint sv = new SurveyPoint();
            ok = SurveyPoint.InterpolateAtAbscissa(traj, 100.0, sv);
            Assert.IsTrue(ok);
            Assert.AreEqual(traj[1].TVD.Value, sv.TVD.Value, 1e-4);
            Assert.AreEqual(traj[1].RiemannianNorth.Value, sv.RiemannianNorth.Value, 1e-4);
            Assert.AreEqual(traj[1].RiemannianEast.Value, sv.RiemannianEast.Value, 1e-4);
            Assert.AreEqual(traj[1].Abscissa.Value, sv.Abscissa.Value, 1e-4);
            Assert.AreEqual(traj[1].Inclination.Value, sv.Inclination.Value, 1e-6);
            Assert.AreEqual(traj[1].Azimuth.Value, sv.Azimuth.Value, 1e-6);
        }

        [Test]
        public void Test3()
        {
            List<SurveyPoint> traj = new List<SurveyPoint>() {
               new SurveyPoint() { X = 6560503.255, Y = 635328.164, Z = 0, MD = 0, Inclination = 0, Azimuth = 0 },
               new SurveyPoint() { MD = 100, Inclination = 1.0 * Numeric.PI/180.0, Azimuth = 30.0 * Numeric.PI/180.0},
               new SurveyPoint() { MD = 200, Inclination = 3.0 * Numeric.PI/180.0, Azimuth = 31.0 * Numeric.PI/180.0}
             };
            bool ok = SurveyPoint.CompleteSurvey(traj);
            Assert.IsTrue(ok);
            List<SurveyPoint>? interpolatedTraj = SurveyPoint.Interpolate(traj, 10.0);
            Assert.NotNull(interpolatedTraj);
            Assert.AreEqual(21, interpolatedTraj.Count);
        }
        [Test]
        public void Test4()
        {
            List<SurveyPoint> traj = new List<SurveyPoint>() {
               new SurveyPoint() { X = 6560503.255, Y = 635328.164, Z = 0, MD = 0, Inclination = 0, Azimuth = 0 },
               new SurveyPoint() { MD = 100, Inclination = 1.0 * Numeric.PI/180.0, Azimuth = 30.0 * Numeric.PI/180.0},
               new SurveyPoint() { MD = 200, Inclination = 3.0 * Numeric.PI/180.0, Azimuth = 31.0 * Numeric.PI/180.0}
             };
            bool ok = SurveyPoint.CompleteSurvey(traj);
            Assert.IsTrue(ok);
            List<SurveyPoint>? interpolatedTraj = SurveyPoint.Interpolate(traj, 10.0, null, new List<(double, string)>() { (4.5, "a"), (5.5, "b"), (125.0, "c") });
            Assert.NotNull(interpolatedTraj);
            Assert.AreEqual(24, interpolatedTraj.Count);
            Assert.AreEqual("a", interpolatedTraj.Single(p => p.MD == 4.5).Annotation);
            Assert.AreEqual("b", interpolatedTraj.Single(p => p.MD == 5.5).Annotation);
            Assert.AreEqual("c", interpolatedTraj.Single(p => p.MD == 125.0).Annotation);
        }

        [Test]
        public void Test5()
        {
            List<SurveyPoint> traj = new List<SurveyPoint>() {
               new SurveyPoint() { X = 6560503.255, Y = 635328.164, Z = 0, MD = 0, Inclination = 0, Azimuth = 0 },
               new SurveyPoint() { MD = 100, Inclination = 1.0 * Numeric.PI/180.0, Azimuth = 30.0 * Numeric.PI/180.0},
               new SurveyPoint() { MD = 200, Inclination = 3.0 * Numeric.PI/180.0, Azimuth = 31.0 * Numeric.PI/180.0}
             };
            bool ok = SurveyPoint.CompleteSurvey(traj);
            Assert.IsTrue(ok);
            List<SurveyPoint>? interpolatedTraj = SurveyPoint.Interpolate(traj, 10.0, null, new List<(double, string)>() { (-1.0, "x"), (4.5, "a"), (5.5, "b"), (125.0, "c"), (205.0, "y") });
            Assert.NotNull(interpolatedTraj);
            Assert.AreEqual(24, interpolatedTraj.Count);
        }

        [Test]
        public void Test6ReferenceDepthAnchorsStepInterpolation()
        {
            List<SurveyPoint> traj = new List<SurveyPoint>() {
               new SurveyPoint() { X = 6560503.255, Y = 635328.164, Z = 0, MD = 0, Inclination = 0, Azimuth = 0 },
               new SurveyPoint() { MD = 100, Inclination = 1.0 * Numeric.PI/180.0, Azimuth = 30.0 * Numeric.PI/180.0},
               new SurveyPoint() { MD = 200, Inclination = 3.0 * Numeric.PI/180.0, Azimuth = 31.0 * Numeric.PI/180.0}
             };
            bool ok = SurveyPoint.CompleteSurvey(traj);
            Assert.IsTrue(ok);

            List<SurveyPoint>? interpolatedTraj = SurveyPoint.Interpolate(traj, 10.0, 5.0);
            Assert.NotNull(interpolatedTraj);
            Assert.That(interpolatedTraj!.Select(p => p.MD).ToArray(), Is.EqualTo(new double?[]
            {
                5.0, 15.0, 25.0, 35.0, 45.0, 55.0, 65.0, 75.0, 85.0, 95.0,
                105.0, 115.0, 125.0, 135.0, 145.0, 155.0, 165.0, 175.0, 185.0, 195.0
            }));
        }

        [Test]
        public void Test7NullStepInterpolatesOnlyExplicitAbscissas()
        {
            List<SurveyPoint> traj = new List<SurveyPoint>() {
               new SurveyPoint() { X = 6560503.255, Y = 635328.164, Z = 0, MD = 0, Inclination = 0, Azimuth = 0, Annotation = "source-start" },
               new SurveyPoint() { MD = 100, Inclination = 1.0 * Numeric.PI/180.0, Azimuth = 30.0 * Numeric.PI/180.0, Annotation = "source-mid"},
               new SurveyPoint() { MD = 200, Inclination = 3.0 * Numeric.PI/180.0, Azimuth = 31.0 * Numeric.PI/180.0, Annotation = "source-end"}
             };
            bool ok = SurveyPoint.CompleteSurvey(traj);
            Assert.IsTrue(ok);

            List<SurveyPoint>? interpolatedTraj = SurveyPoint.Interpolate(traj, null, null, new List<(double, string)>() { (0.0, "start"), (100.0, "mid"), (125.0, "marker"), (200.0, "end") });
            Assert.NotNull(interpolatedTraj);
            Assert.That(interpolatedTraj!.Select(p => p.MD).ToArray(), Is.EqualTo(new double?[] { 0.0, 100.0, 125.0, 200.0 }));
            Assert.That(interpolatedTraj.Select(p => p.Annotation).ToArray(), Is.EqualTo(new string?[] { "start", "mid", "marker", "end" }));
            Assert.False(ReferenceEquals(traj[0], interpolatedTraj[0]));
            Assert.False(ReferenceEquals(traj[1], interpolatedTraj[1]));
            Assert.False(ReferenceEquals(traj[2], interpolatedTraj[3]));
        }

    }
}
