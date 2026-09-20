using System;
using System.Collections.Generic;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// A trajectory running from a known station through an ordered list of targets, drawn with circular
    /// arcs, with constant build and turn curves, or with constant curvature and toolface curves.
    ///
    /// Each target says where the trajectory has to go, and may say which way it has to be heading when
    /// it gets there. A target that only gives a position leaves three conditions, which one section
    /// satisfies; the direction the trajectory carries on in is then whatever that section arrives with.
    /// A target that also gives a direction leaves five, which takes a pair of sections of the same kind
    /// sharing a curvature, so each such target contributes two sections rather than one.
    ///
    /// The sections come back through the base type they have in common, so a caller can walk the whole
    /// trajectory, interpolate along it and read its stations without caring which curve each was drawn
    /// with, and a path may mix the three kinds by setting a type on individual targets.
    /// </summary>
    [Serializable]
    public class TargetAxisPath
    {
        /// <summary>
        /// The station the trajectory sets off from. Its position is needed. Its attitude and its measured
        /// depth are worked out when they are left undefined, as described on Calculate.
        /// </summary>
        public TrajectoryPoint3D Start { get; set; } = new TrajectoryPoint3D();

        /// <summary>
        /// The targets, in the order they are to be reached.
        /// </summary>
        public List<TargetAxis> Targets { get; } = new List<TargetAxis>();

        /// <summary>
        /// Which curve the sections are drawn with, for every target that does not name one itself.
        /// </summary>
        public SectionCurveType CurveType { get; set; } = SectionCurveType.CircularArc;

        /// <summary>
        /// The sections that make up the trajectory, in order, once Calculate has succeeded. A section
        /// keeps its own stations, as every section in this library does, so the station ending one and
        /// the station starting the next are two objects carrying the same values. Calculate checks that
        /// they really do agree before it reports success.
        /// </summary>
        public List<ArcSection> Sections { get; } = new List<ArcSection>();

        /// <summary>
        /// The target that could not be reached, or -1 when there was none. This is what the earlier
        /// version of this calculation lacked: it answered a bare false, which left the caller to work
        /// out for itself which leg of the trajectory was the impossible one.
        /// </summary>
        public int FailedTargetIndex { get; private set; } = -1;

        /// <summary>
        /// What went wrong, when something did.
        /// </summary>
        public TargetAxisFailureReason FailureReason { get; private set; } = TargetAxisFailureReason.None;

        /// <summary>
        /// Which curve the leg that failed was being drawn with, when it had got as far as choosing one.
        /// </summary>
        public SectionCurveType? FailedCurveType { get; private set; } = null;

        /// <summary>
        /// The station the failing leg was setting off from. With the target it was aimed at and the curve
        /// it was being drawn with, this is everything needed to put the same question again on its own.
        /// </summary>
        public TrajectoryPoint3D FailedFrom { get; private set; } = null;

        /// <summary>
        /// The section at which the trajectory stopped joining up, or -1. Only set when the reason is that
        /// it was discontinuous.
        /// </summary>
        public int FailedSectionIndex { get; private set; } = -1;

        /// <summary>
        /// What went wrong, in a sentence, for a log or a message to a user.
        /// </summary>
        public string FailureDescription
        {
            get
            {
                switch (FailureReason)
                {
                    case TargetAxisFailureReason.None:
                        return "nothing went wrong";
                    case TargetAxisFailureReason.UndefinedStart:
                        return "the station the trajectory sets off from does not say where it is";
                    case TargetAxisFailureReason.UndefinedTarget:
                        return "target " + FailedTargetIndex + " does not say where it is";
                    case TargetAxisFailureReason.UndefinedStartDirection:
                        return "the start carries no direction and none could be worked out, because the "
                             + "first target sits where the trajectory starts";
                    case TargetAxisFailureReason.CurveDeclined:
                        return "no " + Describe(FailedCurveType) + " reaches target " + FailedTargetIndex
                             + (TargetImposesAnAxis(FailedTargetIndex) ? " with the direction it asks for" : "")
                             + " from the station before it";
                    case TargetAxisFailureReason.TargetNotReached:
                        return "the " + Describe(FailedCurveType) + " worked out for target " + FailedTargetIndex
                             + " does not arrive at it";
                    case TargetAxisFailureReason.Discontinuous:
                        return "the trajectory does not join up at section " + FailedSectionIndex;
                    default:
                        return "unknown";
                }
            }
        }

        private bool TargetImposesAnAxis(int index)
        {
            return index >= 0 && index < Targets.Count && Targets[index] != null && Targets[index].HasAxis;
        }

        private static string Describe(SectionCurveType? type)
        {
            switch (type)
            {
                case SectionCurveType.CircularArc:
                    return "circular arc";
                case SectionCurveType.ConstantBuildAndTurn:
                    return "constant build and turn curve";
                case SectionCurveType.ConstantCurvatureAndToolface:
                    return "constant curvature and toolface curve";
                default:
                    return "curve";
            }
        }

        /// <summary>
        /// Record what went wrong and hand back false, so that every way out of Calculate says the same
        /// kind of thing about itself.
        /// </summary>
        private bool Fail(TargetAxisFailureReason reason, int targetIndex = -1,
                          SectionCurveType? type = null, TrajectoryPoint3D from = null, int sectionIndex = -1)
        {
            FailureReason = reason;
            FailedTargetIndex = targetIndex;
            FailedCurveType = type;
            FailedSectionIndex = sectionIndex;
            if (from != null)
            {
                FailedFrom = new TrajectoryPoint3D();
                FailedFrom.Set(from);
            }
            return false;
        }

        /// <summary>
        /// Add a target to pass through, with no direction imposed.
        /// </summary>
        public TargetAxis AddTarget(double x, double y, double z)
        {
            TargetAxis target = new TargetAxis(x, y, z);
            Targets.Add(target);
            return target;
        }

        /// <summary>
        /// Add a target to reach heading a given way.
        /// </summary>
        public TargetAxis AddTarget(double x, double y, double z, double inclination, double azimuth)
        {
            TargetAxis target = new TargetAxis(x, y, z, inclination, azimuth);
            Targets.Add(target);
            return target;
        }

        /// <summary>
        /// How closely a section has to arrive at the position of the target it was drawn to, in metres.
        /// The constructions work to around a billionth of the length they cover, so this is a wide margin
        /// and is here to make sure that what is handed back really is a trajectory through the targets
        /// rather than whatever the constructions happened to return.
        /// </summary>
        public double PositionAccuracy { get; set; } = 1.0e-6;

        /// <summary>
        /// How closely a section has to arrive at the direction a target imposes, in radians.
        ///
        /// The default is what the constructions actually promise rather than what they usually manage.
        /// A pair of curves normally meets the direction to within about a ten thousand millionth of a
        /// radian, but where the two stations are so nearly joined by one curve that the junction between
        /// a pair is not pinned down at all, the pair constructions fall back on that single curve and
        /// promise only a ten thousandth of a radian. That is around two ten thousandths of a degree,
        /// orders below what a survey resolves, so it is taken as the default here.
        ///
        /// Asking for less than the constructions promise is allowed and is sometimes what is wanted, but
        /// it will turn down those nearly single curve cases, reporting them as TargetNotReached.
        /// </summary>
        public double AttitudeAccuracy { get; set; } = 1.0e-4;

        /// <summary>
        /// Work out the trajectory.
        ///
        /// The start station is taken as it is when it carries an attitude. When it does not, one is
        /// chosen: if the first target imposes a direction the trajectory sets off in that direction, and
        /// otherwise it sets off pointing straight at the first target. A measured depth of zero is used
        /// when none is given, since only the differences along the trajectory matter.
        ///
        /// Every section is checked against the target it was drawn to before the next one is worked out,
        /// and the trajectory is checked for continuity at the end. Where a target cannot be reached this
        /// returns false, keeps the sections worked out so far, and says what happened: FailedTargetIndex
        /// names the target, FailureReason says whether the construction found nothing or found something
        /// that misses, FailedCurveType says which curve was being drawn, and FailedFrom is the station the
        /// leg was setting off from, so the same question can be put again on its own.
        /// </summary>
        public bool Calculate()
        {
            Sections.Clear();
            FailedTargetIndex = -1;
            FailedSectionIndex = -1;
            FailedCurveType = null;
            FailedFrom = null;
            FailureReason = TargetAxisFailureReason.None;

            if (Start == null ||
                !Numeric.IsDefined(Start.X) || !Numeric.IsDefined(Start.Y) || !Numeric.IsDefined(Start.Z))
            {
                return Fail(TargetAxisFailureReason.UndefinedStart);
            }
            for (int i = 0; i < Targets.Count; i++)
            {
                if (Targets[i] == null || !Targets[i].HasPosition)
                {
                    return Fail(TargetAxisFailureReason.UndefinedTarget, i);
                }
            }
            if (Targets.Count == 0)
            {
                return true;
            }

            TrajectoryPoint3D current = new TrajectoryPoint3D();
            current.Set(Start);
            if (!ResolveStartAttitude(current))
            {
                return Fail(TargetAxisFailureReason.UndefinedStartDirection, 0, null, current);
            }

            for (int i = 0; i < Targets.Count; i++)
            {
                TargetAxis target = Targets[i];
                SectionCurveType type = target.CurveType ?? CurveType;

                TrajectoryPoint3D destination = new TrajectoryPoint3D();
                destination.Set(target.Station);

                TrajectoryPoint3D leavingFrom = new TrajectoryPoint3D();
                leavingFrom.Set(current);

                TrajectoryPoint3D reached = target.HasAxis
                    ? AppendThroughAxis(current, destination, type)
                    : AppendThroughPoint(current, destination, type);

                if (reached == null)
                {
                    // The construction found nothing at all: the targets ask for a curve of this kind
                    // which does not exist. Moving the target, or drawing this leg with another kind of
                    // curve, is what answers it.
                    return Fail(TargetAxisFailureReason.CurveDeclined, i, type, leavingFrom);
                }
                if (!Arrived(reached, target))
                {
                    // Something came back but it does not go where it was asked to. Nothing should do
                    // this, and a trajectory that misses its targets is worse than none at all.
                    return Fail(TargetAxisFailureReason.TargetNotReached, i, type, leavingFrom);
                }
                current = reached;
            }

            int broken = FindDiscontinuity();
            if (broken >= 0)
            {
                return Fail(TargetAxisFailureReason.Discontinuous, -1, null, null, broken);
            }
            return true;
        }

        /// <summary>
        /// Fill in the attitude and the measured depth of the start station when they are missing.
        /// </summary>
        private bool ResolveStartAttitude(TrajectoryPoint3D station)
        {
            if (!Numeric.IsDefined(station.Abscissa))
            {
                station.Abscissa = 0.0;
            }
            if (Numeric.IsDefined(station.Inclination) && Numeric.IsDefined(station.Azimuth))
            {
                return true;
            }
            TargetAxis first = Targets[0];
            if (first.HasAxis)
            {
                // Set off the way the first target asks to be met, which makes the leg up to it as
                // straight as the targets allow.
                station.Inclination = first.Station.Inclination;
                station.Azimuth = first.Station.Azimuth;
                return true;
            }
            // Nothing says which way to set off, so head straight at the first target.
            Vector3D direction = new Vector3D(station, first.Station);
            double? inclination = direction.GetIncl();
            double? azimuth = direction.GetAz();
            if (!Numeric.IsDefined(inclination) || !Numeric.IsDefined(azimuth))
            {
                return false;
            }
            station.Inclination = inclination;
            station.Azimuth = azimuth;
            return true;
        }

        /// <summary>
        /// One section from the station in hand to a target that imposes a position only.
        /// </summary>
        private TrajectoryPoint3D AppendThroughPoint(TrajectoryPoint3D from, TrajectoryPoint3D to,
                                                     SectionCurveType type)
        {
            switch (type)
            {
                case SectionCurveType.CircularArc:
                    {
                        CircularArcSection section = new CircularArcSection(from, to);
                        if (!section.CalculateXYZ())
                        {
                            return null;
                        }
                        Sections.Add(section);
                        return section.End;
                    }
                case SectionCurveType.ConstantBuildAndTurn:
                    {
                        BuildAndTurnArcSection section = new BuildAndTurnArcSection(from, to);
                        if (!section.CalculateXYZ())
                        {
                            return null;
                        }
                        Sections.Add(section);
                        return section.End;
                    }
                case SectionCurveType.ConstantCurvatureAndToolface:
                    {
                        ConstantCurvatureAndToolfaceArcSection section =
                            new ConstantCurvatureAndToolfaceArcSection(from, to);
                        if (!section.CalculateXYZ())
                        {
                            return null;
                        }
                        Sections.Add(section);
                        return section.End;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// The pair of sections from the station in hand to a target that imposes a direction as well as a
        /// position. Where one section already meets the target the pair collapses to it, and only that
        /// one is added rather than a second one of no length.
        /// </summary>
        private TrajectoryPoint3D AppendThroughAxis(TrajectoryPoint3D from, TrajectoryPoint3D to,
                                                    SectionCurveType type)
        {
            switch (type)
            {
                case SectionCurveType.CircularArc:
                    {
                        DoubleArcs pair = new DoubleArcs { Start = from, End = to };
                        if (!pair.CalculateXYZ())
                        {
                            return null;
                        }
                        CircularArcSection Half(TrajectoryPoint3D a, TrajectoryPoint3D b, double? toolface)
                        {
                            CircularArcSection half = new CircularArcSection(a, b);
                            half.Circle.Curvature = pair.DoubleArcCurve.Curvature;
                            half.Circle.ReferenceToolface = toolface;
                            half.Circle.Length = b.Abscissa - a.Abscissa;
                            return half;
                        }
                        AddHalves(from, pair.Intermediate, pair.End,
                                  () => Half(from, pair.Intermediate, pair.DoubleArcCurve.UpstreamReferenceToolface),
                                  () => Half(pair.Intermediate, pair.End, pair.DoubleArcCurve.DownstreamReferenceToolface),
                                  () => Half(from, pair.End, pair.DoubleArcCurve.UpstreamReferenceToolface));
                        return pair.End;
                    }
                case SectionCurveType.ConstantBuildAndTurn:
                    {
                        DoubleBuildAndTurnArcs pair = new DoubleBuildAndTurnArcs { Start = from, End = to };
                        if (!pair.CalculateXYZ())
                        {
                            return null;
                        }
                        NonLocalizedDoubleBuildAndTurnCurve curve = pair.DoubleBuildAndTurnCurve;
                        BuildAndTurnArcSection Half(TrajectoryPoint3D a, TrajectoryPoint3D b,
                                                    double? bur, double? tur, double? length)
                        {
                            BuildAndTurnArcSection half = new BuildAndTurnArcSection(a, b);
                            half.BuildAndTurn.BUR = bur;
                            half.BuildAndTurn.TR = tur;
                            half.BuildAndTurn.Length = length;
                            return half;
                        }
                        AddHalves(from, pair.Intermediate, pair.End,
                                  () => Half(from, pair.Intermediate, curve.UpstreamBUR, curve.UpstreamTR, curve.UpstreamLength),
                                  () => Half(pair.Intermediate, pair.End, curve.DownstreamBUR, curve.DownstreamTR, curve.DownstreamLength),
                                  () => Half(from, pair.End, curve.UpstreamBUR, curve.UpstreamTR, curve.UpstreamLength));
                        return pair.End;
                    }
                case SectionCurveType.ConstantCurvatureAndToolface:
                    {
                        DoubleConstantCurvatureAndToolfaceArcs pair =
                            new DoubleConstantCurvatureAndToolfaceArcs { Start = from, End = to };
                        if (!pair.CalculateXYZ())
                        {
                            return null;
                        }
                        NonLocalizedDoubleConstantCurvatureAndToolfaceCurve curve = pair.DoubleCTCCurve;
                        ConstantCurvatureAndToolfaceArcSection Half(TrajectoryPoint3D a, TrajectoryPoint3D b,
                                                                    double? toolface, double? length)
                        {
                            ConstantCurvatureAndToolfaceArcSection half =
                                new ConstantCurvatureAndToolfaceArcSection(a, b);
                            half.CTCCurve.Curvature = (double)curve.Curvature;
                            half.CTCCurve.Toolface = (double)toolface;
                            half.CTCCurve.Length = length;
                            return half;
                        }
                        AddHalves(from, pair.Intermediate, pair.End,
                                  () => Half(from, pair.Intermediate, curve.UpstreamToolface, curve.UpstreamLength),
                                  () => Half(pair.Intermediate, pair.End, curve.DownstreamToolface, curve.DownstreamLength),
                                  () => Half(from, pair.End, curve.UpstreamToolface, curve.UpstreamLength));
                        return pair.End;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Add the two halves of a pair, or the single section they collapse to when the pair turned out to
        /// need only one curve. A section of no length carries no curve and would only get in the way of
        /// whatever walks the trajectory afterwards.
        /// </summary>
        private void AddHalves(TrajectoryPoint3D from, TrajectoryPoint3D junction, TrajectoryPoint3D to,
                               Func<ArcSection> upstream, Func<ArcSection> downstream, Func<ArcSection> whole)
        {
            bool firstIsEmpty = from.EQ(junction, PositionAccuracy);
            bool secondIsEmpty = to.EQ(junction, PositionAccuracy);
            if (secondIsEmpty && !firstIsEmpty)
            {
                Sections.Add(whole());
                return;
            }
            if (firstIsEmpty && !secondIsEmpty)
            {
                Sections.Add(downstream());
                return;
            }
            if (firstIsEmpty && secondIsEmpty)
            {
                // The target sits where the trajectory already is, so there is nothing to draw.
                return;
            }
            Sections.Add(upstream());
            Sections.Add(downstream());
        }

        /// <summary>
        /// Did the section that was just added actually arrive at the target?
        /// </summary>
        private bool Arrived(TrajectoryPoint3D reached, TargetAxis target)
        {
            if (reached == null ||
                !Numeric.IsDefined(reached.X) || !Numeric.IsDefined(reached.Y) || !Numeric.IsDefined(reached.Z))
            {
                return false;
            }
            double dx = (double)(reached.X - target.Station.X);
            double dy = (double)(reached.Y - target.Station.Y);
            double dz = (double)(reached.Z - target.Station.Z);
            if (System.Math.Sqrt(dx * dx + dy * dy + dz * dz) > PositionAccuracy)
            {
                return false;
            }
            if (!target.HasAxis)
            {
                return true;
            }
            return AttitudeMiss(reached, target.Station) <= AttitudeAccuracy;
        }

        /// <summary>
        /// The first section that does not carry on from the one before it, or -1 when they all do.
        /// </summary>
        private int FindDiscontinuity()
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                ArcSection section = Sections[i];
                if (section.Start == null || section.End == null ||
                    !Numeric.IsDefined(section.Start.Abscissa) || !Numeric.IsDefined(section.End.Abscissa))
                {
                    return i;
                }
                if (Numeric.LT(section.End.Abscissa, section.Start.Abscissa))
                {
                    return i;
                }
                if (i == 0)
                {
                    continue;
                }
                TrajectoryPoint3D before = Sections[i - 1].End;
                if (!before.EQ(section.Start, PositionAccuracy))
                {
                    return i;
                }
                if (AttitudeMiss(before, section.Start) > AttitudeAccuracy)
                {
                    return i;
                }
                if (!Numeric.EQ(before.Abscissa, section.Start.Abscissa, PositionAccuracy))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// The angle between two attitudes, which counts the azimuth for nothing at the vertical and wraps
        /// correctly everywhere else.
        /// </summary>
        private static double AttitudeMiss(TrajectoryPoint3D got, TrajectoryPoint3D wanted)
        {
            if (!Numeric.IsDefined(got.Inclination) || !Numeric.IsDefined(got.Azimuth) ||
                !Numeric.IsDefined(wanted.Inclination) || !Numeric.IsDefined(wanted.Azimuth))
            {
                return double.PositiveInfinity;
            }
            GetTangent(got, out double ax, out double ay, out double az);
            GetTangent(wanted, out double bx, out double by, out double bz);
            double crossX = ay * bz - az * by;
            double crossY = az * bx - ax * bz;
            double crossZ = ax * by - ay * bx;
            return System.Math.Atan2(System.Math.Sqrt(crossX * crossX + crossY * crossY + crossZ * crossZ),
                                     ax * bx + ay * by + az * bz);
        }

        private static void GetTangent(TrajectoryPoint3D point, out double x, out double y, out double z)
        {
            double sinInclination = System.Math.Sin((double)point.Inclination);
            x = sinInclination * System.Math.Cos((double)point.Azimuth);
            y = sinInclination * System.Math.Sin((double)point.Azimuth);
            z = System.Math.Cos((double)point.Inclination);
        }

        /// <summary>
        /// The along hole distance from the start of the trajectory to its end.
        /// </summary>
        public double? Length
        {
            get
            {
                if (Sections.Count == 0)
                {
                    return null;
                }
                TrajectoryPoint3D first = Sections[0].Start;
                TrajectoryPoint3D last = Sections[Sections.Count - 1].End;
                if (!Numeric.IsDefined(first.Abscissa) || !Numeric.IsDefined(last.Abscissa))
                {
                    return null;
                }
                return last.Abscissa - first.Abscissa;
            }
        }

        /// <summary>
        /// The station at a given measured depth along the trajectory, or a point that says it is undefined
        /// when the depth lies outside it.
        /// </summary>
        public CurvilinearPoint3D InterpolateAtMD(double md)
        {
            foreach (ArcSection section in Sections)
            {
                if (section.Start == null || section.End == null ||
                    !Numeric.IsDefined(section.Start.Abscissa) || !Numeric.IsDefined(section.End.Abscissa))
                {
                    continue;
                }
                if (Numeric.LE(md, section.End.Abscissa) || section == Sections[Sections.Count - 1])
                {
                    if (Numeric.GE(md, section.Start.Abscissa))
                    {
                        return section.InterpolateAtMD(md);
                    }
                }
            }
            TrajectoryPoint3D outside = new TrajectoryPoint3D();
            outside.SetUndefined();
            return outside;
        }
    }
}
