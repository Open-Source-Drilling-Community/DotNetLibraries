using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// a section based on a constant curvature and toolface curve in between the start and end of the section
    /// </summary>
    [Serializable]
    public class ConstantCurvatureAndToolfaceArcSection : ArcSection
    {
        public NonLocalizedConstantCurvatureAndToolfaceCurve CTCCurve { get; set; } = new NonLocalizedConstantCurvatureAndToolfaceCurve();
        /// <summary>
        /// 
        /// </summary>
        public override NonLocalizedCurve Curve
        {
            get => CTCCurve;
            set
            {
                if (value is NonLocalizedConstantCurvatureAndToolfaceCurve)
                {
                    CTCCurve = (NonLocalizedConstantCurvatureAndToolfaceCurve)value;
                }
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public ConstantCurvatureAndToolfaceArcSection()
        {
        }
        /// <summary>
        /// initialise the section from two stations, whose values are copied in, as the circular arc
        /// section does. The section keeps its own stations, so a caller that chains sections gets a
        /// trajectory whose stations carry equal values rather than one holding the same objects twice.
        /// </summary>
        public ConstantCurvatureAndToolfaceArcSection(TrajectoryPoint3D start, TrajectoryPoint3D end)
        {
            if (start != null)
            {
                Start.Set(start);
            }
            if (end != null)
            {
                End.Set(end);
            }
        }
        public override bool Calculate()
        {
            if (Numeric.IsDefined(CTCCurve.Curvature) &&
                Numeric.IsDefined(CTCCurve.Toolface) &&
                Numeric.IsDefined(CTCCurve.Length))
            {
                return CalculateLDT();
            }
            else if (Numeric.IsDefined(CTCCurve.Curvature) &&
                     Numeric.IsDefined(CTCCurve.Toolface) &&
                     Numeric.IsDefined(End.Abscissa))
            {
                return CalculateSDT();
            }
            else if (Numeric.IsDefined(CTCCurve.Curvature) &&
                     Numeric.IsDefined(CTCCurve.Toolface) &&
                     Numeric.IsDefined(End.Inclination))
            {
                return CalculateDTI();
            }
            else if (Numeric.IsDefined(CTCCurve.Curvature) &&
                     Numeric.IsDefined(CTCCurve.Toolface) &&
                     Numeric.IsDefined(End.Z))
            {
                return CalculateDTZ();
            }
            else if (Numeric.IsDefined(End.Abscissa) &&
                     Numeric.IsDefined(End.Inclination) &&
                     Numeric.IsDefined(End.Azimuth))
            {
                return CalculateSIA();
            }
            else if (Numeric.IsDefined(CTCCurve.Length) &&
                     Numeric.IsDefined(End.Inclination) &&
                     Numeric.IsDefined(End.Azimuth))
            {
                return CalculateLIA();
            }
            else if (Numeric.IsDefined(CTCCurve.Curvature) &&
                     Numeric.IsDefined(CTCCurve.Toolface) &&
                     Numeric.IsDefined(End.Azimuth))
            {
                return CalculateDTA();
            }
            else if (Numeric.IsDefined(CTCCurve.Curvature) &&
                     Numeric.IsDefined(End.Inclination) &&
                     Numeric.IsDefined(End.Azimuth))
            {
                return CalculateDIA();
            }
            else if (Numeric.IsDefined(End.X) &&
                     Numeric.IsDefined(End.Y) &&
                     Numeric.IsDefined(End.Z))
            {
                return CalculateXYZ();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// The end measured depth, inclination and azimuth are known. Two stations and the distance
        /// between them define the curve outright, so the curvature, the toolface angle and the length
        /// all follow in closed form.
        /// </summary>
        public bool CalculateSIA()
        {
            if (Numeric.IsDefined(End.Abscissa) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                if (Numeric.EQ(Start.Abscissa, End.Abscissa))
                {
                    return CalculateID();
                }
                if (!Start.CompleteCDTSIA(End))
                {
                    return false;
                }
                if (End.Curvature == null || End.Toolface == null)
                {
                    return false;
                }
                CTCCurve.Curvature = End.Curvature.Value;
                CTCCurve.Toolface = End.Toolface.Value;
                CTCCurve.Length = End.Abscissa - Start.Abscissa;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// The length, end inclination and end azimuth are known; the measured depth follows and the
        /// problem becomes the one above.
        /// </summary>
        public bool CalculateLIA()
        {
            if (Numeric.IsDefined(CTCCurve.Length) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                End.Abscissa = Start.Abscissa + CTCCurve.Length;
                return CalculateSIA();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// A target coordinate is known. The length of a constant curvature and toolface curve is a pure
        /// scale on its displacement, so the toolface angle and the dogleg angle are solved against the
        /// direction of the target and the curvature then follows by division.
        /// </summary>
        public bool CalculateXYZ()
        {
            if (Numeric.IsDefined(End.X) &&
                Numeric.IsDefined(End.Y) &&
                Numeric.IsDefined(End.Z))
            {
                if (!Start.CompleteCDTXYZ(End))
                {
                    return false;
                }
                if (End.Curvature == null || End.Toolface == null)
                {
                    return false;
                }
                CTCCurve.Curvature = End.Curvature.Value;
                CTCCurve.Toolface = End.Toolface.Value;
                CTCCurve.Length = End.Abscissa - Start.Abscissa;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Curvature, toolface angle and end inclination are known. The inclination of a constant
        /// curvature and toolface curve is linear in the along hole distance at the rate
        /// bur = curvature*cos(toolface), so the length follows at once.
        /// </summary>
        public bool CalculateDTI()
        {
            if (Numeric.IsDefined(CTCCurve.Curvature) &&
                Numeric.IsDefined(CTCCurve.Toolface) &&
                Numeric.IsDefined(End.Inclination))
            {
                double bur = (double)CTCCurve.Curvature * System.Math.Cos((double)CTCCurve.Toolface);
                double dInclination = (double)(End.Inclination - Start.Inclination);
                if (Numeric.EQ(bur, 0.0) || Numeric.EQ(dInclination, 0.0))
                {
                    // With no build up the inclination cannot change, and nothing then fixes the length.
                    return false;
                }
                double length = dInclination / bur;
                if (!Numeric.GT(length, 0.0))
                {
                    return false;
                }
                CTCCurve.Length = length;
                return CalculateLDT();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Curvature, toolface angle and end true vertical depth are known.
        ///
        /// The vertical of a constant curvature and toolface curve is (sin i1 - sin i0)/bur, exactly as
        /// for a constant build and turn curve, because the inclination is linear in both. The depth
        /// therefore fixes the end inclination through sin i1 = sin i0 + bur*dV, and the shorter of the
        /// two roots lying inside (0, pi) on the correct side of the start inclination is taken.
        /// </summary>
        public bool CalculateDTZ()
        {
            if (Numeric.IsDefined(CTCCurve.Curvature) &&
                Numeric.IsDefined(CTCCurve.Toolface) &&
                Numeric.IsDefined(End.Z))
            {
                double inclinationStart = (double)Start.Inclination;
                double bur = (double)CTCCurve.Curvature * System.Math.Cos((double)CTCCurve.Toolface);
                double dVertical = (double)(End.Z - Start.Z);

                if (Numeric.EQ(bur, 0.0))
                {
                    double cosStart = System.Math.Cos(inclinationStart);
                    if (Numeric.EQ(cosStart, 0.0))
                    {
                        return false;
                    }
                    double straight = dVertical / cosStart;
                    if (!Numeric.GT(straight, 0.0))
                    {
                        return false;
                    }
                    CTCCurve.Length = straight;
                    return CalculateLDT();
                }

                double sinEnd = System.Math.Sin(inclinationStart) + bur * dVertical;
                if (sinEnd < -1.0 || sinEnd > 1.0)
                {
                    return false;
                }
                double principal = System.Math.Asin(sinEnd);
                double length = double.PositiveInfinity;
                foreach (double candidate in new double[] { principal, Numeric.PI - principal })
                {
                    if (candidate <= 0.0 || candidate >= Numeric.PI)
                    {
                        continue;
                    }
                    double trial = (candidate - inclinationStart) / bur;
                    if (Numeric.GT(trial, 0.0) && trial < length)
                    {
                        length = trial;
                    }
                }
                if (double.IsInfinity(length))
                {
                    return false;
                }
                CTCCurve.Length = length;
                return CalculateLDT();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// The curvature and the end attitude are known, the toolface angle and the length are not.
        ///
        /// Over a constant curvature and toolface curve the inclination builds at the constant rate
        /// b = k*cos(f) and the azimuth turns at the rate t = k*sin(f) measured against the effective
        /// sine, so that
        ///
        ///     di = b*L        and        G*da = t*L
        ///
        /// where G depends only on the two inclinations. The two right hand sides are the legs of the
        /// right angled triangle whose hypotenuse is k*L, which gives the toolface angle as their
        /// bearing and the length by dividing the hypotenuse by the curvature. Both are closed form.
        /// </summary>
        public bool CalculateDIA()
        {
            if (Numeric.IsDefined(CTCCurve.Curvature) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                double curvature = (double)CTCCurve.Curvature;
                if (Numeric.EQ(curvature, 0.0))
                {
                    // A straight line holds its attitude, so it reaches the end attitude only if that
                    // is the one it starts with, and then no length is singled out.
                    return false;
                }
                // At the vertical the azimuth carries no meaning and the effective sine vanishes, so the
                // turn drops out of the equations and every end azimuth gives the same curve. Nothing
                // then picks out a toolface angle, and the answer is declined rather than invented.
                double inclinationStart = (double)Start.Inclination;
                double inclinationEnd = (double)End.Inclination;
                if (Numeric.EQ(System.Math.Sin(inclinationStart), 0.0) ||
                    Numeric.EQ(System.Math.Sin(inclinationEnd), 0.0))
                {
                    return false;
                }
                double dInclination = inclinationEnd - inclinationStart;
                double dAzimuth = TrajectoryPoint3D.WrapToPi((double)End.Azimuth - (double)Start.Azimuth);
                double effectiveSine = TrajectoryPoint3D.EffectiveSineCDT(inclinationStart, inclinationEnd);
                if (!Numeric.IsDefined(effectiveSine))
                {
                    return false;
                }
                double turn = effectiveSine * dAzimuth;

                double doglegAngle = System.Math.Sqrt(dInclination * dInclination + turn * turn);
                if (Numeric.EQ(doglegAngle, 0.0))
                {
                    return CalculateID();
                }
                CTCCurve.Toolface = System.Math.Atan2(turn, dInclination);
                CTCCurve.Length = doglegAngle / curvature;
                return CalculateLDT();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// The curvature, the toolface angle and the end azimuth are known.
        ///
        /// Under the substitution u = ln(tan(i/2)) the azimuth of a constant curvature and toolface
        /// curve is linear in u, with da = tan(f)*du. The end azimuth therefore fixes u and so the end
        /// inclination outright, after which the length follows from the constant build up rate. No
        /// iteration is involved.
        ///
        /// The azimuth is carried wrapped, so the turn it implies is only known up to whole turns and
        /// the curve reaching a given end azimuth is not unique. The branch selects which one is meant,
        /// adding that many whole turns to the shortest one, and it defaults to the shortest. A branch
        /// is never chosen silently: when the requested one leads out of the range of inclination or to
        /// a curve of negative length, this declines rather than quietly moving to another.
        /// </summary>
        public bool CalculateDTA(int branch = 0)
        {
            if (Numeric.IsDefined(CTCCurve.Curvature) &&
                Numeric.IsDefined(CTCCurve.Toolface) &&
                Numeric.IsDefined(End.Azimuth))
            {
                double curvature = (double)CTCCurve.Curvature;
                double toolface = (double)CTCCurve.Toolface;
                double cosToolface = System.Math.Cos(toolface);
                double sinToolface = System.Math.Sin(toolface);
                if (Numeric.EQ(curvature, 0.0) || Numeric.EQ(sinToolface, 0.0))
                {
                    // With the toolface in the build plane the azimuth never changes, so it fixes
                    // nothing. The same holds for a straight line.
                    return false;
                }
                double inclinationStart = (double)Start.Inclination;
                if (Numeric.EQ(System.Math.Sin(inclinationStart), 0.0))
                {
                    // At the vertical the azimuth is not defined and the curve cannot be anchored to it.
                    return false;
                }
                double dAzimuth = TrajectoryPoint3D.WrapToPi((double)End.Azimuth - (double)Start.Azimuth)
                                + 2.0 * Numeric.PI * branch;
                if (Numeric.EQ(cosToolface, 0.0))
                {
                    // A purely lateral toolface holds the inclination, so the azimuth turns at the
                    // constant rate k/sin(i) and the length is read off directly.
                    double lateral = dAzimuth * System.Math.Sin(inclinationStart) / (curvature * sinToolface);
                    if (!Numeric.GE(lateral, 0.0))
                    {
                        return false;
                    }
                    CTCCurve.Length = lateral;
                    return CalculateLDT();
                }

                // da = tan(f) * du  with  u = ln(tan(i/2))
                double u = System.Math.Log(System.Math.Tan(0.5 * inclinationStart)) + dAzimuth * cosToolface / sinToolface;
                double inclinationEnd = 2.0 * System.Math.Atan(System.Math.Exp(u));
                if (double.IsNaN(inclinationEnd) || inclinationEnd <= 0.0 || inclinationEnd >= Numeric.PI)
                {
                    return false;
                }
                double length = (inclinationEnd - inclinationStart) / (curvature * cosToolface);
                if (!Numeric.GE(length, 0.0))
                {
                    return false;
                }
                CTCCurve.Length = length;
                return CalculateLDT();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// A section of no length.
        /// </summary>
        public bool CalculateID()
        {
            End.X = Start.X;
            End.Y = Start.Y;
            End.Z = Start.Z;
            End.Inclination = Start.Inclination;
            End.Azimuth = Start.Azimuth;
            End.Abscissa = Start.Abscissa;
            CTCCurve.Curvature = 0.0;
            CTCCurve.Toolface = 0.0;
            CTCCurve.Length = 0.0;
            return true;
        }

        public override CurvilinearPoint3D InterpolateAtMD(double md)
        {
            TrajectoryPoint3D point = new TrajectoryPoint3D();
            point.SetUndefined();
            bool success = InterpolateAtMD(md, point);
            if (success)
            {
                return point;
            }
            else
            {
                return null;
            }
        }
        public bool InterpolateAtMD(double md, TrajectoryPoint3D point)
        {
            if (point != null)
            {
                TrajectoryPoint3D point1 = Start;
                TrajectoryPoint3D point2 = End;
                if (Numeric.EQ(point1.Abscissa, md, Numeric.DEPTH_ACCURACY))
                {
                    point.Set(point1);
                    return true;
                }
                else if (Numeric.EQ(point2.Abscissa, md, Numeric.DEPTH_ACCURACY))
                {
                    point.Set(point2);
                    return true;
                }
                else if (!Numeric.IsBetween(md, (double)point1.Abscissa, (double)point2.Abscissa))
                {
                    return false;
                }
                ConstantCurvatureAndToolfaceArcSection workingSection = new ConstantCurvatureAndToolfaceArcSection();
                workingSection.Start.Set(point1);
                workingSection.End = point;
                point.Abscissa = md;
                workingSection.CTCCurve.Curvature = CTCCurve.Curvature;
                workingSection.CTCCurve.Toolface = CTCCurve.Toolface;
                workingSection.CalculateSDT();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CalculateSDT()
        {
            if (Numeric.IsDefined(CTCCurve.Curvature) &&
                Numeric.IsDefined(CTCCurve.Toolface) &&
                Numeric.IsDefined(End.Abscissa))
            {
                CTCCurve.Length = End.Abscissa - Start.Abscissa;
                return CalculateLDT();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateLDT()
        {
            if (Numeric.IsDefined(CTCCurve.Curvature) &&
                Numeric.IsDefined(CTCCurve.Toolface) &&
                Numeric.IsDefined(CTCCurve.Length))
            {
                double length = (double)CTCCurve.Length;
                End.Abscissa = Start.Abscissa + length;
                // The geometry of a constant curvature and toolface curve is defined once, in
                // OSDC.DotnetLibraries.General.Math, on TrajectoryPoint3D, and used here rather than restated.
                return Start.CompleteCDTSDT(End, (double)CTCCurve.Curvature, (double)CTCCurve.Toolface);
            }
            else
            {
                return false;
            }
        }
    }
}
