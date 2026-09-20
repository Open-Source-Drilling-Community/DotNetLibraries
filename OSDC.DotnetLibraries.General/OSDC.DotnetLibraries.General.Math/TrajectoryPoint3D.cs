using MathNet.Numerics.Integration;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// A curvilinear point that also carries the parameters of the curve arriving at it, together with
    /// the geometry of the three curve models used to build a well path: the circular arc, the constant
    /// build and turn curve, and the constant curvature and toolface curve.
    ///
    /// For each model there are three constructions: from a survey, that is from a measured depth, an
    /// inclination and an azimuth; from a target coordinate; and interpolation at an intermediate
    /// measured depth. They are defined here, once, so that everything which needs them shares a single
    /// implementation rather than keeping its own copy. Copies of this mathematics previously existed in
    /// several places and had drifted apart, each carrying defects the others did not.
    ///
    /// Everything is pure SI: angles in radians, lengths in metres, rates in radians per metre, with the
    /// vertical axis positive downward.
    /// </summary>
    public class TrajectoryPoint3D : CurvilinearPoint3D
    {
        /// <summary>
        /// The curvature of the curve arriving at this station, rad/m.
        /// </summary>
        public double? Curvature { get; set; } = null;
        /// <summary>
        /// The toolface angle of the curve arriving at this station, rad. It is constant along a
        /// constant curvature and toolface curve; along a circular arc it is the angle at the start of
        /// the arc, which is what sets the arc off.
        /// </summary>
        public double? Toolface { get; set; } = null;
        /// <summary>
        /// The build up rate at this station, rad/m.
        /// </summary>
        public double? BUR { get; set; } = null;
        /// <summary>
        /// The turn rate at this station, rad/m.
        /// </summary>
        public double? TUR { get; set; } = null;
        /// <summary>
        /// The vertical section at this station, m.
        /// </summary>
        public double? VerticalSection { get; set; } = null;

        public TrajectoryPoint3D() : base()
        {
        }

        public TrajectoryPoint3D(CurvilinearPoint3D src) : base(src)
        {
            if (src is TrajectoryPoint3D trajectory)
            {
                Curvature = trajectory.Curvature;
                Toolface = trajectory.Toolface;
                BUR = trajectory.BUR;
                TUR = trajectory.TUR;
                VerticalSection = trajectory.VerticalSection;
            }
        }

        public TrajectoryPoint3D(double? x, double? y, double? z, double? s, double? inclination, double? azimuth)
            : base(x, y, z, s, inclination, azimuth)
        {
        }

        /// <summary>
        /// Copies the parameters of the curve arriving at <paramref name="src"/> onto this station.
        /// </summary>
        public void SetCurveParameters(TrajectoryPoint3D src)
        {
            if (src == null)
            {
                return;
            }
            Curvature = src.Curvature;
            Toolface = src.Toolface;
            BUR = src.BUR;
            TUR = src.TUR;
            VerticalSection = src.VerticalSection;
        }

        private const double CDTSeriesThreshold = 1.0e-4;
        private static readonly double[] BTTargetSeedInclinations = { -1.5, -0.6, 0.0, 0.6, 1.5, 2.5 };
        private static readonly int[] BTTargetSeedTurns = { 0, 1, -1, 2, -2 };   // in increasing whole turns
        private static readonly double BTTargetMaxInclinationStep = 0.5 * Numeric.PI;
        /// <summary>
        /// How close to the vertical an inclination is taken to be at it. The constant curvature and
        /// toolface construction loses the azimuth there, and the effective sine it relies on is the
        /// ratio of a vanishing angle to a diverging logarithm, so approaching the vertical has to be
        /// treated as arriving at it well before the rounding of a double makes the ratio meaningless.
        /// </summary>
        private const double InclinationMarginCDT = 1.0e-9;
        private static readonly double BTTargetMaxAzimuthStep = Numeric.PI;
        private static readonly double[] CDTTargetSeedDoglegs = { 0.05, 0.4, 1.0, 2.0, 3.0, 4.5 };
        private const double CDTTargetGridResidualGate = 0.6;

        public bool CompleteCASIA(CurvilinearPoint3D next)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null || next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }
            double x1 = X.Value;
            double y1 = Y.Value;
            double z1 = Z.Value;
            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;
            double s2 = next.Abscissa.Value;
            double i2 = next.Inclination.Value;
            double a2 = next.Azimuth.Value;
            double dm = s2 - s1;
            if (Numeric.EQ(dm, 0))
            {
                next.X = x1;
                next.Y = y1;
                next.Z = z1;
                next.Inclination = i1;
                next.Azimuth = a1;
                next.Abscissa = s2;
                return true;
            }
            double ci1 = System.Math.Cos(i1);
            double si1 = System.Math.Sin(i1);
            double ca1 = System.Math.Cos(a1);
            double sa1 = System.Math.Sin(a1);
            double ci2 = System.Math.Cos(i2);
            double si2 = System.Math.Sin(i2);
            double ca2 = System.Math.Cos(a2);
            double sa2 = System.Math.Sin(a2);
            double dl = DoglegAngle(i1, a1, i2, a2);
            double rf;
            if (Numeric.EQ(dl, 0, 0.02))
            {
                double dl2 = dl * dl;
                rf = 1 + (dl2 / 12.0) * (1.0 + (dl2 / 10.0) * (1 + (dl2 / 168.0) * (1.0 + 31 * dl2 / 18.0)));
            }
            else
            {
                rf = (2.0 / dl) * System.Math.Tan(dl / 2.0);
            }
            next.X = x1 + 0.5 * dm * rf * (si1 * ca1 + si2 * ca2);
            next.Y = y1 + 0.5 * dm * rf * (si1 * sa1 + si2 * sa2);
            next.Z = z1 + 0.5 * dm * rf * (ci1 + ci2);
            if (next is TrajectoryPoint3D target)
            {
                // The curvature of a circular arc is constant and is the dogleg angle over the length,
                // so it is exact rather than estimated. The toolface angle is not constant along the
                // arc; the value reported is the one at its start, which is what sets the arc off and
                // what the circular arc construction from a curvature and a toolface angle takes.
                target.Curvature = dl / dm;
                target.Toolface = StartToolface(i1, a1, i2, a2);
                if (VerticalSection is not null && next.X is not null && next.Y is not null)
                {
                    target.VerticalSection = VerticalSection + System.Math.Sqrt((x1 - next.X.Value) * (x1 - next.X.Value) + (y1 - next.Y.Value) * (y1 - next.Y.Value));
                }
            }
            return true;
        }
        /// <summary>
        /// The integral from <paramref name="inclinationStart"/> to <paramref name="inclinationEnd"/>
        /// of d(inclination)/sin(inclination), which equals ln|tan(inclinationEnd/2)/tan(inclinationStart/2)|.
        /// It is evaluated as a single hyperbolic arc tangent, using ln tan(i/2) = -atanh(cos i) and the
        /// atanh subtraction formula, because the literal difference of logarithms cancels catastrophically
        /// as the two inclinations approach each other and loses up to seven significant digits.
        /// The numerator is expanded as cos(i1) - cos(i2) = 2*sin(iBar)*sin(di/2), which is what removes
        /// the cancellation. The argument is a difference of hyperbolic tangents and is therefore always
        /// strictly inside (-1, 1), so no clamping is needed.
        /// </summary>
        public static double LogTanRatioCDT(double inclinationStart, double inclinationEnd)
        {
            double numerator = 2.0
                * System.Math.Sin(0.5 * (inclinationStart + inclinationEnd))
                * System.Math.Sin(0.5 * (inclinationEnd - inclinationStart));
            double denominator = 1.0 - System.Math.Cos(inclinationStart) * System.Math.Cos(inclinationEnd);
            return System.Math.Atanh(numerator / denominator);
        }
        /// <summary>
        /// The effective sine G = (inclinationEnd - inclinationStart) / LogTanRatioCDT(...) of a constant
        /// curvature and toolface segment. By the mean value theorem for integrals G = sin(xi) for some xi
        /// between the two inclinations, so G is bounded by the extreme values of sin over the segment and
        /// tends smoothly to sin(inclinationStart) as the two inclinations meet. That limit is what removes
        /// the zero build up special case from the curve defining parameters: it is a limit of the general
        /// formula rather than a separate branch. Evaluated branch free and cancellation free.
        /// </summary>
        public static double EffectiveSineCDT(double inclinationStart, double inclinationEnd)
        {
            double dInclination = inclinationEnd - inclinationStart;
            double inclinationBar = 0.5 * (inclinationStart + inclinationEnd);
            double denominator = 1.0 - System.Math.Cos(inclinationStart) * System.Math.Cos(inclinationEnd);
            double x = 2.0 * System.Math.Sin(inclinationBar) * System.Math.Sin(0.5 * dInclination) / denominator;
            return UOverSin(0.5 * dInclination) * XOverAtanh(x) * denominator / System.Math.Sin(inclinationBar);
        }
        /// <summary>
        /// u / sin(u), analytic at u = 0 with value 1.
        /// </summary>
        private static double UOverSin(double u)
        {
            if (System.Math.Abs(u) < CDTSeriesThreshold)
            {
                double u2 = u * u;
                return 1.0 + u2 / 6.0 + 7.0 * u2 * u2 / 360.0;
            }
            return u / System.Math.Sin(u);
        }
        /// <summary>
        /// sin(u) / u, analytic at u = 0 with value 1.
        /// </summary>
        private static double SinOverU(double u)
        {
            if (System.Math.Abs(u) < CDTSeriesThreshold)
            {
                double u2 = u * u;
                return 1.0 - u2 / 6.0 + u2 * u2 / 120.0;
            }
            return System.Math.Sin(u) / u;
        }
        /// <summary>
        /// x / atanh(x), analytic at x = 0 with value 1.
        ///
        /// In exact arithmetic the argument stays strictly inside (-1, 1), reaching the ends only where
        /// the segment touches the vertical. There it is exactly one, and the rounding of a double can
        /// put it a little the other side, where atanh gives back nothing at all. Since atanh diverges
        /// at the ends the ratio tends to zero, so the ends are taken to be that limit and anything past
        /// them is taken to be the end it passed.
        /// </summary>
        private static double XOverAtanh(double x)
        {
            double magnitude = System.Math.Abs(x);
            if (magnitude < CDTSeriesThreshold)
            {
                double x2 = x * x;
                return 1.0 - x2 / 3.0 + 4.0 * x2 * x2 / 45.0;
            }
            if (magnitude >= 1.0)
            {
                return 0.0;
            }
            return x / System.Math.Atanh(x);
        }
        /// <summary>
        /// sqrt(a*a + b*b), computed by scaling so that it neither overflows nor underflows.
        /// </summary>
        protected static double Hypot(double a, double b)
        {
            a = System.Math.Abs(a);
            b = System.Math.Abs(b);
            if (a < b)
            {
                (a, b) = (b, a);
            }
            if (a == 0.0)
            {
                return 0.0;
            }
            double ratio = b / a;
            return a * System.Math.Sqrt(1.0 + ratio * ratio);
        }
        /// <summary>
        /// The Gauss-Legendre order sufficient for roughly 1e-8 m accuracy on the North and East
        /// integrals of a constant curvature and toolface segment. The order is governed by the total
        /// turning of the segment, not by its length. For a routine 30 m survey interval it gives 26.
        /// </summary>
        private static int GetCDTIntegrationOrder(double dInclination, double dAzimuth)
        {
            double turning = System.Math.Abs(dAzimuth) + System.Math.Abs(dInclination);
            int order = (int)System.Math.Ceiling(2.0 * turning) + 24;
            order = System.Math.Max(24, System.Math.Min(512, order));
            if ((order & 1) == 1)
            {
                order++;
            }
            return order;
        }
        /// <summary>
        /// Attitude at the along hole distance <paramref name="s"/> from the start of a constant curvature
        /// and toolface segment whose defining differences have already been established. The azimuth
        /// identity follows from a(s) - a1 = turn*s/G(i1, i(s)); written this way it never divides by the
        /// build up rate and it reproduces the azimuth of the end station exactly at the end of the
        /// segment, so a densified track cannot drift away from its own end station.
        /// </summary>
        private static void GetCDTAttitude(double s, double inclinationStart, double azimuthStart, double dInclination, double dAzimuth, double dm, double effectiveSine, out double inclination, out double azimuth)
        {
            if (dm == 0.0)
            {
                inclination = inclinationStart;
                azimuth = azimuthStart;
                return;
            }
            inclination = inclinationStart + dInclination * (s / dm);
            if (System.Math.Abs(s) < 1.0e-15)
            {
                azimuth = azimuthStart;
                return;
            }
            azimuth = azimuthStart + dAzimuth * (s / dm) * effectiveSine / EffectiveSineCDT(inclinationStart, inclination);
        }
        /// <summary>
        /// Inclination and azimuth of the constant curvature and toolface curve joining this survey to
        /// <paramref name="next"/>, at the abscissa <paramref name="abscissa"/>.
        /// </summary>
        public bool AttitudeAtAbscissaCDT(CurvilinearPoint3D next, double abscissa, out double inclination, out double azimuth, int branch = 0, bool unwrapAzimuth = true)
        {
            inclination = Numeric.UNDEF_DOUBLE;
            azimuth = Numeric.UNDEF_DOUBLE;
            if (next == null || Inclination == null || Azimuth == null || Abscissa == null ||
                next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }
            double dm = next.Abscissa.Value - Abscissa.Value;
            if (Numeric.EQ(dm, 0.0))
            {
                return false;
            }
            double i1 = Inclination.Value;
            double i2 = next.Inclination.Value;
            double a1 = Azimuth.Value;
            double dInclination = i2 - i1;
            double dAzimuth = unwrapAzimuth ? WrapToPi(next.Azimuth.Value - a1) : next.Azimuth.Value - a1;
            dAzimuth += 2.0 * Numeric.PI * branch;
            GetCDTAttitude(abscissa - Abscissa.Value, i1, a1, dInclination, dAzimuth, dm, EffectiveSineCDT(i1, i2), out inclination, out azimuth);
            return Numeric.IsDefined(inclination) && Numeric.IsDefined(azimuth);
        }
        /// <summary>
        /// Apply the constant curvature and toolface method between this survey and the next
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteCDTSIA(CurvilinearPoint3D next)
        {
            return CompleteCDTSIAInternal(next, 0, true, out _, false);
        }
        /// <summary>
        /// Apply the constant curvature and toolface method between this survey and the next, on a chosen
        /// azimuth branch.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="branch">
        /// A survey reports its azimuth modulo 2*PI, so every azimuth change wrap(a2 - a1) + 2*PI*branch
        /// is a legitimate curve through the same two stations. Branch 0 is the minimum dogleg solution
        /// and is the correct default; a nonzero branch is needed only where the physical turn between the
        /// stations genuinely exceeds 180 degrees, which happens near vertical over long intervals, and it
        /// cannot be inferred from the survey pair alone.
        /// </param>
        /// <param name="unwrapAzimuth">
        /// When false the azimuth difference is used exactly as supplied, for callers that already carry a
        /// continuous azimuth. Using the wrong one silently changes the result near north.
        /// </param>
        /// <returns></returns>
        public bool CompleteCDTSIA(CurvilinearPoint3D next, int branch, bool unwrapAzimuth = true)
        {
            return CompleteCDTSIAInternal(next, branch, unwrapAzimuth, out _, false);
        }
        /// <summary>
        /// Apply the constant build and turn method between this survey and the next.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteBTSIA(CurvilinearPoint3D next)
        {
            return CompleteBTSIAInternal(next, 0, true);
        }
        /// <summary>
        /// Apply the constant build and turn method between this survey and the next, on a chosen
        /// azimuth branch.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="branch">
        /// A survey reports its azimuth modulo 2*PI, so every azimuth change wrap(a2 - a1) + 2*PI*branch
        /// describes a curve through the same two stations. Branch 0 is the smallest turn and is the
        /// correct default; a nonzero branch is needed only where the turn between the stations genuinely
        /// exceeds half a turn, which cannot be inferred from the survey pair alone.
        /// </param>
        /// <param name="unwrapAzimuth">
        /// When false the azimuth difference is used exactly as supplied, for callers that already carry
        /// a continuous azimuth.
        /// </param>
        /// <returns></returns>
        public bool CompleteBTSIA(CurvilinearPoint3D next, int branch, bool unwrapAzimuth = true)
        {
            return CompleteBTSIAInternal(next, branch, unwrapAzimuth);
        }
        private bool CompleteBTSIAInternal(CurvilinearPoint3D next, int branch, bool unwrapAzimuth)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null || next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }
            double x1 = X.Value;
            double y1 = Y.Value;
            double z1 = Z.Value;
            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;
            double s2 = next.Abscissa.Value;
            double i2 = next.Inclination.Value;
            double a2 = next.Azimuth.Value;
            double dm = s2 - s1;
            if (Numeric.EQ(dm, 0))
            {
                next.X = x1;
                next.Y = y1;
                next.Z = z1;
                next.Inclination = i1;
                next.Azimuth = a1;
                next.Abscissa = s2;
                if (next is TrajectoryPoint3D degenerate)
                {
                    degenerate.BUR = 0.0;
                    degenerate.TUR = 0.0;
                    degenerate.Curvature = 0.0;
                    degenerate.Toolface = null;
                    degenerate.VerticalSection = VerticalSection;
                }
                return true;
            }

            static double NormalizeAzimuth(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0.0)
                {
                    angle += twoPi;
                }
                return angle;
            }
            static void CanonicalizeInclinationAzimuth(ref double inclination, ref double azimuth)
            {
                while (inclination < 0.0)
                {
                    inclination = -inclination;
                    azimuth += Numeric.PI;
                }
                while (inclination > Numeric.PI)
                {
                    inclination = 2.0 * Numeric.PI - inclination;
                    azimuth += Numeric.PI;
                }
                azimuth = NormalizeAzimuth(azimuth);
            }

            // A caller carrying a continuous azimuth supplies a consistent attitude already, and folding
            // it would destroy the very continuity it carries.
            if (unwrapAzimuth)
            {
                CanonicalizeInclinationAzimuth(ref i2, ref a2);
            }

            double dInclination = i2 - i1;
            // A survey reports its azimuth modulo 2*PI. Taking the difference literally turns an ordinary
            // small turn across north into a turn of almost a full circle the other way, which moves the
            // station by metres over a routine interval.
            double dAzimuth = unwrapAzimuth ? WrapToPi(a2 - a1) : a2 - a1;
            dAzimuth += 2.0 * Numeric.PI * branch;

            double bur = dInclination / dm;
            double tur = dAzimuth / dm;

            // Both angles are linear in the along hole distance, so the two position integrals collapse
            // in closed form. Writing sin(i)cos(a) and sin(i)sin(a) as sums of a single sine and cosine
            // of (i + a) and (i - a) leaves only
            //     integral of sin(phi0 + r*s) ds = L*sin(phi0 + r*L/2)*sinc(r*L/2)
            //     integral of cos(phi0 + r*s) ds = L*cos(phi0 + r*L/2)*sinc(r*L/2)
            // over the segment. Written this way nothing is ever divided by the sum or the difference of
            // the two rates, so the cases where the build up rate equals plus or minus the turn rate stop
            // being special: they are ordinary points of a single expression rather than branches, and
            // the neighbourhood around them no longer loses digits to cancellation.
            double phaseSum = i1 + a1;
            double phaseDifference = i1 - a1;
            double halfSum = 0.5 * (bur + tur) * dm;
            double halfDifference = 0.5 * (bur - tur) * dm;
            double sumWeight = dm * SinOverU(halfSum);
            double differenceWeight = dm * SinOverU(halfDifference);

            double dNorth = 0.5 * (System.Math.Sin(phaseSum + halfSum) * sumWeight
                                 + System.Math.Sin(phaseDifference + halfDifference) * differenceWeight);
            double dEast = 0.5 * (System.Math.Cos(phaseDifference + halfDifference) * differenceWeight
                                - System.Math.Cos(phaseSum + halfSum) * sumWeight);
            // The same primitive gives the vertical, so the zero build up limit needs no special case
            // either: it is dm*cos(i1) automatically.
            double dVertical = dm * System.Math.Cos(i1 + 0.5 * dInclination) * SinOverU(0.5 * dInclination);

            if (!Numeric.IsDefined(dNorth) || !Numeric.IsDefined(dEast) || !Numeric.IsDefined(dVertical))
            {
                return false;
            }

            next.X = x1 + dNorth;
            next.Y = y1 + dEast;
            next.Z = z1 + dVertical;
            next.Abscissa = s2;
            next.Inclination = i2;
            next.Azimuth = unwrapAzimuth ? a2 : a1 + dAzimuth;

            if (next is TrajectoryPoint3D target)
            {
                // The build up and turn rates are the defining constants of the segment, so they are
                // exact rather than estimated. The curvature of the path is not constant along a constant
                // build and turn curve; the value reported is the one holding at the end station, on the
                // same convention the rest of the class uses.
                double sinEnd = System.Math.Sin(i2);
                target.BUR = bur;
                target.TUR = tur;
                target.Curvature = Hypot(bur, tur * sinEnd);
                target.Toolface = System.Math.Atan2(tur * sinEnd, bur);
                if (VerticalSection is not null)
                {
                    target.VerticalSection = VerticalSection + System.Math.Sqrt(dNorth * dNorth + dEast * dEast);
                }
            }
            return true;
        }
        /// <summary>
        /// Apply the circular arc method from this survey to a target coordinate, solving for the
        /// station that reaches it.
        ///
        /// This is closed form and needs no search. A circular arc leaving a known tangent and passing
        /// through a known point is fully determined, because the chord of a circular arc bisects the
        /// angle between the tangents at its two ends. Writing the unit chord as c and the unit start
        /// tangent as t0, the half dogleg is the angle psi between them, and
        ///     curve length = chord * psi / sin(psi)
        ///     end tangent  = 2*(t0 . c)*c - t0          the reflection of t0 in the chord
        ///     curvature    = 2*psi / curve length
        /// The length is evaluated through u/sin(u), which is analytic at zero, so a target lying
        /// straight ahead needs no special case: it gives the chord itself.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteCAXYZ(CurvilinearPoint3D next)
        {
            if (next == null ||
                X == null || Y == null || Z == null ||
                next.X == null || next.Y == null || next.Z == null ||
                Inclination == null || Azimuth == null || Abscissa == null)
            {
                return false;
            }

            double sinInclination = System.Math.Sin(Inclination.Value);
            double tangentNorth = sinInclination * System.Math.Cos(Azimuth.Value);
            double tangentEast = sinInclination * System.Math.Sin(Azimuth.Value);
            double tangentVertical = System.Math.Cos(Inclination.Value);

            double dNorth = next.X.Value - X.Value;
            double dEast = next.Y.Value - Y.Value;
            double dVertical = next.Z.Value - Z.Value;
            double chord = System.Math.Sqrt(dNorth * dNorth + dEast * dEast + dVertical * dVertical);

            if (Numeric.EQ(chord, 0.0))
            {
                next.Abscissa = Abscissa;
                next.Inclination = Inclination;
                next.Azimuth = Azimuth;
                if (next is TrajectoryPoint3D coincident)
                {
                    coincident.Curvature = 0.0;
                    coincident.BUR = 0.0;
                    coincident.TUR = 0.0;
                    coincident.Toolface = null;
                    coincident.VerticalSection = VerticalSection;
                }
                return true;
            }

            double chordNorth = dNorth / chord;
            double chordEast = dEast / chord;
            double chordVertical = dVertical / chord;

            double cosHalfDogleg = tangentNorth * chordNorth + tangentEast * chordEast + tangentVertical * chordVertical;
            if (cosHalfDogleg > 1.0)
            {
                cosHalfDogleg = 1.0;
            }
            if (cosHalfDogleg < -1.0)
            {
                cosHalfDogleg = -1.0;
            }
            // A target behind the start tangent cannot be reached by a circular arc leaving it forwards.
            if (!Numeric.GT(cosHalfDogleg, 0.0))
            {
                return false;
            }

            double halfDogleg = System.Math.Acos(cosHalfDogleg);
            double curveLength = chord * UOverSin(halfDogleg);
            if (!Numeric.IsDefined(curveLength) || !Numeric.GT(curveLength, 0.0))
            {
                return false;
            }

            // The end tangent is the start tangent reflected in the chord.
            double endNorth = 2.0 * cosHalfDogleg * chordNorth - tangentNorth;
            double endEast = 2.0 * cosHalfDogleg * chordEast - tangentEast;
            double endVertical = 2.0 * cosHalfDogleg * chordVertical - tangentVertical;

            next.Abscissa = Abscissa.Value + curveLength;
            next.Inclination = Numeric.AcosEqual(endVertical);
            double endHorizontal = System.Math.Sqrt(endNorth * endNorth + endEast * endEast);
            if (Numeric.EQ(endHorizontal, 0.0))
            {
                // A vertical tangent has no defined azimuth; the circular arc construction keeps the one
                // it started with.
                next.Azimuth = Azimuth;
            }
            else
            {
                double endAzimuth = System.Math.Atan2(endEast, endNorth);
                next.Azimuth = endAzimuth < 0.0 ? endAzimuth + 2.0 * Numeric.PI : endAzimuth;
            }

            if (next is TrajectoryPoint3D target)
            {
                target.Curvature = 2.0 * halfDogleg / curveLength;
                target.Toolface = StartToolface(Inclination.Value, Azimuth.Value,
                    next.Inclination!.Value, next.Azimuth!.Value);
                if (VerticalSection is not null)
                {
                    target.VerticalSection = VerticalSection + System.Math.Sqrt(dNorth * dNorth + dEast * dEast);
                }
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        /// <summary>
        /// Displacement of a constant build and turn curve of unit length that sweeps the given changes
        /// in inclination and azimuth.
        ///
        /// Both angles are linear in the along hole distance, so the attitude at a given fraction of the
        /// curve depends only on the two swept angles and not on the length. The length is therefore a
        /// pure scale on the displacement and cannot influence its direction. Evaluating this costs a
        /// handful of trigonometric calls: the constant build and turn position is closed form, with no
        /// quadrature anywhere.
        /// </summary>
        private bool GetBTUnitDisplacement(double inclinationStart, double azimuthStart, double dInclination, double dAzimuth, out double dNorth, out double dEast, out double dVertical)
        {
            dNorth = 0.0;
            dEast = 0.0;
            dVertical = 0.0;
            TrajectoryPoint3D origin = new TrajectoryPoint3D()
            {
                X = 0.0,
                Y = 0.0,
                Z = 0.0,
                Abscissa = 0.0,
                Inclination = inclinationStart,
                Azimuth = azimuthStart
            };
            TrajectoryPoint3D unit = new TrajectoryPoint3D()
            {
                Abscissa = 1.0,
                Inclination = inclinationStart + dInclination,
                Azimuth = azimuthStart + dAzimuth
            };
            // The azimuth here is continuous by construction and must be used exactly as supplied.
            if (!origin.CompleteBTSIA(unit, 0, false) || unit.X == null || unit.Y == null || unit.Z == null)
            {
                return false;
            }
            dNorth = unit.X.Value;
            dEast = unit.Y.Value;
            dVertical = unit.Z.Value;
            return Numeric.IsDefined(dNorth) && Numeric.IsDefined(dEast) && Numeric.IsDefined(dVertical);
        }
        /// <summary>
        /// Solves the constant build and turn target point problem directly, for the change in
        /// inclination, the change in azimuth and the curve length that carry the curve from this survey
        /// to the given offset.
        ///
        /// Only the two swept angles are searched, against the DIRECTION of the target, because the
        /// length is a pure scale which cannot influence that direction; the length then follows from a
        /// single division. Three unknowns become two, and the scale, which is what makes a three way
        /// search ill conditioned, leaves the iteration altogether.
        ///
        /// The swept azimuth is a free unknown here rather than a station azimuth folded into a single
        /// revolution, so turns beyond half a turn are reachable.
        /// </summary>
        private bool TrySolveBTTargetDirect(double inclinationStart, double azimuthStart, double dNorth, double dEast, double dVertical, double chord, out double dInclination, out double dAzimuth, out double curveLength)
        {
            dInclination = 0.0;
            dAzimuth = 0.0;
            curveLength = 0.0;
            if (!Numeric.GT(chord, 0.0))
            {
                return false;
            }

            double targetVerticalAngle = System.Math.Atan2(System.Math.Sqrt(dNorth * dNorth + dEast * dEast), dVertical);
            double targetBearing = WrapToPi(System.Math.Atan2(dEast, dNorth) - azimuthStart);

            // The planar target is closed form: with no turn the ratio of horizontal to vertical offset
            // is the tangent of the mean inclination, so the swept inclination is twice the difference
            // between the direction of the target and the starting inclination.
            double seedInclination = 2.0 * (targetVerticalAngle - inclinationStart);
            double seedAzimuth = 2.0 * targetBearing;

            // A target point is reached by many constant build and turn curves, differing by whole
            // turns and by length, and they are genuinely different answers rather than the same answer
            // written differently. Every seed is therefore tried and the shortest curve is returned: the
            // least measured depth that reaches the target is the one a caller means, and it is the only
            // choice that does not depend on which basin a particular seed happened to fall into.
            //
            // Trying every seed is affordable here because evaluating a candidate costs a handful of
            // trigonometric calls rather than a quadrature.
            bool found = false;
            double bestInclination = 0.0;
            double bestAzimuth = 0.0;
            double bestLength = double.PositiveInfinity;

            void Consider(double seedSweptInclination, double seedSweptAzimuth)
            {
                if (TrySolveBTTargetFromSeed(inclinationStart, azimuthStart, chord, targetVerticalAngle, targetBearing,
                        seedSweptInclination, seedSweptAzimuth,
                        out double trialInclination, out double trialAzimuth, out double trialLength, out _) &&
                    trialLength < bestLength)
                {
                    found = true;
                    bestInclination = trialInclination;
                    bestAzimuth = trialAzimuth;
                    bestLength = trialLength;
                }
            }

            // Seeds are grouped by how many whole turns they start from, and the groups are tried in
            // increasing order. A curve that wraps an extra revolution has to travel further to reach
            // the same point, so once a group yields a solution no later group can better it and the
            // search stops there. On a routine target that settles at the first group.
            foreach (int wholeTurns in BTTargetSeedTurns)
            {
                if (wholeTurns == 0)
                {
                    Consider(seedInclination, seedAzimuth);
                }
                foreach (double gridInclination in BTTargetSeedInclinations)
                {
                    foreach (double baseAzimuth in new double[] { 2.0 * targetBearing, targetBearing })
                    {
                        Consider(gridInclination, baseAzimuth + 2.0 * Numeric.PI * wholeTurns);
                    }
                }
                if (found)
                {
                    break;
                }
            }

            if (!found)
            {
                return false;
            }
            dInclination = bestInclination;
            dAzimuth = bestAzimuth;
            curveLength = bestLength;
            return true;
        }
        private bool TrySolveBTTargetFromSeed(double inclinationStart, double azimuthStart, double chord, double targetVerticalAngle, double targetBearing, double seedInclination, double seedAzimuth, out double dInclination, out double dAzimuth, out double curveLength, out double achievedResidual)
        {
            const double InclinationMargin = 1.0e-9;
            const int MaxIterations = 60;
            const int MaxBacktracks = 40;

            dInclination = seedInclination;
            dAzimuth = seedAzimuth;
            curveLength = 0.0;
            achievedResidual = double.PositiveInfinity;

            bool Residual(double trialInclination, double trialAzimuth, out double verticalResidual, out double bearingResidual)
            {
                verticalResidual = 0.0;
                bearingResidual = 0.0;
                if (!GetBTUnitDisplacement(inclinationStart, azimuthStart, trialInclination, trialAzimuth,
                        out double fNorth, out double fEast, out double fVertical))
                {
                    return false;
                }
                verticalResidual = System.Math.Atan2(System.Math.Sqrt(fNorth * fNorth + fEast * fEast), fVertical) - targetVerticalAngle;
                bearingResidual = WrapToPi(System.Math.Atan2(fEast, fNorth) - azimuthStart - targetBearing);
                return true;
            }

            if (!Residual(dInclination, dAzimuth, out double g0, out double g1))
            {
                return false;
            }
            double norm = Hypot(g0, g1);

            for (int iteration = 0; iteration < MaxIterations && norm > 1.0e-14; iteration++)
            {
                double hInclination = 1.0e-7 * System.Math.Max(1.0, System.Math.Abs(dInclination));
                double hAzimuth = 1.0e-7 * System.Math.Max(1.0, System.Math.Abs(dAzimuth));
                if (!Residual(dInclination + hInclination, dAzimuth, out double p00, out double p10) ||
                    !Residual(dInclination, dAzimuth + hAzimuth, out double p01, out double p11))
                {
                    return false;
                }
                double j00 = (p00 - g0) / hInclination;
                double j01 = (p01 - g0) / hAzimuth;
                double j10 = (p10 - g1) / hInclination;
                double j11 = (p11 - g1) / hAzimuth;
                double determinant = j00 * j11 - j01 * j10;
                if (!Numeric.IsDefined(determinant) || determinant == 0.0)
                {
                    break;
                }
                double stepInclination = -(j11 * g0 - j01 * g1) / determinant;
                double stepAzimuth = -(j00 * g1 - j10 * g0) / determinant;

                // The target point problem has many solutions, differing by whole turns and by length.
                // An unbounded Newton step can leap several revolutions out of the basin its seed was
                // chosen for and land on a far longer curve that also reaches the target. Limiting how
                // far a single step may travel keeps each seed in its own basin, so trying the seeds in
                // order of increasing turn returns the shortest solution rather than an arbitrary one.
                double stepExcursion = System.Math.Max(System.Math.Abs(stepInclination) / BTTargetMaxInclinationStep,
                                                       System.Math.Abs(stepAzimuth) / BTTargetMaxAzimuthStep);
                if (stepExcursion > 1.0)
                {
                    stepInclination /= stepExcursion;
                    stepAzimuth /= stepExcursion;
                }

                double relaxation = 1.0;
                bool improved = false;
                for (int backtrack = 0; backtrack < MaxBacktracks; backtrack++)
                {
                    double trialInclination = dInclination + relaxation * stepInclination;
                    double trialAzimuth = dAzimuth + relaxation * stepAzimuth;
                    double endInclination = inclinationStart + trialInclination;
                    if (endInclination <= InclinationMargin || endInclination >= Numeric.PI - InclinationMargin)
                    {
                        relaxation *= 0.5;
                        continue;
                    }
                    if (Residual(trialInclination, trialAzimuth, out double t0, out double t1))
                    {
                        double trialNorm = Hypot(t0, t1);
                        if (trialNorm < norm)
                        {
                            dInclination = trialInclination;
                            dAzimuth = trialAzimuth;
                            g0 = t0;
                            g1 = t1;
                            norm = trialNorm;
                            improved = true;
                            break;
                        }
                    }
                    relaxation *= 0.5;
                }
                if (!improved)
                {
                    break;
                }
            }

            achievedResidual = norm;
            if (!(norm <= 1.0e-9))
            {
                return false;
            }
            if (!GetBTUnitDisplacement(inclinationStart, azimuthStart, dInclination, dAzimuth,
                    out double uNorth, out double uEast, out double uVertical))
            {
                return false;
            }
            double unitRange = System.Math.Sqrt(uNorth * uNorth + uEast * uEast + uVertical * uVertical);
            if (!Numeric.GT(unitRange, 0.0))
            {
                return false;
            }
            curveLength = chord / unitRange;
            return Numeric.IsDefined(curveLength) && Numeric.GT(curveLength, 0.0);
        }
        public bool CompleteBTXYZ(CurvilinearPoint3D next)
        {
            if (next == null ||
                X == null ||
                Y == null ||
                Z == null ||
                next.X == null ||
                next.Y == null ||
                next.Z == null ||
                Inclination == null ||
                Azimuth == null ||
                Abscissa == null)
            {
                return false;
            }

            double targetX = next.X.Value;
            double targetY = next.Y.Value;
            double targetZ = next.Z.Value;
            double x1 = X.Value;
            double y1 = Y.Value;
            double z1 = Z.Value;
            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;

            double chord =
                System.Math.Sqrt(
                    (targetX - x1) * (targetX - x1) +
                    (targetY - y1) * (targetY - y1) +
                    (targetZ - z1) * (targetZ - z1));

            if (Numeric.EQ(chord, 0.0))
            {
                next.Abscissa = s1;
                next.Inclination = i1;
                next.Azimuth = a1;
                if (next is TrajectoryPoint3D coincident)
                {
                    coincident.BUR = 0.0;
                    coincident.TUR = 0.0;
                    coincident.Curvature = 0.0;
                    coincident.Toolface = null;
                }
                return true;
            }

            // The length of a constant build and turn curve is a pure scale on its displacement, so only
            // the two swept angles are solved, against the direction of the target, and the length then
            // follows from one division. The swept azimuth is a free unknown rather than a station
            // azimuth folded into a single revolution, so a target whose curve turns by more than half a
            // turn is reachable.
            if (TrySolveBTTargetDirect(i1, a1, targetX - x1, targetY - y1, targetZ - z1, chord,
                    out double directInclination, out double directAzimuth, out double directLength))
            {
                TrajectoryPoint3D direct = new TrajectoryPoint3D()
                {
                    Abscissa = s1 + directLength,
                    Inclination = i1 + directInclination,
                    Azimuth = a1 + directAzimuth
                };
                TrajectoryPoint3D origin = new TrajectoryPoint3D(this) { Abscissa = s1 };
                origin.X = x1;
                origin.Y = y1;
                origin.Z = z1;
                // Confirm the curve really lands on the target, and fill in the curve defining
                // parameters exactly while doing so.
                if (origin.CompleteBTSIA(direct, 0, false) &&
                    direct.X != null && direct.Y != null && direct.Z != null &&
                    System.Math.Abs(direct.X.Value - targetX) <= 1e-7 * System.Math.Max(1.0, chord) &&
                    System.Math.Abs(direct.Y.Value - targetY) <= 1e-7 * System.Math.Max(1.0, chord) &&
                    System.Math.Abs(direct.Z.Value - targetZ) <= 1e-7 * System.Math.Max(1.0, chord))
                {
                    next.Abscissa = direct.Abscissa;
                    next.Inclination = direct.Inclination;
                    next.Azimuth = direct.Azimuth;
                    if (next is TrajectoryPoint3D directTarget)
                    {
                        directTarget.Curvature = direct.Curvature;
                        directTarget.Toolface = direct.Toolface;
                        directTarget.BUR = direct.BUR;
                        directTarget.TUR = direct.TUR;
                        directTarget.VerticalSection = direct.VerticalSection;
                    }
                    return true;
                }
            }

            // No constant build and turn curve reaches the target. The direct reduction above searches
            // the two swept angles, which between them describe every such curve, over whole turns in
            // either direction, so reaching this point means the target is not reachable rather than
            // that a search gave up on it.
            return false;
        }
        /// <summary>
        /// Displacement of a constant curvature and toolface curve of unit curvature carried over a
        /// dogleg angle of <paramref name="dogleg"/>, together with the attitude it ends on.
        ///
        /// Scaling the curvature by a factor while dividing the length by the same factor leaves the
        /// dogleg angle and the toolface angle untouched and scales the whole displacement by the
        /// inverse of that factor. The shape of the curve therefore depends only on the toolface angle
        /// and the dogleg angle, and the curvature is a pure scale on top of it. That holds for a curve
        /// which runs into vertical and continues along its tangent exactly as it does for one that does
        /// not, because both parts of such a curve scale together, so a single reduction covers every
        /// curve the forward construction can produce.
        /// </summary>
        private bool GetCDTUnitCurvatureDisplacement(double inclinationStart, double azimuthStart, double toolface, double dogleg, out double dNorth, out double dEast, out double dVertical)
        {
            dNorth = 0.0;
            dEast = 0.0;
            dVertical = 0.0;
            if (!Numeric.GT(dogleg, 0.0) || !Numeric.IsDefined(dogleg) || !Numeric.IsDefined(toolface))
            {
                return false;
            }
            TrajectoryPoint3D origin = new TrajectoryPoint3D()
            {
                X = 0.0,
                Y = 0.0,
                Z = 0.0,
                Abscissa = 0.0,
                Inclination = inclinationStart,
                Azimuth = azimuthStart
            };
            TrajectoryPoint3D unit = new TrajectoryPoint3D() { Abscissa = dogleg };
            if (!origin.CompleteCDTSDT(unit, 1.0, toolface) || unit.X == null || unit.Y == null || unit.Z == null)
            {
                return false;
            }
            dNorth = unit.X.Value;
            dEast = unit.Y.Value;
            dVertical = unit.Z.Value;
            return Numeric.IsDefined(dNorth) && Numeric.IsDefined(dEast) && Numeric.IsDefined(dVertical);
        }
        /// <summary>
        /// Solves the constant curvature and toolface target point problem directly, for the curvature,
        /// the toolface angle and the curve length that carry the curve from this survey to an offset of
        /// (<paramref name="dNorth"/>, <paramref name="dEast"/>, <paramref name="dVertical"/>).
        ///
        /// Only the toolface angle and the dogleg angle are searched, against the DIRECTION of the
        /// target, because the curvature is a pure scale which cannot influence that direction. The
        /// curvature then follows from a single division and the length from one more. Three unknowns
        /// therefore become two, and the scale, which is what makes the three way search ill
        /// conditioned, is removed from the iteration entirely.
        ///
        /// The seed comes from the planar target, which is closed form: for a target lying in the
        /// vertical plane through the start azimuth the ratio of horizontal to vertical offset is the
        /// tangent of the mean inclination, so the end inclination is 2*atan2(dHorizontal, dVertical)
        /// minus the start inclination exactly. That seed carries the great majority of targets in about
        /// three iterations; a small grid covers the rest.
        /// </summary>
        private bool TrySolveCDTTargetDirect(double inclinationStart, double azimuthStart, double dNorth, double dEast, double dVertical, double chord, out double curvature, out double toolface, out double curveLength)
        {
            curvature = 0.0;
            toolface = 0.0;
            curveLength = 0.0;
            if (!Numeric.GT(chord, 0.0))
            {
                return false;
            }

            double targetVerticalAngle = System.Math.Atan2(System.Math.Sqrt(dNorth * dNorth + dEast * dEast), dVertical);
            double targetBearing = WrapToPi(System.Math.Atan2(dEast, dNorth) - azimuthStart);

            // Closed form planar solution, expressed as a toolface angle and a dogleg angle.
            double planarInclinationEnd = System.Math.Max(1.0e-6, System.Math.Min(Numeric.PI - 1.0e-6, 2.0 * targetVerticalAngle - inclinationStart));
            double seedBuild = planarInclinationEnd - inclinationStart;
            double seedTurn = EffectiveSineCDT(inclinationStart, planarInclinationEnd) * 2.0 * targetBearing;
            double seedToolface = System.Math.Atan2(seedTurn, seedBuild);
            double seedDogleg = Hypot(seedBuild, seedTurn);
            if (!Numeric.GT(seedDogleg, 1.0e-6))
            {
                seedDogleg = 1.0e-3;
            }

            if (TrySolveCDTTargetFromSeed(inclinationStart, azimuthStart, chord, targetVerticalAngle, targetBearing,
                    seedToolface, seedDogleg, out curvature, out toolface, out curveLength, out double primaryResidual))
            {
                return true;
            }

            // Retrying from a grid only pays off where a solution exists but the planar seed sits in the
            // wrong basin. When the first attempt stalls far from the target direction there is usually
            // no such curve at all, and the grid would be pure waste.
            if (!(primaryResidual <= CDTTargetGridResidualGate))
            {
                return false;
            }

            foreach (double gridDogleg in CDTTargetSeedDoglegs)
            {
                foreach (double gridToolface in new double[]
                    { seedToolface, seedToolface + Numeric.PI / 2.0, seedToolface - Numeric.PI / 2.0, seedToolface + Numeric.PI })
                {
                    if (TrySolveCDTTargetFromSeed(inclinationStart, azimuthStart, chord, targetVerticalAngle, targetBearing,
                            gridToolface, gridDogleg, out curvature, out toolface, out curveLength, out _))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool TrySolveCDTTargetFromSeed(double inclinationStart, double azimuthStart, double chord, double targetVerticalAngle, double targetBearing, double seedToolface, double seedDogleg, out double curvature, out double toolface, out double curveLength, out double achievedResidual)
        {
            const double MinimumDogleg = 1.0e-7;
            double MaximumDogleg = 8.0 * Numeric.PI;
            const int MaxIterations = 60;
            const int MaxBacktracks = 40;

            curvature = 0.0;
            curveLength = 0.0;
            achievedResidual = double.PositiveInfinity;
            toolface = seedToolface;
            double dogleg = System.Math.Max(MinimumDogleg, System.Math.Min(MaximumDogleg, seedDogleg));

            bool Residual(double trialToolface, double trialDogleg, out double verticalResidual, out double bearingResidual)
            {
                verticalResidual = 0.0;
                bearingResidual = 0.0;
                if (!GetCDTUnitCurvatureDisplacement(inclinationStart, azimuthStart, trialToolface, trialDogleg,
                        out double fNorth, out double fEast, out double fVertical))
                {
                    return false;
                }
                verticalResidual = System.Math.Atan2(System.Math.Sqrt(fNorth * fNorth + fEast * fEast), fVertical) - targetVerticalAngle;
                bearingResidual = WrapToPi(System.Math.Atan2(fEast, fNorth) - azimuthStart - targetBearing);
                return true;
            }

            if (!Residual(toolface, dogleg, out double g0, out double g1))
            {
                return false;
            }
            double norm = Hypot(g0, g1);

            for (int iteration = 0; iteration < MaxIterations && norm > 1.0e-14; iteration++)
            {
                double hToolface = 1.0e-7 * System.Math.Max(1.0, System.Math.Abs(toolface));
                double hDogleg = 1.0e-7 * System.Math.Max(1.0, System.Math.Abs(dogleg));
                if (!Residual(toolface + hToolface, dogleg, out double p00, out double p10) ||
                    !Residual(toolface, System.Math.Min(MaximumDogleg, dogleg + hDogleg), out double p01, out double p11))
                {
                    return false;
                }
                double j00 = (p00 - g0) / hToolface;
                double j01 = (p01 - g0) / hDogleg;
                double j10 = (p10 - g1) / hToolface;
                double j11 = (p11 - g1) / hDogleg;
                double determinant = j00 * j11 - j01 * j10;
                if (!Numeric.IsDefined(determinant) || determinant == 0.0)
                {
                    break;
                }
                double stepToolface = -(j11 * g0 - j01 * g1) / determinant;
                double stepDogleg = -(j00 * g1 - j10 * g0) / determinant;

                double relaxation = 1.0;
                bool improved = false;
                for (int backtrack = 0; backtrack < MaxBacktracks; backtrack++)
                {
                    double trialToolface = toolface + relaxation * stepToolface;
                    double trialDogleg = System.Math.Max(MinimumDogleg, System.Math.Min(MaximumDogleg, dogleg + relaxation * stepDogleg));
                    if (Residual(trialToolface, trialDogleg, out double t0, out double t1))
                    {
                        double trialNorm = Hypot(t0, t1);
                        if (trialNorm < norm)
                        {
                            toolface = trialToolface;
                            dogleg = trialDogleg;
                            g0 = t0;
                            g1 = t1;
                            norm = trialNorm;
                            improved = true;
                            break;
                        }
                    }
                    relaxation *= 0.5;
                }
                if (!improved)
                {
                    break;
                }
            }

            achievedResidual = norm;
            if (!(norm <= 1.0e-9))
            {
                return false;
            }
            if (!GetCDTUnitCurvatureDisplacement(inclinationStart, azimuthStart, toolface, dogleg,
                    out double uNorth, out double uEast, out double uVertical))
            {
                return false;
            }
            double unitRange = System.Math.Sqrt(uNorth * uNorth + uEast * uEast + uVertical * uVertical);
            if (!Numeric.GT(unitRange, 0.0))
            {
                return false;
            }
            // The unit curvature curve spans unitRange for a dogleg of this size, and the target spans
            // chord, so the curvature is the ratio and the length follows from the dogleg angle.
            curvature = unitRange / chord;
            if (!Numeric.IsDefined(curvature) || !Numeric.GT(curvature, 0.0))
            {
                return false;
            }
            curveLength = dogleg / curvature;
            toolface = WrapToPi(toolface);
            return Numeric.IsDefined(curveLength) && Numeric.GT(curveLength, 0.0);
        }
        public bool CompleteCDTXYZ(CurvilinearPoint3D next)
            => CompleteCDTXYZInternal(next, out _, false);

        public bool CompleteCDTXYZ(CurvilinearPoint3D next, out double? curvature, out double? toolface)
        {
            bool result = CompleteCDTXYZInternal(next, out List<TrajectoryPoint3D> solutions, true);
            if (result && solutions.Count > 0 && solutions[0] != null)
            {
                curvature = solutions[0].Curvature;
                toolface = solutions[0].Toolface;
                next.Abscissa = solutions[0].Abscissa;
                next.Inclination = solutions[0].Inclination;
                next.Azimuth = solutions[0].Azimuth;
            }
            else
            {
                curvature = null;
                toolface = null;
            }
            return result;
        }
        public bool CompleteCASDT(CurvilinearPoint3D? next, double dls, double TF)
        {
            if ((next == null) ||
                IsUndefined() ||
                Inclination == null ||
                Azimuth == null ||
                Abscissa == null ||
                next.Abscissa == null ||
                Numeric.IsUndefined(next.Abscissa))
            {
                return false;
            }
            else if (Numeric.EQ(dls, 0.0))
            {
                CurvilinearPoint3D f = next;
                double dm = f.Abscissa.Value - Abscissa.Value;
                double si = System.Math.Sin(Inclination.Value);
                f.Inclination = Inclination;
                f.Azimuth = Azimuth;
                f.X = X + dm * si * System.Math.Cos(Azimuth.Value);
                f.Y = Y + dm * si * System.Math.Sin(Azimuth.Value);
                f.Z = Z + dm * System.Math.Cos(Inclination.Value);
                return true;
            }
            else if (Numeric.EQ(Inclination, 0.0))
            {
                CurvilinearPoint3D f = next;
                double dm = f.Abscissa.Value - Abscissa.Value;
                f.Inclination = dm * dls;
                f.Azimuth = TF;
                f.X = X + System.Math.Cos(f.Azimuth.Value) * (1 - System.Math.Cos(f.Inclination.Value)) / dls;
                f.Y = Y + System.Math.Sin(f.Azimuth.Value) * (1 - System.Math.Cos(f.Inclination.Value)) / dls;
                f.Z = Z + System.Math.Sin(f.Inclination.Value) / dls;
                return true;
            }
            else
            {
                CurvilinearPoint3D p1 = new();
                CurvilinearPoint3D p2 = new();
                p1.X = 0;
                p1.Y = 0;
                p1.Z = 0;
                p2.X = 0;
                p2.Y = 0;
                p2.Z = 0;
                CurvilinearPoint3D f = next;
                double dm = f.Abscissa.Value - Abscissa.Value;
                double teta = dm * dls;
                double st = System.Math.Sin(teta);
                double ct = System.Math.Cos(teta);
                p1.X = (1 - ct) / dls;
                p1.Y = 0.0;
                p1.Z = st / dls;
                Point3D r = TransCoord3RotsReversed(TF, p1);
                f.X = X + r.X;
                f.Y = Y + r.Y;
                f.Z = Z + r.Z;
                p1.X = st;
                p1.Y = 0.0;
                p1.Z = ct;
                r = TransCoord3RotsReversed(TF, p1);
                f.Inclination = Numeric.AcosEqual(r.Z);
                if (Numeric.EQ(r.Z, 1.0))
                {
                    f.Azimuth = Azimuth;
                }
                else
                {
                    if (r.X == null || r.Y == null || (Numeric.EQ(r.X, 0.0) && Numeric.EQ(r.Y, 0.0)))
                    {
                        f.Azimuth = null;
                    }
                    else
                    {
                        double teta2 = Numeric.AcosEqual(r.X.Value / System.Math.Sqrt(r.X.Value * r.X.Value + r.Y.Value * r.Y.Value));
                        if (r.Y >= 0.0)
                        {
                            f.Azimuth = teta2;
                        }
                        else
                        {
                            f.Azimuth = 2.0 * System.Math.PI - teta2;
                        }
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// Apply the constant curvature and toolface method from this survey over a curve of the given
        /// curvature and toolface angle, up to the abscissa carried by <paramref name="next"/>.
        ///
        /// The construction is the closed form of the governing equations
        ///     di/ds = bur,   da/ds = turn/sin(i),   bur = DLS*cos(TF),   turn = DLS*sin(TF),
        /// so that
        ///     i1 = i0 + bur*l,   a1 = a0 + turn*l/G(i0, i1)
        /// with G the effective sine of the segment. Because the inverse
        /// <see cref="CompleteCDTSIA(CurvilinearPoint3D)"/> uses the same effective sine, the two are
        /// exact inverses of one another to near machine precision. The turn parameter is signed, so a
        /// toolface angle on either side of the high side is honoured; there is no branch on the build up
        /// rate and no ln(tan/tan) difference to cancel.
        /// </summary>
        /// <param name="next">Receives the station. Its abscissa is the requested end of the curve.</param>
        /// <param name="DLS">Curvature of the curve, rad/m, non negative.</param>
        /// <param name="TF">Signed toolface angle, rad. Positive is a right hand turn off the high side.</param>
        /// <returns></returns>
        public bool CompleteCDTSDT(TrajectoryPoint3D? next, double DLS, double TF)
        {
            if ((next == null) ||
                IsUndefined() ||
                Z == null ||
                Inclination == null ||
                Azimuth == null ||
                Abscissa == null ||
                next.Abscissa == null ||
                Numeric.IsUndefined(next.Abscissa))
            {
                return false;
            }
            if (Numeric.EQ(DLS, 0))
            {
                return CompleteCASDT(next, DLS, TF);
            }
            // A constant curvature and toolface curve is undefined at exactly vertical, so a curve that
            // starts there has to be built as a circular arc.
            if (Numeric.EQ(Inclination.Value, 0.0) || Numeric.EQ(Inclination.Value, Numeric.PI))
            {
                return CompleteCASDT(next, DLS, TF);
            }

            double i0 = Inclination.Value;
            double a0 = Azimuth.Value;
            double l = next.Abscissa.Value - Abscissa.Value;
            double bur = DLS * System.Math.Cos(TF);
            double turn = DLS * System.Math.Sin(TF);

            // A curve of no length is the start station itself. Propagating it would divide by the
            // length in the attitude identity below.
            if (Numeric.EQ(l, 0.0))
            {
                next.X = X;
                next.Y = Y;
                next.Z = Z;
                next.Inclination = i0;
                next.Azimuth = a0;
                next.Curvature = DLS;
                next.Toolface = TF;
                next.BUR = bur;
                double sinStart = System.Math.Sin(i0);
                next.TUR = Numeric.EQ(sinStart, 0.0) ? 0.0 : turn / sinStart;
                next.VerticalSection = VerticalSection;
                return true;
            }

            // Where, if anywhere, does the curve reach vertical inside the interval? The inclination is
            // linear in the along hole distance, so the crossings are immediate.
            double lArc = l;
            double inclinationAtVertical = 0.0;
            bool reachesVertical = false;
            if (!Numeric.EQ(bur, 0.0))
            {
                double crossing = double.PositiveInfinity;
                double sToZero = -i0 / bur;
                double sToPi = (Numeric.PI - i0) / bur;
                if (sToZero > 0.0 && sToZero < crossing)
                {
                    crossing = sToZero;
                    inclinationAtVertical = 0.0;
                }
                if (sToPi > 0.0 && sToPi < crossing)
                {
                    crossing = sToPi;
                    inclinationAtVertical = Numeric.PI;
                }
                // The crossing and the requested length are worked out separately by the caller and by
                // this method, so comparing them alone is not enough: a caller that asks for exactly the
                // length reaching vertical, as the inclination combinations do, lands a rounding either
                // side of it. Landing a hair short is the dangerous side, because the effective sine
                // used below diverges at vertical and its argument, exactly one there, rounds past one
                // and gives back nothing. So the end inclination is tested against the vertical with the
                // same margin used elsewhere, and anything inside it is treated as arriving there.
                double inclinationEnd = i0 + bur * l;
                if (l > 0.0 &&
                    (crossing <= System.Math.Abs(l) ||
                     inclinationEnd <= InclinationMarginCDT ||
                     inclinationEnd >= Numeric.PI - InclinationMarginCDT))
                {
                    reachesVertical = true;
                    lArc = System.Math.Min(crossing, System.Math.Abs(l));
                }
            }

            // A curve with no turn is planar, its azimuth is constant, and it passes through vertical
            // perfectly cleanly: the inclination simply reflects and the azimuth flips by exactly pi,
            // which is the same attitude written the other way round. Nothing has to stop there.
            bool planar = Numeric.EQ(turn, 0.0);
            if (planar)
            {
                reachesVertical = false;
                lArc = l;
            }

            double i1Raw = i0 + bur * lArc;
            if (reachesVertical)
            {
                i1Raw = inclinationAtVertical;
            }

            double dx;
            double dy;
            double dz = lArc * System.Math.Cos(0.5 * (i0 + i1Raw)) * SinOverU(0.5 * (i1Raw - i0));
            double a1;

            if (reachesVertical)
            {
                // The azimuth diverges logarithmically at vertical: the curve spirals without bound and
                // never arrives at a finite azimuth. The position integral still converges, because
                // sin(i) damps the oscillation to nothing, so the vertical point itself is well defined.
                // Integrate it, then carry on straight ahead for whatever length remains, which is the
                // tangent continuation a curve has to use to leave a vertical section.
                GetCDTSpiralPosition(i0, a0, bur, turn, inclinationAtVertical, out dx, out dy);
                // Azimuth carries no meaning at exactly vertical. The circular arc construction keeps the
                // azimuth of the start station there, and this does the same.
                a1 = a0;
                double lTangent = l - lArc;
                dz += lTangent * System.Math.Cos(inclinationAtVertical);
            }
            else
            {
                double effectiveSine = EffectiveSineCDT(i0, i1Raw);
                a1 = a0 + turn * lArc / effectiveSine;
                double dInclination = i1Raw - i0;
                double dAzimuth = a1 - a0;
                int order = GetCDTIntegrationOrder(dInclination, dAzimuth);
                dx = GaussLegendreRule.Integrate(
                    s =>
                    {
                        GetCDTAttitude(s, i0, a0, dInclination, dAzimuth, lArc, effectiveSine, out double inclination, out double azimuth);
                        return System.Math.Sin(inclination) * System.Math.Cos(azimuth);
                    },
                    0.0, lArc, order);
                dy = GaussLegendreRule.Integrate(
                    s =>
                    {
                        GetCDTAttitude(s, i0, a0, dInclination, dAzimuth, lArc, effectiveSine, out double inclination, out double azimuth);
                        return System.Math.Sin(inclination) * System.Math.Sin(azimuth);
                    },
                    0.0, lArc, order);
            }

            next.X = X.Value + dx;
            next.Y = Y.Value + dy;
            next.Z = Z.Value + dz;
            next.Curvature = DLS;
            next.Toolface = TF;

            if (reachesVertical)
            {
                next.Inclination = inclinationAtVertical;
                next.Azimuth = a1;
                // The end station lies on the straight continuation, or exactly at vertical, and in both
                // cases the local rates there are zero. The repository already reports a zero turn rate
                // at a singular inclination.
                next.BUR = Numeric.EQ(l, lArc) ? bur : 0.0;
                next.TUR = 0.0;
            }
            else
            {
                next.Inclination = i1Raw;
                next.Azimuth = a1;
                CanonicalizeAttitude(next);
                next.BUR = bur;
                double sinEnd = System.Math.Sin(next.Inclination.Value);
                next.TUR = Numeric.EQ(sinEnd, 0.0) ? 0.0 : turn / sinEnd;
            }

            if (VerticalSection is not null && X is not null && Y is not null && next.X is not null && next.Y is not null)
            {
                next.VerticalSection = VerticalSection + System.Math.Sqrt((X.Value - next.X.Value) * (X.Value - next.X.Value) + (Y.Value - next.Y.Value) * (Y.Value - next.Y.Value));
            }

            return true;
        }
        /// <summary>
        /// Folds an attitude back into the canonical ranges: inclination into [0, PI], azimuth into
        /// [0, 2*PI). A negative inclination, or one past PI, is the same direction written the other way
        /// round, with the azimuth turned by PI. Shared by both curve constructions so that a station
        /// they report is expressed the same way whichever one produced it.
        /// </summary>
        private static void CanonicalizeAttitude(TrajectoryPoint3D point)
        {
            if (point.Inclination == null || point.Azimuth == null)
            {
                return;
            }
            double inclination = point.Inclination.Value;
            double azimuth = point.Azimuth.Value;
            while (inclination < 0.0)
            {
                inclination = -inclination;
                azimuth += Numeric.PI;
            }
            while (inclination > Numeric.PI)
            {
                inclination = 2.0 * Numeric.PI - inclination;
                azimuth += Numeric.PI;
            }
            double twoPi = 2.0 * Numeric.PI;
            azimuth %= twoPi;
            if (azimuth < 0.0)
            {
                azimuth += twoPi;
            }
            point.Inclination = inclination;
            point.Azimuth = azimuth;
        }
        /// <summary>
        /// North and East offsets of the portion of a turning constant curvature and toolface curve that
        /// runs from the start inclination all the way to vertical.
        ///
        /// In the along hole distance the azimuth of that portion oscillates without bound, so the
        /// integral cannot be taken in that variable. Substituting u = ln tan(i/2), which is the natural
        /// variable of the curve, gives
        ///     a(u)   = a0 + (turn/bur)*(u - u0),        linear in u
        ///     sin(i) = sech(u)
        ///     ds     = sin(i) du / bur
        /// so that the offsets become
        ///     dN = (1/bur) * integral of sech(u)^2 * cos(a(u)) du
        /// over u from u0 to minus or plus infinity. The amplitude sech(u)^2 decays like 4*exp(-2|u|),
        /// so the tail past |u| = UMax contributes less than 2*exp(-2*UMax)/|bur| and the oscillation,
        /// being uniform in u, is resolved by panels of a fixed number of periods each.
        /// </summary>
        private static void GetCDTSpiralPosition(double inclinationStart, double azimuthStart, double bur, double turn, double inclinationAtVertical, out double dNorth, out double dEast)
        {
            // sech(u)^2 below 4*exp(-2*25), so the neglected tail is far below a nanometre for any
            // curvature that reaches vertical within a real interval.
            const double UMax = 25.0;
            const int PanelOrder = 12;

            double u0 = System.Math.Log(System.Math.Tan(0.5 * inclinationStart));
            double uEnd = Numeric.EQ(inclinationAtVertical, 0.0) ? -UMax : UMax;
            if (uEnd < u0 && u0 < -UMax)
            {
                uEnd = u0;
            }
            if (uEnd > u0 && u0 > UMax)
            {
                uEnd = u0;
            }

            double slope = turn / bur;
            double span = System.Math.Abs(uEnd - u0);
            // Two panels per period of the oscillation, and never fewer than eight over the whole span.
            double periods = System.Math.Abs(slope) * span / (2.0 * Numeric.PI);
            int panels = (int)System.Math.Ceiling(2.0 * periods) + 8;
            panels = System.Math.Max(8, System.Math.Min(4096, panels));

            double north = 0.0;
            double east = 0.0;
            double step = (uEnd - u0) / panels;
            for (int p = 0; p < panels; p++)
            {
                double lo = u0 + p * step;
                double hi = lo + step;
                north += GaussLegendreRule.Integrate(
                    u =>
                    {
                        double sech = 1.0 / System.Math.Cosh(u);
                        return sech * sech * System.Math.Cos(azimuthStart + slope * (u - u0));
                    },
                    System.Math.Min(lo, hi), System.Math.Max(lo, hi), PanelOrder) * System.Math.Sign(step);
                east += GaussLegendreRule.Integrate(
                    u =>
                    {
                        double sech = 1.0 / System.Math.Cosh(u);
                        return sech * sech * System.Math.Sin(azimuthStart + slope * (u - u0));
                    },
                    System.Math.Min(lo, hi), System.Math.Max(lo, hi), PanelOrder) * System.Math.Sign(step);
            }

            dNorth = north / bur;
            dEast = east / bur;
        }
        /// <summary>
        /// Calculate the coordinates of the result survey station by interpolation between this survey station and the next survey station, for a given curvilinear abscissa
        /// </summary>
        /// <param name="next"></param>
        /// <param name="abscissa"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool InterpolateAtAbscissaCA(ICurvilinear3D next, double abscissa, ICurvilinear3D result)
        {
            if (next == null || result == null ||
                X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null ||
                next.X == null || next.Y == null || next.Z == null || next.Inclination == null || next.Azimuth == null || next.Abscissa == null)
            {
                return false;
            }
            // Either end of the interval is the station itself, answered exactly rather than by
            // propagating. These are tested before the range check because densifying a segment by
            // stepping abscissa = start + length*j/n lands the last step a few units in the last place
            // beyond the end station, and rejecting that outright would fail on the most natural way of
            // calling this.
            if (Numeric.EQ(abscissa, Abscissa.Value))
            {
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.Abscissa = Abscissa;
                result.Inclination = Inclination;
                result.Azimuth = Azimuth;
                return true;
            }
            if (Numeric.EQ(abscissa, next.Abscissa.Value))
            {
                result.X = next.X;
                result.Y = next.Y;
                result.Z = next.Z;
                result.Abscissa = next.Abscissa;
                result.Inclination = next.Inclination;
                result.Azimuth = next.Azimuth;
                return true;
            }

            if (!Numeric.IsBetween(abscissa, (double)Abscissa, (double)next.Abscissa))
            {
                return false;
            }
            result.Abscissa = abscissa;
            double x1 = (double)X;
            double y1 = (double)Y;
            double z1 = (double)Z;
            double i1 = (double)Inclination;
            double a1 = (double)Azimuth;
            double s1 = (double)Abscissa;
            double x2 = (double)next.X;
            double y2 = (double)next.Y;
            double z2 = (double)next.Z;
            double i2 = (double)next.Inclination;
            double a2 = (double)next.Azimuth;
            double s2 = (double)next.Abscissa;
            double si1 = System.Math.Sin(i1);
            double si2 = System.Math.Sin(i2);
            double DL = DoglegAngle(i1, a1, i2, a2);
            double DM = s2 - s1;
            if (Numeric.EQ(DM, 0))
            {
                if (Numeric.EQ(s1, abscissa))
                {
                    result.X = X;
                    result.Y = Y;
                    result.Z = Z;
                    result.Abscissa = Abscissa;
                    result.Inclination = Inclination;
                    result.Azimuth = Azimuth;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                double dx = x2 - x1;
                double dy = y2 - y1;
                double ci1 = System.Math.Cos(i1);
                double ca1 = System.Math.Cos(a1);
                double sa1 = System.Math.Sin(a1);
                double ci2 = System.Math.Cos(i2);
                double numerator = si2 * System.Math.Sin(a2 - a1);
                double denominator = si2 * ci1 * System.Math.Cos(a2 - a1) - si1 * ci2;
                double tf = System.Math.Atan2(numerator, denominator);
                double dls = DL / DM;
                double dm = abscissa - s1;
                if (Numeric.EQ(dls, 0))
                {
                    result.Inclination = i1;
                    result.Azimuth = a1;
                    result.X = x1 + dm * ca1 * si1;
                    result.Y = y1 + dm * sa1 * si1;
                    result.Z = z1 + dm * ci1;
                    return true;
                }
                else
                {
                    if (Numeric.EQ(i1, 0))
                    {
                        double incl = dm * dls;
                        result.Inclination = incl;
                        double az = System.Math.Atan2(dy, dx);
                        result.Azimuth = az;
                        double ci = System.Math.Cos(incl);
                        result.X = x1 + System.Math.Cos(az) * (1 - ci) / dls;
                        result.Y = y1 + System.Math.Sin(az) * (1 - ci) / dls;
                        result.Z = z1 + System.Math.Sin(incl) / dls;
                        return true;
                    }
                    else
                    {
                        if (Numeric.IsUndefined(tf))
                        {
                            return false;
                        }
                        double theta = dm * dls;
                        double deltaXp = (1 - System.Math.Cos(theta)) / dls;
                        double deltaZp = System.Math.Sin(theta) / dls;
                        double ctf = System.Math.Cos(tf);
                        double stf = System.Math.Sin(tf);
                        result.X = x1 + deltaXp * (ctf * ca1 * ci1 - sa1 * stf) + deltaZp * si1 * ca1;
                        result.Y = y1 + deltaXp * (stf * ca1 + ctf * sa1 * ci1) + deltaZp * si1 * sa1;
                        result.Z = z1 - deltaXp * ctf * si1 + deltaZp * ci1;
                        double deltaXt = System.Math.Sin(theta);
                        double deltaZt = System.Math.Cos(theta);
                        double xt = deltaXt * (ctf * ca1 * ci1 - sa1 * stf) + deltaZt * si1 * ca1;
                        double yt = deltaXt * (stf * ca1 + ctf * sa1 * ci1) + deltaZt * si1 * sa1;
                        double zt = -deltaXt * ctf * si1 + deltaZt * ci1;
                        result.Inclination = Numeric.AcosEqual(zt);
                        // The guard here is on the tangent, not on the height of the start station: a
                        // vertical tangent is the case with no defined azimuth. Testing the Z coordinate
                        // left a genuinely vertical tangent to fall through to an arc tangent of zero
                        // over zero, and forced the azimuth of any station that happened to sit one
                        // metre below its reference.
                        double horizontal = System.Math.Sqrt(xt * xt + yt * yt);
                        if (Numeric.EQ(horizontal, 0.0))
                        {
                            result.Azimuth = a1;
                        }
                        else
                        {
                            double azimuth = System.Math.Atan2(yt, xt);
                            result.Azimuth = azimuth < 0.0 ? azimuth + 2.0 * Numeric.PI : azimuth;
                        }
                        // The curvature and the toolface angle at the start belong to the arc, so every
                        // point taken from it carries the same pair.
                        if (result is TrajectoryPoint3D interpolatedTarget)
                        {
                            interpolatedTarget.Curvature = dls;
                            interpolatedTarget.Toolface = tf;
                        }
                        return true;
                    }
                }
            }
        }
        public bool InterpolateAtAbscissaCDT(ICurvilinear3D next, double abscissa, ICurvilinear3D result)
        {
            if (next == null || result == null ||
                X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null ||
                next.X == null || next.Y == null || next.Z == null || next.Inclination == null || next.Azimuth == null || next.Abscissa == null)
            {
                return false;
            }
            // Either end of the interval is the station itself, answered exactly rather than by
            // propagating, so that a densified track always carries its own stations unaltered.
            //
            // These are deliberately tested before the range check. Densifying a segment by stepping
            // abscissa = start + length*j/n lands the last step a few units in the last place beyond the
            // end station, and rejecting that outright would fail on the most natural way of calling
            // this. Matching an end station within tolerance snaps to it instead.
            if (Numeric.EQ(abscissa, Abscissa.Value))
            {
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.Abscissa = Abscissa;
                result.Inclination = Inclination;
                result.Azimuth = Azimuth;
                CopyCDTLocalParameters(this, result);
                return true;
            }
            if (Numeric.EQ(abscissa, next.Abscissa.Value))
            {
                result.X = next.X;
                result.Y = next.Y;
                result.Z = next.Z;
                result.Abscissa = next.Abscissa;
                result.Inclination = next.Inclination;
                result.Azimuth = next.Azimuth;
                CopyCDTLocalParameters(next as TrajectoryPoint3D, result);
                return true;
            }

            if (!Numeric.IsBetween(abscissa, (double)Abscissa, (double)next.Abscissa))
            {
                return false;
            }

            if (!GetCDTSegmentParameters(next, out double curvature, out double toolface))
            {
                return false;
            }

            TrajectoryPoint3D interpolated = new()
            {
                Abscissa = abscissa
            };
            if (!CompleteCDTSDT(interpolated, curvature, toolface))
            {
                return false;
            }

            result.X = interpolated.X;
            result.Y = interpolated.Y;
            result.Z = interpolated.Z;
            result.Abscissa = interpolated.Abscissa;
            result.Inclination = interpolated.Inclination;
            result.Azimuth = interpolated.Azimuth;
            CopyCDTLocalParameters(interpolated, result);
            return true;
        }
        /// <summary>
        /// Carries the curve defining parameters of a constant curvature and toolface point onto an
        /// interpolation result, when that result is able to hold them. The propagation already works
        /// them out exactly at the point in question, including reporting no build up and no turn on the
        /// straight continuation past a vertical crossing, so they are copied rather than recomputed.
        /// </summary>
        private static void CopyCDTLocalParameters(TrajectoryPoint3D? source, ICurvilinear3D destination)
        {
            if (source == null || destination is not TrajectoryPoint3D target)
            {
                return;
            }
            target.Curvature = source.Curvature;
            target.Toolface = source.Toolface;
            target.BUR = source.BUR;
            target.TUR = source.TUR;
            target.VerticalSection = source.VerticalSection;
        }
        /// <summary>
        /// The curvature and toolface angle of the constant curvature and toolface segment running from
        /// this survey to <paramref name="next"/>.
        ///
        /// They are taken from <paramref name="next"/> when it carries them, so that a segment built on a
        /// nonzero azimuth branch, or one that runs into vertical and continues along its tangent, keeps
        /// the definition it was created with. Otherwise they are derived from the two stations by the
        /// closed form inverse, which means interpolation no longer depends on a caller having populated
        /// those properties: two stations and the distance between them define the curve on their own.
        /// </summary>
        protected bool GetCDTSegmentParameters(ICurvilinear3D next, out double curvature, out double toolface)
        {
            curvature = 0.0;
            toolface = 0.0;
            if (next is TrajectoryPoint3D carried && carried.Curvature != null && carried.Toolface != null)
            {
                curvature = carried.Curvature.Value;
                toolface = carried.Toolface.Value;
                return Numeric.IsDefined(curvature) && Numeric.IsDefined(toolface);
            }
            if (next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }
            TrajectoryPoint3D derived = new TrajectoryPoint3D()
            {
                Abscissa = next.Abscissa,
                Inclination = next.Inclination,
                Azimuth = next.Azimuth
            };
            if (!CompleteCDTSIA(derived) || derived.Curvature == null || derived.Toolface == null)
            {
                return false;
            }
            curvature = derived.Curvature.Value;
            toolface = derived.Toolface.Value;
            return true;
        }
        public bool InterpolateAtAbscissaBT(ICurvilinear3D next, double abscissa, ICurvilinear3D result)
        {
            if (next == null || result == null ||
                X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null ||
                next.X == null || next.Y == null || next.Z == null || next.Inclination == null || next.Azimuth == null || next.Abscissa == null)
            {
                return false;
            }
            // Either end of the interval is the station itself, answered exactly rather than by
            // propagating. These are tested before the range check because densifying a segment by
            // stepping abscissa = start + length*j/n lands the last step a few units in the last place
            // beyond the end station, and rejecting that outright would fail on the most natural way of
            // calling this.
            if (Numeric.EQ(abscissa, Abscissa.Value))
            {
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.Abscissa = Abscissa;
                result.Inclination = Inclination;
                result.Azimuth = Azimuth;
                return true;
            }
            if (Numeric.EQ(abscissa, next.Abscissa.Value))
            {
                result.X = next.X;
                result.Y = next.Y;
                result.Z = next.Z;
                result.Abscissa = next.Abscissa;
                result.Inclination = next.Inclination;
                result.Azimuth = next.Azimuth;
                return true;
            }

            if (!Numeric.IsBetween(abscissa, (double)Abscissa, (double)next.Abscissa))
            {
                return false;
            }

            // A segment of no length was already answered by the two checks above: the abscissa asked
            // for has to equal the single station for the range check to have passed at all.
            double s1 = Abscissa.Value;
            result.Abscissa = abscissa;

            if (!GetBTSegmentRates(next, out double bur, out double tur))
            {
                return false;
            }

            double dm = abscissa - s1;
            TrajectoryPoint3D interpolated = new TrajectoryPoint3D()
            {
                Abscissa = abscissa,
                Inclination = Inclination.Value + bur * dm,
                Azimuth = Azimuth.Value + tur * dm
            };
            // The attitude just built up is continuous by construction, so it must not be wrapped again.
            if (!CompleteBTSIA(interpolated, 0, false))
            {
                return false;
            }

            // The attitude was built up continuously, which can leave the azimuth outside a single
            // revolution. Report it the way every other station in this class is reported.
            CanonicalizeAttitude(interpolated);

            result.X = interpolated.X;
            result.Y = interpolated.Y;
            result.Z = interpolated.Z;
            result.Inclination = interpolated.Inclination;
            result.Azimuth = interpolated.Azimuth;
            CopyCDTLocalParameters(interpolated, result);
            return true;
        }
        /// <summary>
        /// The build up and turn rates of the constant build and turn segment running from this survey
        /// to <paramref name="next"/>.
        ///
        /// They are taken from <paramref name="next"/> when it carries them, so that interpolation uses
        /// exactly the rates the segment was built with. Otherwise they are derived by running the
        /// segment construction itself, rather than by differencing the two stations here, so that the
        /// two cannot disagree on how the attitude of the far station is to be read: the construction
        /// folds an inclination back into its canonical range and takes the azimuth the short way round,
        /// and an interpolation that differenced the raw values would describe a different curve.
        /// </summary>
        protected bool GetBTSegmentRates(ICurvilinear3D next, out double bur, out double tur)
        {
            bur = 0.0;
            tur = 0.0;
            if (next is TrajectoryPoint3D carried && carried.BUR != null && carried.TUR != null)
            {
                bur = carried.BUR.Value;
                tur = carried.TUR.Value;
                return Numeric.IsDefined(bur) && Numeric.IsDefined(tur);
            }
            if (next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }
            TrajectoryPoint3D derived = new TrajectoryPoint3D()
            {
                Abscissa = next.Abscissa,
                Inclination = next.Inclination,
                Azimuth = next.Azimuth
            };
            if (!CompleteBTSIA(derived) || derived.BUR == null || derived.TUR == null)
            {
                return false;
            }
            bur = derived.BUR.Value;
            tur = derived.TUR.Value;
            return true;
        }

        protected bool CompleteCDTSIAInternal(CurvilinearPoint3D next, int branch, bool unwrapAzimuth, out List<TrajectoryPoint3D> solutions, bool collectSolutions)
        {
            solutions = new List<TrajectoryPoint3D>();
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null || next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }

            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;
            double i2 = next.Inclination.Value;
            double a2 = next.Azimuth.Value;
            double s2 = next.Abscissa.Value;
            double dm = s2 - s1;

            static double NormalizeAzimuth(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0.0)
                {
                    angle += twoPi;
                }
                return angle;
            }
            static void CanonicalizeInclinationAzimuth(ref double inclination, ref double azimuth)
            {
                while (inclination < 0.0)
                {
                    inclination = -inclination;
                    azimuth += Numeric.PI;
                }
                while (inclination > Numeric.PI)
                {
                    inclination = 2.0 * Numeric.PI - inclination;
                    azimuth += Numeric.PI;
                }
                azimuth = NormalizeAzimuth(azimuth);
            }

            // A caller that already carries a continuous azimuth supplies a consistent attitude, and
            // folding it back into the canonical ranges would destroy the very continuity it carries.
            if (unwrapAzimuth)
            {
                CanonicalizeInclinationAzimuth(ref i2, ref a2);
            }

            // A constant curvature and toolface curve is undefined at exactly vertical, because the
            // effective sine integral diverges there. Fall back on the circular arc construction, which is
            // well defined, as the caller must anyway do to leave or enter a vertical section.
            if (Numeric.EQ(i1, 0.0) || Numeric.EQ(i1, Numeric.PI) || Numeric.EQ(i2, 0.0) || Numeric.EQ(i2, Numeric.PI))
            {
                bool okCircularArc = CompleteCASIA(next);
                if (okCircularArc && next is TrajectoryPoint3D verticalTarget)
                {
                    // CompleteCASIA only places the station, so fill in the curve defining parameters
                    // here, on the convention the rest of the class already uses at a singular
                    // inclination: the turn rate is zero and the segment is a pure build or drop.
                    double verticalBur = Numeric.EQ(dm, 0.0) ? 0.0 : (i2 - i1) / dm;
                    verticalTarget.BUR = verticalBur;
                    verticalTarget.TUR = 0.0;
                    verticalTarget.Curvature = System.Math.Abs(verticalBur);
                    verticalTarget.Toolface = Numeric.GE(verticalBur, 0.0) ? 0.0 : Numeric.PI;
                    if (VerticalSection is not null && X is not null && Y is not null && next.X is not null && next.Y is not null)
                    {
                        verticalTarget.VerticalSection = VerticalSection + System.Math.Sqrt((X.Value - next.X.Value) * (X.Value - next.X.Value) + (Y.Value - next.Y.Value) * (Y.Value - next.Y.Value));
                    }
                }
                if (okCircularArc && collectSolutions)
                {
                    solutions.Add(new TrajectoryPoint3D()
                    {
                        X = next.X,
                        Y = next.Y,
                        Z = next.Z,
                        Abscissa = next.Abscissa,
                        Inclination = next.Inclination,
                        Azimuth = next.Azimuth
                    });
                }
                return okCircularArc;
            }

            if (Numeric.EQ(dm, 0))
            {
                next.X = X;
                next.Y = Y;
                next.Z = Z;
                next.Inclination = i1;
                next.Azimuth = a1;
                next.Abscissa = s2;
                return true;
            }

            // The curve defining parameters are closed form. With the effective sine G of the segment,
            //     bur = di/dm,   turn = G*da/dm,   dls = hypot(bur, turn),   toolface = atan2(turn, bur).
            // There is no iteration, no root finding, no case branching on the build up rate and no trial
            // and error test for the sign of the toolface.
            double dInclination = i2 - i1;
            double dAzimuth = unwrapAzimuth ? WrapToPi(a2 - a1) : a2 - a1;
            dAzimuth += 2.0 * Numeric.PI * branch;

            double effectiveSine = EffectiveSineCDT(i1, i2);
            double bur = dInclination / dm;
            double turn = effectiveSine * dAzimuth / dm;
            double dls = Hypot(bur, turn);
            double toolface = System.Math.Atan2(turn, bur);
            if (!Numeric.IsDefined(dls) || !Numeric.IsDefined(toolface))
            {
                return false;
            }

            // The vertical is closed form. Written as dm*cos(iBar)*sinc(di/2) rather than
            // (sin i2 - sin i1)/bur it has no build up rate in the denominator, so the zero build up limit
            // dm*cos(i1) comes out automatically.
            double inclinationBar = 0.5 * (i1 + i2);
            double dz = dm * System.Math.Cos(inclinationBar) * SinOverU(0.5 * dInclination);

            // North and East have no closed form. The integrand is analytic on the closed interval, so
            // Gauss-Legendre converges spectrally where composite Simpson only converges at fourth order;
            // a fixed low order Simpson rule reaches decimetre errors on strongly turning segments.
            int order = GetCDTIntegrationOrder(dInclination, dAzimuth);
            double dx = GaussLegendreRule.Integrate(
                s =>
                {
                    GetCDTAttitude(s, i1, a1, dInclination, dAzimuth, dm, effectiveSine, out double inclination, out double azimuth);
                    return System.Math.Sin(inclination) * System.Math.Cos(azimuth);
                },
                0.0, dm, order);
            double dy = GaussLegendreRule.Integrate(
                s =>
                {
                    GetCDTAttitude(s, i1, a1, dInclination, dAzimuth, dm, effectiveSine, out double inclination, out double azimuth);
                    return System.Math.Sin(inclination) * System.Math.Sin(azimuth);
                },
                0.0, dm, order);

            next.X = X.Value + dx;
            next.Y = Y.Value + dy;
            next.Z = Z.Value + dz;
            next.Abscissa = s2;
            next.Inclination = i2;
            next.Azimuth = unwrapAzimuth ? a2 : a1 + dAzimuth;

            double? verticalSection = null;
            if (VerticalSection is not null)
            {
                verticalSection = VerticalSection + System.Math.Sqrt(dx * dx + dy * dy);
            }
            if (next is TrajectoryPoint3D target)
            {
                target.Curvature = dls;
                target.Toolface = toolface;
                target.BUR = bur;
                target.TUR = turn / System.Math.Sin(i2);
                target.VerticalSection = verticalSection;
            }
            if (collectSolutions)
            {
                solutions.Add(new TrajectoryPoint3D()
                {
                    X = next.X,
                    Y = next.Y,
                    Z = next.Z,
                    Abscissa = next.Abscissa,
                    Inclination = next.Inclination,
                    Azimuth = next.Azimuth,
                    Curvature = dls,
                    Toolface = toolface,
                    BUR = bur,
                    TUR = turn / System.Math.Sin(i2),
                    VerticalSection = verticalSection
                });
            }
            return true;
        }

        protected bool CompleteCDTXYZInternal(CurvilinearPoint3D next, out List<TrajectoryPoint3D> solutions, bool collectSolutions)
        {
            List<TrajectoryPoint3D> solutionList = [];
            solutions = solutionList;
            if (next == null || X == null || Y == null || Z == null || next.X == null || next.Y == null || next.Z == null || Inclination == null || Azimuth == null || Abscissa == null)
            {
                return false;
            }

            double x1 = X.Value;
            double y1 = Y.Value;
            double z1 = Z.Value;
            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;
            double targetX = next.X.Value;
            double targetY = next.Y.Value;
            double targetZ = next.Z.Value;
            double dx = targetX - x1;
            double dy = targetY - y1;
            double dz = targetZ - z1;
            double chord = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (Numeric.EQ(chord, 0.0))
            {
                next.Abscissa = s1;
                next.Inclination = i1;
                next.Azimuth = a1;
                double curvature = 0.0;
                double? toolface = null;
                if (collectSolutions)
                {
                    solutionList.Add(new TrajectoryPoint3D()
                    {
                        Abscissa = s1,
                        Inclination = i1,
                        Azimuth = a1,
                        X = targetX,
                        Y = targetY,
                        Z = targetZ,
                        Curvature = curvature,
                        Toolface = toolface
                    });
                }
                return true;
            }

            double tanX = System.Math.Sin(i1) * System.Math.Cos(a1);
            double tanY = System.Math.Sin(i1) * System.Math.Sin(a1);
            double tanZ = System.Math.Cos(i1);
            double forwardProjection = dx * tanX + dy * tanY + dz * tanZ;
            if (System.Math.Abs(chord - forwardProjection) <= 1e-8 * System.Math.Max(1.0, chord))
            {
                next.Abscissa = s1 + chord;
                next.Inclination = i1;
                next.Azimuth = a1;
                double curvature = 0.0;
                double? toolface = null;
                // The segment is straight, so its curve defining parameters are known exactly rather
                // than needing to be estimated afterwards. The toolface angle of a straight segment is
                // genuinely undefined and stays null.
                if (next is TrajectoryPoint3D straightTarget)
                {
                    straightTarget.Curvature = curvature;
                    straightTarget.Toolface = toolface;
                    straightTarget.BUR = 0.0;
                    straightTarget.TUR = 0.0;
                    if (VerticalSection is not null)
                    {
                        straightTarget.VerticalSection = VerticalSection + System.Math.Sqrt(dx * dx + dy * dy);
                    }
                }
                if (collectSolutions)
                {
                    solutionList.Add(new TrajectoryPoint3D()
                    {
                        Abscissa = s1 + chord,
                        Inclination = i1,
                        Azimuth = a1,
                        X = targetX,
                        Y = targetY,
                        Z = targetZ,
                        Curvature = curvature,
                        Toolface = toolface
                    });
                }
                return true;
            }


            // Direct reduction of the target point problem, tried before the general search below.
            //
            // Writing the curve in terms of its end inclination and its azimuth change rather than its
            // curvature and toolface angle, the attitude at a fraction f of the curve depends only on
            // (i1, a1, endInclination, dAzimuth), never on the length. The displacement is therefore
            //     D = curveLength * F(i1, a1, endInclination, dAzimuth),   |F| <= 1
            // so the length drops out of the DIRECTION of D entirely. Two unknowns are matched against
            // the target direction and the length then follows from one division, instead of searching
            // all three at once. The parametrisation has no singularity where the build up rate
            // vanishes, which the curvature and toolface pair does.
            //
            // The seed is the planar solution, which is closed form: for a target in the vertical plane
            // through the start azimuth the ratio of horizontal to vertical offset is tan of the mean
            // inclination, so endInclination = 2*atan2(dHorizontal, dVertical) - i1 exactly.
            if (Numeric.GT(chord, 0.0) && !Numeric.EQ(i1, 0.0) && !Numeric.EQ(i1, Numeric.PI))
            {
                if (TrySolveCDTTargetDirect(i1, a1, dx, dy, dz, chord,
                        out double solvedCurvature, out double solvedToolface, out double solvedLength))
                {
                    TrajectoryPoint3D direct = new TrajectoryPoint3D() { Abscissa = s1 + solvedLength };
                    // Re-run the closed form construction on the solved parameters. This both confirms
                    // that the curve really lands on the target and fills in the curve defining
                    // parameters exactly, with no finite differences anywhere.
                    //
                    // Re-run the forward construction on the solved parameters. The curve is defined
                    // by its curvature and toolface angle, which are unambiguous whatever azimuth branch
                    // the end station happens to fall on, so this both confirms that it lands on the
                    // target and fills in the station and the curve defining parameters exactly.
                    TrajectoryPoint3D verify = new TrajectoryPoint3D(this) { Abscissa = s1 };
                    verify.X = x1;
                    verify.Y = y1;
                    verify.Z = z1;
                    if (verify.CompleteCDTSDT(direct, solvedCurvature, solvedToolface) &&
                        direct.X != null && direct.Y != null && direct.Z != null &&
                        System.Math.Abs(direct.X.Value - targetX) <= 1e-7 * System.Math.Max(1.0, chord) &&
                        System.Math.Abs(direct.Y.Value - targetY) <= 1e-7 * System.Math.Max(1.0, chord) &&
                        System.Math.Abs(direct.Z.Value - targetZ) <= 1e-7 * System.Math.Max(1.0, chord))
                    {
                        next.Abscissa = direct.Abscissa;
                        next.Inclination = direct.Inclination;
                        next.Azimuth = direct.Azimuth;
                        if (next is TrajectoryPoint3D directTarget)
                        {
                            directTarget.Curvature = direct.Curvature;
                            directTarget.Toolface = direct.Toolface;
                            directTarget.BUR = direct.BUR;
                            directTarget.TUR = direct.TUR;
                            directTarget.VerticalSection = direct.VerticalSection;
                        }
                        if (collectSolutions)
                        {
                            solutionList.Add(new TrajectoryPoint3D()
                            {
                                Abscissa = direct.Abscissa,
                                Inclination = direct.Inclination,
                                Azimuth = direct.Azimuth,
                                X = targetX,
                                Y = targetY,
                                Z = targetZ,
                                Curvature = direct.Curvature,
                                Toolface = direct.Toolface,
                                BUR = direct.BUR,
                                TUR = direct.TUR,
                                VerticalSection = direct.VerticalSection
                            });
                        }
                        return true;
                    }
                }
            }
            // No constant curvature and toolface curve reaches the target. The direct reduction above
            // covers every curve the forward construction can produce, including one that runs into
            // vertical and continues along its tangent, so reaching this point means the target is not
            // reachable rather than that the search gave up.
            return false;
        }
    }
}
