using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Statistics;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    /// <summary>
    /// A list of SurveyStation
    /// </summary>
    public class SurveyStationList : List<SurveyStation>
    {

        /// <summary>
        /// calculate the whole survey station list from the starting survey.
        /// The first survey must be complete
        /// </summary>
        /// <returns></returns>
        public bool Calculate()
        {
            return SurveyPoint.Calculate(this);
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
        public SurveyList? Realize()
        {
            if (Count > 0 && 
                this.Last<SurveyStation>() is { } station &&
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
                            SurveyList realization = new();

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
                            int last = Count - 1;
                            for (int i = last-1; i >= 0; i--)
                            {
                                station = this[i];
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
                                station = this.First<SurveyStation>();
                                survey.Inclination = station.Inclination;
                                survey.Azimuth = station.Azimuth;
                                survey.MD = station.MD;
                                int c = realization.Count;
                                for (int i = 1; i < c; i++)
                                {
                                    realization[i - 1].CompleteXYZ(realization[i]);
                                }
                                return realization;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
