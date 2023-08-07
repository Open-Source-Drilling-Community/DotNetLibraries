using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// Describe a 3D Point
    /// </summary>
    [Serializable]
    public class Point3DGlobalCoordinates : Point3D, IPoint3DGlobalCoordinates, IEquivalent<IPoint3D>
    {
        public double? LatitudeWGS84 { get; set; }

        public double? LongitudeWGS84 { get; set; }

        public double? TvdWGS84 { get; set; }
        /// <summary>
        /// Default constructor
        /// </summary>
        public Point3DGlobalCoordinates()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="pt"></param>
        public Point3DGlobalCoordinates(IPoint3DGlobalCoordinates pt)
        {
            if (pt != null)
            {
                X = pt.X;
                Y = pt.Y;
                Z = pt.Z;
            }
        }
        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Point3DGlobalCoordinates(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3DGlobalCoordinates(double? x, double? y, double? z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// constructor with initialization from an array
        /// </summary>
        /// <param name="dat"></param>
        public Point3DGlobalCoordinates(double[] dat)
        {
            if (dat != null && dat.Length >= 3)
            {
                X = dat[0];
                Y = dat[1];
                Z = dat[2];
            }
        }
        /// <summary>
        /// cloning
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new Point3DGlobalCoordinates(this);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void SetUndefined()
        {
            base.SetUndefined();
            LatitudeWGS84 = Numeric.UNDEF_DOUBLE;
            LongitudeWGS84 = Numeric.UNDEF_DOUBLE;
            TvdWGS84 = Numeric.UNDEF_DOUBLE;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsUndefined()
        {
            return base.IsUndefined() || Numeric.IsUndefined(LatitudeWGS84) || Numeric.IsUndefined(LongitudeWGS84) || Numeric.IsUndefined(TvdWGS84);
        }
        /// <summary>
        /// set the coordinates to zero
        /// </summary>
        public override void SetZero()
        {
            base.SetZero();
            LatitudeWGS84 = 0;
            LongitudeWGS84 = 0;
            TvdWGS84 = 0;
        }
        /// <summary>
        /// this point is zero if both components are zero
        /// </summary>
        /// <returns></returns>
        public override bool IsZero()
        {
            return base.IsZero() && Numeric.EQ(LatitudeWGS84, 0) && Numeric.EQ(LongitudeWGS84, 0) && Numeric.EQ(TvdWGS84, 0);
        }
    }
}
