using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public class UncertaintyEllipse
    {
        /// <summary>
        /// The center of the ellipse of uncertainty
        /// </summary>
        public SurveyPoint? EllipseCenter { get; set; }
        /// <summary>
        /// The semi-major and semi-minor axis of the ellipse of uncertainty
        /// </summary>
        public Vector2D? EllipseRadii { get; set; }
        /// <summary>
        /// The coordinates of the ellipse of uncertainty
        /// </summary>
        public List<SurveyPoint>? EllipseVertices { get; set; }
        /// <summary>
        /// Angle used to orient ellipse vertices in the plane of the ellipse
        /// </summary>
        public double? EllipseOrientationAngle { get; set; }

        /// <summary>
        /// The bounding box of the ellipse of uncertainty, relative to the Riemannian manifold
        /// </summary>
        public BoundingBox3D? BoundingBox { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public UncertaintyEllipse() : base()
        {
        }

        /// <summary>
        /// Discretize the ellipse of uncertainty according to its parameters
        /// The coordinates of the discretized ellipse are computed as a list of survey points
        /// </summary>
        /// <param name="zOffset">an offset along the axis of the uncertainty ellipse to position the discretized ellipse from</param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        public bool DiscretizeEllipse(int meshSectorCount, double zOffset = 0)
        {
            if (EllipseCenter != null &&
                EllipseCenter.Inclination is double incl &&
                EllipseCenter.Azimuth is double azim &&
                EllipseCenter.X is double X &&
                EllipseCenter.Y is double Y &&
                EllipseCenter.Z is double Z &&
                EllipseRadii != null &&
                EllipseRadii[0] is double semiMajorAxis &&
                EllipseRadii[1] is double semiMinorAxis)
            {
                double sinI = System.Math.Sin(incl);
                double cosI = System.Math.Cos(incl);
                double sinA = System.Math.Sin(azim);
                double cosA = System.Math.Cos(azim);
                bool useInclAz = true;
                bool usePhi = false;
                // Default numbering of the ellipse vertices
                if (useInclAz)
                {
                    if (EllipseRadii[0] != null && EllipseRadii[1] != null)
                    {
                        //crossSectionMeshDensity = (int)Numeric.Max(Numeric.Max(EllipseRadii[0], EllipseRadii[1]), crossSectionMeshDensity);
                        Matrix3x3 Rz = new();
                        Rz.RotZAssign((double)EllipseCenter.Azimuth);
                        Matrix3x3 Ry = new();
                        Ry.RotYAssign((double)EllipseCenter.Inclination);
                        IMatrix R = Rz.Multiply(Ry);

                        if (R != null &&
                            R[0, 0] is double r00 &&
                            R[0, 1] is double r01 &&
                            R[0, 2] is double r02 &&
                            R[1, 0] is double r10 &&
                            R[1, 1] is double r11 &&
                            R[1, 2] is double r12 &&
                            R[2, 0] is double r20 &&
                            R[2, 1] is double r21 &&
                            R[2, 2] is double r22
                            )
                            for (int j = 0; j <= meshSectorCount; j++)
                            {
                                double phi = (double)j * 2.0 * Math.PI / (double)meshSectorCount;
                                double xCyl = semiMajorAxis * System.Math.Cos(phi);
                                double yCyl = semiMinorAxis * System.Math.Sin(phi);
                                double zCyl = zOffset;
                                //double xNEH = cosI * cosA * xCyl - sinA * yCyl + sinI * cosA * zCyl;
                                //double yNEH = cosI * sinA * xCyl + cosA * yCyl + sinI * sinA * zCyl;
                                //double zNEH = -sinI * xCyl + cosI * zCyl;
                                double xNEH = r00 * xCyl + r01 * yCyl + r02 * zCyl;
                                double yNEH = r10 * xCyl + r11 * yCyl + r12 * zCyl;
                                double zNEH = r20 * xCyl + r21 * yCyl + r22 * zCyl;
                                xNEH += X;
                                yNEH += Y;
                                zNEH += Z;
                                SurveyPoint point = new SurveyPoint() { X = xNEH, Y = yNEH, Z = zNEH };
                                EllipseVertices ??= new();
                                EllipseVertices.Add(point);
                                // Initialize ellipse bounding box to ellipse center
                                BoundingBox = new(X, Y, Z, X, Y, Z);
                                if (xNEH < BoundingBox.MinX) BoundingBox.MinX = xNEH;
                                if (xNEH > BoundingBox.MaxX) BoundingBox.MaxX = xNEH;
                                if (yNEH < BoundingBox.MinY) BoundingBox.MinY = yNEH;
                                if (yNEH > BoundingBox.MaxY) BoundingBox.MaxY = yNEH;
                                if (zNEH < BoundingBox.MinZ) BoundingBox.MinZ = zNEH;
                                if (zNEH > BoundingBox.MaxZ) BoundingBox.MaxZ = zNEH;
                            }
                    }
                }
                // Guided numbering of the ellipse vertices
                else if (usePhi &&
                    EllipseOrientationAngle is double orientationAngle)
                {
                    //double sinP = System.Math.Sin((double)MeshAngularOffset);
                    //double cosP = System.Math.Cos((double)MeshAngularOffset);

                    Matrix3x3 Rz = new();
                    Rz.RotZAssign((double)EllipseCenter.Azimuth);
                    Matrix3x3 Ry = new();
                    Ry.RotYAssign((double)EllipseCenter.Inclination);
                    Matrix3x3 R0 = (Matrix3x3)Rz.Multiply(Ry);
                    Matrix3x3 Rz2 = new();
                    Rz2.RotZAssign((double)orientationAngle);
                    IMatrix R = R0.Multiply(Rz2);
                    if (R != null &&
                            R[0, 0] is double r00 &&
                            R[0, 1] is double r01 &&
                            R[0, 2] is double r02 &&
                            R[1, 0] is double r10 &&
                            R[1, 1] is double r11 &&
                            R[1, 2] is double r12 &&
                            R[2, 0] is double r20 &&
                            R[2, 1] is double r21 &&
                            R[2, 2] is double r22
                            )
                    {
                        for (int j = 0; j <= meshSectorCount; j++)
                        {
                            double phi = (double)j * 2.0 * Math.PI / (double)meshSectorCount;
                            double xCyl = semiMajorAxis * System.Math.Cos(phi);
                            double yCyl = semiMinorAxis * System.Math.Sin(phi);
                            double zCyl = zOffset;
                            //double xNEH0 = (cosP * cosI * cosA - sinP * sinA * cosI) * xCyl - (cosP * sinA + sinP * cosA) * yCyl + (cosP * sinI * cosA - sinP * sinA * sinA) * zCyl;
                            //double yNEH0 = (sinP * cosI * sinA - cosP * sinA * cosI) * xCyl + (cosP * cosA - sinP * sinA) * yCyl + (cosP * sinI * sinA + sinP * cosA * sinI) * zCyl;
                            //double zNEH0 = -sinI * xCyl + cosI * zCyl;
                            double xNEH = r00 * xCyl + r01 * yCyl + r02 * zCyl;
                            double yNEH = r10 * xCyl + r11 * yCyl + r12 * zCyl;
                            double zNEH = r20 * xCyl + r21 * yCyl + r22 * zCyl;
                            //if (MeshAngularOffset > 0.5)
                            //{
                            //    bool ok = false;
                            //}
                            xNEH += X;
                            yNEH += Y;
                            zNEH += Z;
                            SurveyPoint point = new() { X = xNEH, Y = yNEH, Z = zNEH };
                            //if (phi == 0)
                            //{
                            //    pointx.Add(point);

                            //}
                            //if (phi == Math.PI / 2 + Math.PI)
                            //{
                            //    pointy.Add(point);

                            //}
                            EllipseVertices ??= new();
                            EllipseVertices.Add(point);
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
