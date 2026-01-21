using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Statistics;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public class UncertaintyEllipsoid
    {
        /// <summary>
        /// The center of the ellipsoid of uncertainty
        /// </summary>
        public SurveyStation? EllipsoidSurveyStation { get; set; }
        /// <summary>
        /// The confidence factor used to compute the ellipsoid of uncertainty
        /// </summary>
        public double? ConfidenceFactor { get; set; } = 0.95;
        /// <summary>
        /// The scaling factor used to compute the ellipsoid of uncertainty
        /// </summary>
        public double? ScalingFactor { get; set; } = 1.0;
        /// <summary>
        /// The principal axes of the ellipsoid of uncertainty
        /// </summary>
        public Vector3D? EllipsoidRadii { get; set; }
        /// <summary>
        /// The horizontal projection of the ellipsoid of uncertainty
        /// </summary>
        public UncertaintyEllipse? HorizontalEllipse { get; set; }
        /// <summary>
        /// The vertical projection of the ellipsoid of uncertainty
        /// </summary>
        public UncertaintyEllipse? VerticalEllipse { get; set; }
        /// <summary>
        /// The projection of the ellipsoid of uncertainty on a plane perpendicular to the direction of an associated survey station
        /// </summary>
        public UncertaintyEllipse? PerpendicularEllipse { get; set; }
        /// <summary>
        /// The point of the ellipsoid at the highest TVD (exact calculation)
        /// </summary>
        public Point3D? PointAtHighestTVD { get; set; }
        /// <summary>
        /// The point of the ellipsoid at the lowest TVD (exact calculation)
        /// </summary>
        public Point3D? PointAtLowestTVD { get; set; }
        /// <summary>
        /// default constructor
        /// </summary>
        public UncertaintyEllipsoid() : base()
        {
        }

        /// <summary>
        /// Calculate the parameters of the ellipsoid of uncertainty corresponding to given confidence factor,
        /// relative to the Riemannian manifold
        /// </summary>
        /// <param name="confidenceFactor">the confidence factor associated to the ellipsoid</param>
        /// <param name="surveyStation">the survey station associated to the ellipsoid</param>
        /// <param name="scalingFactor">the scaling factor to apply to the ellipsoid</param>
        /// <param name="boreholeRadius">the radius of the borehole to apply to the ellipsoid</param>
        /// <returns></returns>
        public bool Calculate()
        {
            if (EllipsoidSurveyStation is { } &&
                EllipsoidSurveyStation.EigenValues is { } ev &&
                EllipsoidSurveyStation.BoreholeRadius is double boreholeRadius &&
                ev.X is double evX &&
                ev.Y is double evY &&
                ev.Z is double evZ &&
                ConfidenceFactor is double confidenceFactor &&
                ScalingFactor is double scalingFactor)
            {
                double chiSquare = Statistics.GetChiSquare3D(confidenceFactor);
                double kScale = System.Math.Sqrt(chiSquare);
                EllipsoidRadii = new()
                {
                    X = (kScale * System.Math.Sqrt(evX) + boreholeRadius) * scalingFactor,
                    Y = (kScale * System.Math.Sqrt(evY) + boreholeRadius) * scalingFactor,
                    Z = (kScale * System.Math.Sqrt(evZ) + boreholeRadius) * scalingFactor
                };
                return
                    CalculateHorizontalEllipseParameters() &&
                    CalculateVerticalEllipseParameters() &&
                    CalculatePerpendicularEllipseParameters();
            }
            return false;
        }

        /// <summary>
        /// Calculate the ellipse parameters resulting from the projection of the ellipsoid of uncertainty on the horizontal plane (RiemannianNorth, RiemannianEast),
        /// relative to the Riemannian manifold
        /// </summary>
        /// <param name="confidenceFactor">the confidence factor associated to the ellipsoid</param>
        /// <param name="surveyStation">the survey station associated to the ellipsoid</param>
        /// <param name="scalingFactor">the scaling factor to apply to the ellipsoid</param>
        /// <param name="boreholeRadius">the radius of the borehole to apply to the ellipsoid</param>
        /// <returns></returns>
        public bool CalculateHorizontalEllipseParameters()
        {
            if (EllipsoidSurveyStation is { } surveyStation && 
                surveyStation.Covariance is { } cov &&
                surveyStation.BoreholeRadius is double boreholeRadius &&
                    cov[0, 0] is double cov00 &&
                    cov[0, 1] is double cov01 &&
                    cov[1, 1] is double cov11 &&
                    ConfidenceFactor is double confidenceFactor &&
                    ScalingFactor is double scalingFactor)
            {
                HorizontalEllipse = new() { EllipseCenter = surveyStation };
                double chiSquare = Statistics.GetChiSquare3D(confidenceFactor);
                double sinP, cosP;
                if (Numeric.EQ(cov00, cov11))
                {
                    HorizontalEllipse.EllipseOrientationAngle = (Numeric.PI / 4.0) * ((cov01 >= 0.0) ? 1.0 : -1.0);
                }
                else
                {
                    HorizontalEllipse.EllipseOrientationAngle = 0.5 * System.Math.Atan2(2.0 * cov01, cov00 - cov11);
                }
                sinP = System.Math.Sin((double)HorizontalEllipse.EllipseOrientationAngle);
                cosP = System.Math.Cos((double)HorizontalEllipse.EllipseOrientationAngle);
                double? tmp = 2.0 * sinP * cosP * cov01;
                HorizontalEllipse.EllipseRadii = new()
                {
                    X = scalingFactor * Numeric.SqrtEqual(chiSquare * (cov00 * cosP * cosP + cov11 * sinP * sinP + tmp)) + boreholeRadius,
                    Y = scalingFactor * Numeric.SqrtEqual(chiSquare * (cov00 * sinP * sinP + cov11 * cosP * cosP - tmp)) + boreholeRadius
                };


                if (HorizontalEllipse.EllipseRadii.X < HorizontalEllipse.EllipseRadii.Y)
                {
                    (HorizontalEllipse.EllipseRadii.X, HorizontalEllipse.EllipseRadii.Y) =
                        (HorizontalEllipse.EllipseRadii.Y, HorizontalEllipse.EllipseRadii.X);
                    HorizontalEllipse.EllipseOrientationAngle += 0.5 * Numeric.PI;
                }
                if (HorizontalEllipse.EllipseOrientationAngle < 0.0)
                {
                    HorizontalEllipse.EllipseOrientationAngle += Numeric.PI;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculate the ellipse parameters resulting from the projection of the ellipsoid of uncertainty, in the vertical direction (TVD), 
        /// relative to the Riemannian manifold
        /// </summary>
        /// <param name="confidenceFactor">the confidence factor associated to the ellipsoid</param>
        /// <param name="surveyStation">the survey station associated to the ellipsoid</param>
        /// <param name="scalingFactor">the scaling factor to apply to the ellipsoid</param>
        /// <param name="boreholeRadius">the radius of the borehole to apply to the ellipsoid</param>
        /// <returns>true if calculations went ok, false otherwise</returns>
        public bool CalculateVerticalEllipseParameters()
        {
            if (EllipsoidSurveyStation is { } s &&
                s.BoreholeRadius is double boreholeRadius &&
                s.Azimuth is double azim && 
                s.Covariance is { } cov &&
                    cov[0, 0] is double cov00 &&
                    cov[0, 1] is double cov01 &&
                    cov[1, 1] is double cov11 &&
                    cov[0, 2] is double cov02 &&
                    cov[1, 2] is double cov12 &&
                    cov[2, 2] is double cov22 &&
                    ConfidenceFactor is double confidenceFactor &&
                    ScalingFactor is double scalingFactor)
            {
                VerticalEllipse = new() { EllipseCenter = s };
                double chiSquare = Statistics.GetChiSquare3D(confidenceFactor);
                double cosA = System.Math.Cos(azim);
                double sinA = System.Math.Sin(azim);
                double K11 = cov00 * cosA * cosA + 2 * cov01 * cosA * sinA + cov11 * sinA * sinA;
                double K13 = cov02 * cosA + cov12 * sinA;
                double K33 = cov22;
                if (Numeric.EQ(K11, K33))
                {
                    VerticalEllipse.EllipseOrientationAngle = 0.25 * Numeric.PI * ((K13 < 0.0) ? 1.0 : -1.0);
                }
                else
                {
                    VerticalEllipse.EllipseOrientationAngle = 0.5 * System.Math.Atan((2.0 * K13) / (K11 - K33));
                }
                VerticalEllipse.EllipseOrientationAngle += Numeric.PI / 2.0;
                double sinP = System.Math.Sin((double)VerticalEllipse.EllipseOrientationAngle);
                double cosP = System.Math.Cos((double)VerticalEllipse.EllipseOrientationAngle);
                double? tmp = 2.0 * sinP * cosP * K13;
                VerticalEllipse.EllipseRadii = new()
                {
                    X = scalingFactor * Numeric.SqrtEqual(chiSquare * (K11 * sinP * sinP + K33 * cosP * cosP - tmp)) + boreholeRadius,
                    Y = scalingFactor * Numeric.SqrtEqual(chiSquare * (K11 * cosP * cosP + K33 * sinP * sinP + tmp)) + boreholeRadius
                };
                if (VerticalEllipse.EllipseRadii.X < VerticalEllipse.EllipseRadii.Y)
                {
                    (VerticalEllipse.EllipseRadii.X, VerticalEllipse.EllipseRadii.Y) =
                        (VerticalEllipse.EllipseRadii.Y, VerticalEllipse.EllipseRadii.X);
                    VerticalEllipse.EllipseOrientationAngle += 0.5 * Numeric.PI;
                }
                if (VerticalEllipse.EllipseOrientationAngle > 2.0 * Numeric.PI)
                {
                    VerticalEllipse.EllipseOrientationAngle -= 2.0 * Numeric.PI;
                }
                if (VerticalEllipse.EllipseOrientationAngle < 0.0)
                {
                    VerticalEllipse.EllipseOrientationAngle += Numeric.PI;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculate the ellipse resulting from the projection of the ellipsoid of uncertainty in the direction perpendicular to the survey points
        /// </summary>
        /// <param name="confidenceFactor">the confidence factor associated to the ellipsoid</param>
        /// <param name="surveyStation">the survey station associated to the ellipsoid</param>
        /// <param name="scalingFactor">the scaling factor to apply to the ellipsoid</param>
        /// <param name="boreholeRadius">the radius of the borehole to apply to the ellipsoid</param>
        /// <returns>true if calculation went ok, false otherwise</returns>
        public bool CalculatePerpendicularEllipseParameters()
        {
            if (EllipsoidSurveyStation is { } s &&
                s.BoreholeRadius is double boreholeRadius &&
                s.Inclination is double incl &&
                    s.Azimuth is double azim &&
                    s.Covariance is { } cov && cov[0, 0] is double cov00 &&
                        cov[0, 1] is double cov01 &&
                        cov[1, 1] is double cov11 &&
                        cov[0, 2] is double cov02 &&
                        cov[1, 2] is double cov12 &&
                        cov[2, 2] is double cov22 &&
                    ConfidenceFactor is double confidenceFactor &&
                    ScalingFactor is double scalingFactor)
            {
                PerpendicularEllipse = new() { EllipseCenter = s };
                double chiSquare = Statistics.GetChiSquare3D(confidenceFactor);
                double cosI = System.Math.Cos(incl);
                double sinI = System.Math.Sin(incl);
                double cosA = System.Math.Cos(azim);
                double sinA = System.Math.Sin(azim);
                double K22 = cosA * cosI * (cov00 * cosA * cosI + cov01 * cosI * sinA - cov02 * sinI) + cosI * sinA * (cov01 * cosA * cosI + cov11 * cosI * sinA - cov12 * sinI) - sinI * (cov02 * cosA * cosI + cov12 * cosI * sinA - cov22 * sinI);
                double K23 = cosA * (cov01 * cosA * cosI + cov11 * cosI * sinA - cov12 * sinI) - sinA * (cov00 * cosA * cosI + cov01 * cosI * sinA - cov02 * sinI);
                double K33 = cosA * (cov11 * cosA - cov01 * sinA) - sinA * (cov01 * cosA - cov00 * sinA);
                if (Numeric.EQ(K22, K33))
                {
                    PerpendicularEllipse.EllipseOrientationAngle = 0.25 * Numeric.PI * ((K23 >= 0.0) ? 1.0 : -1.0);
                }
                else
                {
                    PerpendicularEllipse.EllipseOrientationAngle = 0.5 * System.Math.Atan((2.0 * K23) / (K22 - K33));
                }
                // to transform compare to vertical instead of horizontal
                double sinP = System.Math.Sin((double)PerpendicularEllipse.EllipseOrientationAngle);
                double cosP = System.Math.Cos((double)PerpendicularEllipse.EllipseOrientationAngle);
                double? tmp = 2.0 * sinP * cosP * K23;
                PerpendicularEllipse.EllipseRadii = new()
                {
                    X = scalingFactor * Numeric.SqrtEqual(chiSquare * (K22 * sinP * sinP + K33 * cosP * cosP - tmp)) + boreholeRadius,
                    Y = scalingFactor * Numeric.SqrtEqual(chiSquare * (K22 * cosP * cosP + K33 * sinP * sinP + tmp)) + boreholeRadius
                };
                if (PerpendicularEllipse.EllipseRadii.X < PerpendicularEllipse.EllipseRadii.Y)
                {
                    // swap
                    (PerpendicularEllipse.EllipseRadii.X, PerpendicularEllipse.EllipseRadii.Y) =
                        (PerpendicularEllipse.EllipseRadii.Y, PerpendicularEllipse.EllipseRadii.X);
                }
                else
                {
                    PerpendicularEllipse.EllipseOrientationAngle += 0.5 * Numeric.PI;
                }
                if (PerpendicularEllipse.EllipseOrientationAngle > 2.0 * Numeric.PI)
                {
                    PerpendicularEllipse.EllipseOrientationAngle -= 2.0 * Numeric.PI;
                }
                if (PerpendicularEllipse.EllipseOrientationAngle < 0.0)
                {
                    PerpendicularEllipse.EllipseOrientationAngle += Numeric.PI;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculates analytically the highest and the lowest position in the vertical direction (TVD) reached by points on the ellipsoid of uncertainty,
        /// relative to the Riemannian manifold. The method is described in https://doi.org/10.2118/170330-MS
        /// </summary>
        /// <param name="high">the highest position in the vertical direction (TVD)</param>
        /// <param name="low">the lowest position in the vertical direction (TVD)</param>
        /// <returns>true if calculation went ok, false otherwise</returns>
        public bool CalculateExactExtremumsInDepth()
        {
            if (EllipsoidSurveyStation is { } s &&
                s.Covariance is { } cov &&
                    cov[0, 0] is double h11 &&
                    cov[0, 1] is double h12 &&
                    cov[0, 2] is double h13 &&
                    cov[1, 1] is double h22 &&
                    cov[1, 2] is double h23 &&
                    cov[2, 2] is double h33 &&
                    ConfidenceFactor is double confidenceFactor)
            {
                PointAtHighestTVD = new();
                PointAtLowestTVD = new();
                // calculate the parameters of the ellipsoid
                double chiSquare = Statistics.GetChiSquare3D(confidenceFactor);
                // inverse the matrix
                double determinant = (h11 * h22 - h12 * h12) * h33 - h11 * h23 * h23 + 2 * h12 * h13 * h23 - h13 * h13 * h22;
                double H11 = (h22 * h33 - h23 * h23) / determinant;
                double H21 = -(h12 * h33 - h13 * h23) / determinant;
                double H31 = (h12 * h23 - h13 * h22) / determinant;
                double H12 = H21;
                double H22 = (h11 * h33 - h13 * h13) / determinant;
                double H32 = -(h11 * h23 - h12 * h13) / determinant;
                double H13 = H31;
                double H23 = H32;
                double H33 = (h11 * h22 - h12 * h12) / determinant;

                // calculate extremum in Z
                double denominator = H11 * H22 - H12 * H12;
                double dl = Numeric.SqrtEqual(chiSquare * ((H11 * H22 * H33) / denominator - (H12 * H12 * H33) / denominator - (H11 * H23 * H23) / denominator + (2.0 * H12 * H13 * H23) / denominator - (H13 * H13 * H22) / denominator));
                determinant = (H11 * H22 - H12 * H12) * H33 - H11 * H23 * H23 + 2 * H12 * H13 * H23 - H13 * H13 * H22;
                PointAtHighestTVD.X = dl * (H13 * H22 - H12 * H23) / determinant;
                PointAtHighestTVD.Y = dl * (-H12 * H13 + H11 * H23) / determinant;
                PointAtHighestTVD.Z = dl * (H12 * H12 - H11 * H22) / determinant;
                PointAtLowestTVD.X = -PointAtHighestTVD.X;
                PointAtLowestTVD.Y = -PointAtHighestTVD.Y;
                PointAtLowestTVD.Z = -PointAtHighestTVD.Z;

                if (PointAtHighestTVD.Z < PointAtLowestTVD.Z)
                {
                    //swap
                    (PointAtLowestTVD.X, PointAtHighestTVD.X) = (PointAtHighestTVD.X, PointAtLowestTVD.X);
                    (PointAtLowestTVD.Y, PointAtHighestTVD.Y) = (PointAtHighestTVD.Y, PointAtLowestTVD.Y);
                    (PointAtLowestTVD.Z, PointAtHighestTVD.Z) = (PointAtHighestTVD.Z, PointAtLowestTVD.Z);
                }
                //add bias and survey position
                PointAtHighestTVD.X += ((s.Bias == null || s.Bias.X == null) ? 0 : s.Bias.X) + s.X;
                PointAtHighestTVD.Y += ((s.Bias == null || s.Bias.Y == null) ? 0 : s.Bias.Y) + s.Y;
                PointAtHighestTVD.Z += ((s.Bias == null || s.Bias.Z == null) ? 0 : s.Bias.Z) + s.Z;
                PointAtLowestTVD.X += ((s.Bias == null || s.Bias.X == null) ? 0 : s.Bias.X) + s.X;
                PointAtLowestTVD.Y += ((s.Bias == null || s.Bias.Y == null) ? 0 : s.Bias.Y) + s.Y;
                PointAtLowestTVD.Z += ((s.Bias == null || s.Bias.Z == null) ? 0 : s.Bias.Z) + s.Z;
                return true;
            }
            return false;
        }
    }
}
