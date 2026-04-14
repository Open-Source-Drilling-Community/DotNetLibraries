using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying.UnitTest
{
    public class SurveyTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            double acc = 1e-6;
            SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Abscissa = 10, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0 };
            sv1.CompleteFromSIA(sv2);
            SurveyPoint result = new SurveyPoint();
            sv1.InterpolateAtAbscissa(sv2, 0, result);
            Assert.AreEqual(sv1.Abscissa.Value, result.Abscissa.Value, acc);
            Assert.AreEqual(sv1.Inclination.Value, result.Inclination.Value, acc);
            Assert.AreEqual(sv1.Azimuth.Value, result.Azimuth.Value, acc);
            Assert.AreEqual(sv1.X.Value, result.X.Value, acc);
            Assert.AreEqual(sv1.Y.Value, result.Y.Value, acc);
            Assert.AreEqual(sv1.Z.Value, result.Z.Value, acc);
        }
        [Test]
        public void Test2()
        {
            double acc = 1e-6;
            SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Abscissa = 10, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0 };
            sv1.CompleteFromSIA(sv2);
            SurveyPoint result = new SurveyPoint();
            sv1.InterpolateAtAbscissa(sv2, sv2.Abscissa.Value, result);
            Assert.AreEqual(sv2.Abscissa.Value, result.Abscissa.Value, acc);
            Assert.AreEqual(sv2.Inclination.Value, result.Inclination.Value, acc);
            Assert.AreEqual(sv2.Azimuth.Value, result.Azimuth.Value, acc);
            Assert.AreEqual(sv2.X.Value, result.X.Value, acc);
            Assert.AreEqual(sv2.Y.Value, result.Y.Value, acc);
            Assert.AreEqual(sv2.Z.Value, result.Z.Value, acc);
        }
        [Test]
        public void Test3()
        {
            double acc = 1e-6;
            SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Abscissa = 10, Inclination = 0, Azimuth = 0 };
            sv1.CompleteFromSIA(sv2);
            SurveyPoint result = new SurveyPoint();
            sv1.InterpolateAtAbscissa(sv2, sv2.Abscissa.Value / 2.0, result);
            Assert.AreEqual(sv2.Abscissa.Value / 2.0, result.Abscissa.Value, acc);
            Assert.AreEqual(sv2.Inclination.Value, result.Inclination.Value, acc);
            Assert.AreEqual(sv2.Azimuth.Value, result.Azimuth.Value, acc);
            Assert.AreEqual(sv2.X.Value, result.X.Value, acc);
            Assert.AreEqual(sv2.Y.Value, result.Y.Value, acc);
            Assert.AreEqual(sv2.Z.Value / 2.0, result.Z.Value, acc);
        }

        [Test]
        public void Test4()
        {
            SurveyPoint sv1 = new SurveyPoint() { Latitude = 0, Longitude = 0, Z = 0 };
            Assert.AreEqual(0, sv1.X, 1e-3);
            Assert.AreEqual(0, sv1.Y, 1e-3);
        }

        [Test]
        public void Test5()
        {
            SurveyPoint sv1 = new SurveyPoint() { X = 0, Y = 0, Z = 0 };
            Assert.AreEqual(0, sv1.Latitude, 1e-5);
            Assert.AreEqual(0, sv1.Longitude, 1e-5);
        }

        [Test]
        public void Test6()
        {
            SurveyPoint sv1 = new SurveyPoint() { Latitude = 58.93438 * System.Math.PI / 180.0, Longitude = 5 * System.Math.PI / 180.0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Latitude = sv1.Latitude, Longitude = 0, Z = 0 };
            double? distEast1 = sv2.Riemannian2DDistanceVincenty(sv1.Latitude, sv1.Longitude);
            double? distEast2 = sv2.Riemannian2DDistanceKarney(sv1.Latitude, sv1.Longitude);
            Assert.NotNull(distEast1);
            Assert.NotNull(distEast2);
            Assert.That(distEast1.Value, Is.EqualTo(distEast2.Value).Within(1e-3));
            Assert.NotNull(distEast1);
            Assert.LessOrEqual(distEast1.Value, sv1.Y);
            SurveyPoint sv3 = new SurveyPoint() { Latitude = 0, Longitude = sv1.Longitude, Z = 0 };
            double? distNorth1 = sv3.Riemannian2DDistanceVincenty(sv1.Latitude, sv1.Longitude);
            double? distNorth2 = sv3.Riemannian2DDistanceKarney(sv1.Latitude, sv1.Longitude);
            Assert.NotNull(distNorth1);
            Assert.NotNull(distNorth2);
            Assert.That(distNorth1.Value, Is.EqualTo(distNorth2.Value).Within(1e-3));
            double delta = distNorth1.Value - sv1.X.Value;
            Assert.That(distNorth1.Value, Is.EqualTo(sv1.X).Within(1e-3));
        }

        [Test]
        public void Test7()
        {
            const int latitudeCount = 100;
            const int longitudeCount = 100;
            const double latitudeTolerance = 1e-6;
            const double longitudeTolerance = 1e-6;

            static double NormalizeLongitude(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0.0)
                {
                    angle += twoPi;
                }
                return angle;
            }

            static double SignedLongitudeDifference(double angle1, double angle2)
            {
                double twoPi = 2.0 * Numeric.PI;
                double diff = NormalizeLongitude(angle1) - NormalizeLongitude(angle2);
                if (diff > Numeric.PI)
                {
                    diff -= twoPi;
                }
                else if (diff < -Numeric.PI)
                {
                    diff += twoPi;
                }
                return diff;
            }

            static (double Latitude, double Longitude) Canonicalize(double latitude, double longitude)
            {
                double twoPi = 2.0 * Numeric.PI;
                double canonicalLatitude = latitude;
                double canonicalLongitude = longitude;

                while (canonicalLatitude > Numeric.PI / 2.0)
                {
                    canonicalLatitude = Numeric.PI - canonicalLatitude;
                    canonicalLongitude += Numeric.PI;
                }

                while (canonicalLatitude < -Numeric.PI / 2.0)
                {
                    canonicalLatitude = -Numeric.PI - canonicalLatitude;
                    canonicalLongitude += Numeric.PI;
                }

                canonicalLongitude %= twoPi;
                if (canonicalLongitude < 0.0)
                {
                    canonicalLongitude += twoPi;
                }

                return (canonicalLatitude, canonicalLongitude);
            }

            List<string> failures = new();

            for (int i = 0; i < latitudeCount; i++)
            {
                double latitude = -Numeric.PI + (2.0 * Numeric.PI) * (i + 0.5) / latitudeCount;

                for (int j = 0; j < longitudeCount; j++)
                {
                    double longitude = (2.0 * Numeric.PI) * (j + 0.5) / longitudeCount;

                    SurveyPoint sv1 = new SurveyPoint() { Latitude = latitude, Longitude = longitude, TVD = 0 };
                    Assert.NotNull(sv1.RiemannianNorth);
                    Assert.NotNull(sv1.RiemannianEast);

                    SurveyPoint sv2 = new SurveyPoint()
                    {
                        RiemannianNorth = sv1.RiemannianNorth,
                        RiemannianEast = sv1.RiemannianEast,
                        TVD = 0
                    };

                    if (sv2.Latitude == null || sv2.Longitude == null)
                    {
                        failures.Add($"Null inverse result for latitude={latitude:R}, longitude={longitude:R}.");
                        continue;
                    }

                    var expected = Canonicalize(latitude, longitude);
                    double latitudeError = System.Math.Abs(sv2.Latitude.Value - expected.Latitude);
                    double longitudeError = System.Math.Abs(SignedLongitudeDifference(sv2.Longitude.Value, expected.Longitude));

                    if (latitudeError > latitudeTolerance || longitudeError > longitudeTolerance)
                    {
                        failures.Add(
                            $"Round-trip mismatch for latitude={latitude:R}, longitude={longitude:R}: " +
                            $"expected canonical latitude={expected.Latitude:R}, expected canonical longitude={expected.Longitude:R}, " +
                            $"inverse latitude={sv2.Latitude.Value:R}, inverse longitude={sv2.Longitude.Value:R}, " +
                            $"dLat={latitudeError:R}, dLon={longitudeError:R}.");
                    }
                }
            }

            Assert.That(failures, Is.Empty,
                failures.Count == 0
                    ? "All latitude/longitude round trips succeeded."
                    : string.Join(Environment.NewLine, failures.Take(20)));
        }


        [Test]
        public void Test8()
        {
            SurveyPoint sv = new SurveyPoint() { Z = 0, Latitude = 0, Longitude = 0 };
            SphericalPoint3D sphericalPoint3D = sv.GetSphericalPoint();
            Assert.NotNull(sphericalPoint3D);
            Assert.NotNull(sphericalPoint3D.X);
            Assert.NotNull(sphericalPoint3D.Y);
            Assert.NotNull(sphericalPoint3D.Z);
            Assert.AreEqual(General.Common.Constants.EarthSemiMajorAxisWGS84, sphericalPoint3D.X, 1e-6);
            Assert.AreEqual(0, sphericalPoint3D.Y, 1e-6);
            Assert.AreEqual(0, sphericalPoint3D.Z, 1e-6);
        }

        [Test]
        public void Test9()
        {
            SurveyPoint sv = new SurveyPoint() { Z = 0, Latitude = 0, Longitude = System.Math.PI / 2.0 };
            SphericalPoint3D sphericalPoint3D = sv.GetSphericalPoint();
            Assert.NotNull(sphericalPoint3D);
            Assert.NotNull(sphericalPoint3D.X);
            Assert.NotNull(sphericalPoint3D.Y);
            Assert.NotNull(sphericalPoint3D.Z);
            Assert.AreEqual(0, sphericalPoint3D.X, 1e-6);
            Assert.AreEqual(General.Common.Constants.EarthSemiMajorAxisWGS84, sphericalPoint3D.Y, 1e-6);
            Assert.AreEqual(0, sphericalPoint3D.Z, 1e-6);
        }

        [Test]
        public void Test10()
        {
            double lat1 = 0;
            double lat2 = 0;
            double lon1 = 0;
            double lon2 = 0;
            SurveyPoint sv1 = new SurveyPoint() { Latitude = lat1, Longitude = lon1, TVD = 0 };
            double? dist = sv1.Riemannian2DDistance(lat2, lon2);
            Assert.NotNull(dist);
            Assert.AreEqual(0, dist, 1e-4);
        }
        [Test]
        public void Test11()
        {
            double lat1 = 0;
            double lat2 = 0;
            double lon1 = 0;
            double lon2 = 1.0 * Numeric.PI / 180.0;
            SurveyPoint sv1 = new SurveyPoint() { Latitude = lat1, Longitude = lon1, TVD = 0 };
            double? dist = sv1.Riemannian2DDistance(lat2, lon2);
            double R = Constants.EarthSemiMajorAxisWGS84;
            double arc = R * (lon2 - lon1);
            Assert.NotNull(dist);
            Assert.AreEqual(arc, dist, 1e-4);
        }
        [Test]
        public void Test12()
        {
            double lat1 = 75.0 * Numeric.PI / 180.0;
            double lat2 = lat1;
            double lon1 = 0;
            double lon2 = 1.0 * Numeric.PI / 180.0;
            SurveyPoint sv1 = new SurveyPoint() { Latitude = lat1, Longitude = lon1, TVD = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Latitude = lat2, Longitude = lon2, TVD = 0 };
            double? dist = sv1.Riemannian2DDistance(lat2, lon2);
            double a = Constants.EarthSemiMajorAxisWGS84;
            double f = 1.0 / Constants.EarthInverseFlateningWGS84;
            double b = a - a * f;
            double sinLat = System.Math.Sin(lat1);
            double e2 = (a * a - b * b) / (a * a);
            double R = a / System.Math.Sqrt(1 - e2 * sinLat * sinLat);
            R = System.Math.Cos(lat1) * R;
            double arc = R * (lon2 - lon1);
            Assert.NotNull(dist);
            Assert.NotNull(sv2.RiemannianEast);
            Assert.NotNull(sv1.RiemannianEast);
            Assert.AreEqual(sv2.RiemannianEast.Value - sv1.RiemannianEast.Value, dist, 0.75);
        }
        [Test]
        public void Test13()
        {
            double lat1 = 0.0 * Numeric.PI / 180.0;
            double lat2 = 1.0 * Numeric.PI / 180.0;
            double lon1 = 0;
            double lon2 = 0;
            SurveyPoint sv1 = new SurveyPoint() { Latitude = lat1, Longitude = lon1, TVD = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Latitude = lat2, Longitude = lon2, TVD = 0 };
            double? dist = sv1.Riemannian2DDistance(lat2, lon2);
            Assert.NotNull(dist);
            Assert.NotNull(sv2.RiemannianNorth);
            Assert.NotNull(sv1.RiemannianNorth);
            Assert.AreEqual(sv2.RiemannianNorth.Value - sv1.RiemannianNorth.Value, dist, 0.75);
        }

        [Test]
        public void Test14()
        {
            const int northCount = 100;
            const int eastCount = 100;
            const double coordinateTolerance = 1e-3;

            SurveyPoint northLimit = new SurveyPoint() { Latitude = Numeric.PI / 2.0, Longitude = 0.0, TVD = 0.0 };
            SurveyPoint eastLimit = new SurveyPoint() { Latitude = 0.0, Longitude = 2.0 * Numeric.PI, TVD = 0.0 };

            Assert.NotNull(northLimit.RiemannianNorth);
            Assert.NotNull(eastLimit.RiemannianEast);

            double maxNorth = northLimit.RiemannianNorth.Value;
            double maxEast = eastLimit.RiemannianEast.Value;

            List<string> failures = new();

            for (int i = 0; i < northCount; i++)
            {
                double riemannianNorth = -maxNorth + (2.0 * maxNorth) * (i + 0.5) / northCount;

                for (int j = 0; j < eastCount; j++)
                {
                    double riemannianEast = maxEast * (j + 0.5) / eastCount;

                    SurveyPoint sv1 = new SurveyPoint()
                    {
                        RiemannianNorth = riemannianNorth,
                        RiemannianEast = riemannianEast,
                        TVD = 0.0
                    };

                    if (sv1.Latitude == null || sv1.Longitude == null)
                    {
                        failures.Add($"Null inverse for north={riemannianNorth:R}, east={riemannianEast:R}.");
                        continue;
                    }

                    SurveyPoint sv2 = new SurveyPoint()
                    {
                        Latitude = sv1.Latitude,
                        Longitude = sv1.Longitude,
                        TVD = 0.0
                    };

                    if (sv2.RiemannianNorth == null || sv2.RiemannianEast == null)
                    {
                        failures.Add(
                            $"Null forward round-trip for north={riemannianNorth:R}, east={riemannianEast:R}, " +
                            $"latitude={sv1.Latitude.Value:R}, longitude={sv1.Longitude.Value:R}.");
                        continue;
                    }

                    double northError = System.Math.Abs(sv2.RiemannianNorth.Value - riemannianNorth);
                    double eastError = System.Math.Abs(sv2.RiemannianEast.Value - riemannianEast);

                    if (northError > coordinateTolerance || eastError > coordinateTolerance)
                    {
                        failures.Add(
                            $"Round-trip mismatch for north={riemannianNorth:R}, east={riemannianEast:R}: " +
                            $"latitude={sv1.Latitude.Value:R}, longitude={sv1.Longitude.Value:R}, " +
                            $"back north={sv2.RiemannianNorth.Value:R}, back east={sv2.RiemannianEast.Value:R}, " +
                            $"dNorth={northError:R}, dEast={eastError:R}.");
                    }
                }
            }

            Assert.That(failures, Is.Empty,
                failures.Count == 0
                    ? "All Riemannian north/east round trips succeeded."
                    : string.Join(Environment.NewLine, failures.Take(20)));
        }

        [Test]
        public void Test15CompleteCASIAToCAXYZRoundTrip()
        {
            const int initialCaseCount = 100;
            const int nextCaseCount = 100;
            const double angleEpsilon = 1e-6;
            const double tolerance = 1e-4;

            static double NormalizeAngle(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0)
                {
                    angle += twoPi;
                }
                return angle;
            }

            static double SignedAngleDifference(double angle1, double angle2)
            {
                double diff = NormalizeAngle(angle1) - NormalizeAngle(angle2);
                if (diff > Numeric.PI)
                {
                    diff -= 2.0 * Numeric.PI;
                }
                else if (diff < -Numeric.PI)
                {
                    diff += 2.0 * Numeric.PI;
                }
                return diff;
            }

            Random random = new Random(123456789);
            int totalCount = 0;
            int successCount = 0;
            List<string> failures = new();
            for (int i = 0; i < initialCaseCount; i++)
            {
                SurveyPoint start = new SurveyPoint()
                {
                    X = 0.0,
                    Y = 0.0,
                    Z = 0.0,
                    Abscissa = 0.0,
                    Inclination = angleEpsilon + (Numeric.PI - 2.0 * angleEpsilon) * random.NextDouble(),
                    Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                };

                for (int j = 0; j < nextCaseCount; j++)
                {
                    totalCount++;
                    double length = 1000.0 * random.NextDouble();
                    SurveyPoint computed = new SurveyPoint()
                    {
                        Abscissa = length,
                        Inclination = angleEpsilon + (Numeric.PI - 2.0 * angleEpsilon) * random.NextDouble(),
                        Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                    };

                    bool okForward = start.CompleteCASIA(computed);
                    if (!okForward || computed.X == null || computed.Y == null || computed.Z == null)
                    {
                        failures.Add($"Forward failed for initial case {i}, next case {j}.");
                        continue;
                    }

                    CurvilinearPoint3D recovered = new CurvilinearPoint3D()
                    {
                        X = computed.X,
                        Y = computed.Y,
                        Z = computed.Z
                    };

                    bool okInverse = start.CompleteCAXYZ(recovered);
                    if (!okInverse || recovered.Abscissa == null || recovered.Inclination == null || recovered.Azimuth == null)
                    {
                        failures.Add($"Inverse failed for initial case {i}, next case {j}.");
                        continue;
                    }

                    bool okAbscissa = System.Math.Abs(computed.Abscissa!.Value - recovered.Abscissa.Value) <= tolerance;
                    bool okInclination = System.Math.Abs(computed.Inclination!.Value - recovered.Inclination.Value) <= tolerance;
                    bool okAzimuth = System.Math.Abs(SignedAngleDifference(computed.Azimuth!.Value, recovered.Azimuth.Value)) <= tolerance;
                    if (okAbscissa && okInclination && okAzimuth)
                    {
                        successCount++;
                    }
                    else
                    {
                        failures.Add(
                            $"Mismatch for initial case {i}, next case {j}: " +
                            $"expected s={computed.Abscissa.Value:R}, i={computed.Inclination.Value:R}, a={computed.Azimuth.Value:R}; " +
                            $"actual s={recovered.Abscissa.Value:R}, i={recovered.Inclination.Value:R}, a={recovered.Azimuth.Value:R}.");
                    }
                }
            }

            string summary =
                $"Successful cases: {successCount}/{totalCount}. " +
                $"{(failures.Count > 0 ? "First failures: " + string.Join(" | ", failures.Take(10)) : "All cases succeeded.")}";
            if (failures.Count > 0)
            {
                Assert.Fail(summary);
            }
            else
            {
                Assert.Pass(summary);
            }
        }

        [Test]
        public void Test15InterpolateListWithMethodAndRequiredAbscissas()
        {
            SurveyPoint start = new SurveyPoint()
            {
                X = 0.0,
                Y = 0.0,
                Z = 0.0,
                Abscissa = 0.0,
                Inclination = 0.2,
                Azimuth = 0.3
            };

            SurveyPoint end = new SurveyPoint()
            {
                Abscissa = 30.0,
                Inclination = 0.45,
                Azimuth = 0.9
            };

            Assert.That(start.CompleteFromSIA(end, TrajectoryCalculationType.ConstantBuildAndTurnMethod), Is.True);

            List<SurveyPoint>? interpolated = SurveyPoint.Interpolate(
                new List<SurveyPoint>() { start, end },
                10.0,
                TrajectoryCalculationType.ConstantBuildAndTurnMethod,
                null,
                new List<double>() { 15.0 });

            Assert.NotNull(interpolated);
            Assert.That(interpolated!.Count, Is.EqualTo(5));
            Assert.That(interpolated.Select(sp => sp.MD).ToArray(), Is.EqualTo(new double?[] { 0.0, 10.0, 15.0, 20.0, 30.0 }));

            SurveyPoint expectedAt15 = new SurveyPoint();
            Assert.That(start.InterpolateAtAbscissa(end, 15.0, expectedAt15, TrajectoryCalculationType.ConstantBuildAndTurnMethod), Is.True);

            SurveyPoint actualAt15 = interpolated.Single(sp => sp.MD == 15.0);
            Assert.That(actualAt15.X, Is.EqualTo(expectedAt15.X).Within(1e-8));
            Assert.That(actualAt15.Y, Is.EqualTo(expectedAt15.Y).Within(1e-8));
            Assert.That(actualAt15.Z, Is.EqualTo(expectedAt15.Z).Within(1e-8));
            Assert.That(actualAt15.Inclination, Is.EqualTo(expectedAt15.Inclination).Within(1e-8));
            Assert.That(actualAt15.Azimuth, Is.EqualTo(expectedAt15.Azimuth).Within(1e-8));
        }

        [Test]
        public void Test16InterpolateListAddsPointsForChordConstraint()
        {
            SurveyPoint start = new SurveyPoint()
            {
                X = 0.0,
                Y = 0.0,
                Z = 0.0,
                Abscissa = 0.0,
                Inclination = 0.0,
                Azimuth = 0.0
            };

            SurveyPoint end = new SurveyPoint()
            {
                Abscissa = 100.0,
                Inclination = Numeric.PI / 2.0,
                Azimuth = 0.0
            };

            Assert.That(start.CompleteFromSIA(end, TrajectoryCalculationType.MinimumCurvatureMethod), Is.True);

            List<SurveyPoint>? coarse = SurveyPoint.Interpolate(
                new List<SurveyPoint>() { start, end },
                100.0,
                TrajectoryCalculationType.MinimumCurvatureMethod,
                null);

            List<SurveyPoint>? refined = SurveyPoint.Interpolate(
                new List<SurveyPoint>() { start, end },
                100.0,
                TrajectoryCalculationType.MinimumCurvatureMethod,
                0.01);

            Assert.NotNull(coarse);
            Assert.NotNull(refined);
            Assert.That(coarse!.Count, Is.EqualTo(2));
            Assert.That(refined!.Count, Is.GreaterThan(2));
            Assert.That(refined.First().MD, Is.EqualTo(0.0));
            Assert.That(refined.Last().MD, Is.EqualTo(100.0));

            for (int i = 1; i < refined.Count; i++)
            {
                Assert.That(refined[i].MD, Is.GreaterThan(refined[i - 1].MD));
            }
        }

        [Test]
        public void Test16CompleteBTSIAToBTXYZRoundTrip()
        {
            const int initialCaseCount = 100;
            const int nextCaseCount = 100;
            const double angleEpsilon = 1e-6;
            const double tolerance = 1e-4;

            static double NormalizeAngle(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0)
                {
                    angle += twoPi;
                }
                return angle;
            }

            static double SignedAngleDifference(double angle1, double angle2)
            {
                double diff = NormalizeAngle(angle1) - NormalizeAngle(angle2);
                if (diff > Numeric.PI)
                {
                    diff -= 2.0 * Numeric.PI;
                }
                else if (diff < -Numeric.PI)
                {
                    diff += 2.0 * Numeric.PI;
                }
                return diff;
            }

            Random random = new Random(987654321);
            int totalCount = 0;
            int successCount = 0;
            List<string> failures = new();
            for (int i = 0; i < initialCaseCount; i++)
            {
                SurveyPoint start = new SurveyPoint()
                {
                    X = 0.0,
                    Y = 0.0,
                    Z = 0.0,
                    Abscissa = 0.0,
                    Inclination = angleEpsilon + (Numeric.PI - 2.0 * angleEpsilon) * random.NextDouble(),
                    Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                };

                for (int j = 0; j < nextCaseCount; j++)
                {
                    totalCount++;
                    double length = 1000.0 * random.NextDouble();
                    SurveyPoint computed = new SurveyPoint()
                    {
                        Abscissa = length,
                        Inclination = angleEpsilon + (Numeric.PI - 2.0 * angleEpsilon) * random.NextDouble(),
                        Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                    };

                    bool okForward = start.CompleteBTSIA(computed);
                    if (!okForward || computed.X == null || computed.Y == null || computed.Z == null)
                    {
                        failures.Add($"Forward failed for initial case {i}, next case {j}.");
                        continue;
                    }

                    CurvilinearPoint3D recovered = new CurvilinearPoint3D()
                    {
                        X = computed.X,
                        Y = computed.Y,
                        Z = computed.Z
                    };

                    bool okInverse = start.CompleteBTXYZ(recovered);
                    if (!okInverse || recovered.Abscissa == null || recovered.Inclination == null || recovered.Azimuth == null)
                    {
                        failures.Add($"Inverse failed for initial case {i}, next case {j}.");
                        continue;
                    }

                    bool okAbscissa = System.Math.Abs(computed.Abscissa!.Value - recovered.Abscissa.Value) <= tolerance;
                    bool okInclination = System.Math.Abs(computed.Inclination!.Value - recovered.Inclination.Value) <= tolerance;
                    bool okAzimuth = System.Math.Abs(SignedAngleDifference(computed.Azimuth!.Value, recovered.Azimuth.Value)) <= tolerance;
                    if (okAbscissa && okInclination && okAzimuth)
                    {
                        successCount++;
                    }
                    else
                    {
                        failures.Add(
                            $"Mismatch for initial case {i}, next case {j}: " +
                            $"expected s={computed.Abscissa.Value:R}, i={computed.Inclination.Value:R}, a={computed.Azimuth.Value:R}; " +
                            $"actual s={recovered.Abscissa.Value:R}, i={recovered.Inclination.Value:R}, a={recovered.Azimuth.Value:R}.");
                    }
                }
            }

            string summary =
                $"Successful cases: {successCount}/{totalCount}. " +
                $"{(failures.Count > 0 ? "First failures: " + string.Join(" | ", failures.Take(10)) : "All cases succeeded.")}";
            if (failures.Count > 0)
            {
                Assert.Fail(summary);
            }
            else
            {
                Assert.Pass(summary);
            }
        }

        [Test]
        public void Test17CompleteCDTSIAToCDTXYZRoundTrip()
        {
            const int initialCaseCount = 30;
            const int nextCaseCount = 30;
            const double angleEpsilon = 1e-3;
            const double tolerance = 1e-5;
            const int maxGenerationAttempts = 100;
            double maxInclination = 2.0 * Numeric.PI / 3.0;
            double maxDogleg = Numeric.PI;

            static double NormalizeAngle(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0)
                {
                    angle += twoPi;
                }
                return angle;
            }

            static double SignedAngleDifference(double angle1, double angle2)
            {
                double diff = NormalizeAngle(angle1) - NormalizeAngle(angle2);
                if (diff > Numeric.PI)
                {
                    diff -= 2.0 * Numeric.PI;
                }
                else if (diff < -Numeric.PI)
                {
                    diff += 2.0 * Numeric.PI;
                }
                return diff;
            }
            static string DescribeForwardSolutions(List<SurveyPoint> solutions)
            {
                if (solutions == null || solutions.Count == 0)
                {
                    return "no forward solutions";
                }
                return string.Join("; ", solutions.Take(3).Select((solution, index) =>
                    $"forward#{index}: dls={(solution.Curvature?.ToString("R") ?? "null")}, tf={(solution.Toolface?.ToString("R") ?? "null")}, dogleg={(solution.Curvature != null && solution.Abscissa != null ? (solution.Curvature.Value * solution.Abscissa.Value).ToString("R") : "null")}"));
            }

            Random random = new Random(192837465);
            int totalCount = 0;
            int successCount = 0;
            List<string> failures = new();
            for (int i = 0; i < initialCaseCount; i++)
            {
                SurveyPoint start = new SurveyPoint()
                {
                    X = 0.0,
                    Y = 0.0,
                    Z = 0.0,
                    Abscissa = 0.0,
                    Inclination = angleEpsilon + (maxInclination - 2.0 * angleEpsilon) * random.NextDouble(),
                    Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                };

                for (int j = 0; j < nextCaseCount; j++)
                {
                    totalCount++;
                    SurveyPoint? computed = null;
                    List<SurveyPoint> forwardSolutions = new();
                    bool okForward = false;

                    for (int attempt = 0; attempt < maxGenerationAttempts; attempt++)
                    {
                        double length = 1000.0 * random.NextDouble();
                        SurveyPoint candidate = new SurveyPoint()
                        {
                            Abscissa = length,
                            Inclination = angleEpsilon + (maxInclination - 2.0 * angleEpsilon) * random.NextDouble(),
                            Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                        };

                        okForward = start.CompleteCDTSIA(candidate, out List<SurveyPoint> candidateForwardSolutions);
                        if (!okForward || candidate.X == null || candidate.Y == null || candidate.Z == null || candidateForwardSolutions.Count == 0)
                        {
                            continue;
                        }

                        bool withinDogleg = candidateForwardSolutions.Any(solution =>
                            solution.Curvature != null &&
                            solution.Abscissa != null &&
                            Numeric.LE(solution.Curvature.Value * solution.Abscissa.Value, maxDogleg + tolerance));
                        if (!withinDogleg)
                        {
                            continue;
                        }

                        computed = candidate;
                        forwardSolutions = candidateForwardSolutions;
                        break;
                    }

                    if (!okForward || computed == null || computed.X == null || computed.Y == null || computed.Z == null || forwardSolutions.Count == 0)
                    {
                        failures.Add($"Forward failed for initial case {i}, next case {j}.");
                        continue;
                    }

                    CurvilinearPoint3D recovered = new CurvilinearPoint3D()
                    {
                        X = computed.X,
                        Y = computed.Y,
                        Z = computed.Z
                    };

                    bool okInverse = start.CompleteCDTXYZ(recovered, out List<SurveyPoint> recoveredSolutions);
                    if (!okInverse || recovered.Abscissa == null || recovered.Inclination == null || recovered.Azimuth == null)
                    {
                        failures.Add(
                            $"Inverse failed for initial case {i}, next case {j}: " +
                            $"target x={computed.X.Value:R}, y={computed.Y.Value:R}, z={computed.Z.Value:R}, " +
                            $"expected s={computed.Abscissa.Value:R}, i={computed.Inclination.Value:R}, a={computed.Azimuth.Value:R}; " +
                            $"{DescribeForwardSolutions(forwardSolutions)}.");
                        continue;
                    }

                    bool MatchesCanonicalSIA(CurvilinearPoint3D candidate)
                        => candidate.Abscissa != null &&
                           candidate.Inclination != null &&
                           candidate.Azimuth != null &&
                           System.Math.Abs(computed.Abscissa!.Value - candidate.Abscissa.Value) <= tolerance &&
                           System.Math.Abs(computed.Inclination!.Value - candidate.Inclination.Value) <= tolerance &&
                           System.Math.Abs(SignedAngleDifference(computed.Azimuth!.Value, candidate.Azimuth.Value)) <= tolerance;

                    bool MatchesTargetXYZThroughForward(CurvilinearPoint3D candidate)
                    {
                        if (candidate.Abscissa == null || candidate.Inclination == null || candidate.Azimuth == null)
                        {
                            return false;
                        }

                        SurveyPoint candidateForward = new SurveyPoint()
                        {
                            Abscissa = candidate.Abscissa,
                            Inclination = candidate.Inclination,
                            Azimuth = candidate.Azimuth
                        };

                        if (!start.CompleteCDTSIA(candidateForward, out List<SurveyPoint> candidateForwardSolutions) || candidateForwardSolutions.Count == 0)
                        {
                            return false;
                        }

                        return candidateForwardSolutions.Any(solution =>
                            solution.X != null &&
                            solution.Y != null &&
                            solution.Z != null &&
                            System.Math.Abs(computed.X!.Value - solution.X.Value) <= tolerance &&
                            System.Math.Abs(computed.Y!.Value - solution.Y.Value) <= tolerance &&
                            System.Math.Abs(computed.Z!.Value - solution.Z.Value) <= tolerance);
                    }

                    if (MatchesCanonicalSIA(recovered) ||
                        MatchesTargetXYZThroughForward(recovered) ||
                        recoveredSolutions.Any(candidate => MatchesCanonicalSIA(candidate) || MatchesTargetXYZThroughForward(candidate)))
                    {
                        successCount++;
                    }
                    else
                    {
                        failures.Add(
                            $"Mismatch for initial case {i}, next case {j}: " +
                            $"target x={computed.X.Value:R}, y={computed.Y.Value:R}, z={computed.Z.Value:R}, " +
                            $"expected s={computed.Abscissa.Value:R}, i={computed.Inclination.Value:R}, a={computed.Azimuth.Value:R}; " +
                            $"{DescribeForwardSolutions(forwardSolutions)}; " +
                            $"actual s={recovered.Abscissa.Value:R}, i={recovered.Inclination.Value:R}, a={recovered.Azimuth.Value:R}.");
                    }
                }
            }

            string summary =
                $"Successful cases: {successCount}/{totalCount}. " +
                $"{(failures.Count > 0 ? "First failures: " + string.Join(" | ", failures.Take(10)) : "All cases succeeded.")}";
            if (failures.Count > 0)
            {
                Assert.Fail(summary);
            }
            else
            {
                Assert.Pass(summary);
            }
        }

        [Test]
        public void Test18CompleteCDTSDTToCDTSIARoundTrip()
        {
            const int initialCaseCount = 100;
            const int nextCaseCount = 100;
            const double inclinationEpsilon = 1e-3;
            const double toolfaceEpsilon = 1e-3;
            const double tolerance = 1e-4;
            const int maxGenerationAttempts = 100;
            double maxInclination = Numeric.PI;
            double maxDogleg = Numeric.PI;

            Random random = new Random(564738291);
            int totalCount = 0;
            int successCount = 0;
            List<string> failures = new();
            for (int i = 0; i < initialCaseCount; i++)
            {
                SurveyPoint start = new SurveyPoint()
                {
                    X = 0.0,
                    Y = 0.0,
                    Z = 0.0,
                    Abscissa = 0.0,
                    Inclination = inclinationEpsilon + (maxInclination - 2.0 * inclinationEpsilon) * random.NextDouble(),
                    Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                };

                for (int j = 0; j < nextCaseCount; j++)
                {
                    totalCount++;
                    SurveyPoint? computed = null;
                    bool okForward = false;
                    for (int attempt = 0; attempt < maxGenerationAttempts; attempt++)
                    {
                        double dls = 1e-6 + 0.02 * random.NextDouble();
                        double maxLength = System.Math.Min(1000.0, maxDogleg / dls);
                        double length = maxLength * random.NextDouble();
                        double tf = toolfaceEpsilon + (2.0 * Numeric.PI - 2.0 * toolfaceEpsilon) * random.NextDouble();
                        if (System.Math.Abs(tf - Numeric.PI / 2.0) < toolfaceEpsilon)
                        {
                            tf += 2.0 * toolfaceEpsilon;
                        }
                        if (System.Math.Abs(tf - 3.0 * Numeric.PI / 2.0) < toolfaceEpsilon)
                        {
                            tf += 2.0 * toolfaceEpsilon;
                        }

                        SurveyPoint candidate = new SurveyPoint()
                        {
                            Abscissa = length
                        };

                        okForward = start.CompleteCDTSDT(candidate, dls, tf);
                        if (!okForward || candidate.Inclination == null || candidate.Azimuth == null || candidate.X == null || candidate.Y == null || candidate.Z == null)
                        {
                            continue;
                        }
                        if (candidate.Inclination.Value > maxInclination)
                        {
                            continue;
                        }

                        computed = candidate;
                        break;
                    }

                    if (!okForward || computed == null)
                    {
                        failures.Add($"Forward generation failed for initial case {i}, next case {j} within domain constraints.");
                        continue;
                    }

                    CurvilinearPoint3D recovered = new CurvilinearPoint3D()
                    {
                        Abscissa = computed.Abscissa,
                        Inclination = computed.Inclination,
                        Azimuth = computed.Azimuth
                    };

                    bool okInverse = start.CompleteCDTSIA(recovered, out List<SurveyPoint> recoveredSolutions);
                    if (!okInverse || recovered.X == null || recovered.Y == null || recovered.Z == null || recoveredSolutions.Count == 0)
                    {
                        failures.Add($"Inverse failed for initial case {i}, next case {j}.");
                        continue;
                    }

                    bool anySolutionMatches = recoveredSolutions.Any(solution =>
                        solution.X != null &&
                        solution.Y != null &&
                        solution.Z != null &&
                        System.Math.Abs(computed.X.Value - solution.X.Value) <= tolerance &&
                        System.Math.Abs(computed.Y.Value - solution.Y.Value) <= tolerance &&
                        System.Math.Abs(computed.Z.Value - solution.Z.Value) <= tolerance);
                    if (anySolutionMatches)
                    {
                        successCount++;
                    }
                    else
                    {
                        failures.Add(
                            $"Mismatch for initial case {i}, next case {j}: " +
                            $"expected x={computed.X.Value:R}, y={computed.Y.Value:R}, z={computed.Z.Value:R}; " +
                            $"actual x={recovered.X.Value:R}, y={recovered.Y.Value:R}, z={recovered.Z.Value:R}.");
                    }
                }
            }

            string summary =
                $"Successful cases: {successCount}/{totalCount}. " +
                $"{(failures.Count > 0 ? "First failures: " + string.Join(" | ", failures.Take(10)) : "All cases succeeded.")}";
            if (failures.Count > 0)
            {
                Assert.Fail(summary);
            }
            else
            {
                Assert.Pass(summary);
            }
        }

        [Test]
        public void Test19CompleteCDTSDTToCDTXYZRoundTrip()
        {
            const int initialCaseCount = 40;
            const int nextCaseCount = 40;
            const double inclinationEpsilon = 1e-3;
            const double toolfaceEpsilon = 1e-3;
            const double tolerance = 5e-6;

            static double NormalizeAngle(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0)
                {
                    angle += twoPi;
                }
                return angle;
            }

            static double SignedAngleDifference(double angle1, double angle2)
            {
                double diff = NormalizeAngle(angle1) - NormalizeAngle(angle2);
                if (diff > Numeric.PI)
                {
                    diff -= 2.0 * Numeric.PI;
                }
                else if (diff < -Numeric.PI)
                {
                    diff += 2.0 * Numeric.PI;
                }
                return diff;
            }

            Random random = new Random(918273645);
            int totalCount = 0;
            int successCount = 0;
            List<string> failures = new();
            for (int i = 0; i < initialCaseCount; i++)
            {
                SurveyPoint start = new SurveyPoint()
                {
                    X = 0.0,
                    Y = 0.0,
                    Z = 0.0,
                    Abscissa = 0.0,
                    Inclination = inclinationEpsilon + (Numeric.PI - 2.0 * inclinationEpsilon) * random.NextDouble(),
                    Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                };

                for (int j = 0; j < nextCaseCount; j++)
                {
                    totalCount++;
                    double dls = 1e-6 + 0.02 * random.NextDouble();
                    double maxLength = System.Math.Min(1000.0, Numeric.PI / dls);
                    double length = maxLength * random.NextDouble();
                    double tf = toolfaceEpsilon + (2.0 * Numeric.PI - 2.0 * toolfaceEpsilon) * random.NextDouble();
                    if (System.Math.Abs(tf - Numeric.PI / 2.0) < toolfaceEpsilon)
                    {
                        tf += 2.0 * toolfaceEpsilon;
                    }
                    if (System.Math.Abs(tf - 3.0 * Numeric.PI / 2.0) < toolfaceEpsilon)
                    {
                        tf += 2.0 * toolfaceEpsilon;
                    }

                    SurveyPoint computed = new SurveyPoint()
                    {
                        Abscissa = length
                    };

                    bool okForward = start.CompleteCDTSDT(computed, dls, tf);
                    if (!okForward || computed.X == null || computed.Y == null || computed.Z == null || computed.Inclination == null || computed.Azimuth == null)
                    {
                        failures.Add($"Forward failed for initial case {i}, next case {j}.");
                        continue;
                    }

                    CurvilinearPoint3D recovered = new CurvilinearPoint3D()
                    {
                        X = computed.X,
                        Y = computed.Y,
                        Z = computed.Z
                    };

                    bool okInverse = start.CompleteCDTXYZ(recovered, out List<SurveyPoint> recoveredSolutions);

                    bool Matches(CurvilinearPoint3D candidate)
                    {
                        return candidate.Abscissa != null &&
                               candidate.Inclination != null &&
                               candidate.Azimuth != null &&
                               System.Math.Abs(computed.Abscissa!.Value - candidate.Abscissa.Value) <= tolerance &&
                               System.Math.Abs(computed.Inclination!.Value - candidate.Inclination.Value) <= tolerance &&
                               System.Math.Abs(SignedAngleDifference(computed.Azimuth!.Value, candidate.Azimuth.Value)) <= tolerance;
                    }

                    bool MatchesXYZ(CurvilinearPoint3D candidate)
                    {
                        if (candidate.X == null || candidate.Y == null || candidate.Z == null)
                        {
                            return false;
                        }
                        return System.Math.Abs(computed.X!.Value - candidate.X.Value) <= tolerance &&
                               System.Math.Abs(computed.Y!.Value - candidate.Y.Value) <= tolerance &&
                               System.Math.Abs(computed.Z!.Value - candidate.Z.Value) <= tolerance;
                    }

                    string DescribeCandidate(SurveyPoint candidate, string label)
                    {
                        if (candidate.Abscissa == null || candidate.Inclination == null || candidate.Azimuth == null)
                        {
                            return $"{label} incomplete";
                        }

                        if (candidate.X != null && candidate.Y != null && candidate.Z != null)
                        {
                            string dls = (candidate.Curvature != null) ? candidate.Curvature.Value.ToString("R") : "?";
                            string tf = (candidate.Toolface != null) ? candidate.Toolface.Value.ToString("R") : "?";
                            string sdt = $"{label} x={candidate.X.Value:R}, y={candidate.Y.Value:R}, z={candidate.Z.Value:R}, s={candidate.Abscissa.Value:R}, incl={candidate.Inclination.Value:R}, az={candidate.Azimuth.Value:R}, dls={dls}, tf={tf}";
                            return $"{sdt}; {label}";
                        }

                        return $"{label} has no coordinates";
                    }

                    bool matched = false;
                    if (okInverse && (Matches(recovered) || MatchesXYZ(recovered)))
                    {
                        matched = true;
                    }
                    else
                    {
                        foreach (CurvilinearPoint3D candidate in recoveredSolutions)
                        {
                            if (Matches(candidate) || MatchesXYZ(candidate))
                            {
                                matched = true;
                                break;
                            }
                        }
                    }

                    if (matched)
                    {
                        successCount++;
                    }
                    else
                    {
                        if (!okInverse && recoveredSolutions.Count == 0)
                        {
                            failures.Add($"Inverse failed for initial case {i}, next case {j}.");
                            continue;
                        }

                        string actual;
                        if (okInverse && recovered.Abscissa != null && recovered.Inclination != null && recovered.Azimuth != null)
                        {
                            SurveyPoint? act = null;
                            foreach (var sp in recoveredSolutions)
                            {
                                if (sp.X != null && sp.Y != null && sp.Z != null &&
                                    System.Math.Abs(computed.X.Value - sp.X.Value) <= tolerance &&
                                    System.Math.Abs(computed.Y.Value - sp.Y.Value) <= tolerance &&
                                    System.Math.Abs(computed.Z.Value - sp.Z.Value) <= tolerance)
                                {
                                    act = sp;
                                    break;
                                }
                            }
                            if (act != null)
                            {
                                actual = DescribeCandidate(act, "actual");
                            }
                            else
                            {
                                actual = DescribeCandidate(new SurveyPoint()
                                {
                                    MD = recovered.Abscissa,
                                    Inclination = recovered.Inclination,
                                    Azimuth = recovered.Azimuth,
                                    X = recovered.X,
                                    Y = recovered.Y,
                                    Z = recovered.Z
                                },
                                    "actual");
                            }
                        }
                        else if (recoveredSolutions.Count > 0)
                        {
                            SurveyPoint? act = null;
                            int bestIdx = -1;
                            double minDistance = double.MaxValue;
                            for (int idx = 0; idx < recoveredSolutions.Count; idx++)
                            {
                                if (recoveredSolutions[idx] != null &&
                                    recoveredSolutions[idx].X != null &&
                                    recoveredSolutions[idx].Y != null &&
                                    recoveredSolutions[idx].Z != null)
                                {
                                    double dist = System.Math.Sqrt(
                                        System.Math.Pow(computed.X.Value - recoveredSolutions[idx].X!.Value, 2) +
                                        System.Math.Pow(computed.Y.Value - recoveredSolutions[idx].Y!.Value, 2) +
                                        System.Math.Pow(computed.Z.Value - recoveredSolutions[idx].Z!.Value, 2));
                                    if (dist < minDistance)
                                    {
                                        minDistance = dist;
                                        bestIdx = idx;
                                    }
                                }
                            }
                            if (bestIdx >= 0)
                            {
                                act = recoveredSolutions[bestIdx];
                            }
                            if (act != null)
                            {
                                actual = DescribeCandidate(act, "candidate #" + bestIdx + " dist= " + minDistance.ToString("F3"));
                            }
                            else
                            {
                                actual = "inverse returned candidates with no coordinates";
                            }
                        }
                        else
                        {
                            actual = "inverse returned no complete candidate";
                        }
                        failures.Add(
                            $"Mismatch for initial case {i}, next case {j}: " +
                            $"expected x={computed.X.Value:R}, y={computed.Y.Value:R}, z={computed.Z.Value:R}, s={computed.Abscissa.Value:R}, incl={computed.Inclination.Value:R}, az={computed.Azimuth.Value:R}, dls={dls:R}, tf={tf:R}; " +
                            $"{actual}.");
                    }
                }
            }
            string summary =
                $"Successful cases: {successCount}/{totalCount}. " +
                $"{(failures.Count > 0 ? "First failures: " + string.Join(" | ", failures.Take(20)) : "All cases succeeded.")}";
            if (failures.Count > 0)
            {
                Assert.Fail(summary);
            }
            else
            {
                Assert.Pass(summary);
            }
        }

        [Test]
        public void Test20InterpolateAtAbscissaCAPlaneAndCurvatureConsistency()
        {
            const int initialCaseCount = 100;
            const int nextCaseCount = 100;
            const int interpolationCount = 10;
            const double angleEpsilon = 1e-6;
            const double tolerance = 1e-5;
            const double planeTolerance = 1e-4;
            double maxInclination = 2.0 * Numeric.PI / 3.0;

            static (double x, double y, double z) Tangent(double inclination, double azimuth)
            {
                return
                (
                    System.Math.Sin(inclination) * System.Math.Cos(azimuth),
                    System.Math.Sin(inclination) * System.Math.Sin(azimuth),
                    System.Math.Cos(inclination)
                );
            }

            static (double x, double y, double z) Cross((double x, double y, double z) left, (double x, double y, double z) right)
            {
                return
                (
                    left.y * right.z - left.z * right.y,
                    left.z * right.x - left.x * right.z,
                    left.x * right.y - left.y * right.x
                );
            }

            static double Dot((double x, double y, double z) left, (double x, double y, double z) right)
                => left.x * right.x + left.y * right.y + left.z * right.z;

            static double Norm((double x, double y, double z) value)
                => System.Math.Sqrt(Dot(value, value));

            static double Dogleg(double inclination1, double azimuth1, double inclination2, double azimuth2)
            {
                double si1 = System.Math.Sin(inclination1);
                double si2 = System.Math.Sin(inclination2);
                double ci12 = System.Math.Cos(inclination2 - inclination1);
                double ca12 = System.Math.Cos(azimuth2 - azimuth1);
                double cosine = ci12 - (1.0 - ca12) * si2 * si1;
                cosine = System.Math.Max(-1.0, System.Math.Min(1.0, cosine));
                return System.Math.Acos(cosine);
            }

            static double CurvatureFromAngles(double inclination1, double azimuth1, double abscissa1, double inclination2, double azimuth2, double abscissa2)
            {
                double deltaAbscissa = abscissa2 - abscissa1;
                if (Numeric.EQ(deltaAbscissa, 0))
                {
                    return 0.0;
                }
                return Dogleg(inclination1, azimuth1, inclination2, azimuth2) / deltaAbscissa;
            }

            static string DescribePoint(string label, SurveyPoint point)
                => $"{label}: " +
                   $"s={(point.Abscissa?.ToString("R") ?? "null")}, " +
                   $"incl={(point.Inclination?.ToString("R") ?? "null")}, " +
                   $"az={(point.Azimuth?.ToString("R") ?? "null")}, " +
                   $"x={(point.X?.ToString("R") ?? "null")}, " +
                   $"y={(point.Y?.ToString("R") ?? "null")}, " +
                   $"z={(point.Z?.ToString("R") ?? "null")}, " +
                   $"curv={(point.Curvature?.ToString("R") ?? "null")}";

            static string DescribeCase(int initialCaseIndex, int nextCaseIndex, int? interpolationIndex, double? abscissa, SurveyPoint start, SurveyPoint end)
            {
                string interpolationText = interpolationIndex.HasValue
                    ? $", interpolation={interpolationIndex.Value}, targetAbscissa={(abscissa?.ToString("R") ?? "null")}"
                    : string.Empty;
                return $"initial case={initialCaseIndex}, next case={nextCaseIndex}{interpolationText}; " +
                       $"{DescribePoint("start", start)}; {DescribePoint("end", end)}";
            }

            Random random = new Random(246813579);
            int totalCount = 0;
            int successCount = 0;
            List<string> failures = new();

            for (int i = 0; i < initialCaseCount; i++)
            {
                SurveyPoint start = new SurveyPoint()
                {
                    X = 0.0,
                    Y = 0.0,
                    Z = 0.0,
                    Abscissa = 0.0,
                    Inclination = angleEpsilon + (maxInclination - 2.0 * angleEpsilon) * random.NextDouble(),
                    Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                };

                for (int j = 0; j < nextCaseCount; j++)
                {
                    SurveyPoint end = new SurveyPoint()
                    {
                        Abscissa = 1e-3 + (1000.0 - 1e-3) * random.NextDouble(),
                        Inclination = angleEpsilon + (maxInclination - 2.0 * angleEpsilon) * random.NextDouble(),
                        Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                    };

                    bool okForward = start.CompleteCASIA(end);
                    if (!okForward ||
                        end.X == null || end.Y == null || end.Z == null ||
                        end.Inclination == null || end.Azimuth == null || end.Abscissa == null)
                    {
                        for (int k = 0; k < interpolationCount; k++)
                        {
                            totalCount++;
                            failures.Add(
                                "Forward calculation failed. " +
                                DescribeCase(i, j, k + 1, null, start, end));
                        }
                        continue;
                    }

                    for (int k = 1; k <= interpolationCount; k++)
                    {
                        totalCount++;
                        double abscissa = end.Abscissa.Value * k / (interpolationCount + 1.0);
                        SurveyPoint interpolated = new SurveyPoint();
                        bool okInterpolation = start.InterpolateAtAbscissaCA(end, abscissa, interpolated);
                        if (!okInterpolation ||
                            interpolated.X == null || interpolated.Y == null || interpolated.Z == null ||
                            interpolated.Abscissa == null || interpolated.Inclination == null || interpolated.Azimuth == null)
                        {
                            failures.Add(
                                "Interpolation failed. " +
                                DescribeCase(i, j, k, abscissa, start, end) + "; " +
                                DescribePoint("interpolated", interpolated));
                            continue;
                        }

                        SurveyPoint reconstructedEnd = new SurveyPoint()
                        {
                            Abscissa = end.Abscissa,
                            Inclination = end.Inclination,
                            Azimuth = end.Azimuth
                        };
                        bool okTail = interpolated.CompleteCASIA(reconstructedEnd);
                        if (!okTail ||
                            reconstructedEnd.X == null || reconstructedEnd.Y == null || reconstructedEnd.Z == null ||
                            reconstructedEnd.Inclination == null || reconstructedEnd.Azimuth == null || reconstructedEnd.Abscissa == null)
                        {
                            failures.Add(
                                "Tail reconstruction failed. " +
                                DescribeCase(i, j, k, abscissa, start, end) + "; " +
                                DescribePoint("interpolated", interpolated) + "; " +
                                DescribePoint("reconstructedEnd", reconstructedEnd));
                            continue;
                        }

                        double expectedCurvature = CurvatureFromAngles(
                            start.Inclination.Value, start.Azimuth.Value, start.Abscissa.Value,
                            end.Inclination.Value, end.Azimuth.Value, end.Abscissa.Value);
                        double actualCurvature = CurvatureFromAngles(
                            interpolated.Inclination.Value, interpolated.Azimuth.Value, interpolated.Abscissa.Value,
                            reconstructedEnd.Inclination.Value, reconstructedEnd.Azimuth.Value, reconstructedEnd.Abscissa.Value);

                        bool okCurvature = System.Math.Abs(actualCurvature - expectedCurvature) <= tolerance;
                        bool okX = System.Math.Abs(reconstructedEnd.X.Value - end.X.Value) <= tolerance;
                        bool okY = System.Math.Abs(reconstructedEnd.Y.Value - end.Y.Value) <= tolerance;
                        bool okZ = System.Math.Abs(reconstructedEnd.Z.Value - end.Z.Value) <= tolerance;

                        var tInterpolated = Tangent(interpolated.Inclination.Value, interpolated.Azimuth.Value);
                        var tEnd = Tangent(reconstructedEnd.Inclination.Value, reconstructedEnd.Azimuth.Value);
                        var normal = Cross(tInterpolated, tEnd);
                        if (Norm(normal) <= 1e-10)
                        {
                            var chordToEnd = (
                                reconstructedEnd.X.Value - interpolated.X.Value,
                                reconstructedEnd.Y.Value - interpolated.Y.Value,
                                reconstructedEnd.Z.Value - interpolated.Z.Value
                            );
                            normal = Cross(tInterpolated, chordToEnd);
                        }

                        double planeDistance = 0.0;
                        double normalNorm = Norm(normal);
                        if (normalNorm > 1e-10)
                        {
                            var chordToStart = (
                                start.X!.Value - interpolated.X.Value,
                                start.Y!.Value - interpolated.Y.Value,
                                start.Z!.Value - interpolated.Z.Value
                            );
                            planeDistance = System.Math.Abs(Dot(normal, chordToStart)) / normalNorm;
                        }

                        bool okPlane = planeDistance <= planeTolerance;
                        if (okCurvature && okX && okY && okZ && okPlane)
                        {
                            successCount++;
                        }
                        else
                        {
                            failures.Add(
                                "Consistency mismatch. " +
                                DescribeCase(i, j, k, abscissa, start, end) + "; " +
                                DescribePoint("interpolated", interpolated) + "; " +
                                DescribePoint("reconstructedEnd", reconstructedEnd) + "; " +
                                $"checks: curvature={okCurvature}, x={okX}, y={okY}, z={okZ}, plane={okPlane}; " +
                                $"results: expectedCurvature={expectedCurvature:R}, actualCurvature={actualCurvature:R}, " +
                                $"expectedEnd=({end.X.Value:R}, {end.Y.Value:R}, {end.Z.Value:R}), " +
                                $"actualEnd=({reconstructedEnd.X.Value:R}, {reconstructedEnd.Y.Value:R}, {reconstructedEnd.Z.Value:R}), " +
                                $"planeDistance={planeDistance:R}, planeTolerance={planeTolerance:R}, tolerance={tolerance:R}.");
                        }
                    }
                }
            }

            string summary =
                $"Successful cases: {successCount}/{totalCount}. " +
                $"Failed cases: {failures.Count}. " +
                $"{(failures.Count > 0 ? Environment.NewLine + string.Join(Environment.NewLine, failures) : "All cases succeeded.")}";

            Assert.That(failures, Is.Empty, summary);
        }

        [Test]
        public void Test21InterpolateAtAbscissaBTBURAndTURConsistency()
        {
            const int initialCaseCount = 100;
            const int nextCaseCount = 100;
            const int interpolationCount = 10;
            const double angleEpsilon = 1e-3;
            const double tolerance = 1e-5;
            const double caTolerance = 1e-3;

            static string DescribePoint(string label, SurveyPoint point)
                => $"{label}: " +
                   $"s={(point.Abscissa?.ToString("R") ?? "null")}, " +
                   $"incl={(point.Inclination?.ToString("R") ?? "null")}, " +
                   $"az={(point.Azimuth?.ToString("R") ?? "null")}, " +
                   $"x={(point.X?.ToString("R") ?? "null")}, " +
                   $"y={(point.Y?.ToString("R") ?? "null")}, " +
                   $"z={(point.Z?.ToString("R") ?? "null")}, " +
                   $"bur={(point.BUR?.ToString("R") ?? "null")}, " +
                   $"tur={(point.TUR?.ToString("R") ?? "null")}";

            static string DescribeCase(int initialCaseIndex, int nextCaseIndex, int? interpolationIndex, double? abscissa, SurveyPoint start, SurveyPoint end)
            {
                string interpolationText = interpolationIndex.HasValue
                    ? $", interpolation={interpolationIndex.Value}, targetAbscissa={(abscissa?.ToString("R") ?? "null")}"
                    : string.Empty;
                return $"initial case={initialCaseIndex}, next case={nextCaseIndex}{interpolationText}; " +
                       $"{DescribePoint("start", start)}; {DescribePoint("end", end)}";
            }

            Random random = new Random(135792468);
            int totalCount = 0;
            int successCount = 0;
            List<string> failures = new();

            for (int i = 0; i < initialCaseCount; i++)
            {
                SurveyPoint start = new SurveyPoint()
                {
                    X = 0.0,
                    Y = 0.0,
                    Z = 0.0,
                    Abscissa = 0.0,
                    Inclination = 0.2 + (Numeric.PI - 0.4) * random.NextDouble(),
                    Azimuth = 0.2 + (2.0 * Numeric.PI - 0.4) * random.NextDouble()
                };

                for (int j = 0; j < nextCaseCount; j++)
                {
                    double length = 1.0 + 999.0 * random.NextDouble();
                    double deltaInclination = -0.5 + random.NextDouble();
                    double endInclination = System.Math.Max(angleEpsilon, System.Math.Min(Numeric.PI - angleEpsilon, start.Inclination!.Value + deltaInclination));
                    double deltaAzimuth = -1.0 + 2.0 * random.NextDouble();
                    double endAzimuth = start.Azimuth!.Value + deltaAzimuth;

                    SurveyPoint end = new SurveyPoint()
                    {
                        Abscissa = length,
                        Inclination = endInclination,
                        Azimuth = endAzimuth
                    };

                    bool okForward = start.CompleteFromSIA(end, TrajectoryCalculationType.ConstantBuildAndTurnMethod);
                    if (!okForward ||
                        end.X == null || end.Y == null || end.Z == null ||
                        end.Abscissa == null || end.Inclination == null || end.Azimuth == null ||
                        end.BUR == null || end.TUR == null)
                    {
                        for (int k = 0; k < interpolationCount; k++)
                        {
                            totalCount++;
                            failures.Add(
                                "Forward BT calculation failed. " +
                                DescribeCase(i, j, k + 1, null, start, end));
                        }
                        continue;
                    }

                    for (int k = 1; k <= interpolationCount; k++)
                    {
                        totalCount++;
                        double abscissa = end.Abscissa.Value * k / (interpolationCount + 1.0);
                        SurveyPoint interpolated = new SurveyPoint();
                        bool okInterpolation = start.InterpolateAtAbscissa(end, abscissa, interpolated, TrajectoryCalculationType.ConstantBuildAndTurnMethod);
                        if (!okInterpolation ||
                            interpolated.X == null || interpolated.Y == null || interpolated.Z == null ||
                            interpolated.Abscissa == null || interpolated.Inclination == null || interpolated.Azimuth == null)
                        {
                            failures.Add(
                                "BT interpolation failed. " +
                                DescribeCase(i, j, k, abscissa, start, end) + "; " +
                                DescribePoint("interpolated", interpolated));
                            continue;
                        }

                        SurveyPoint reconstructedEnd = new SurveyPoint()
                        {
                            Abscissa = end.Abscissa,
                            Inclination = end.Inclination,
                            Azimuth = end.Azimuth
                        };
                        bool okTail = interpolated.CompleteFromSIA(reconstructedEnd, TrajectoryCalculationType.ConstantBuildAndTurnMethod);
                        if (!okTail ||
                            reconstructedEnd.X == null || reconstructedEnd.Y == null || reconstructedEnd.Z == null ||
                            reconstructedEnd.Abscissa == null || reconstructedEnd.Inclination == null || reconstructedEnd.Azimuth == null ||
                            reconstructedEnd.BUR == null || reconstructedEnd.TUR == null)
                        {
                            failures.Add(
                                "BT tail reconstruction failed. " +
                                DescribeCase(i, j, k, abscissa, start, end) + "; " +
                                DescribePoint("interpolated", interpolated) + "; " +
                                DescribePoint("reconstructedEnd", reconstructedEnd));
                            continue;
                        }

                        bool okBUR = System.Math.Abs(reconstructedEnd.BUR.Value - end.BUR.Value) <= tolerance;
                        bool okTUR = System.Math.Abs(reconstructedEnd.TUR.Value - end.TUR.Value) <= tolerance;

                        if (okBUR && okTUR)
                        {
                            successCount++;
                        }
                        else
                        {
                            failures.Add(
                                "BT BUR/TUR mismatch. " +
                                DescribeCase(i, j, k, abscissa, start, end) + "; " +
                                DescribePoint("interpolated", interpolated) + "; " +
                                DescribePoint("reconstructedEnd", reconstructedEnd) + "; " +
                                $"checks: bur={okBUR}, tur={okTUR}; " +
                                $"results: expectedBUR={end.BUR.Value:R}, actualBUR={reconstructedEnd.BUR.Value:R}, " +
                                $"expectedTUR={end.TUR.Value:R}, actualTUR={reconstructedEnd.TUR.Value:R}.");
                        }
                    }
                }
            }

            string summary =
                $"Successful cases: {successCount}/{totalCount}. " +
                $"Failed cases: {failures.Count}. " +
                $"{(failures.Count > 0 ? Environment.NewLine + string.Join(Environment.NewLine, failures) : "All cases succeeded.")}";

            Assert.That(failures, Is.Empty, summary);
        }

        [Test]
        public void Test22InterpolateAtAbscissaCDTCurvatureAndToolfaceConsistency()
        {
            const int initialCaseCount = 100;
            const int nextCaseCount = 100;
            const int interpolationCount = 10;
            const double tolerance = 1e-4;

            static string DescribePoint(string label, SurveyPoint point)
                => $"{label}: " +
                   $"s={(point.Abscissa?.ToString("R") ?? "null")}, " +
                   $"incl={(point.Inclination?.ToString("R") ?? "null")}, " +
                   $"az={(point.Azimuth?.ToString("R") ?? "null")}, " +
                   $"x={(point.X?.ToString("R") ?? "null")}, " +
                   $"y={(point.Y?.ToString("R") ?? "null")}, " +
                   $"z={(point.Z?.ToString("R") ?? "null")}, " +
                   $"curv={(point.Curvature?.ToString("R") ?? "null")}, " +
                   $"tf={(point.Toolface?.ToString("R") ?? "null")}";

            static string DescribeCase(int initialCaseIndex, int nextCaseIndex, int? interpolationIndex, double? abscissa, SurveyPoint start, SurveyPoint end)
            {
                string interpolationText = interpolationIndex.HasValue
                    ? $", interpolation={interpolationIndex.Value}, targetAbscissa={(abscissa?.ToString("R") ?? "null")}"
                    : string.Empty;
                return $"initial case={initialCaseIndex}, next case={nextCaseIndex}{interpolationText}; " +
                       $"{DescribePoint("start", start)}; {DescribePoint("end", end)}";
            }

            Random random = new Random(864209753);
            int totalCount = 0;
            int successCount = 0;
            List<string> failures = new();

            for (int i = 0; i < initialCaseCount; i++)
            {
                SurveyPoint start = new SurveyPoint()
                {
                    X = 0.0,
                    Y = 0.0,
                    Z = 0.0,
                    Abscissa = 0.0,
                    Inclination = 0.3 + 0.7 * random.NextDouble(),
                    Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                };

                for (int j = 0; j < nextCaseCount; j++)
                {
                    SurveyPoint end = new SurveyPoint()
                    {
                        Abscissa = 10.0 + 990.0 * random.NextDouble()
                    };

                    double dls = 1e-4 + 2e-2 * random.NextDouble();
                    double tf = -Numeric.PI + 2.0 * Numeric.PI * random.NextDouble();

                    bool okForward = start.CompleteCDTSDT(end, dls, tf);
                    if (!okForward ||
                        end.X == null || end.Y == null || end.Z == null ||
                        end.Abscissa == null || end.Inclination == null || end.Azimuth == null ||
                        end.Curvature == null || end.Toolface == null)
                    {
                        for (int k = 0; k < interpolationCount; k++)
                        {
                            totalCount++;
                            failures.Add(
                                "Forward CDT calculation failed. " +
                                DescribeCase(i, j, k + 1, null, start, end));
                        }
                        continue;
                    }

                    for (int k = 1; k <= interpolationCount; k++)
                    {
                        totalCount++;
                        double abscissa = end.Abscissa.Value * k / (interpolationCount + 1.0);
                        SurveyPoint interpolated = new SurveyPoint();
                        bool okInterpolation = start.InterpolateAtAbscissa(end, abscissa, interpolated, TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod);
                        if (!okInterpolation ||
                            interpolated.X == null || interpolated.Y == null || interpolated.Z == null ||
                            interpolated.Abscissa == null || interpolated.Inclination == null || interpolated.Azimuth == null)
                        {
                            failures.Add(
                                "CDT interpolation failed. " +
                                DescribeCase(i, j, k, abscissa, start, end) + "; " +
                                DescribePoint("interpolated", interpolated));
                            continue;
                        }

                        SurveyPoint expectedInterpolated = new SurveyPoint()
                        {
                            Abscissa = abscissa
                        };
                        bool okExpected = start.CompleteCDTSDT(expectedInterpolated, end.Curvature.Value, end.Toolface.Value);
                        if (!okExpected ||
                            expectedInterpolated.X == null || expectedInterpolated.Y == null || expectedInterpolated.Z == null ||
                            expectedInterpolated.Abscissa == null || expectedInterpolated.Inclination == null || expectedInterpolated.Azimuth == null ||
                            expectedInterpolated.Curvature == null || expectedInterpolated.Toolface == null)
                        {
                            failures.Add(
                                "Expected CDT interpolation calculation failed. " +
                                DescribeCase(i, j, k, abscissa, start, end) + "; " +
                                DescribePoint("interpolated", interpolated) + "; " +
                                DescribePoint("expectedInterpolated", expectedInterpolated));
                            continue;
                        }

                        bool okCurvature = System.Math.Abs(expectedInterpolated.Curvature.Value - end.Curvature.Value) <= tolerance;
                        bool okToolface = System.Math.Abs(expectedInterpolated.Toolface.Value - end.Toolface.Value) <= tolerance;
                        bool okX = System.Math.Abs(interpolated.X.Value - expectedInterpolated.X.Value) <= tolerance;
                        bool okY = System.Math.Abs(interpolated.Y.Value - expectedInterpolated.Y.Value) <= tolerance;
                        bool okZ = System.Math.Abs(interpolated.Z.Value - expectedInterpolated.Z.Value) <= tolerance;
                        bool okInclination = System.Math.Abs(interpolated.Inclination.Value - expectedInterpolated.Inclination.Value) <= tolerance;
                        bool okAzimuth = System.Math.Abs(interpolated.Azimuth.Value - expectedInterpolated.Azimuth.Value) <= tolerance;

                        if (okCurvature && okToolface && okX && okY && okZ && okInclination && okAzimuth)
                        {
                            successCount++;
                        }
                        else
                        {
                            failures.Add(
                                "CDT consistency mismatch. " +
                                DescribeCase(i, j, k, abscissa, start, end) + "; " +
                                DescribePoint("interpolated", interpolated) + "; " +
                                DescribePoint("expectedInterpolated", expectedInterpolated) + "; " +
                                $"checks: curvature={okCurvature}, toolface={okToolface}, x={okX}, y={okY}, z={okZ}, inclination={okInclination}, azimuth={okAzimuth}; " +
                                $"results: expectedCurvature={end.Curvature.Value:R}, actualCurvature={expectedInterpolated.Curvature.Value:R}, " +
                                $"expectedToolface={end.Toolface.Value:R}, actualToolface={expectedInterpolated.Toolface.Value:R}, " +
                                $"expectedInterpolated=({expectedInterpolated.X.Value:R}, {expectedInterpolated.Y.Value:R}, {expectedInterpolated.Z.Value:R}, {expectedInterpolated.Inclination.Value:R}, {expectedInterpolated.Azimuth.Value:R}), " +
                                $"actualInterpolated=({interpolated.X.Value:R}, {interpolated.Y.Value:R}, {interpolated.Z.Value:R}, {interpolated.Inclination.Value:R}, {interpolated.Azimuth.Value:R}).");
                        }
                    }
                }
            }

            string summary =
                $"Successful cases: {successCount}/{totalCount}. " +
                $"Failed cases: {failures.Count}. " +
                $"{(failures.Count > 0 ? Environment.NewLine + string.Join(Environment.NewLine, failures) : "All cases succeeded.")}";

            Assert.That(failures, Is.Empty, summary);
        }

        [Test]
        public void Test23InterpolateAtAbscissaSurveyPointCalculationMethodConsistency()
        {
            const int initialCaseCount = 100;
            const int nextCaseCount = 100;
            const int interpolationCount = 10;
            const double angleEpsilon = 1e-3;
            const double tolerance = 1e-5;
            const double caTolerance = 1e-3;

            static double NormalizeAngle(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0.0)
                {
                    angle += twoPi;
                }
                return angle;
            }

            static double SignedAngleDifference(double angle1, double angle2)
            {
                double diff = NormalizeAngle(angle1) - NormalizeAngle(angle2);
                if (diff > Numeric.PI)
                {
                    diff -= 2.0 * Numeric.PI;
                }
                else if (diff < -Numeric.PI)
                {
                    diff += 2.0 * Numeric.PI;
                }
                return diff;
            }

            static string DescribePoint(string label, SurveyPoint point)
                => $"{label}: " +
                   $"s={(point.Abscissa?.ToString("R") ?? "null")}, " +
                   $"incl={(point.Inclination?.ToString("R") ?? "null")}, " +
                   $"az={(point.Azimuth?.ToString("R") ?? "null")}, " +
                   $"x={(point.X?.ToString("R") ?? "null")}, " +
                   $"y={(point.Y?.ToString("R") ?? "null")}, " +
                   $"z={(point.Z?.ToString("R") ?? "null")}, " +
                   $"curv={(point.Curvature?.ToString("R") ?? "null")}, " +
                   $"tf={(point.Toolface?.ToString("R") ?? "null")}, " +
                   $"bur={(point.BUR?.ToString("R") ?? "null")}, " +
                   $"tur={(point.TUR?.ToString("R") ?? "null")}";

            static string DescribeCase(string method, int initialCaseIndex, int nextCaseIndex, int interpolationIndex, double abscissa, SurveyPoint start, SurveyPoint end)
                => $"{method}: initial case={initialCaseIndex}, next case={nextCaseIndex}, interpolation={interpolationIndex}, targetAbscissa={abscissa:R}; " +
                   $"{DescribePoint("start", start)}; {DescribePoint("end", end)}";

            Random random = new Random(975318642);
            int totalCount = 0;
            int successCount = 0;
            List<string> failures = new();

            for (int i = 0; i < initialCaseCount; i++)
            {
                SurveyPoint start = new SurveyPoint()
                {
                    X = 0.0,
                    Y = 0.0,
                    Z = 0.0,
                    Abscissa = 0.0,
                    Inclination = 0.3 + 0.7 * random.NextDouble(),
                    Azimuth = 2.0 * Numeric.PI * random.NextDouble()
                };

                for (int j = 0; j < nextCaseCount; j++)
                {
                    double length = 10.0 + 990.0 * random.NextDouble();
                    double endInclination = System.Math.Max(
                        angleEpsilon,
                        System.Math.Min(Numeric.PI - angleEpsilon, start.Inclination!.Value + (-0.5 + random.NextDouble())));
                    double endAzimuth = start.Azimuth!.Value + (-1.0 + 2.0 * random.NextDouble());

                    SurveyPoint endCA = new SurveyPoint()
                    {
                        Abscissa = length,
                        Inclination = endInclination,
                        Azimuth = endAzimuth
                    };
                    bool okCAForward = start.CompleteFromSIA(endCA, TrajectoryCalculationType.MinimumCurvatureMethod);

                    SurveyPoint endBT = new SurveyPoint()
                    {
                        Abscissa = length,
                        Inclination = endInclination,
                        Azimuth = endAzimuth
                    };
                    bool okBTForward = start.CompleteFromSIA(endBT, TrajectoryCalculationType.ConstantBuildAndTurnMethod);

                    SurveyPoint endCDT = new SurveyPoint()
                    {
                        Abscissa = length
                    };
                    double dls = 1e-4 + 2e-2 * random.NextDouble();
                    double tf = -Numeric.PI + 2.0 * Numeric.PI * random.NextDouble();
                    bool okCDTForward = start.CompleteCDTSDT(endCDT, dls, tf);

                    for (int k = 1; k <= interpolationCount; k++)
                    {
                        double abscissa = length * k / (interpolationCount + 1.0);

                        totalCount++;
                        if (!okCAForward ||
                            endCA.X == null || endCA.Y == null || endCA.Z == null ||
                            endCA.Abscissa == null || endCA.Inclination == null || endCA.Azimuth == null ||
                            endCA.Curvature == null)
                        {
                            failures.Add("CA forward calculation failed. " + DescribeCase("CA", i, j, k, abscissa, start, endCA));
                        }
                        else
                        {
                            SurveyPoint interpolatedCA = new SurveyPoint();
                            bool okInterpolation = start.InterpolateAtAbscissa(endCA, abscissa, interpolatedCA, TrajectoryCalculationType.MinimumCurvatureMethod);
                            double expectedCurvature = endCA.Curvature.Value;
                            if (!okInterpolation || interpolatedCA.Curvature == null)
                            {
                                failures.Add("CA interpolation failed. " + DescribeCase("CA", i, j, k, abscissa, start, endCA) + "; " + DescribePoint("interpolated", interpolatedCA));
                            }
                            else if (System.Math.Abs(interpolatedCA.Curvature.Value - expectedCurvature) <= caTolerance)
                            {
                                successCount++;
                            }
                            else
                            {
                                failures.Add(
                                    "CA curvature mismatch. " +
                                    DescribeCase("CA", i, j, k, abscissa, start, endCA) + "; " +
                                    DescribePoint("interpolated", interpolatedCA) + "; " +
                                    $"expectedCurvature={expectedCurvature:R}, actualCurvature={interpolatedCA.Curvature.Value:R}.");
                            }
                        }

                        totalCount++;
                        if (!okBTForward ||
                            endBT.X == null || endBT.Y == null || endBT.Z == null ||
                            endBT.Abscissa == null || endBT.Inclination == null || endBT.Azimuth == null ||
                            endBT.BUR == null || endBT.TUR == null)
                        {
                            failures.Add("BT forward calculation failed. " + DescribeCase("BT", i, j, k, abscissa, start, endBT));
                        }
                        else
                        {
                            SurveyPoint interpolatedBT = new SurveyPoint();
                            bool okInterpolation = start.InterpolateAtAbscissa(endBT, abscissa, interpolatedBT, TrajectoryCalculationType.ConstantBuildAndTurnMethod);
                            if (!okInterpolation || interpolatedBT.BUR == null || interpolatedBT.TUR == null)
                            {
                                failures.Add("BT interpolation failed. " + DescribeCase("BT", i, j, k, abscissa, start, endBT) + "; " + DescribePoint("interpolated", interpolatedBT));
                            }
                            else
                            {
                                bool okBUR = System.Math.Abs(interpolatedBT.BUR.Value - endBT.BUR.Value) <= tolerance;
                                bool okTUR = System.Math.Abs(interpolatedBT.TUR.Value - endBT.TUR.Value) <= tolerance;
                                if (okBUR && okTUR)
                                {
                                    successCount++;
                                }
                                else
                                {
                                    failures.Add(
                                        "BT BUR/TUR mismatch. " +
                                        DescribeCase("BT", i, j, k, abscissa, start, endBT) + "; " +
                                        DescribePoint("interpolated", interpolatedBT) + "; " +
                                        $"expectedBUR={endBT.BUR.Value:R}, actualBUR={interpolatedBT.BUR.Value:R}, " +
                                        $"expectedTUR={endBT.TUR.Value:R}, actualTUR={interpolatedBT.TUR.Value:R}.");
                                }
                            }
                        }

                        totalCount++;
                        if (!okCDTForward ||
                            endCDT.X == null || endCDT.Y == null || endCDT.Z == null ||
                            endCDT.Abscissa == null || endCDT.Inclination == null || endCDT.Azimuth == null ||
                            endCDT.Curvature == null || endCDT.Toolface == null)
                        {
                            failures.Add("CDT forward calculation failed. " + DescribeCase("CDT", i, j, k, abscissa, start, endCDT));
                        }
                        else
                        {
                            SurveyPoint interpolatedCDT = new SurveyPoint();
                            bool okInterpolation = start.InterpolateAtAbscissa(endCDT, abscissa, interpolatedCDT, TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod);
                            if (!okInterpolation || interpolatedCDT.Curvature == null || interpolatedCDT.Toolface == null)
                            {
                                failures.Add("CDT interpolation failed. " + DescribeCase("CDT", i, j, k, abscissa, start, endCDT) + "; " + DescribePoint("interpolated", interpolatedCDT));
                            }
                            else
                            {
                                bool okCurvature = System.Math.Abs(interpolatedCDT.Curvature.Value - endCDT.Curvature.Value) <= tolerance;
                                bool okToolface = System.Math.Abs(SignedAngleDifference(interpolatedCDT.Toolface.Value, endCDT.Toolface.Value)) <= tolerance;
                                if (okCurvature && okToolface)
                                {
                                    successCount++;
                                }
                                else
                                {
                                    failures.Add(
                                        "CDT curvature/toolface mismatch. " +
                                        DescribeCase("CDT", i, j, k, abscissa, start, endCDT) + "; " +
                                        DescribePoint("interpolated", interpolatedCDT) + "; " +
                                        $"expectedCurvature={endCDT.Curvature.Value:R}, actualCurvature={interpolatedCDT.Curvature.Value:R}, " +
                                        $"expectedToolface={endCDT.Toolface.Value:R}, actualToolface={interpolatedCDT.Toolface.Value:R}.");
                                }
                            }
                        }
                    }
                }
            }

            string summary =
                $"Successful checks: {successCount}/{totalCount}. " +
                $"Failed checks: {failures.Count}. " +
                $"{(failures.Count > 0 ? Environment.NewLine + string.Join(Environment.NewLine, failures) : "All checks succeeded.")}";

            Assert.That(failures, Is.Empty, summary);
        }
    }
}
