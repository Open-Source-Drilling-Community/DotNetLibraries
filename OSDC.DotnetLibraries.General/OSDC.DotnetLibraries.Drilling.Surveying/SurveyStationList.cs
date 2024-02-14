using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Statistics;
using System.Drawing;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    /// <summary>
    /// A list of SurveyStation
    /// </summary>
    public class SurveyStationList : List<SurveyStation>
    {

        /// <summary>
        /// calculate the whole survey station list from the starting survey.
        /// The first survey shall be complete
        /// </summary>
        /// <returns></returns>
        public bool Calculate()
        {
            return Survey.Calculate(this);
        }

        /// <summary>
        /// Realize a SurveyList generated using the covariance matrices long the SurveyStationList.
        /// The last survey station is used to draw a value using its covariance matrix.
        /// This point is drawn in the coordinate system oriented by the principal components of the covariance.
        /// The corresponding chi square is determined as well as the latitude and longitude one the corresponding ellipsoid.
        /// Previous survey station covariances are used to find a point (in the local coordinate system defined by the principal directions)
        /// which is at the surface of an ellipoid at the same chi square and with the same latitude and longitude as those drawn from the
        /// last survey station.
        /// Each of those points are converted back into the global coordinate system.
        /// The length, inclination, azimuth etc. are calculated using CompleteXYZ.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public SurveyList? Realize()
        {
            if (Count > 0 &&
                this.Last<SurveyStation>()?.Covariance != null)
            {
                SurveyStation station = this.Last<SurveyStation>();
                Matrix3x3 eigenVectors = new Matrix3x3();
                Vector3D eigenValues = new Vector3D();
                station.DiagonalizeCovariance(eigenVectors, eigenValues);
                if (eigenValues[0] != null && eigenValues[1] != null & eigenValues[2] != null &&
                    Numeric.GT(eigenValues[0], 0) && Numeric.GT(eigenValues[1], 0) && Numeric.GT(eigenValues[2], 0) &&
                    eigenVectors[0, 0] != null &&
                    eigenVectors[0, 1] != null &&
                    eigenVectors[0, 2] != null &&
                    eigenVectors[1, 0] != null &&
                    eigenVectors[1, 1] != null &&
                    eigenVectors[1, 2] != null &&
                    eigenVectors[2, 0] != null &&
                    eigenVectors[2, 1] != null &&
                    eigenVectors[2, 2] != null)
                {
                    double e02 = eigenValues[0].Value;
                    double e12 = eigenValues[1].Value;
                    double e22 = eigenValues[2].Value;
                    double e0 = System.Math.Sqrt(e02);
                    double e1 = System.Math.Sqrt(e12);
                    double e2 = System.Math.Sqrt(e22);
                    GaussianDistribution dist0 = new GaussianDistribution(0, e0);
                    GaussianDistribution dist1 = new GaussianDistribution(0, e1);
                    GaussianDistribution dist2 = new GaussianDistribution(0, e2);
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
                        SphericalPoint3D pt = new SphericalPoint3D() { X= a0.Value, Y = a1.Value, Z= a2.Value };
                        if (pt.Longitude != null && pt.Latitude != null)
                        {
                            double latitude = pt.Latitude.Value;
                            double longitude = pt.Longitude.Value;
                            SurveyList realization = new SurveyList();

                            double p11 = eigenVectors[0, 0].Value;
                            double p12 = eigenVectors[0, 1].Value;
                            double p13 = eigenVectors[0, 2].Value;
                            double p21 = eigenVectors[1, 0].Value;
                            double p22 = eigenVectors[1, 1].Value;
                            double p23 = eigenVectors[1, 2].Value;
                            double p31 = eigenVectors[2, 0].Value;
                            double p32 = eigenVectors[2, 1].Value;
                            double p33 = eigenVectors[2, 2].Value;

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

                            Survey point = new Survey();
                            point.X = station.RiemannianNorth + ((station.Bias == null || station.Bias.X == null) ? 0 : station.Bias.X.Value) + X;
                            point.Y = station.RiemannianEast + ((station.Bias == null || station.Bias.Y == null) ? 0 : station.Bias.Y.Value) + Y;
                            point.Z = station.TVD + ((station.Bias == null || station.Bias.Z == null) ? 0 : station.Bias.Z.Value) + Z;
                            double cosLat = Math.Cos(latitude);
                            double sinLat = Math.Sin(latitude);
                            double cosLon = Math.Cos(longitude);
                            double sinLon = Math.Sin(longitude);
                            double cosLat2 = cosLat * cosLat;
                            double sinLat2 = sinLat * sinLat;  
                            double cosLon2 = cosLon * cosLon;
                            double sinLon2 = sinLon * sinLon;
                            realization.Insert(0, point);
                            int last = Count - 1;
                            for (int i = last-1; i >= 0; i--)
                            {
                                station = this[i];
                                if (station != null &&
                                    station.DiagonalizeCovariance(eigenVectors, eigenValues) &&
                                    eigenValues[0] != null && eigenValues[1] != null & eigenValues[2] != null &&
                                    Numeric.GT(eigenValues[0], 0) && Numeric.GT(eigenValues[1], 0) && Numeric.GT(eigenValues[2], 0) &&
                                    eigenVectors[0, 0] != null &&
                                    eigenVectors[0, 1] != null &&
                                    eigenVectors[0, 2] != null &&
                                    eigenVectors[1, 0] != null &&
                                    eigenVectors[1, 1] != null &&
                                    eigenVectors[1, 2] != null &&
                                    eigenVectors[2, 0] != null &&
                                    eigenVectors[2, 1] != null &&
                                    eigenVectors[2, 2] != null)
                                {
                                    e02 = eigenValues[0].Value;
                                    e12 = eigenValues[1].Value;
                                    e22 = eigenValues[2].Value;
                                    double denom = cosLat2 * cosLon2 / e02 + cosLat2 * sinLon2 / e12 + sinLat2 / e22;
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
                                        point = new Survey();
                                        point.X = station.RiemannianNorth + ((station.Bias == null || station.Bias.X == null) ? 0 : station.Bias.X.Value) + X;
                                        point.Y = station.RiemannianEast + ((station.Bias == null || station.Bias.Y == null) ? 0 : station.Bias.Y.Value) + Y;
                                        point.Z = station.TVD + ((station.Bias == null || station.Bias.Z == null) ? 0 : station.Bias.Z.Value) + Z;
                                        realization.Insert(0, point);
                                    }
                                }
                            }
                            // calculate the inclination, azimuth, length, etc.
                            if (realization.Count > 0)
                            {
                                Survey survey = realization.First<Survey>();
                                station = this.First<SurveyStation>();
                                survey.Inclination = station.Inclination;
                                survey.Azimuth = station.Azimuth;
                                survey.MD = station.MD;
                                int c = realization.Count;
                                for (int i = 1; i < c; i++)
                                {
                                    realization[i - 1].CompleteXYZ(realization[i]);
                                }
                                return  realization;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
