using System;
using System.Collections.Generic;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// A well path made of sections drawn with circular arcs, with constant build and turn curves or with
    /// constant curvature and toolface curves, worked out from a completely defined starting station and
    /// from quantities imposed here and there along it.
    ///
    /// Whatever the curve, a section leaving a known station has three degrees of freedom. A path of n
    /// sections therefore needs three times n quantities imposed on it, and they do not have to be spread
    /// evenly: a section carrying one and another carrying five settle a path of two sections just as two
    /// sections carrying three each do. What a surplus on a later section does is reach back and settle
    /// the sections before it, since where they end is where it begins.
    ///
    /// That is what makes this more than a section by section calculation, and it is done here by writing
    /// every curve in the one form they all share. A section leaving a known station is settled by its
    /// length and the two parameters of its curve - the curvature and the toolface angle for a circular
    /// arc and for a constant curvature and toolface curve, the build up rate and the turn rate for a
    /// constant build and turn curve - and running those forward gives the station it ends on. A run of
    /// sections is then a function of three numbers each, every quantity imposed is one equation, and the
    /// two counts agree by construction. What is left is an ordinary square system, solved by Newton from
    /// a guess laid out along the path. The quantities imposed directly on a curve are not solved for at
    /// all but put straight in, which leaves the system as small as it can be.
    ///
    /// A run of sections whose quantities already add up to three times its length is settled on its own,
    /// without reference to what follows, so a path divides into the shortest such runs and each is worked
    /// out in turn. Where a run is a single section carrying exactly its three quantities, the closed form
    /// that section already knows is used and nothing is solved numerically at all.
    /// </summary>
    [Serializable]
    public class ComplexPath
    {
        /// <summary>
        /// The station the path sets off from, which has to be completely defined: position, attitude and
        /// measured depth.
        /// </summary>
        public TrajectoryPoint3D Start { get; set; } = new TrajectoryPoint3D();

        /// <summary>
        /// The sections, in order.
        /// </summary>
        public List<ComplexPathSection> Sections { get; } = new List<ComplexPathSection>();

        /// <summary>
        /// The sections as they came out, in order, once Calculate has succeeded.
        /// </summary>
        public List<ArcSection> SolvedSections { get; } = new List<ArcSection>();

        /// <summary>
        /// What went wrong, when something did.
        /// </summary>
        public ComplexPathFailureReason FailureReason { get; private set; } = ComplexPathFailureReason.None;

        /// <summary>
        /// The section the trouble was with, or -1.
        /// </summary>
        public int FailedSectionIndex { get; private set; } = -1;

        /// <summary>
        /// How closely the quantities imposed have to be honoured, in metres for lengths and positions and
        /// in radians for angles. Curvatures are held to this divided by a length, so that a curvature and
        /// a position are asked for to the same relative accuracy.
        /// </summary>
        public double Accuracy { get; set; } = 1.0e-6;

        /// <summary>
        /// What went wrong, in a sentence.
        /// </summary>
        public string FailureDescription
        {
            get
            {
                switch (FailureReason)
                {
                    case ComplexPathFailureReason.None:
                        return "nothing went wrong";
                    case ComplexPathFailureReason.UndefinedStart:
                        return "the station the path sets off from is not completely defined";
                    case ComplexPathFailureReason.SectionWithoutParameters:
                        return "section " + FailedSectionIndex + " imposes nothing, so where it hands over "
                             + "to the next one could slide along the path without changing anything";
                    case ComplexPathFailureReason.ParameterOfAnotherCurve:
                        return "section " + FailedSectionIndex + " imposes a parameter belonging to another "
                             + "kind of curve than the one it is drawn with";
                    case ComplexPathFailureReason.WrongNumberOfParameters:
                        return "the path imposes " + TotalParameterCount() + " quantities where a path of "
                             + Sections.Count + " sections needs " + (3 * Sections.Count);
                    case ComplexPathFailureReason.OverDeterminedSections:
                        return "the sections up to and including section " + FailedSectionIndex + " impose "
                             + "more than the three degrees of freedom each of them has, and a surplus "
                             + "cannot be answered by the sections that come after it";
                    case ComplexPathFailureReason.NotSolved:
                        return "no path of these curves satisfying the quantities imposed was found, for the "
                             + "run of sections ending at section " + FailedSectionIndex;
                    case ComplexPathFailureReason.ParameterNotHonoured:
                        return "a path was worked out but section " + FailedSectionIndex + " does not honour "
                             + "one of the quantities imposed on it";
                    default:
                        return "unknown";
                }
            }
        }

        /// <summary>
        /// Add a section drawn with the given curve.
        /// </summary>
        public ComplexPathSection AddSection(SectionCurveType curveType)
        {
            ComplexPathSection section = new ComplexPathSection(curveType);
            Sections.Add(section);
            return section;
        }

        private int TotalParameterCount()
        {
            int total = 0;
            foreach (ComplexPathSection section in Sections)
            {
                if (section != null)
                {
                    total += section.ParameterCount;
                }
            }
            return total;
        }

        /// <summary>
        /// Work out the path.
        /// </summary>
        public bool Calculate()
        {
            SolvedSections.Clear();
            FailureReason = ComplexPathFailureReason.None;
            FailedSectionIndex = -1;
            // The answer is written back onto the sections, so what was asked for has to be kept aside
            // before that happens or the check afterwards would be comparing the answer with itself.
            RememberWhatWasImposed();
            foreach (ComplexPathSection section in Sections)
            {
                if (section != null)
                {
                    section.Solved = null;
                }
            }

            if (Start == null || Start.IsUndefined() ||
                !Numeric.IsDefined(Start.X) || !Numeric.IsDefined(Start.Y) || !Numeric.IsDefined(Start.Z) ||
                !Numeric.IsDefined(Start.Inclination) || !Numeric.IsDefined(Start.Azimuth) ||
                !Numeric.IsDefined(Start.Abscissa))
            {
                return Fail(ComplexPathFailureReason.UndefinedStart);
            }
            if (Sections.Count == 0)
            {
                return true;
            }

            // How many quantities each section imposes, and whether they are the right ones.
            int[] counts = new int[Sections.Count];
            for (int i = 0; i < Sections.Count; i++)
            {
                ComplexPathSection section = Sections[i];
                if (section == null || section.End == null)
                {
                    return Fail(ComplexPathFailureReason.SectionWithoutParameters, i);
                }
                if (section.HasParameterOfAnotherCurve)
                {
                    return Fail(ComplexPathFailureReason.ParameterOfAnotherCurve, i);
                }
                counts[i] = section.ParameterCount;
                if (counts[i] == 0)
                {
                    return Fail(ComplexPathFailureReason.SectionWithoutParameters, i);
                }
            }

            int total = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                total += counts[i];
            }
            if (total != 3 * Sections.Count)
            {
                return Fail(ComplexPathFailureReason.WrongNumberOfParameters);
            }

            // A shortfall may be carried forward, a surplus may not. So no run of sections counted from the
            // start may impose more than three times its length, and wherever it imposes exactly that, the
            // run is settled on its own and the path may be cut there.
            List<int> blockEnds = new List<int>();
            int running = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                running += counts[i];
                if (running > 3 * (i + 1))
                {
                    return Fail(ComplexPathFailureReason.OverDeterminedSections, i);
                }
                if (running == 3 * (i + 1))
                {
                    blockEnds.Add(i);
                }
            }

            TrajectoryPoint3D station = new TrajectoryPoint3D();
            station.Set(Start);
            int from = 0;
            foreach (int to in blockEnds)
            {
                if (!SolveBlock(station, from, to))
                {
                    return false;
                }
                station = new TrajectoryPoint3D();
                station.Set(Sections[to].Solved.End);
                from = to + 1;
            }
            return true;
        }

        private bool Fail(ComplexPathFailureReason reason, int sectionIndex = -1)
        {
            FailureReason = reason;
            FailedSectionIndex = sectionIndex;
            return false;
        }

        // ------------------------------------------------------------------------------------------
        // Working out one run of sections
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Work out the sections from first to last, which between them impose exactly three times as many
        /// quantities as there are of them and so are settled without reference to what follows.
        /// </summary>
        private bool SolveBlock(TrajectoryPoint3D station, int first, int last)
        {
            // A single section carrying exactly its own three quantities is what the sections already know
            // how to do in closed form, so nothing is solved numerically here.
            if (first == last && Sections[first].ParameterCount == 3)
            {
                ArcSection direct = BuildFromSpecification(station, Sections[first]);
                if (direct != null && direct.Calculate() && Usable(direct.End))
                {
                    Record(first, direct);
                    return HonoursItsParameters(first) ? true
                         : Fail(ComplexPathFailureReason.ParameterNotHonoured, first);
                }
                // Otherwise fall through and let the general solve try, which reaches combinations the
                // closed forms do not carry.
            }

            int count = last - first + 1;
            Unknown[] unknowns = ListUnknowns(first, last);
            Constraint[] constraints = ListConstraints(first, last);
            if (unknowns.Length != constraints.Length)
            {
                // The counting above makes these agree; this is here so that a mistake in it is caught
                // rather than quietly producing a system that cannot be square.
                return Fail(ComplexPathFailureReason.WrongNumberOfParameters, last);
            }
            if (unknowns.Length == 0)
            {
                // Everything was imposed directly on the curves, so there is nothing to solve.
                double[] nothing = new double[0];
                if (!Forward(station, first, last, nothing, out ArcSection[] built))
                {
                    return Fail(ComplexPathFailureReason.NotSolved, last);
                }
                return Accept(first, built);
            }

            double lengthScale = LengthScale(station, first, last);
            foreach (double[] guess in Guesses(station, first, last, unknowns, lengthScale))
            {
                if (Settle(station, first, last, unknowns, constraints, lengthScale, guess,
                           out ArcSection[] built))
                {
                    return Accept(first, built);
                }
            }
            return Fail(ComplexPathFailureReason.NotSolved, last);
        }

        private bool Accept(int first, ArcSection[] built)
        {
            for (int k = 0; k < built.Length; k++)
            {
                Record(first + k, built[k]);
            }
            for (int k = 0; k < built.Length; k++)
            {
                if (!HonoursItsParameters(first + k))
                {
                    return Fail(ComplexPathFailureReason.ParameterNotHonoured, first + k);
                }
            }
            return true;
        }

        /// <summary>
        /// Write what came out back onto the section and onto the list of solved sections.
        /// </summary>
        private void Record(int index, ArcSection built)
        {
            ComplexPathSection section = Sections[index];
            section.Solved = built;
            section.End.Set(built.End);
            ReadCurve(built, out double? length, out double? firstParameter, out double? secondParameter);
            section.Length = length;
            section.FirstCurveParameter = firstParameter;
            section.SecondCurveParameter = secondParameter;
            SolvedSections.Add(built);
        }

        // ------------------------------------------------------------------------------------------
        // The unknowns and the equations
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Which of the three numbers settling a section is not imposed directly on its curve.
        /// </summary>
        private enum Quantity { Length, FirstCurveParameter, SecondCurveParameter }

        private struct Unknown
        {
            public int Section;
            public Quantity Which;
        }

        /// <summary>
        /// Which quantity imposed at the end of a section has to be met.
        /// </summary>
        private enum Imposed { X, Y, Z, Inclination, Azimuth, Abscissa }

        private struct Constraint
        {
            public int Section;
            public Imposed Which;
            public double Value;
        }

        private Unknown[] ListUnknowns(int first, int last)
        {
            List<Unknown> unknowns = new List<Unknown>();
            for (int i = first; i <= last; i++)
            {
                ComplexPathSection section = Sections[i];
                if (!Numeric.IsDefined(section.Length))
                {
                    unknowns.Add(new Unknown { Section = i, Which = Quantity.Length });
                }
                if (!Numeric.IsDefined(section.FirstCurveParameter))
                {
                    unknowns.Add(new Unknown { Section = i, Which = Quantity.FirstCurveParameter });
                }
                if (!Numeric.IsDefined(section.SecondCurveParameter))
                {
                    unknowns.Add(new Unknown { Section = i, Which = Quantity.SecondCurveParameter });
                }
            }
            return unknowns.ToArray();
        }

        private Constraint[] ListConstraints(int first, int last)
        {
            List<Constraint> constraints = new List<Constraint>();
            for (int i = first; i <= last; i++)
            {
                ComplexPathSection section = Sections[i];
                if (Numeric.IsDefined(section.End.X))
                {
                    constraints.Add(new Constraint { Section = i, Which = Imposed.X, Value = (double)section.End.X });
                }
                if (Numeric.IsDefined(section.End.Y))
                {
                    constraints.Add(new Constraint { Section = i, Which = Imposed.Y, Value = (double)section.End.Y });
                }
                if (Numeric.IsDefined(section.End.Z))
                {
                    constraints.Add(new Constraint { Section = i, Which = Imposed.Z, Value = (double)section.End.Z });
                }
                if (Numeric.IsDefined(section.End.Inclination))
                {
                    constraints.Add(new Constraint { Section = i, Which = Imposed.Inclination, Value = (double)section.End.Inclination });
                }
                if (Numeric.IsDefined(section.End.Azimuth))
                {
                    constraints.Add(new Constraint { Section = i, Which = Imposed.Azimuth, Value = (double)section.End.Azimuth });
                }
                // The measured depth at the end is an equation only when the length was not imposed; when
                // it was, the two say the same thing and the length was put straight in.
                if (!Numeric.IsDefined(section.Length) && Numeric.IsDefined(section.End.Abscissa))
                {
                    constraints.Add(new Constraint { Section = i, Which = Imposed.Abscissa, Value = (double)section.End.Abscissa });
                }
            }
            return constraints.ToArray();
        }

        // ------------------------------------------------------------------------------------------
        // Running the sections forward
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Build the sections of a run from the station before them and from the numbers settling them,
        /// taking each number either from the unknowns being solved for or from what was imposed.
        /// </summary>
        private bool Forward(TrajectoryPoint3D station, int first, int last, double[] values,
                             out ArcSection[] built)
        {
            int count = last - first + 1;
            built = new ArcSection[count];
            TrajectoryPoint3D current = station;
            int at = 0;
            for (int i = first; i <= last; i++)
            {
                ComplexPathSection section = Sections[i];
                double length = Numeric.IsDefined(section.Length) ? (double)section.Length : values[at++];
                double firstParameter = Numeric.IsDefined(section.FirstCurveParameter)
                    ? (double)section.FirstCurveParameter : values[at++];
                double secondParameter = Numeric.IsDefined(section.SecondCurveParameter)
                    ? (double)section.SecondCurveParameter : values[at++];
                if (!Numeric.GT(length, 0.0) || double.IsNaN(firstParameter) || double.IsNaN(secondParameter))
                {
                    return false;
                }
                ArcSection piece = BuildForward(current, section.CurveType, length, firstParameter, secondParameter);
                if (piece == null || !Usable(piece.End))
                {
                    return false;
                }
                built[i - first] = piece;
                current = piece.End;
            }
            return true;
        }

        /// <summary>
        /// One section of the given kind, leaving a station with a length and the two parameters of its
        /// curve. This is the one form all three curves share, and the whole construction rests on it.
        /// </summary>
        private static ArcSection BuildForward(TrajectoryPoint3D station, SectionCurveType type,
                                               double length, double firstParameter, double secondParameter)
        {
            switch (type)
            {
                case SectionCurveType.CircularArc:
                    {
                        CircularArcSection arc = new CircularArcSection(station, new TrajectoryPoint3D());
                        arc.Circle.Curvature = firstParameter;
                        arc.Circle.ReferenceToolface = secondParameter;
                        arc.Circle.Length = length;
                        return arc.CalculateLDT() ? arc : null;
                    }
                case SectionCurveType.ConstantBuildAndTurn:
                    {
                        BuildAndTurnArcSection curve = new BuildAndTurnArcSection(station, new TrajectoryPoint3D());
                        curve.BuildAndTurn.BUR = firstParameter;
                        curve.BuildAndTurn.TR = secondParameter;
                        curve.BuildAndTurn.Length = length;
                        return curve.CalculateLBT() ? curve : null;
                    }
                case SectionCurveType.ConstantCurvatureAndToolface:
                    {
                        ConstantCurvatureAndToolfaceArcSection curve =
                            new ConstantCurvatureAndToolfaceArcSection(station, new TrajectoryPoint3D());
                        curve.CTCCurve.Curvature = firstParameter;
                        curve.CTCCurve.Toolface = secondParameter;
                        curve.CTCCurve.Length = length;
                        return curve.CalculateLDT() ? curve : null;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// A section of the right kind carrying what the caller imposed, for the closed forms to work on.
        /// </summary>
        private static ArcSection BuildFromSpecification(TrajectoryPoint3D station, ComplexPathSection section)
        {
            switch (section.CurveType)
            {
                case SectionCurveType.CircularArc:
                    {
                        CircularArcSection arc = new CircularArcSection(station, new TrajectoryPoint3D());
                        arc.End.Set(section.End);
                        arc.Circle.Curvature = section.Curvature;
                        arc.Circle.ReferenceToolface = section.Toolface;
                        arc.Circle.Length = section.Length;
                        return arc;
                    }
                case SectionCurveType.ConstantBuildAndTurn:
                    {
                        BuildAndTurnArcSection curve = new BuildAndTurnArcSection(station, new TrajectoryPoint3D());
                        curve.End.Set(section.End);
                        curve.BuildAndTurn.BUR = section.BUR;
                        curve.BuildAndTurn.TR = section.TurnRate;
                        curve.BuildAndTurn.Length = section.Length;
                        return curve;
                    }
                case SectionCurveType.ConstantCurvatureAndToolface:
                    {
                        ConstantCurvatureAndToolfaceArcSection curve =
                            new ConstantCurvatureAndToolfaceArcSection(station, new TrajectoryPoint3D());
                        curve.End.Set(section.End);
                        curve.CTCCurve.Curvature = section.Curvature;
                        curve.CTCCurve.Toolface = section.Toolface;
                        curve.CTCCurve.Length = section.Length;
                        return curve;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// The length and the two curve parameters of a section that has been worked out.
        /// </summary>
        private static void ReadCurve(ArcSection section, out double? length,
                                      out double? firstParameter, out double? secondParameter)
        {
            if (section is CircularArcSection arc)
            {
                length = arc.Circle.Length;
                firstParameter = arc.Circle.Curvature;
                secondParameter = arc.Circle.ReferenceToolface;
                return;
            }
            if (section is BuildAndTurnArcSection turn)
            {
                length = turn.BuildAndTurn.Length;
                firstParameter = turn.BuildAndTurn.BUR;
                secondParameter = turn.BuildAndTurn.TR;
                return;
            }
            if (section is ConstantCurvatureAndToolfaceArcSection toolface)
            {
                length = toolface.CTCCurve.Length;
                firstParameter = toolface.CTCCurve.Curvature;
                secondParameter = toolface.CTCCurve.Toolface;
                return;
            }
            length = null;
            firstParameter = null;
            secondParameter = null;
        }

        private static bool Usable(TrajectoryPoint3D station)
        {
            return station != null &&
                   Numeric.IsDefined(station.X) && Numeric.IsDefined(station.Y) && Numeric.IsDefined(station.Z) &&
                   Numeric.IsDefined(station.Inclination) && Numeric.IsDefined(station.Azimuth) &&
                   Numeric.IsDefined(station.Abscissa);
        }

        // ------------------------------------------------------------------------------------------
        // The equations, and settling them
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// How far the sections built from a set of numbers are from the quantities imposed on them. The
        /// lengths and positions are divided by a length taken from the run itself so that they sit
        /// alongside the angles, which keeps the system from being pulled about by its units.
        /// </summary>
        private bool Residuals(TrajectoryPoint3D station, int first, int last, Constraint[] constraints,
                               double lengthScale, double[] values, out double[] residuals,
                               out ArcSection[] built)
        {
            residuals = new double[constraints.Length];
            if (!Forward(station, first, last, values, out built))
            {
                return false;
            }
            for (int c = 0; c < constraints.Length; c++)
            {
                TrajectoryPoint3D end = built[constraints[c].Section - first].End;
                switch (constraints[c].Which)
                {
                    case Imposed.X:
                        residuals[c] = ((double)end.X - constraints[c].Value) / lengthScale;
                        break;
                    case Imposed.Y:
                        residuals[c] = ((double)end.Y - constraints[c].Value) / lengthScale;
                        break;
                    case Imposed.Z:
                        residuals[c] = ((double)end.Z - constraints[c].Value) / lengthScale;
                        break;
                    case Imposed.Abscissa:
                        residuals[c] = ((double)end.Abscissa - constraints[c].Value) / lengthScale;
                        break;
                    case Imposed.Inclination:
                        residuals[c] = (double)end.Inclination - constraints[c].Value;
                        break;
                    case Imposed.Azimuth:
                        residuals[c] = TrajectoryPoint3D.WrapToPi((double)end.Azimuth - constraints[c].Value);
                        break;
                }
                if (double.IsNaN(residuals[c]) || double.IsInfinity(residuals[c]))
                {
                    return false;
                }
            }
            return true;
        }

        private static double Norm(double[] values)
        {
            double sum = 0.0;
            for (int i = 0; i < values.Length; i++)
            {
                sum += values[i] * values[i];
            }
            return sum;
        }

        /// <summary>
        /// Newton on the square system, with the step cut back when it fails to improve. The derivatives
        /// are taken by a central difference, which costs rate of convergence rather than final accuracy.
        /// </summary>
        private bool Settle(TrajectoryPoint3D station, int first, int last, Unknown[] unknowns,
                            Constraint[] constraints, double lengthScale, double[] guess,
                            out ArcSection[] built)
        {
            built = null;
            int n = unknowns.Length;
            double[] values = (double[])guess.Clone();
            if (!Residuals(station, first, last, constraints, lengthScale, values,
                           out double[] residuals, out built))
            {
                return false;
            }
            double norm = Norm(residuals);
            double tolerance = Accuracy / System.Math.Max(1.0, lengthScale);
            double target = n * tolerance * tolerance;

            double[] steps = new double[n];
            for (int i = 0; i < n; i++)
            {
                steps[i] = StepFor(unknowns[i], lengthScale);
            }

            for (int iteration = 0; iteration < SettleIterations; iteration++)
            {
                if (norm <= target)
                {
                    return true;
                }
                double[][] jacobian = new double[constraints.Length][];
                for (int c = 0; c < constraints.Length; c++)
                {
                    jacobian[c] = new double[n];
                }
                for (int i = 0; i < n; i++)
                {
                    double[] forward = (double[])values.Clone();
                    double[] backward = (double[])values.Clone();
                    forward[i] += steps[i];
                    backward[i] -= steps[i];
                    if (!Residuals(station, first, last, constraints, lengthScale, forward, out double[] up, out _) ||
                        !Residuals(station, first, last, constraints, lengthScale, backward, out double[] down, out _))
                    {
                        return false;
                    }
                    for (int c = 0; c < constraints.Length; c++)
                    {
                        jacobian[c][i] = (up[c] - down[c]) / (2.0 * steps[i]);
                    }
                }

                double[] right = new double[n];
                for (int c = 0; c < n; c++)
                {
                    right[c] = -residuals[c];
                }
                if (!SolveLinear(jacobian, right, out double[] step))
                {
                    return false;
                }

                bool improved = false;
                double damping = 1.0;
                for (int cut = 0; cut < SettleCutbacks; cut++)
                {
                    double[] trial = new double[n];
                    for (int i = 0; i < n; i++)
                    {
                        trial[i] = values[i] + damping * step[i];
                    }
                    if (Residuals(station, first, last, constraints, lengthScale, trial,
                                  out double[] trialResiduals, out ArcSection[] trialBuilt))
                    {
                        double trialNorm = Norm(trialResiduals);
                        if (trialNorm < norm)
                        {
                            values = trial;
                            residuals = trialResiduals;
                            built = trialBuilt;
                            norm = trialNorm;
                            improved = true;
                            break;
                        }
                    }
                    damping *= 0.5;
                }
                if (!improved)
                {
                    break;
                }
            }
            return norm <= target;
        }

        private const int SettleIterations = 60;
        private const int SettleCutbacks = 40;

        /// <summary>
        /// The difference used to take a derivative in one unknown, in that unknown's own units.
        ///
        /// A length is stepped by a millionth of the run, and a rate by whatever turns the run through a
        /// millionth of a radian, so that every derivative is taken against a comparable change in where
        /// the path ends up. A toolface angle is an angle already and is stepped as one.
        /// </summary>
        private double StepFor(Unknown unknown, double lengthScale)
        {
            double scale = System.Math.Max(1.0, lengthScale);
            if (unknown.Which == Quantity.Length)
            {
                return 1.0e-6 * scale;
            }
            bool isRate = unknown.Which == Quantity.FirstCurveParameter ||
                          Sections[unknown.Section].CurveType == SectionCurveType.ConstantBuildAndTurn;
            return isRate ? 1.0e-7 / scale : 1.0e-7;
        }

        /// <summary>
        /// Solve a square system by elimination, taking the largest pivot available at each step.
        /// </summary>
        private static bool SolveLinear(double[][] matrix, double[] right, out double[] solution)
        {
            int n = right.Length;
            solution = new double[n];
            double[][] a = new double[n][];
            for (int i = 0; i < n; i++)
            {
                a[i] = new double[n + 1];
                for (int j = 0; j < n; j++)
                {
                    a[i][j] = matrix[i][j];
                }
                a[i][n] = right[i];
            }
            for (int column = 0; column < n; column++)
            {
                int pivot = column;
                for (int row = column + 1; row < n; row++)
                {
                    if (System.Math.Abs(a[row][column]) > System.Math.Abs(a[pivot][column]))
                    {
                        pivot = row;
                    }
                }
                if (System.Math.Abs(a[pivot][column]) < 1.0e-300)
                {
                    return false;
                }
                double[] swap = a[column];
                a[column] = a[pivot];
                a[pivot] = swap;
                for (int row = column + 1; row < n; row++)
                {
                    double factor = a[row][column] / a[column][column];
                    if (factor == 0.0)
                    {
                        continue;
                    }
                    for (int j = column; j <= n; j++)
                    {
                        a[row][j] -= factor * a[column][j];
                    }
                }
            }
            for (int row = n - 1; row >= 0; row--)
            {
                double sum = a[row][n];
                for (int j = row + 1; j < n; j++)
                {
                    sum -= a[row][j] * solution[j];
                }
                solution[row] = sum / a[row][row];
                if (double.IsNaN(solution[row]) || double.IsInfinity(solution[row]))
                {
                    return false;
                }
            }
            return true;
        }

        // ------------------------------------------------------------------------------------------
        // Guesses to start from
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// A length taken from the run itself, used to put the lengths and positions on the same footing as
        /// the angles and to size the guesses.
        /// </summary>
        private double LengthScale(TrajectoryPoint3D station, int first, int last)
        {
            double scale = 0.0;
            for (int i = first; i <= last; i++)
            {
                ComplexPathSection section = Sections[i];
                if (Numeric.IsDefined(section.Length))
                {
                    scale += (double)section.Length;
                }
                else if (Numeric.IsDefined(section.End.Abscissa))
                {
                    scale = System.Math.Max(scale, (double)section.End.Abscissa - (double)station.Abscissa);
                }
                else if (Numeric.IsDefined(section.End.X) && Numeric.IsDefined(section.End.Y) &&
                         Numeric.IsDefined(section.End.Z))
                {
                    double dx = (double)section.End.X - (double)station.X;
                    double dy = (double)section.End.Y - (double)station.Y;
                    double dz = (double)section.End.Z - (double)station.Z;
                    scale = System.Math.Max(scale, System.Math.Sqrt(dx * dx + dy * dy + dz * dz));
                }
            }
            if (!Numeric.GT(scale, 0.0))
            {
                scale = DefaultSectionLength * (last - first + 1);
            }
            return scale;
        }

        private const double DefaultSectionLength = 100.0;

        /// <summary>
        /// Guesses to start Newton from, in the order they are worth trying.
        ///
        /// The first lays the run out along the single arc that joins its two ends where that is known,
        /// which puts both the lengths and the curvatures near where they have to end up. The rest fall
        /// back on a straight run and then on turning progressively harder, which between them reach the
        /// solutions the first guess is too far from.
        /// </summary>
        private IEnumerable<double[]> Guesses(TrajectoryPoint3D station, int first, int last,
                                              Unknown[] unknowns, double lengthScale)
        {
            int count = last - first + 1;
            double perSection = lengthScale / count;

            double guessCurvature = 0.0;
            double guessToolface = 0.0;
            // Where the run has to finish somewhere known, the single arc that gets there says roughly how
            // hard it has to turn and which way.
            ComplexPathSection lastSection = Sections[last];
            if (Numeric.IsDefined(lastSection.End.X) && Numeric.IsDefined(lastSection.End.Y) &&
                Numeric.IsDefined(lastSection.End.Z))
            {
                CircularArcSection arc = new CircularArcSection(station, new TrajectoryPoint3D());
                arc.End.Set(lastSection.End);
                if (arc.CalculateXYZ() && Numeric.IsDefined(arc.Circle.Curvature) &&
                    Numeric.IsDefined(arc.Circle.ReferenceToolface))
                {
                    guessCurvature = (double)arc.Circle.Curvature;
                    guessToolface = (double)arc.Circle.ReferenceToolface;
                    if (Numeric.IsDefined(arc.Circle.Length) && Numeric.GT(arc.Circle.Length, 0.0))
                    {
                        perSection = (double)arc.Circle.Length / count;
                    }
                }
            }

            yield return Lay(unknowns, perSection, guessCurvature, guessToolface);
            yield return Lay(unknowns, perSection, 0.0, 0.0);
            yield return Lay(unknowns, perSection, guessCurvature, guessToolface + Numeric.PI);
            yield return Lay(unknowns, 0.5 * perSection, 2.0 * guessCurvature, guessToolface);
            yield return Lay(unknowns, 2.0 * perSection, 0.5 * guessCurvature, guessToolface);
            for (int k = 0; k < ExtraGuesses; k++)
            {
                double angle = 2.0 * Numeric.PI * k / ExtraGuesses;
                yield return Lay(unknowns, perSection, DefaultCurvature, angle);
            }
        }

        private const int ExtraGuesses = 6;
        private const double DefaultCurvature = 1.0e-3;

        private double[] Lay(Unknown[] unknowns, double length, double curvature, double toolface)
        {
            double[] values = new double[unknowns.Length];
            for (int i = 0; i < unknowns.Length; i++)
            {
                ComplexPathSection section = Sections[unknowns[i].Section];
                switch (unknowns[i].Which)
                {
                    case Quantity.Length:
                        values[i] = System.Math.Max(1.0e-3, length);
                        break;
                    case Quantity.FirstCurveParameter:
                        // For a constant build and turn curve the first parameter is a build up rate, which
                        // is the part of the turning that goes into the inclination.
                        values[i] = section.CurveType == SectionCurveType.ConstantBuildAndTurn
                            ? curvature * System.Math.Cos(toolface)
                            : curvature;
                        break;
                    case Quantity.SecondCurveParameter:
                        values[i] = section.CurveType == SectionCurveType.ConstantBuildAndTurn
                            ? curvature * System.Math.Sin(toolface)
                            : toolface;
                        break;
                }
            }
            return values;
        }

        // ------------------------------------------------------------------------------------------
        // Checking what came out
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Does the section that came out honour everything that was imposed on it? Nothing is handed back
        /// until this holds for every section.
        /// </summary>
        private bool HonoursItsParameters(int index)
        {
            ComplexPathSection section = Sections[index];
            ArcSection built = section.Solved;
            if (built == null || !Usable(built.End))
            {
                return false;
            }
            double scale = System.Math.Max(1.0, System.Math.Abs((double)built.End.Abscissa - (double)built.Start.Abscissa));
            ComplexPathSection imposed = Original(index);

            if (Numeric.IsDefined(imposed.End.X) &&
                System.Math.Abs((double)built.End.X - (double)imposed.End.X) > Accuracy * scale)
            {
                return false;
            }
            if (Numeric.IsDefined(imposed.End.Y) &&
                System.Math.Abs((double)built.End.Y - (double)imposed.End.Y) > Accuracy * scale)
            {
                return false;
            }
            if (Numeric.IsDefined(imposed.End.Z) &&
                System.Math.Abs((double)built.End.Z - (double)imposed.End.Z) > Accuracy * scale)
            {
                return false;
            }
            if (Numeric.IsDefined(imposed.End.Abscissa) &&
                System.Math.Abs((double)built.End.Abscissa - (double)imposed.End.Abscissa) > Accuracy * scale)
            {
                return false;
            }
            if (Numeric.IsDefined(imposed.End.Inclination) &&
                System.Math.Abs((double)built.End.Inclination - (double)imposed.End.Inclination) > Accuracy)
            {
                return false;
            }
            if (Numeric.IsDefined(imposed.End.Azimuth) &&
                System.Math.Abs(TrajectoryPoint3D.WrapToPi((double)built.End.Azimuth - (double)imposed.End.Azimuth)) > Accuracy)
            {
                return false;
            }
            ReadCurve(built, out double? length, out double? firstParameter, out double? secondParameter);
            if (Numeric.IsDefined(imposed.Length) &&
                System.Math.Abs((double)length - (double)imposed.Length) > Accuracy * scale)
            {
                return false;
            }
            if (Numeric.IsDefined(imposed.FirstCurveParameter) &&
                System.Math.Abs((double)firstParameter - (double)imposed.FirstCurveParameter) > Accuracy / scale)
            {
                return false;
            }
            if (Numeric.IsDefined(imposed.SecondCurveParameter) &&
                System.Math.Abs(TrajectoryPoint3D.WrapToPi((double)secondParameter - (double)imposed.SecondCurveParameter)) > Accuracy)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// What was imposed on a section before the answer was written back onto it.
        /// </summary>
        private ComplexPathSection Original(int index)
        {
            return originals_ != null && index < originals_.Count ? originals_[index] : Sections[index];
        }

        private List<ComplexPathSection> originals_ = null;

        /// <summary>
        /// Keep a copy of what was imposed, since the answer is written back onto the sections and the
        /// check afterwards has to compare against what was asked for rather than against itself.
        /// </summary>
        internal void RememberWhatWasImposed()
        {
            originals_ = new List<ComplexPathSection>();
            foreach (ComplexPathSection section in Sections)
            {
                ComplexPathSection copy = new ComplexPathSection(section.CurveType);
                copy.End.Set(section.End);
                copy.Length = section.Length;
                copy.Curvature = section.Curvature;
                copy.Toolface = section.Toolface;
                copy.BUR = section.BUR;
                copy.TurnRate = section.TurnRate;
                originals_.Add(copy);
            }
        }
    }
}
