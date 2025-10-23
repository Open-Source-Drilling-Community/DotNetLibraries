using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public class SurveyStation : SurveyPoint
    {
        protected static double[,] chiSquare3D_ = new double[,] { { 0.05, 0.10, 0.2, 0.3, 0.5, 0.7, 0.8, 0.9, 0.95, 0.99, 0.999 }, { 0.35, 0.58, 1.01, 1.42, 2.37, 3.66, 4.64, 6.25, 7.82, 11.34, 16.27 } };
        /// <summary>
        /// wellbore position uncertainty Covariance matrix
        /// </summary>
        public SymmetricMatrix3x3? Covariance { get; set; } = null;
        /// <summary>
        /// A bias as used in the Wolff and de Wardt Wellbore position uncertainty model
        /// </summary>
        public Vector3D? Bias { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="confidenceFactor"></param>
        /// <param name="radii"></param>
        /// <param name="transformation"></param>
        /// <param name="scalingFactor"></param>
        /// <param name="boreholeRadius"></param>
        /// <returns></returns>
        public bool CalculateEllipsoid(double confidenceFactor, Vector3D radii, Matrix3x3 transformation, double scalingFactor = 1.0, double boreholeRadius = 0.0)
        {
            Vector3D eigenValues = new Vector3D();
            if (DiagonalizeCovariance(transformation, eigenValues) &&
                eigenValues.X != null &&
                eigenValues.Y != null &&
                eigenValues.Z != null)
            {
                double chiSquare = GetChiSquare3D(confidenceFactor);
                double kScale = System.Math.Sqrt(chiSquare);
                radii.X = (kScale * System.Math.Sqrt(eigenValues.X.Value) + boreholeRadius) * scalingFactor;
                radii.Y = (kScale * System.Math.Sqrt(eigenValues.Y.Value) + boreholeRadius) * scalingFactor;
                radii.Z = (kScale * System.Math.Sqrt(eigenValues.Z.Value) + boreholeRadius) * scalingFactor;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="confidenceFactor"></param>
        /// <param name="radii"></param>
        /// <param name="angle"></param>
        /// <param name="scalingFactor"></param>
        /// <param name="boreholeRadius"></param>
        /// <returns></returns>
        public bool CalculateHorizontalEllipse(double confidenceFactor, Vector2D radii, out double angle, double scalingFactor = 1.0, double boreholeRadius = 0.0)
        {
            if (Covariance != null &&
                Covariance[0, 0] != null &&
                Covariance[0, 1] != null &&
                Covariance[1, 1] != null)
            {
                double chiSquare = GetChiSquare3D(confidenceFactor);
                double sinP, cosP;
                double cov00 = Covariance[0, 0].Value;
                double cov01 = Covariance[0, 1].Value;
                double cov11 = Covariance[1, 1].Value;
                if (Numeric.EQ(cov00, cov11))
                {
                    angle = (Numeric.PI / 4.0) * ((cov01 >= 0.0) ? 1.0 : -1.0);
                }
                else
                {
                    angle = 0.5 * System.Math.Atan2(2.0 * cov01, cov00 - cov11);
                }
                sinP = System.Math.Sin(angle);
                cosP = System.Math.Cos(angle);
                double? tmp = 2.0 * sinP * cosP * cov01;
                radii.X = scalingFactor * Numeric.SqrtEqual(chiSquare * (cov00 * cosP * cosP + cov11 * sinP * sinP + tmp)) + boreholeRadius;
                radii.Y = scalingFactor * Numeric.SqrtEqual(chiSquare * (cov00 * sinP * sinP + cov11 * cosP * cosP - tmp)) + boreholeRadius;
                if (radii.X < radii.Y)
                {
                    tmp = radii.X;
                    radii.X = radii.Y;
                    radii.Y = tmp;
                    angle += 0.5 * Numeric.PI;
                }
                if (angle < 0.0)
                {
                    angle += Numeric.PI;
                }
                return true;
            }
            angle = 0;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="confidenceFactor"></param>
        /// <param name="radii"></param>
        /// <param name="angle"></param>
        /// <param name="scalingFactor"></param>
        /// <param name="boreholeRadius"></param>
        /// <returns></returns>
        public bool CalculateVerticalEllipse(double confidenceFactor, Vector2D radii, out double angle, double scalingFactor = 1.0, double boreholeRadius = 0.0)
        {
            if (Azimuth != null &&
                Covariance != null &&
                Covariance[0, 0] != null &&
                Covariance[0, 1] != null &&
                Covariance[1,1] != null &&
                Covariance[0,2] != null &&
                Covariance[1, 2] != null &&
                Covariance[2,2] != null)
            {
                double chiSquare = GetChiSquare3D(confidenceFactor);
                double cosA = System.Math.Cos(Azimuth.Value);
                double sinA = System.Math.Sin(Azimuth.Value);
                double cov00 = Covariance[0, 0].Value;
                double cov01 = Covariance[0, 1].Value;
                double cov11 = Covariance[1, 1].Value;
                double cov02 = Covariance[0, 2].Value;
                double cov12 = Covariance[1, 2].Value;
                double cov22 = Covariance[2, 2].Value;
                double K11 = cov00 * cosA * cosA + 2 * cov01 * cosA * sinA + cov11 * sinA * sinA;
                double K13 = cov02 * cosA + cov12 * sinA;
                double K33 = cov22;
                if (Numeric.EQ(K11, K33))
                {
                    angle = 0.25 * Numeric.PI * ((K13 < 0.0) ? 1.0 : -1.0);
                }
                else
                {
                    angle = 0.5 * System.Math.Atan((2.0 * K13) / (K11 - K33));
                }
                angle += Numeric.PI / 2.0;
                double sinP = System.Math.Sin(angle);
                double cosP = System.Math.Cos(angle);
                double? tmp = 2.0 * sinP * cosP * K13;
                radii.Y = scalingFactor * Numeric.SqrtEqual(chiSquare * (K11 * cosP * cosP + K33 * sinP * sinP + tmp)) + boreholeRadius;
                radii.X = scalingFactor * Numeric.SqrtEqual(chiSquare * (K11 * sinP * sinP + K33 * cosP * cosP - tmp)) + boreholeRadius;
                if (radii.X < radii.Y)
                {
                    tmp = radii.X;
                    radii.X = radii.Y;
                    radii.Y = tmp;
                    angle += 0.5 * Numeric.PI;
                }
                if (angle > 2.0 * Numeric.PI)
                {
                    angle -= 2.0 * Numeric.PI;
                }
                if (angle < 0.0)
                {
                    angle += Numeric.PI;
                }
                return true;
            }
            angle = 0;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="confidenceFactor"></param>
        /// <param name="radii"></param>
        /// <param name="angle"></param>
        /// <param name="scalingFactor"></param>
        /// <param name="boreholeRadius"></param>
        /// <returns></returns>
        public bool CalculatePerpendicularEllipse(double confidenceFactor, Vector2D radii, out double angle, double scalingFactor = 1.0, double boreholeRadius = 0.0)
        {
            if (Inclination != null &&
                Azimuth != null &&
                Covariance != null &&
                Covariance[0, 0] != null &&
                Covariance[0, 1] != null &&
                Covariance[1, 1] != null &&
                Covariance[0, 2] != null &&
                Covariance[1, 2] != null &&
                Covariance[2, 2] != null)
            {
                double chiSquare = GetChiSquare3D(confidenceFactor);
                double cosI = System.Math.Cos(Inclination.Value);
                double sinI = System.Math.Sin(Inclination.Value);
                double cosA = System.Math.Cos(Azimuth.Value);
                double sinA = System.Math.Sin(Azimuth.Value);
                double cov00 = Covariance[0, 0].Value;
                double cov01 = Covariance[0, 1].Value;
                double cov11 = Covariance[1, 1].Value;
                double cov02 = Covariance[0, 2].Value;
                double cov12 = Covariance[1, 2].Value;
                double cov22 = Covariance[2, 2].Value;
                double K22 = cosA * cosI * (cov00 * cosA * cosI + cov01 * cosI * sinA - cov02 * sinI) + cosI * sinA * (cov01 * cosA * cosI + cov11 * cosI * sinA - cov12 * sinI) - sinI * (cov02 * cosA * cosI + cov12 * cosI * sinA - cov22 * sinI);
                double K23 = cosA * (cov01 * cosA * cosI + cov11 * cosI * sinA - cov12 * sinI) - sinA * (cov00 * cosA * cosI + cov01 * cosI * sinA - cov02 * sinI);
                double K33 = cosA * (cov11 * cosA - cov01 * sinA) - sinA * (cov01 * cosA - cov00 * sinA);
                if (Numeric.EQ(K22, K33))
                {
                    angle = 0.25 * Numeric.PI * ((K23 >= 0.0) ? 1.0 : -1.0);
                }
                else
                {
                    angle = 0.5 * System.Math.Atan((2.0 * K23) / (K22 - K33));
                }
                // to transform compare to vertical instead of horizontal
                double sinP = System.Math.Sin(angle);
                double cosP = System.Math.Cos(angle);
                double? tmp = 2.0 * sinP * cosP * K23;
                radii.Y = scalingFactor * Numeric.SqrtEqual(chiSquare * (K22 * cosP * cosP + K33 * sinP * sinP + tmp)) + boreholeRadius;
                radii.X = scalingFactor * Numeric.SqrtEqual(chiSquare * (K22 * sinP * sinP + K33 * cosP * cosP - tmp)) + boreholeRadius;
                if (radii.X < radii.Y)
                {
                    tmp = radii.X;
                    radii.X = radii.Y;
                    radii.Y = tmp;
                }
                else
                {
                    angle += 0.5 * Numeric.PI;
                }
                if (angle > 2.0 * Numeric.PI)
                {
                    angle -= 2.0 * Numeric.PI;
                }
                if (angle < 0.0)
                {
                    angle += Numeric.PI;
                }
                return true;
            }
            angle = 0;
            return false;
        }
        /// <summary>
        /// Calculation the highest and the lowest position in the Z-direction, using the method
        /// described in https://doi.org/10.2118/170330-MS
        /// </summary>
        /// <param name="confidenceFactor"></param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        public bool CalculateExtremumsInDepth(double confidenceFactor, Point3D high, Point3D low)
        {
            if (Covariance != null &&
                Covariance[0, 0] != null &&
                Covariance[0, 1] != null &&
                Covariance[0, 2] != null &&
                Covariance[1, 1] != null &&
                Covariance[1, 2] != null &&
                Covariance[2, 2] != null)
            {
                // calculate the parameters of the ellipsoid
                double chiSquare = GetChiSquare3D(confidenceFactor);
                // inverse the matrix
                double h11 = Covariance[0, 0].Value;
                double h12 = Covariance[0, 1].Value;
                double h13 = Covariance[0, 2].Value;
                double h22 = Covariance[1, 1].Value;
                double h23 = Covariance[1, 2].Value;
                double h33 = Covariance[2, 2].Value;
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
                high.X = dl * (H13 * H22 - H12 * H23) / determinant;
                high.Y = dl * (-H12 * H13 + H11 * H23) / determinant;
                high.Z = dl * (H12 * H12 - H11 * H22) / determinant;
                low.X = -high.X;
                low.Y = -high.Y;
                low.Z = -high.Z;

                if (high.Z < low.Z)
                {
                    //swap
                    double? tt = high.X;
                    high.X = low.X;
                    low.X = tt;
                    tt = high.Y;
                    high.Y = low.Y;
                    low.Y = tt;
                    tt = high.Z;
                    high.Z = low.Z;
                    low.Z = tt;
                }
                //add bias and survey position
                high.X += ((Bias == null || Bias.X == null) ? 0 : Bias.X) + X;
                high.Y += ((Bias == null || Bias.Y == null) ? 0 : Bias.Y) + Y;
                high.Z += ((Bias == null || Bias.Z == null) ? 0 : Bias.Z) + Z;
                low.X += ((Bias == null || Bias.X == null) ? 0 : Bias.X) + X;
                low.Y += ((Bias == null || Bias.Y == null) ? 0 : Bias.Y) + Y;
                low.Z += ((Bias == null || Bias.Z == null) ? 0 : Bias.Z) + Z;
                return true;
            }
            return false;
        }

        public bool CalculateSphericalCoordinatesAtPosition(SphericalPoint3D point)
        {
            Matrix3x3 eigenVectors = new Matrix3x3();
            Vector3D eigenValues = new Vector3D();
            if (RiemannianNorth != null &&
                RiemannianEast != null &&
                TVD != null &&
                point.X != null &&
                point.Y != null &&
                point.Z != null &&
                DiagonalizeCovariance(eigenVectors, eigenValues) &&
                eigenVectors[0, 0] != null &&
                eigenVectors[0, 1] != null &&
                eigenVectors[0, 2] != null &&
                eigenVectors[1, 0] != null &&
                eigenVectors[1, 1] != null &&
                eigenVectors[1, 2] != null &&
                eigenVectors[2, 0] != null &&
                eigenVectors[2, 1] != null &&
                eigenVectors[2, 2] != null &&
                eigenValues[0] != null &&
                eigenValues[1] != null &&
                eigenValues[2] != null)
            {
                double p11 = eigenVectors[0, 0].Value;
                double p12 = eigenVectors[0, 1].Value;
                double p13 = eigenVectors[0, 2].Value;
                double p21 = eigenVectors[1, 0].Value;
                double p22 = eigenVectors[1, 1].Value;
                double p23 = eigenVectors[1, 2].Value;
                double p31 = eigenVectors[2, 0].Value;
                double p32 = eigenVectors[2, 1].Value;
                double p33 = eigenVectors[2, 2].Value;

                double a = eigenValues[0].Value;
                double b = eigenValues[1].Value;
                double c = eigenValues[2].Value;

                double x = point.X.Value - RiemannianNorth.Value - ((Bias == null || Bias.X == null) ? 0 : Bias.X.Value);
                double y = point.Y.Value - RiemannianEast.Value - ((Bias == null || Bias.X == null) ? 0 : Bias.Y.Value);
                double z = point.Z.Value - TVD.Value - ((Bias == null || Bias.X == null) ? 0 : Bias.Z.Value);

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
            Matrix3x3 eigenVectors = new Matrix3x3();
            Vector3D eigenValues = new Vector3D();
            if (RiemannianNorth != null &&
                RiemannianEast != null &&
                TVD != null &&
                point.R != null &&
                point.Latitude != null &&
                point.Longitude != null &&
                DiagonalizeCovariance(eigenVectors, eigenValues) &&
                eigenVectors[0, 0] != null &&
                eigenVectors[0, 1] != null &&
                eigenVectors[0, 2] != null &&
                eigenVectors[1, 0] != null &&
                eigenVectors[1, 1] != null &&
                eigenVectors[1, 2] != null &&
                eigenVectors[2, 0] != null &&
                eigenVectors[2, 1] != null &&
                eigenVectors[2, 2] != null &&
                eigenValues[0] != null &&
                eigenValues[1] != null &&
                eigenValues[2] != null)
            {
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


                double ci = System.Math.Cos(point.Latitude.Value);
                double si = System.Math.Sin(point.Latitude.Value);
                double ca = System.Math.Cos(point.Longitude.Value);
                double sa = System.Math.Sin(point.Longitude.Value);
                double x = point.R.Value * ci * ca;
                double y = point.R.Value * ci* sa;
                double z = point.R.Value * si;

                double X = pi11 * x + pi21 * y + pi31 * z;
                double Y = pi12 * x + pi22 * y + pi32 * z;
                double Z = pi13 * x + pi23 * y + pi33 * z;

                point.X = RiemannianNorth + ((Bias == null || Bias.X == null) ? 0 : Bias.X.Value) + X;
                point.Y = RiemannianEast + ((Bias == null || Bias.Y == null) ? 0 : Bias.Y.Value) + Y;
                point.Z = TVD + ((Bias == null || Bias.Z == null) ? 0 : Bias.Z.Value) + Z;

                return true;
            }
            return false;
        }

        /// <summary>
        /// find the eigenvalues and eigenvectors of the covariance matrix
        /// </summary>
        /// <param name="eigenVectors"></param>
        /// <param name="eigenValues"></param>
        public bool DiagonalizeCovariance(Matrix3x3 eigenVectors, Vector3D eigenValues)
        {
            if (Covariance != null &&
                Covariance[0, 0] != null &&
                Covariance[0, 1] != null &&
                Covariance[0, 2] != null &&
                Covariance[1, 1] != null &&
                Covariance[1, 2] != null &&
                Covariance[2, 2] != null &&
                eigenVectors != null &&
                eigenValues != null &&
                eigenVectors.ColumnCount == 3 &&
                eigenVectors.RowCount == 3)
            {
                double[,] z = new double[3, 3];
                z[0, 0] = Covariance[0, 0].Value;
                z[0, 1] = Covariance[0, 1].Value;
                z[0, 2] = Covariance[0, 2].Value;
                z[1, 0] = Covariance[1, 0].Value;
                z[1, 1] = Covariance[1, 1].Value;
                z[1, 2] = Covariance[1, 2].Value;
                z[2, 0] = Covariance[2, 0].Value;
                z[2, 1] = Covariance[2, 1].Value;
                z[2, 2] = Covariance[2, 2].Value;
                double[] d = new double[4];
                double[] e = new double[3];
                // tranform a symmetric matrix into a tridiagonal matrix
                tred2(3, z, d, e);
                // QL decomposition
                tql2(3, z, d, e);
                //transfer eigenvectors
                eigenVectors[0, 0] = z[0, 0];
                eigenVectors[0, 1] = z[0, 1];
                eigenVectors[0, 2] = z[0, 2];
                eigenVectors[1, 0] = z[1, 0];
                eigenVectors[1, 1] = z[1, 1];
                eigenVectors[1, 2] = z[1, 2];
                eigenVectors[2, 0] = z[2, 0];
                eigenVectors[2, 1] = z[2, 1];
                eigenVectors[2, 2] = z[2, 2];
                // transfer eigen values
                eigenValues[0] = d[0];
                eigenValues[1] = d[1];
                eigenValues[2] = d[2];
                return true;
            }
            return false;
        }
        internal static double GetConfidenceFactor(double chiSquare3D)
        {
            if (Numeric.IsUndefined(chiSquare3D))
            {
                return Numeric.UNDEF_DOUBLE;
            }
            else
            {
                int last = chiSquare3D_.GetLength(1) - 1;
                if (chiSquare3D < chiSquare3D_[1, 0])
                {
                    double factor = (chiSquare3D - chiSquare3D_[1, 0]) / (chiSquare3D_[1, 1] - chiSquare3D_[1, 0]);
                    return chiSquare3D_[0, 0] + factor * (chiSquare3D_[0, 1] - chiSquare3D_[0, 0]);
                }
                else if (chiSquare3D >= chiSquare3D_[1, last])
                {
                    double factor = (chiSquare3D - chiSquare3D_[1, last - 1]) / (chiSquare3D_[1, last] - chiSquare3D_[1, last - 1]);
                    return chiSquare3D_[0, last - 1] + factor * (chiSquare3D_[0, last] - chiSquare3D_[0, last - 1]);
                }
                else
                {
                    for (int i = 0; i < last; i++)
                    {
                        if (chiSquare3D >= chiSquare3D_[1, i] && chiSquare3D < chiSquare3D_[1, i + 1])
                        {
                            double factor = (chiSquare3D - chiSquare3D_[1, i]) / (chiSquare3D_[1, i + 1] - chiSquare3D_[1, i]);
                            return chiSquare3D_[0, i] + factor * (chiSquare3D_[0, i + 1] - chiSquare3D_[0, i]);
                        }
                    }
                    return Numeric.UNDEF_DOUBLE;
                }
            }
        }
        private static double GetChiSquare3D(double p)
        {
            if (Numeric.IsUndefined(p))
            {
                return Numeric.UNDEF_DOUBLE;
            }
            else
            {
                int last = chiSquare3D_.GetLength(1) - 1;
                if (p < chiSquare3D_[0, 0])
                {
                    double factor = (p - chiSquare3D_[0, 0]) / (chiSquare3D_[0, 1] - chiSquare3D_[0, 0]);
                    return chiSquare3D_[1, 0] + factor * (chiSquare3D_[1, 1] - chiSquare3D_[1, 0]);
                }
                else if (p >= chiSquare3D_[0, last])
                {
                    double factor = (p - chiSquare3D_[0, last - 1]) / (chiSquare3D_[0, last] - chiSquare3D_[0, last - 1]);
                    return chiSquare3D_[1, last - 1] + factor * (chiSquare3D_[1, last] - chiSquare3D_[1, last - 1]);
                }
                else
                {
                    for (int i = 0; i < last; i++)
                    {
                        if (p >= chiSquare3D_[0, i] && p < chiSquare3D_[0, i + 1])
                        {
                            double factor = (p - chiSquare3D_[0, i]) / (chiSquare3D_[0, i + 1] - chiSquare3D_[0, i]);
                            return chiSquare3D_[1, i] + factor * (chiSquare3D_[1, i + 1] - chiSquare3D_[1, i]);
                        }
                    }
                    return Numeric.UNDEF_DOUBLE;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="V"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private void tred2(int n, double[,] V, double[] d, double[] e)
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


        // Symmetric tridiagonal QL algorithm.

        private void tql2(int n, double[,] V, double[] d, double[] e)
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

        // <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double Pythag(double a, double b)
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
