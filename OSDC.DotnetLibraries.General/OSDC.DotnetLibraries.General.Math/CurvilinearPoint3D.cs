using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class CurvilinearPoint3D : Point3DGlobalCoordinates, ICurvilinear3D, IEquatable<CurvilinearPoint3D>
    {
        protected override bool SynchronizeGeodeticCoordinates => false;

        /// <summary>
        /// the curvilinear abscissa
        /// </summary>
        public virtual double? Abscissa { get; set; } = null;
        /// <summary>
        /// the inclination (0 is vertical)
        /// </summary>
        public virtual double? Inclination { get; set; } = null;
        /// <summary>
        /// the azimuth (0 is in the x-direction)
        /// </summary>
        public virtual double? Azimuth { get; set; } = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CurvilinearPoint3D() : base()
        {
        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public CurvilinearPoint3D(CurvilinearPoint3D src) : base(src)
        {
            if (src != null)
            {
                Abscissa = src.Abscissa;
                Inclination = src.Inclination;
                Azimuth = src.Azimuth;
            }
        }
        /// <summary>
        /// Constructor with initialization
        /// </summary>
        /// <param name="s"></param>
        /// <param name="inclination"></param>
        /// <param name="azimuth"></param>
        public CurvilinearPoint3D(double s, double inclination, double azimuth)
        {
            Abscissa = s;
            Inclination = inclination;
            Azimuth = azimuth;
        }
        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="s"></param>
        /// <param name="tangent"></param>
        public CurvilinearPoint3D(double s, Vector3D tangent)
        {
            Abscissa = s;
            if (tangent != null)
            {
                Inclination = tangent.GetIncl();
                Azimuth = tangent.GetAz();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="s"></param>
        /// <param name="tangent"></param>
        public CurvilinearPoint3D(IPoint3D pt, double s, Vector3D tangent) : base(pt)
        {
            Abscissa = s;
            if (tangent != null)
            {
                Inclination = tangent.GetIncl();
                Azimuth = tangent.GetAz();
            }
        }
        public CurvilinearPoint3D(double? x, double? y, double? z, double? s, double? inclination, double? azimuth)
        {
            X = x;
            Y = y;
            Z = z;
            Abscissa = s;
            Inclination = inclination;
            Azimuth = azimuth;
        }

        /// <summary>
        /// equal at numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(CurvilinearPoint3D? cmp)
        {
            if (cmp == null) return false;
            return base.Equals(cmp) && Numeric.EQ(Abscissa, cmp.Abscissa) && Numeric.EQ(Inclination, cmp.Inclination) && Numeric.EQ(Azimuth, cmp.Azimuth);
        }
        /// <summary>
        /// equal at given accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(CurvilinearPoint3D? cmp, double precision)
        {
            if (cmp == null) return false;
            return base.EQ(cmp, precision) && Numeric.EQ(Abscissa, cmp.Abscissa, precision) && Numeric.EQ(Inclination, cmp.Inclination, precision) && Numeric.EQ(Azimuth, cmp.Azimuth, precision);
        }
        /// <summary>
        /// equal at numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool EQ(ICurvilinear3D cmp)
        {
            if (cmp == null) return false;
            return base.Equals(cmp) && Numeric.EQ(Abscissa, cmp.Abscissa) && Numeric.EQ(Inclination, cmp.Inclination) && Numeric.EQ(Azimuth, cmp.Azimuth);
        }
        /// <summary>
        /// equal at given accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool EQ(ICurvilinear3D cmp, double precision)
        {
            if (cmp == null) return false;
            return base.EQ(cmp, precision) && Numeric.EQ(Abscissa, cmp.Abscissa, precision) && Numeric.EQ(Inclination, cmp.Inclination, precision) && Numeric.EQ(Azimuth, cmp.Azimuth, precision);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Correct()
        {
            if (Inclination != null && Azimuth != null)
            {
                if (Inclination < 0.0)
                {
                    Inclination = System.Math.Abs((double)Inclination);
                    Azimuth = Numeric.AngleNormalized(Azimuth + Numeric.PI);
                }
                else if (Inclination > Numeric.PI)
                {
                    Inclination -= Numeric.PI;
                    Azimuth = Numeric.AngleNormalized(Azimuth + Numeric.PI);
                }
            }
        }

        /// <summary>
        /// cloning
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new CurvilinearPoint3D(this);
        }
        public void Set(CurvilinearPoint3D point)
        {
            if (point != null)
            {
                base.Set(point);
                Abscissa = point.Abscissa;
                Inclination = point.Inclination;
                Azimuth = point.Azimuth;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3D GetTangent()
        {
            return Vector3D.CreateSpheric(1.0, (double)Inclination, (double)Azimuth);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void SetUndefined()
        {
            base.SetUndefined();
            Abscissa = Numeric.UNDEF_DOUBLE;
            Inclination = Numeric.UNDEF_DOUBLE;
            Azimuth = Numeric.UNDEF_DOUBLE;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsUndefined()
        {
            return base.IsUndefined() || Numeric.IsUndefined(Abscissa) || Numeric.IsUndefined(Inclination) || Numeric.IsUndefined(Azimuth);
        }
        /// <summary>
        /// set the coordinates to zero
        /// </summary>
        public override void SetZero()
        {
            base.SetZero();
            Abscissa = 0;
            Inclination = 0;
            Azimuth = 0;
        }
        /// <summary>
        /// this point is zero if both components are zero
        /// </summary>
        /// <returns></returns>
        public override bool IsZero()
        {
            return base.IsZero() && Numeric.EQ(Abscissa, 0) && Numeric.EQ(Inclination, 0.0) && Numeric.EQ(Azimuth, 0.0);
        }

        /// <summary>
        /// Wraps an angle difference into (-PI, PI]. System.Math.IEEERemainder is used rather than the
        /// operator %, which returns a result carrying the sign of the dividend: the naive form
        /// (a + PI) % (2*PI) - PI is wrong for a negative angle and also loses the last bit or two on
        /// small differences. IEEERemainder returns [-PI, PI], so the only fix up needed is mapping an
        /// exact -PI onto +PI to make the interval half open.
        /// </summary>
        public static double WrapToPi(double angle)
        {
            double wrapped = System.Math.IEEERemainder(angle, 2.0 * Numeric.PI);
            if (wrapped <= -Numeric.PI)
            {
                wrapped += 2.0 * Numeric.PI;
            }
            return wrapped;
        }

        /// <summary>
        /// Apply the minimum curvature method between this survey and the next
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        /// <summary>
        /// The dogleg angle between two attitudes, that is the angle between the two unit tangents.
        ///
        /// Evaluated in the half angle form
        ///     2*asin( sqrt( sin^2(di/2) + sin(i1)*sin(i2)*sin^2(da/2) ) )
        /// rather than as the arc cosine of the dot product. The dot product of two nearly parallel
        /// tangents is within rounding of one, where the arc cosine has an infinite derivative and loses
        /// about half of the available digits: at a dogleg of 1e-9 rad it returns nothing but noise,
        /// whereas this form stays accurate to the last bit over the whole range. It is also bounded by
        /// construction, so no clamping is needed.
        ///
        /// The result is invariant to whole turns in either azimuth, because sin^2(da/2) is.
        /// </summary>
        public static double DoglegAngle(double inclinationStart, double azimuthStart, double inclinationEnd, double azimuthEnd)
        {
            double halfInclination = System.Math.Sin(0.5 * (inclinationEnd - inclinationStart));
            double halfAzimuth = System.Math.Sin(0.5 * (azimuthEnd - azimuthStart));
            double squared = halfInclination * halfInclination
                + System.Math.Sin(inclinationStart) * System.Math.Sin(inclinationEnd) * halfAzimuth * halfAzimuth;
            if (squared <= 0.0)
            {
                return 0.0;
            }
            double root = System.Math.Sqrt(squared);
            return 2.0 * System.Math.Asin(root > 1.0 ? 1.0 : root);
        }

        /// <summary>
        /// The toolface angle at the START of the circular arc joining two attitudes.
        ///
        /// Along a circular arc the toolface angle is not constant, unlike along a constant curvature and
        /// toolface curve, so a single value has to be chosen. The one at the start of the arc is the
        /// meaningful one: it is the angle the arc is set off at, and it is exactly the argument
        /// <see cref="CompleteCASDT"/> takes, so reporting it makes the two constructions inverses of one
        /// another.
        ///
        /// With h the high side and r the right hand side at the start station, the arc turns towards
        /// t1 - (t1.t0)t0, whose components along h and r give
        ///     toolface = atan2( sin(i2)*sin(a2-a1), sin(i2)*cos(i1)*cos(a2-a1) - sin(i1)*cos(i2) ).
        /// Being built from a difference of azimuths inside a sine and a cosine, it is unaffected by
        /// whole turns in either azimuth.
        /// </summary>
        public static double StartToolface(double inclinationStart, double azimuthStart, double inclinationEnd, double azimuthEnd)
        {
            double sinStart = System.Math.Sin(inclinationStart);
            double cosStart = System.Math.Cos(inclinationStart);
            double sinEnd = System.Math.Sin(inclinationEnd);
            double cosEnd = System.Math.Cos(inclinationEnd);
            double dAzimuth = azimuthEnd - azimuthStart;
            double alongRightHandSide = sinEnd * System.Math.Sin(dAzimuth);
            double alongHighSide = sinEnd * cosStart * System.Math.Cos(dAzimuth) - sinStart * cosEnd;
            return System.Math.Atan2(alongRightHandSide, alongHighSide);
        }
        public double? GetToolface(ICurvilinear3D p)
        {
            if (Inclination != null && Azimuth != null && p != null && p.Inclination != null && p.Azimuth != null)
            {
                double azimuth1 = (double)Azimuth;
                double azimuth2 = (double)p.Azimuth;
                double inclination1 = (double)Inclination;
                double inclination2 = (double)p.Inclination;
                double cosI1 = System.Math.Cos(inclination1);
                double sinI1 = System.Math.Sin(inclination1);
                double cosI2 = System.Math.Cos(inclination2);
                double sinI2 = System.Math.Sin(inclination2);
                return StartToolface(inclination1, azimuth1, inclination2, azimuth2);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// return the toolface
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double? GetAngle(IPoint3D p)
        {
            if (Inclination != null && Azimuth != null)
            {
                return GetAngle(p, System.Math.Cos((double)Inclination), System.Math.Sin((double)Inclination), System.Math.Cos((double)Azimuth), System.Math.Sin((double)Azimuth));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// return the toolface
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ci"></param>
        /// <param name="si"></param>
        /// <param name="ca"></param>
        /// <param name="sa"></param>
        /// <returns></returns>
        public double? GetAngle(IPoint3D p, double ci, double si, double ca, double sa)
        {
            double? dx = p.X - X;
            double? dy = p.Y - Y;
            double? dz = p.Z - Z;
            double? x = ((dx * ci * ca) + (dy * sa * ci) - (dz * si));
            double? y = ((-dx * sa) + (dy * ca));
            double? length = Numeric.SqrtEqual(x * x + y * y);
            if (Numeric.EQ(length, 0))
            {
                return 0.0;
            }
            else
            {
                if (y >= 0)
                {
                    return Numeric.AcosEqual(x / length);
                }
                else
                {
                    return 2.0 * Numeric.PI - Numeric.AcosEqual(x / length);
                }
            }
        }
        /// <summary>
        /// return the build up rate
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double? GetBUR(CurvilinearPoint3D p)
        {
            if (p != null)
            {
                if (Numeric.EQ(Abscissa, p.Abscissa))
                {
                    return 0.0;
                }
                else
                {
                    return (p.Inclination - Inclination) / (p.Abscissa - Abscissa);
                }
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }
        /// <summary>
        /// return the turn rate
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double? GetTR(CurvilinearPoint3D p)
        {
            if (p != null && p.Azimuth != null && Azimuth != null)
            {
                if (Numeric.EQ(Abscissa, p.Abscissa))
                {
                    return 0.0;
                }
                else
                {
                    // The azimuth is reported modulo a full turn, so the difference has to be taken
                    // the short way round. Note that "% 2.0 * Numeric.PI" does not do that: it binds as
                    // "(x % 2.0) * PI", taking the remainder modulo two and then multiplying by pi.
                    return WrapToPi((double)p.Azimuth - (double)Azimuth) / (p.Abscissa - Abscissa);
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// return the curvature
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double? GetDLS(CurvilinearPoint3D p)
        {
            if (p != null && p.Inclination != null && Inclination != null)
            {
                if (Numeric.EQ(Abscissa, p.Abscissa))
                {
                    return 0;
                }
                else
                {
                    return DoglegAngle((double)Inclination, (double)Azimuth, (double)p.Inclination, (double)p.Azimuth) / (p.Abscissa - Abscissa);
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Return the toolface
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double? GetTF(CurvilinearPoint3D p)
        {
            if (p != null)
            {
                return GetAngle(p);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// length of the arc in the horizontal projection
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double? GetVerticalSection(IPoint3D p)
        {
            if (p != null)
            {
                return Numeric.SqrtEqual((p.X - X) * (p.X - X) + (p.Y - Y) * (p.Y - Y));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public Point3D GetAlignedPoint(double l)
        {
            if (Azimuth != null && Inclination != null)
            {
                Point3D p = new Point3D();
                double ca = System.Math.Cos((double)Azimuth);
                double sa = System.Math.Sin((double)Azimuth);
                double ci = System.Math.Cos((double)Inclination);
                double si = System.Math.Sin((double)Inclination);
                p.X = X + l * si * ca;
                p.Y = Y + l * si * sa;
                p.Z = Z + l * ci;
                return p;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// complete the coordinates of the argument based on the minimum curvature method
        /// </summary>
        /// <param name="p"></param>
        public void MinimumCurvatureMethod(CurvilinearPoint3D p)
        {
            if (p != null && p.Inclination != null && p.Azimuth != null && Inclination != null && Azimuth != null && Abscissa != null && p.Abscissa != null)
            {
                if (Numeric.EQ(p.Abscissa, Abscissa))
                {
                    p.X = X;
                    p.Y = Y;
                    p.Z = Z;
                }
                else
                {
                    double si2 = System.Math.Sin((double)p.Inclination);
                    double ci2 = System.Math.Cos((double)p.Inclination);
                    double sa2 = System.Math.Sin((double)p.Azimuth);
                    double ca2 = System.Math.Cos((double)p.Azimuth);
                    double si = System.Math.Sin((double)Inclination);
                    double ci = System.Math.Cos((double)Inclination);
                    double sa = System.Math.Sin((double)Azimuth);
                    double ca = System.Math.Cos((double)Azimuth);
                    double dm = (double)p.Abscissa - (double)Abscissa;
                    double dl = DoglegAngle((double)Inclination, (double)Azimuth, (double)p.Inclination, (double)p.Azimuth);
                    double rf = 0;
                    if (Numeric.LE(dl, 0.02))
                    {
                        rf = 0.5 * (1 + (dl * dl / 12.0) * (1 + (dl * dl / 10.0) * (1.0 + (dl * dl / 168.0) * (1.0 + 31.0 * dl * dl / 18.0))));
                    }
                    else
                    {
                        rf = System.Math.Tan(dl / 2.0) / dl;
                    }
                    p.Z = Z + dm * rf * (ci + ci2);
                    p.X = X + dm * rf * (si * ca + si2 * ca2);
                    p.Y = Y + dm * rf * (si * sa + si2 * sa2);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="inclination"></param>
        /// <param name="azimuth"></param>
        /// <param name="s"></param>
        /// <param name="i"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static CurvilinearPoint3D CreateSpheric(double l, double inclination, double azimuth, double s, double i, double a)
        {
            double ca = System.Math.Cos(azimuth);
            double sa = System.Math.Sin(azimuth);
            double ci = System.Math.Cos(inclination);
            double si = System.Math.Sin(inclination);
            CurvilinearPoint3D p = new CurvilinearPoint3D
            {
                X = l * ca * si,
                Y = l * sa * si,
                Z = l * ci,
                Abscissa = s,
                Inclination = i,
                Azimuth = a
            };
            return p;
        }
        public Point3D TransCoord3RotsReversed(double tf, Point3D p1)
        {
            if (p1 != null && Azimuth != null && Inclination != null && p1.X != null && p1.Y != null && p1.Z != null)
            {
                Point3D p2 = new Point3D();
                double ca = System.Math.Cos((double)Azimuth);
                double sa = System.Math.Sin((double)Azimuth);
                double ci = System.Math.Cos((double)Inclination);
                double si = System.Math.Sin((double)Inclination);
                double ct = System.Math.Cos(tf);
                double st = System.Math.Sin(tf);
                double dx = (double)p1.X;
                double dy = (double)p1.Y;
                double dz = (double)p1.Z;
                p2.X = dx * (ct * ca * ci - sa * st) - dy * (ct * sa + st * ci * ca) + dz * si * ca;
                p2.Y = dx * (st * ca + ct * sa * ci) + dy * (ct * ca + st * ci * sa) + dz * si * sa;
                p2.Z = -ct * si * dx + st * si * dy + ci * dz;
                return p2;
            }
            else
            {
                return null;
            }
        }
        public Point3D TransCoord2PtsTg(Point3D pf, Point3D p1)
        {
            if (pf != null && p1 != null && Azimuth != null && Inclination != null && pf.Y != null && Y != null && pf.Z != null && Z != null && pf.X != null && X != null)
            {
                double ca = System.Math.Cos((double)Azimuth);
                double sa = System.Math.Sin((double)Azimuth);
                double ci = System.Math.Cos((double)Inclination);
                double si = System.Math.Sin((double)Inclination);
                double xk = si * ca;
                double yk = si * sa;
                double zk = ci;
                double xi1 = (double)(ci * (pf.Y - Y) - si * sa * (pf.Z - Z));
                double yi1 = (double)(si * ca * (pf.Z - Z) - ci * (pf.X - X));
                double zi1 = (double)(si * sa * (pf.X - X) - si * ca * (pf.Y - Y));
                double sqrti = System.Math.Sqrt(xi1 * xi1 + yi1 * yi1 + zi1 * zi1);
                if (!Numeric.EQ(sqrti, 0))
                {
                    double xi = xi1 / sqrti;
                    double yi = yi1 / sqrti;
                    double zi = zi1 / sqrti;
                    double xj1 = yi1 * ci - zi1 * si * sa;
                    double yj1 = zi1 * si * ca - xi1 * ci;
                    double zj1 = xi1 * si * sa - yi1 * si * ca;
                    double sqrtj = -System.Math.Sqrt(xj1 * xj1 + yj1 * yj1 + zj1 * zj1);
                    if (!Numeric.EQ(sqrtj, 0))
                    {
                        double xj = xj1 / sqrtj;
                        double yj = yj1 / sqrtj;
                        double zj = zj1 / sqrtj;
                        double x1 = (double)(p1.X - X);
                        double y1 = (double)(p1.Y - Y);
                        double z1 = (double)(p1.Z - Z);
                        Point3D p2 = new Point3D
                        {
                            X = xi * x1 + yi * y1 + zi * z1,
                            Y = xj * x1 + yj * y1 + zj * z1,
                            Z = xk * x1 + yk * y1 + zk * z1
                        };
                        return p2;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public Point3D? TransCoord2PtsTgReversed(Point3D pf, Point3D p1)
        {
            if (pf != null && p1 != null && Azimuth != null && Inclination != null && pf.Y != null && Y != null && pf.Z != null && Z != null && pf.X != null && X != null)
            {
                double ca = System.Math.Cos((double)Azimuth);
                double sa = System.Math.Sin((double)Azimuth);
                double ci = System.Math.Cos((double)Inclination);
                double si = System.Math.Sin((double)Inclination);
                double xk = si * ca;
                double yk = si * sa;
                double zk = ci;
                double xi1 = (double)(ci * (pf.Y - Y) - si * sa * (pf.Z - Z));
                double yi1 = (double)(si * ca * (pf.Z - Z) - ci * (pf.X - X));
                double zi1 = (double)(si * sa * (pf.X - X) - si * ca * (pf.Y - Y));
                double sqrti = System.Math.Sqrt(xi1 * xi1 + yi1 * yi1 + zi1 * zi1);
                if (!Numeric.EQ(sqrti, 0))
                {
                    double xi = xi1 / sqrti;
                    double yi = yi1 / sqrti;
                    double zi = zi1 / sqrti;
                    double xj1 = yi1 * ci - zi1 * si * sa;
                    double yj1 = zi1 * si * ca - xi1 * ci;
                    double zj1 = xi1 * si * sa - yi1 * si * ca;
                    double sqrtj = -System.Math.Sqrt(xj1 * xj1 + yj1 * yj1 + zj1 * zj1);
                    if (!Numeric.EQ(sqrtj, 0))
                    {
                        double xj = xj1 / sqrtj;
                        double yj = yj1 / sqrtj;
                        double zj = zj1 / sqrtj;
                        Point3D p2 = new Point3D
                        {
                            X = xi * p1.X + xj * p1.Y + xk * p1.Z + X,
                            Y = yi * p1.X + yj * p1.Y + yk * p1.Z + Y,
                            Z = zi * p1.X + zj * p1.Y + zk * p1.Z + Z
                        };
                        return p2;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
    }
}
