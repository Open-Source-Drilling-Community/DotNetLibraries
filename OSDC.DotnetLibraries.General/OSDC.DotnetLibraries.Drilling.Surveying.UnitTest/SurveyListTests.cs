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
            List<SurveyPoint>? interpolatedTraj = SurveyPoint.Interpolate(traj, 10.0, new List<double>() { 4.5, 5.5, 125.0});
            Assert.NotNull(interpolatedTraj);
            Assert.AreEqual(24, interpolatedTraj.Count);
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
            List<SurveyPoint>? interpolatedTraj = SurveyPoint.Interpolate(traj, 10.0, new List<double>() {-1.0,  4.5, 5.5, 125.0, 205.0});
            Assert.NotNull(interpolatedTraj);
            Assert.AreEqual(24, interpolatedTraj.Count);
        }

    }
}
