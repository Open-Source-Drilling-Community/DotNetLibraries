using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// a section based on a constant build and turn curve in between the start and end of the section
    /// </summary>
    [Serializable]
    public class BuildAndTurnArcSection : ArcSection
    {
        public NonLocalizedBuildAndTurnCurve BuildAndTurn { get; set; } = new NonLocalizedBuildAndTurnCurve();
        /// <summary>
        /// 
        /// </summary>
        public override NonLocalizedCurve Curve
        {
            get => BuildAndTurn;
            set
            {
                if (value is NonLocalizedBuildAndTurnCurve)
                {
                    BuildAndTurn = (NonLocalizedBuildAndTurnCurve)value;
                }
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public BuildAndTurnArcSection()
        {
        }
        /// <summary>
        /// initialise the section from two stations, whose values are copied in, as the circular arc
        /// section does. The section keeps its own stations, so a caller that chains sections gets a
        /// trajectory whose stations carry equal values rather than one holding the same objects twice.
        /// </summary>
        public BuildAndTurnArcSection(TrajectoryPoint3D start, TrajectoryPoint3D end)
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

        public override CurvilinearPoint3D InterpolateAtMD(double md)
        {
            BuildAndTurnArcSection sec = new BuildAndTurnArcSection();
            sec.Start = Start;
            sec.BuildAndTurn = new NonLocalizedBuildAndTurnCurve(BuildAndTurn);
            sec.BuildAndTurn.Length = (double?)md - Start.Abscissa;
            sec.End = new TrajectoryPoint3D();
            sec.End.Abscissa = md;
            sec.CalculateLBT();
            return sec.End;
        }

        public override bool Calculate()
        {
            if (End != null && Curve != null)
            {
                if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                    Numeric.IsDefined(BuildAndTurn.TR) &&
                    Numeric.IsDefined(BuildAndTurn.Length))
                {
                    return CalculateLBT();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Abscissa))
                {
                    return CalculateBTS();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateBTZ();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateBTA();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateBTI();
                }
                else if (Numeric.IsDefined(End.Abscissa) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateSIA();
                }
                else if (Numeric.IsDefined(BuildAndTurn.Length) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateLIA();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateBIA();
                }
                else if (Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateTIA();
                }
                else if (Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Abscissa) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateTSI();
                }
                else if (Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(BuildAndTurn.Length) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateTLI();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(End.Abscissa) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateBSI();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(BuildAndTurn.Length) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateBLI();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(End.Azimuth) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateBAZ();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateBIZ();
                }
                else if (Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateTIZ();
                }
                else if (Numeric.IsDefined(End.Abscissa) &&
                         Numeric.IsDefined(End.Azimuth) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateSAZ();
                }
                else if (Numeric.IsDefined(BuildAndTurn.Length) &&
                         Numeric.IsDefined(End.Azimuth) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateLAZ();
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
            else
            {
                return false;
            }
        }

        public bool CalculateLBT()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(BuildAndTurn.Length))
            {
                End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                return CalculateBTS();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBTS()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Abscissa))
            {
                BuildAndTurn.Length = End.Abscissa - Start.Abscissa;
                // The geometry of a constant build and turn curve is defined once, in
                // OSDC.DotnetLibraries.General.Math, on TrajectoryPoint3D, and used here rather than restated.
                // The attitude is built up from the rates, so it is continuous by construction and is
                // handed over as such rather than wrapped.
                double length = (double)BuildAndTurn.Length;
                End.Inclination = Start.Inclination + BuildAndTurn.BUR * length;
                End.Azimuth = Start.Azimuth + BuildAndTurn.TR * length;
                return Start.CompleteBTSIA(End, 0, false);
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBTZ()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Z))
            {
                if (Numeric.EQ(BuildAndTurn.BUR, 0))
                {
                    if (Numeric.EQ(System.Math.Cos((double)Start.Inclination), 0))
                    {
                        if (Numeric.EQ(End.Z, Start.Z))
                        {
                            CalculateID();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        BuildAndTurn.Length = (End.Z - Start.Z) / System.Math.Cos((double)Start.Inclination);
                        End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                        return CalculateBTS();
                    }
                }
                else
                {
                    // The vertical gained over a build and turn curve is (sin i1 - sin i0)/bur because
                    // the inclination is linear in the along hole distance. The end true vertical depth
                    // therefore fixes the sine of the end inclination, which leaves two candidate
                    // inclinations in (0, pi): the principal one and its supplement. Only taking the
                    // principal value would make every inclination past the horizontal unreachable, so
                    // both are tried and the one giving the shortest curve of positive length is kept.
                    double sinEnd = System.Math.Sin((double)Start.Inclination) + (double)(End.Z - Start.Z) * (double)BuildAndTurn.BUR;
                    if (sinEnd < -1.0 || sinEnd > 1.0)
                    {
                        return false;
                    }
                    double principal = System.Math.Asin(sinEnd);
                    double best = double.NaN;
                    double shortest = double.PositiveInfinity;
                    foreach (double candidate in new double[] { principal, Numeric.PI - principal })
                    {
                        if (candidate < 0.0 || candidate > Numeric.PI)
                        {
                            continue;
                        }
                        double trial = (candidate - (double)Start.Inclination) / (double)BuildAndTurn.BUR;
                        if (Numeric.GE(trial, 0.0) && trial < shortest)
                        {
                            shortest = trial;
                            best = candidate;
                        }
                    }
                    if (double.IsNaN(best))
                    {
                        return false;
                    }
                    End.Inclination = best;
                    return CalculateBTI();
                }
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBTA()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Azimuth))
            {
                if (!Numeric.EQ(BuildAndTurn.TR, 0))
                {
                    BuildAndTurn.Length = (End.Azimuth - Start.Azimuth) / BuildAndTurn.TR;
                    End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                    return CalculateBTS();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool CalculateBTI()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Inclination))
            {
                if (!Numeric.EQ(BuildAndTurn.BUR, 0))
                {
                    // The inclination of a build and turn curve varies linearly with the along hole
                    // distance and, unlike the azimuth, it does not wrap: it is an angle from the
                    // vertical confined to [0, pi]. So when the requested change of inclination runs
                    // against the sign of the build up rate there is no curve reaching it, and adding
                    // a whole turn would only drive the attitude through the vertical and back round.
                    double length = (double)(End.Inclination - Start.Inclination) / (double)BuildAndTurn.BUR;
                    if (!Numeric.GE(length, 0.0))
                    {
                        return false;
                    }
                    BuildAndTurn.Length = length;
                    // The end measured depth has to be set as well, or the construction below has
                    // nothing to work from and declines.
                    End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                    return CalculateBTS();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool CalculateSIA()
        {
            if (Numeric.IsDefined(End.Abscissa) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                BuildAndTurn.Length = End.Abscissa - Start.Abscissa;
                if (Numeric.EQ(Start.Abscissa, End.Abscissa))
                {
                    return CalculateID();
                }
                else
                {
                    // The rates come from the curve construction rather than from differencing the two
                    // stations here, so that the azimuth is taken the short way round. Differencing the
                    // reported azimuths directly reads an ordinary small turn across north as a turn of
                    // almost a full circle the other way.
                    if (!Start.CompleteBTSIA(End))
                    {
                        return false;
                    }
                    BuildAndTurn.BUR = End.BUR;
                    BuildAndTurn.TR = End.TUR;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public bool CalculateLIA()
        {
            if (Numeric.IsDefined(BuildAndTurn.Length) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                return CalculateSIA();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Build up rate, end inclination and end azimuth are known. The build up rate fixes the length
        /// through the swept inclination, and the turn rate then follows from the azimuth taken the
        /// short way round.
        /// </summary>
        public bool CalculateBIA()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                double bur = (double)BuildAndTurn.BUR;
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
                BuildAndTurn.Length = length;
                End.Abscissa = Start.Abscissa + length;
                BuildAndTurn.TR = CurvilinearPoint3D.WrapToPi((double)(End.Azimuth - Start.Azimuth)) / length;
                return CalculateBTS();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Turn rate, end inclination and end azimuth are known. The turn rate fixes the length through
        /// the swept azimuth, and the build up rate then follows from the inclination.
        /// </summary>
        public bool CalculateTIA()
        {
            if (Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                double tur = (double)BuildAndTurn.TR;
                double dAzimuth = CurvilinearPoint3D.WrapToPi((double)(End.Azimuth - Start.Azimuth));
                if (Numeric.EQ(tur, 0.0) || Numeric.EQ(dAzimuth, 0.0))
                {
                    // With no turn the azimuth cannot change, and nothing then fixes the length.
                    return false;
                }
                double length = dAzimuth / tur;
                if (!Numeric.GT(length, 0.0))
                {
                    return false;
                }
                BuildAndTurn.Length = length;
                End.Abscissa = Start.Abscissa + length;
                BuildAndTurn.BUR = (double)(End.Inclination - Start.Inclination) / length;
                return CalculateBTS();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Turn rate, end measured depth and end inclination are known. The measured depth fixes the
        /// length and the inclination then fixes the build up rate.
        /// </summary>
        public bool CalculateTSI()
        {
            if (Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Abscissa) &&
                Numeric.IsDefined(End.Inclination))
            {
                double length = (double)(End.Abscissa - Start.Abscissa);
                if (!Numeric.GT(length, 0.0))
                {
                    return false;
                }
                BuildAndTurn.Length = length;
                BuildAndTurn.BUR = (double)(End.Inclination - Start.Inclination) / length;
                return CalculateBTS();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateTLI()
        {
            if (Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(BuildAndTurn.Length) &&
                Numeric.IsDefined(End.Inclination))
            {
                End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                return CalculateTSI();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Build up rate, end measured depth and end inclination are known, which does not define a
        /// curve. The measured depth fixes the length and the build up rate then already determines the
        /// end inclination, so that third value carries no new information and nothing anywhere in the
        /// set constrains the turn rate: every turn rate gives a curve satisfying all three. The problem
        /// is under determined rather than merely hard, so it is declined.
        /// </summary>
        public bool CalculateBSI()
        {
            return false;
        }

        public bool CalculateBLI()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.Length) &&
                Numeric.IsDefined(End.Inclination))
            {
                End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                return CalculateBSI();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Build up rate, end azimuth and end true vertical depth are known.
        ///
        /// The vertical of a constant build and turn curve is (sin i1 - sin i0)/bur, so the depth fixes
        /// the end inclination through sin i1 = sin i0 + bur*dV. That has two roots in (0, pi); the one
        /// giving the shorter curve on the correct side of the start inclination is taken.
        /// </summary>
        public bool CalculateBAZ()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(End.Azimuth) &&
                Numeric.IsDefined(End.Z))
            {
                double bur = (double)BuildAndTurn.BUR;
                double dVertical = (double)(End.Z - Start.Z);
                double inclinationStart = (double)Start.Inclination;
                if (Numeric.EQ(bur, 0.0))
                {
                    double cosStart = System.Math.Cos(inclinationStart);
                    if (Numeric.EQ(cosStart, 0.0))
                    {
                        return false;
                    }
                    double horizontalLength = dVertical / cosStart;
                    if (!Numeric.GT(horizontalLength, 0.0))
                    {
                        return false;
                    }
                    BuildAndTurn.Length = horizontalLength;
                    End.Abscissa = Start.Abscissa + horizontalLength;
                    BuildAndTurn.TR = CurvilinearPoint3D.WrapToPi((double)(End.Azimuth - Start.Azimuth)) / horizontalLength;
                    return CalculateBTS();
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
                        End.Inclination = candidate;
                    }
                }
                if (double.IsInfinity(length))
                {
                    return false;
                }
                BuildAndTurn.Length = length;
                End.Abscissa = Start.Abscissa + length;
                BuildAndTurn.TR = CurvilinearPoint3D.WrapToPi((double)(End.Azimuth - Start.Azimuth)) / length;
                return CalculateBTS();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Build up rate, end inclination and end true vertical depth are known, which does not define a
        /// curve. The build up rate and the inclination between them already fix the length, and the
        /// depth then follows, so it carries no new information; nothing in the set constrains the turn
        /// rate. The problem is under determined rather than merely hard, so it is declined.
        /// </summary>
        public bool CalculateBIZ()
        {
            return false;
        }
        /// <summary>
        /// Turn rate, end inclination and end true vertical depth are known.
        ///
        /// The vertical fixes the build up rate directly, since (sin i1 - sin i0)/bur is the vertical and
        /// both inclinations are then known, and the length follows from the swept inclination.
        /// </summary>
        public bool CalculateTIZ()
        {
            if (Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Z))
            {
                double inclinationStart = (double)Start.Inclination;
                double inclinationEnd = (double)End.Inclination;
                double dVertical = (double)(End.Z - Start.Z);
                double dInclination = inclinationEnd - inclinationStart;
                double length;
                if (Numeric.EQ(dInclination, 0.0))
                {
                    // No build up: the vertical is simply the length times the cosine of the inclination.
                    double cosStart = System.Math.Cos(inclinationStart);
                    if (Numeric.EQ(cosStart, 0.0))
                    {
                        return false;
                    }
                    length = dVertical / cosStart;
                    BuildAndTurn.BUR = 0.0;
                }
                else
                {
                    if (Numeric.EQ(dVertical, 0.0))
                    {
                        return false;
                    }
                    double bur = (System.Math.Sin(inclinationEnd) - System.Math.Sin(inclinationStart)) / dVertical;
                    if (Numeric.EQ(bur, 0.0))
                    {
                        return false;
                    }
                    BuildAndTurn.BUR = bur;
                    length = dInclination / bur;
                }
                if (!Numeric.GT(length, 0.0))
                {
                    return false;
                }
                BuildAndTurn.Length = length;
                End.Abscissa = Start.Abscissa + length;
                return CalculateBTS();
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// End measured depth, end azimuth and end true vertical depth are known.
        ///
        /// The measured depth fixes the length, and the vertical then fixes the build up rate through
        ///     dV = L * cos( i0 + u ) * sin(u)/u ,   u = bur*L/2
        /// which is solved for u by bisection over the interval that keeps the end inclination inside
        /// (0, pi). The turn rate follows from the azimuth taken the short way round.
        /// </summary>
        public bool CalculateSAZ()
        {
            if (Numeric.IsDefined(End.Abscissa) &&
                Numeric.IsDefined(End.Azimuth) &&
                Numeric.IsDefined(End.Z))
            {
                double length = (double)(End.Abscissa - Start.Abscissa);
                if (!Numeric.GT(length, 0.0))
                {
                    return false;
                }
                double inclinationStart = (double)Start.Inclination;
                double target = (double)(End.Z - Start.Z) / length;

                // u is half the swept inclination; the end inclination i0 + 2u must stay inside (0, pi).
                double lower = -0.5 * inclinationStart + 1.0e-12;
                double upper = 0.5 * (Numeric.PI - inclinationStart) - 1.0e-12;
                if (!(upper > lower))
                {
                    return false;
                }

                static double VerticalRatio(double u, double inclination)
                {
                    double sinc = System.Math.Abs(u) < 1.0e-8 ? 1.0 - u * u / 6.0 : System.Math.Sin(u) / u;
                    return System.Math.Cos(inclination + u) * sinc;
                }

                // Bracket the root by scanning, then bisect. The ratio is not monotone over the whole
                // interval for every start inclination, so a bracket is searched for rather than assumed.
                const int scan = 400;
                double previousU = lower;
                double previousValue = VerticalRatio(lower, inclinationStart) - target;
                double rootLow = double.NaN;
                double rootHigh = double.NaN;
                for (int i = 1; i <= scan; i++)
                {
                    double u = lower + (upper - lower) * i / scan;
                    double value = VerticalRatio(u, inclinationStart) - target;
                    if (Numeric.EQ(value, 0.0))
                    {
                        rootLow = u;
                        rootHigh = u;
                        break;
                    }
                    if (previousValue * value < 0.0)
                    {
                        rootLow = previousU;
                        rootHigh = u;
                        break;
                    }
                    previousU = u;
                    previousValue = value;
                }
                if (double.IsNaN(rootLow))
                {
                    return false;
                }
                for (int i = 0; i < 200 && rootHigh - rootLow > 1.0e-15; i++)
                {
                    double middle = 0.5 * (rootLow + rootHigh);
                    if ((VerticalRatio(rootLow, inclinationStart) - target) * (VerticalRatio(middle, inclinationStart) - target) <= 0.0)
                    {
                        rootHigh = middle;
                    }
                    else
                    {
                        rootLow = middle;
                    }
                }
                double half = 0.5 * (rootLow + rootHigh);

                BuildAndTurn.Length = length;
                BuildAndTurn.BUR = 2.0 * half / length;
                BuildAndTurn.TR = CurvilinearPoint3D.WrapToPi((double)(End.Azimuth - Start.Azimuth)) / length;
                return CalculateBTS();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateLAZ()
        {
            if (Numeric.IsDefined(BuildAndTurn.Length) &&
                Numeric.IsDefined(End.Azimuth) &&
                Numeric.IsDefined(End.Z))
            {
                End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                return CalculateSAZ();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateXYZ()
        {
            if (Numeric.IsDefined(End.X) &&
                Numeric.IsDefined(End.Y) &&
                Numeric.IsDefined(End.Z))
            {
                if (!Start.CompleteBTXYZ(End))
                {
                    return false;
                }
                BuildAndTurn.BUR = End.BUR;
                BuildAndTurn.TR = End.TUR;
                BuildAndTurn.Length = End.Abscissa - Start.Abscissa;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CalculateID()
        {
            End.X = Start.X;
            End.Y = Start.Y;
            End.Z = Start.Z;
            End.Inclination = Start.Inclination;
            End.Azimuth = Start.Azimuth;
            End.Abscissa = Start.Abscissa;
            BuildAndTurn.BUR = 0.0;
            BuildAndTurn.TR = 0.0;
            BuildAndTurn.Length = 0.0;
            return true;
        }
    }
}
