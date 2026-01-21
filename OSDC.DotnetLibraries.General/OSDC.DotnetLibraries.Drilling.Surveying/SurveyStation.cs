using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Statistics;
using System.Linq;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public class SurveyStation : SurveyPoint
    {
        /// <summary>
        /// The covariance matrix describing the wellbore position uncertainty at this survey point, relative to the Riemannian manifold
        /// </summary>
        public SymmetricMatrix3x3? Covariance { get; set; }
        /// <summary>
        /// The eigen vectors of the covariance matrix
        /// </summary>
        public Matrix3x3? EigenVectors { get; set; }
        /// <summary>
        /// The eigen values of the covariance matrix
        /// </summary>
        public Vector3D? EigenValues { get; set; }
        /// <summary>
        /// A bias as used in the Wolff and de Wardt Wellbore position uncertainty model
        /// </summary>
        public Vector3D? Bias { get; set; }
        /// <summary>
        ///  accessor to the survey station uncertainty
        /// </summary>
        public SurveyInstrument? SurveyTool { get; set; }
        /// <summary>
        /// The borehole radius at the survey station
        /// </summary>
        public double? BoreholeRadius { get; set; } = 0.0;

        /// <summary>
        /// Calculates the eigen vectors and eigen values of the covariance matrix
        /// </summary>
        /// <returns>true if calculation went ok, false otherwise</returns>
        public bool CalculateEigenProperties()
        {
            if (Covariance is { } cov && cov[0, 0] is double cov00 &&
                                         cov[0, 1] is double cov01 &&
                                         cov[0, 2] is double cov02 &&
                                         cov[1, 0] is double cov10 &&
                                         cov[1, 1] is double cov11 &&
                                         cov[1, 2] is double cov12 &&
                                         cov[2, 0] is double cov20 &&
                                         cov[2, 1] is double cov21 &&
                                         cov[2, 2] is double cov22)
            {
                EigenVectors = new();
                EigenValues = new();
                double[,] z = new double[3, 3];
                z[0, 0] = cov00;
                z[0, 1] = cov01;
                z[0, 2] = cov02;
                z[1, 0] = cov10;
                z[1, 1] = cov11;
                z[1, 2] = cov12;
                z[2, 0] = cov20;
                z[2, 1] = cov21;
                z[2, 2] = cov22;
                double[] d = new double[4];
                double[] e = new double[3];
                // tranform a symmetric matrix into a tridiagonal matrix
                Tred2(3, z, d, e);
                // QL decomposition
                Tql2(3, z, d, e);
                //transfer eigenvectors
                EigenVectors[0, 0] = z[0, 0];
                EigenVectors[0, 1] = z[0, 1];
                EigenVectors[0, 2] = z[0, 2];
                EigenVectors[1, 0] = z[1, 0];
                EigenVectors[1, 1] = z[1, 1];
                EigenVectors[1, 2] = z[1, 2];
                EigenVectors[2, 0] = z[2, 0];
                EigenVectors[2, 1] = z[2, 1];
                EigenVectors[2, 2] = z[2, 2];
                // transfer eigen values
                EigenValues[0] = d[0];
                EigenValues[1] = d[1];
                EigenValues[2] = d[2];
                return true;
            }
            return false;
        }
        public bool CalculateSphericalCoordinatesAtPosition(SphericalPoint3D point)
        {
            if (RiemannianNorth is double rNorth && RiemannianEast is double rEast && TVD is double tvd &&
                point.X is double px && point.Y is double py && point.Z is double pz &&
                EigenVectors is { } &&
                EigenVectors[0, 0] is double e00 &&
                EigenVectors[0, 1] is double e01 &&
                EigenVectors[0, 2] is double e02 &&
                EigenVectors[1, 0] is double e10 &&
                EigenVectors[1, 1] is double e11 &&
                EigenVectors[1, 2] is double e12 &&
                EigenVectors[2, 0] is double e20 &&
                EigenVectors[2, 1] is double e21 &&
                EigenVectors[2, 2] is double e22 &&
                EigenValues is { } &&
                EigenValues[0] is double ev0 &&
                EigenValues[1] is double ev1 &&
                EigenValues[2] is double ev2)
            {
                double p11 = e00;
                double p12 = e01;
                double p13 = e02;
                double p21 = e10;
                double p22 = e11;
                double p23 = e12;
                double p31 = e20;
                double p32 = e21;
                double p33 = e22;

                double a = ev0;
                double b = ev1;
                double c = ev2;

                double x = px - rNorth - ((Bias == null || Bias.X == null) ? 0 : Bias.X.Value);
                double y = py - rEast - ((Bias == null || Bias.Y == null) ? 0 : Bias.Y.Value);
                double z = pz - tvd - ((Bias == null || Bias.Z == null) ? 0 : Bias.Z.Value);

                double X = p11 * x + p21 * y + p31 * z;
                double Y = p12 * x + p22 * y + p32 * z;
                double Z = p13 * x + p23 * y + p33 * z;
                point.Longitude = System.Math.Atan2(Y * System.Math.Sqrt(a), X * System.Math.Sqrt(b));
                if (!Numeric.EQ(c, 0))
                {
                    point.Latitude = System.Math.Asin(Z / System.Math.Sqrt(c));
                }
                else
                {
                    point.Latitude = 0.0;
                }
                point.R = System.Math.Sqrt(x * x + y * y + z * z);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool CalculateRiemannianCoordinatesAtSphericalPosition(SphericalPoint3D point)
        {
            if (RiemannianNorth is double rNorth && RiemannianEast is double rEast && TVD is double tvd &&
                point.R is double pr &&
                point.Latitude is double pLat &&
                point.Longitude is double pLon &&
                EigenVectors is { } &&
                EigenVectors[0, 0] is double e00 &&
                EigenVectors[0, 1] is double e01 &&
                EigenVectors[0, 2] is double e02 &&
                EigenVectors[1, 0] is double e10 &&
                EigenVectors[1, 1] is double e11 &&
                EigenVectors[1, 2] is double e12 &&
                EigenVectors[2, 0] is double e20 &&
                EigenVectors[2, 1] is double e21 &&
                EigenVectors[2, 2] is double e22)
            {
                double p11 = e00;
                double p12 = e01;
                double p13 = e02;
                double p21 = e10;
                double p22 = e11;
                double p23 = e12;
                double p31 = e20;
                double p32 = e21;
                double p33 = e22;

                // calculate the inverse of the eigenvectors
                double determinant = (p11 * p22 - p12 * p21) * p33 + (p13 * p21 - p11 * p23) * p32 + (p12 * p23 - p13 * p22) * p31;
                double pi11 = (p22 * p33 - p23 * p32) / determinant;
                double pi21 = -(p12 * p33 - p13 * p32) / determinant;
                double pi31 = (p12 * p23 - p13 * p22) / determinant;
                double pi12 = -(p21 * p33 - p23 * p31) / determinant;
                double pi22 = (p11 * p33 - p13 * 31) / determinant;
                double pi32 = -(p11 * p23 - p13 * p21) / determinant;
                double pi13 = (p21 * p32 - p22 * p31) / determinant;
                double pi23 = -(p11 * p32 - p12 * p31) / determinant;
                double pi33 = (p11 * p22 - p12 * p21) / determinant;


                double ci = System.Math.Cos(pLat);
                double si = System.Math.Sin(pLat);
                double ca = System.Math.Cos(pLon);
                double sa = System.Math.Sin(pLon);
                double x = pr * ci * ca;
                double y = pr * ci * sa;
                double z = pr * si;

                double X = pi11 * x + pi21 * y + pi31 * z;
                double Y = pi12 * x + pi22 * y + pi32 * z;
                double Z = pi13 * x + pi23 * y + pi33 * z;

                point.X = rNorth + ((Bias == null || Bias.X == null) ? 0 : Bias.X.Value) + X;
                point.Y = rEast + ((Bias == null || Bias.Y == null) ? 0 : Bias.Y.Value) + Y;
                point.Z = tvd + ((Bias == null || Bias.Z == null) ? 0 : Bias.Z.Value) + Z;

                return true;
            }
            return false;
        }
        /// <summary>
        /// Realize a SurveyList generated using the covariance matrices along the SurveyStationList.
        /// The last survey station is used to draw a value using its covariance matrix.
        /// This point is drawn in the coordinate system oriented by the principal components of the covariance.
        /// The corresponding chi square is determined as well as the latitude and longitude of the corresponding ellipsoid.
        /// Previous survey station covariances are used to find a point
        ///     - in the local coordinate system defined by the principal directions (lat/long)
        ///     - at the surface of an ellipoid
        ///     - at the same chi square
        ///     - and with the same latitude and longitude as those drawn from the last survey station (because the error is systematic, hence the same at every station)
        /// Each of those points are converted back into the global coordinate system.
        /// The length, inclination, azimuth etc. are calculated using CompleteXYZ.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<SurveyPoint>? Realize(List<SurveyStation> surveyStationList)
        {
            if (surveyStationList.Count > 0 &&
                surveyStationList.Last<SurveyStation>() is { } station &&
                station.Covariance is { })
            {
                if (station.CalculateEigenProperties() &&
                    station.EigenVectors is { } eigenVectors &&
                    station.EigenValues is { } eigenValues &&
                    eigenVectors[0, 0] is double e00 &&
                    eigenVectors[0, 1] is double e01 &&
                    eigenVectors[0, 2] is double e02 &&
                    eigenVectors[1, 0] is double e10 &&
                    eigenVectors[1, 1] is double e11 &&
                    eigenVectors[1, 2] is double e12 &&
                    eigenVectors[2, 0] is double e20 &&
                    eigenVectors[2, 1] is double e21 &&
                    eigenVectors[2, 2] is double e22 &&
                    eigenValues[0] is double ev0 &&
                    eigenValues[1] is double ev1 &&
                    eigenValues[2] is double ev2 &&
                    Numeric.GT(ev0, 0) && Numeric.GT(ev1, 0) && Numeric.GT(ev2, 0))
                {
                    GaussianDistribution dist0 = new(0, System.Math.Sqrt(ev0));
                    GaussianDistribution dist1 = new(0, System.Math.Sqrt(ev1));
                    GaussianDistribution dist2 = new(0, System.Math.Sqrt(ev2));
                    // draw three values
                    double? a0 = dist0.Realize();
                    double? a1 = dist1.Realize();
                    double? a2 = dist2.Realize();
                    if (a0 != null && a1 != null && a2 != null)
                    {
                        double a02 = a0.Value * a0.Value;
                        double a12 = a1.Value * a1.Value;
                        double a22 = a2.Value * a2.Value;
                        double chiSquare = a02 / e02 + a12 / e12 + a22 / e22;
                        SphericalPoint3D pt = new() { X = a0.Value, Y = a1.Value, Z = a2.Value };
                        if (pt.Longitude is double longitude && pt.Latitude is double latitude)
                        {
                            List<SurveyPoint> realization = new();

                            double p11 = e00;
                            double p12 = e01;
                            double p13 = e02;
                            double p21 = e10;
                            double p22 = e11;
                            double p23 = e12;
                            double p31 = e20;
                            double p32 = e21;
                            double p33 = e22;

                            // calculate the inverse of the eigenvectors
                            double determinant = (p11 * p22 - p12 * p21) * p33 + (p13 * p21 - p11 * p23) * p32 + (p12 * p23 - p13 * p22) * p31;
                            double pi11 = (p22 * p33 - p23 * p32) / determinant;
                            double pi21 = -(p12 * p33 - p13 * p32) / determinant;
                            double pi31 = (p12 * p23 - p13 * p22) / determinant;
                            double pi12 = -(p21 * p33 - p23 * p31) / determinant;
                            double pi22 = (p11 * p33 - p13 * 31) / determinant;
                            double pi32 = -(p11 * p23 - p13 * p21) / determinant;
                            double pi13 = (p21 * p32 - p22 * p31) / determinant;
                            double pi23 = -(p11 * p32 - p12 * p31) / determinant;
                            double pi33 = (p11 * p22 - p12 * p21) / determinant;

                            double X = pi11 * a0.Value + pi21 * a1.Value + pi31 * a2.Value;
                            double Y = pi12 * a0.Value + pi22 * a1.Value + pi32 * a2.Value;
                            double Z = pi13 * a0.Value + pi23 * a1.Value + pi33 * a2.Value;

                            SurveyPoint point = new()
                            {
                                X = station.RiemannianNorth + ((station.Bias == null || station.Bias.X == null) ? 0 : station.Bias.X.Value) + X,
                                Y = station.RiemannianEast + ((station.Bias == null || station.Bias.Y == null) ? 0 : station.Bias.Y.Value) + Y,
                                Z = station.TVD + ((station.Bias == null || station.Bias.Z == null) ? 0 : station.Bias.Z.Value) + Z
                            };
                            double cosLat = Math.Cos(latitude);
                            double sinLat = Math.Sin(latitude);
                            double cosLon = Math.Cos(longitude);
                            double sinLon = Math.Sin(longitude);
                            double cosLat2 = cosLat * cosLat;
                            double sinLat2 = sinLat * sinLat;
                            double cosLon2 = cosLon * cosLon;
                            double sinLon2 = sinLon * sinLon;
                            realization.Insert(0, point);
                            int last = surveyStationList.Count - 1;
                            for (int i = last - 1; i >= 0; i--)
                            {
                                station = surveyStationList[i];
                                if (station is { } &&
                                    station.CalculateEigenProperties() &&
                                    station.EigenValues is { } eValues &&
                                    eValues[0] is double eiv0 &&
                                    eValues[1] is double eiv1 &&
                                    eValues[2] is double eiv2 &&
                                    Numeric.GT(eiv0, 0) && Numeric.GT(eiv1, 0) && Numeric.GT(eiv2, 0))
                                {
                                    double denom = cosLat2 * cosLon2 / eiv0 + cosLat2 * sinLon2 / eiv1 + sinLat2 / eiv2;
                                    double r = 0;
                                    if (!Numeric.EQ(denom, 0))
                                    {
                                        r = System.Math.Sqrt(chiSquare / denom);
                                    }
                                    pt.SetSpherical(r, latitude, longitude);
                                    if (pt.X != null && pt.Y != null && pt.Z != null)
                                    {
                                        X = pt.X.Value;
                                        Y = pt.Y.Value;
                                        Z = pt.Z.Value;
                                        point = new SurveyPoint
                                        {
                                            X = station.RiemannianNorth + ((station.Bias == null || station.Bias.X == null) ? 0 : station.Bias.X.Value) + X,
                                            Y = station.RiemannianEast + ((station.Bias == null || station.Bias.Y == null) ? 0 : station.Bias.Y.Value) + Y,
                                            Z = station.TVD + ((station.Bias == null || station.Bias.Z == null) ? 0 : station.Bias.Z.Value) + Z
                                        };
                                        realization.Insert(0, point);
                                    }
                                }
                            }
                            // calculate the inclination, azimuth, length, etc.
                            if (realization.Count > 0)
                            {
                                SurveyPoint survey = realization.First<SurveyPoint>();
                                station = surveyStationList.First<SurveyStation>();
                                survey.Inclination = station.Inclination;
                                survey.Azimuth = station.Azimuth;
                                survey.MD = station.MD;
                                int c = realization.Count;
                                for (int i = 1; i < c; i++)
                                {
                                    realization[i - 1].CompleteFromXYZ(realization[i]);
                                }
                                return realization;
                            }
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        private static void Tred2(int n, double[,] V, double[] d, double[] e)
        {

            //  This is derived from the Algol procedures tred2 by
            //  Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
            //  Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
            //  Fortran subroutine in EISPACK.

            for (int j = 0; j < n; j++)
            {
                d[j] = V[n - 1, j];
            }

            // Householder reduction to tridiagonal form.

            for (int i = n - 1; i > 0; i--)
            {

                // Scale to avoid under/overflow.

                double scale = 0.0;
                double h = 0.0;
                for (int k = 0; k < i; k++)
                {
                    scale = scale + System.Math.Abs(d[k]);
                }
                if (scale == 0.0)
                {
                    e[i] = d[i - 1];
                    for (int j = 0; j < i; j++)
                    {
                        d[j] = V[i - 1, j];
                        V[i, j] = 0.0;
                        V[j, i] = 0.0;
                    }
                }
                else
                {

                    // Generate Householder vector.

                    for (int k = 0; k < i; k++)
                    {
                        d[k] /= scale;
                        h += d[k] * d[k];
                    }
                    double f = d[i - 1];
                    double g = System.Math.Sqrt(h);
                    if (f > 0)
                    {
                        g = -g;
                    }
                    e[i] = scale * g;
                    h = h - f * g;
                    d[i - 1] = f - g;
                    for (int j = 0; j < i; j++)
                    {
                        e[j] = 0.0;
                    }

                    // Apply similarity transformation to remaining columns.

                    for (int j = 0; j < i; j++)
                    {
                        f = d[j];
                        V[j, i] = f;
                        g = e[j] + V[j, j] * f;
                        for (int k = j + 1; k <= i - 1; k++)
                        {
                            g += V[k, j] * d[k];
                            e[k] += V[k, j] * f;
                        }
                        e[j] = g;
                    }
                    f = 0.0;
                    for (int j = 0; j < i; j++)
                    {
                        e[j] /= h;
                        f += e[j] * d[j];
                    }
                    double hh = f / (h + h);
                    for (int j = 0; j < i; j++)
                    {
                        e[j] -= hh * d[j];
                    }
                    for (int j = 0; j < i; j++)
                    {
                        f = d[j];
                        g = e[j];
                        for (int k = j; k <= i - 1; k++)
                        {
                            V[k, j] -= (f * e[k] + g * d[k]);
                        }
                        d[j] = V[i - 1, j];
                        V[i, j] = 0.0;
                    }
                }
                d[i] = h;
            }

            // Accumulate transformations.

            for (int i = 0; i < n - 1; i++)
            {
                V[n - 1, i] = V[i, i];
                V[i, i] = 1.0;
                double h = d[i + 1];
                if (h != 0.0)
                {
                    for (int k = 0; k <= i; k++)
                    {
                        d[k] = V[k, i + 1] / h;
                    }
                    for (int j = 0; j <= i; j++)
                    {
                        double g = 0.0;
                        for (int k = 0; k <= i; k++)
                        {
                            g += V[k, i + 1] * V[k, j];
                        }
                        for (int k = 0; k <= i; k++)
                        {
                            V[k, j] -= g * d[k];
                        }
                    }
                }
                for (int k = 0; k <= i; k++)
                {
                    V[k, i + 1] = 0.0;
                }
            }
            for (int j = 0; j < n; j++)
            {
                d[j] = V[n - 1, j];
                V[n - 1, j] = 0.0;
            }
            V[n - 1, n - 1] = 1.0;
            e[0] = 0.0;
        }
        /// <summary>
        /// Symmetric tridiagonal QL algorithm
        /// </summary>
        private static void Tql2(int n, double[,] V, double[] d, double[] e)
        {

            //  This is derived from the Algol procedures tql2, by
            //  Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
            //  Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
            //  Fortran subroutine in EISPACK.

            for (int i = 1; i < n; i++)
            {
                e[i - 1] = e[i];
            }
            e[n - 1] = 0.0;

            double f = 0.0;
            double tst1 = 0.0;
            double eps = System.Math.Pow(2.0, -52.0);
            for (int l = 0; l < n; l++)
            {

                // Find small subdiagonal element

                tst1 = System.Math.Max(tst1, System.Math.Abs(d[l]) + System.Math.Abs(e[l]));
                int m = l;
                while (m < n)
                {
                    if (System.Math.Abs(e[m]) <= eps * tst1)
                    {
                        break;
                    }
                    m++;
                }

                // If m == l, d[l] is an eigenvalue,
                // otherwise, iterate.

                if (m > l)
                {
                    int iter = 0;
                    do
                    {
                        iter = iter + 1;  // (Could check iteration count here.)

                        // Compute implicit shift

                        double g = d[l];
                        double p = (d[l + 1] - g) / (2.0 * e[l]);
                        double r = Pythag(p, 1.0);
                        if (p < 0)
                        {
                            r = -r;
                        }
                        d[l] = e[l] / (p + r);
                        d[l + 1] = e[l] * (p + r);
                        double dl1 = d[l + 1];
                        double h = g - d[l];
                        for (int i = l + 2; i < n; i++)
                        {
                            d[i] -= h;
                        }
                        f += h;

                        // Implicit QL transformation.

                        p = d[m];
                        double c = 1.0;
                        double c2 = c;
                        double c3 = c;
                        double el1 = e[l + 1];
                        double s = 0.0;
                        double s2 = 0.0;
                        for (int i = m - 1; i >= l; i--)
                        {
                            c3 = c2;
                            c2 = c;
                            s2 = s;
                            g = c * e[i];
                            h = c * p;
                            r = Pythag(p, e[i]);
                            e[i + 1] = s * r;
                            s = e[i] / r;
                            c = p / r;
                            p = c * d[i] - s * g;
                            d[i + 1] = h + s * (c * g + s * d[i]);

                            // Accumulate transformation.

                            for (int k = 0; k < n; k++)
                            {
                                h = V[k, i + 1];
                                V[k, i + 1] = s * V[k, i] + c * h;
                                V[k, i] = c * V[k, i] - s * h;
                            }
                        }
                        p = -s * s2 * c3 * el1 * e[l] / dl1;
                        e[l] = s * p;
                        d[l] = c * p;

                        // Check for convergence.

                    } while (System.Math.Abs(e[l]) > eps * tst1);
                }
                d[l] = d[l] + f;
                e[l] = 0.0;
            }

            // Sort eigenvalues and corresponding vectors.

            for (int i = 0; i < n - 1; i++)
            {
                int k = i;
                double p = d[i];
                for (int j = i + 1; j < n; j++)
                {
                    if (d[j] < p)
                    {
                        k = j;
                        p = d[j];
                    }
                }
                if (k != i)
                {
                    d[k] = d[i];
                    d[i] = p;
                    for (int j = 0; j < n; j++)
                    {
                        p = V[j, i];
                        V[j, i] = V[j, k];
                        V[j, k] = p;
                    }
                }
            }
        }
        /// <summary>
        /// Pythagoras theorem
        /// </summary>
        /// <returns></returns>
        private static double Pythag(double a, double b)
        {
            double absa = System.Math.Abs(a);
            double absb = System.Math.Abs(b);
            if (absa > absb)
            {
                return absa * System.Math.Sqrt(1 + absb * absb / (absa * absa));
            }
            else
            {
                if (Numeric.EQ(absb, 0))
                {
                    return 0.0;
                }
                else
                {
                    return absb * System.Math.Sqrt(1.0 + absa * absa / (absb * absb));
                }
            }
        }
    }
}
