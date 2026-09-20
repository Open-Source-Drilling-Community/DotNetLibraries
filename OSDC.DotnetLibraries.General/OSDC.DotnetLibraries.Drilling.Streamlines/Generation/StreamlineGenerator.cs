using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// the settings of a whole generation
    /// </summary>
    public class StreamlineGeneratorOptions
    {
        /// <summary>
        /// how the octree is laid out
        /// </summary>
        public StreamlineGridOptions Grid { get; set; } = new StreamlineGridOptions();

        /// <summary>
        /// how the flow is solved
        /// </summary>
        public FlowFieldOptions Flow { get; set; } = new FlowFieldOptions();

        /// <summary>
        /// how the streamlines are traced
        /// </summary>
        public StreamlineTracerOptions Tracer { get; set; } = new StreamlineTracerOptions();

        /// <summary>
        /// how far the direction a planned well leaves in is held, m
        /// </summary>
        public double ConduitLength { get; set; } = 30.0;

        /// <summary>
        /// how many streamlines to produce
        /// </summary>
        public int StreamlineCount { get; set; } = 100;

        /// <summary>
        /// How much easier the medium is made along the direction a source leaves in, over
        /// <see cref="SourceGuideLength"/>. One leaves the medium isotropic there.
        /// <para>
        /// The conduit already imposes that direction exactly, but only while it lasts: at its last cell
        /// the constraint stops dead and a homogeneous medium lets the flow spread as a point source
        /// would. A guide carries the preference on past the conduit and lets it fade, so the flow turns
        /// over a length rather than at a face.
        /// </para>
        /// </summary>
        public double SourceGuideContrast { get; set; } = 1.0;

        /// <summary>
        /// how far past a source its guide reaches, m
        /// </summary>
        public double SourceGuideLength { get; set; } = 300.0;

        /// <summary>
        /// how far across the direction a source guide reaches, m
        /// </summary>
        public double SourceGuideWidth { get; set; } = 150.0;

        /// <summary>
        /// How much easier the medium is made along a spine running the whole way from each source to the
        /// target, honouring the direction at both ends. One leaves it out.
        /// <para>
        /// This is the guide that turns. A guide of fixed direction only moves the corner to wherever the
        /// guide stops; a guide whose direction follows a curve asks the flow to turn the way the curve
        /// turns. It replaces the two end guides rather than joining them, so setting this leaves
        /// <see cref="SourceGuideContrast"/> alone.
        /// </para>
        /// </summary>
        public double SpineGuideContrast { get; set; } = 1.0;

        /// <summary>
        /// how far across the spine its guide reaches, m
        /// </summary>
        public double SpineGuideWidth { get; set; } = 150.0;

        /// <summary>
        /// Whether the conduits at both ends follow a curve instead of a straight line.
        /// <para>
        /// A straight conduit holds its direction and then stops, so the whole turn has to be taken at
        /// its mouth: the departure leaves vertical and kinks, and the landing is a chimney of dead
        /// straight hole standing over the target. A conduit shaped to the reference spine turns as it
        /// goes, at the spine's own rate, so it hands the flow over already pointing where it is going
        /// and the same length of hole does useful work.
        /// </para>
        /// <para>
        /// What it costs is the exactness of the arrival. A chain of cells following a curve is a
        /// staircase at the scale of one cell, so the direction is right over a station length but not
        /// over one segment. Leave this off where the arrival angle has to be exact.
        /// </para>
        /// </summary>
        public bool ShapeDepartureToSpine { get; set; } = false;

        /// <summary>
        /// Whether the landing conduits follow the same curve rather than standing straight on the
        /// target normal.
        /// <para>
        /// Shaping the landing is the weaker of the two. A chain of cells is face connected, so it can
        /// only step along an axis, and a tube whose drift is small next to its cells becomes a straight
        /// run broken by occasional square steps — which reads as a far sharper turn than the curve it is
        /// meant to follow. The departure runs through the fine cells around the slot and does not suffer
        /// from it; the landing sits in cells the size of the target's thickness and does.
        /// </para>
        /// </summary>
        public bool ShapeLandingToSpine { get; set; } = false;

        /// <summary>
        /// The curve the conduits are shaped to, or null to use a plain curve between the two ends.
        /// <para>
        /// A curve drawn straight between the ends knows nothing about the wells in the way. Measured on
        /// Ullrigg it enters one of them 163 m out and stays inside for 57 of its 513 samples, so a
        /// conduit laid along it runs into a wall and stops there, which is worse than no conduit at all.
        /// A path that has already been traced cannot do that, having come through open cells the whole
        /// way, which is what <see cref="StreamlineGenerator.GenerateShaped"/> uses.
        /// </para>
        /// </summary>
        public IReadOnlyList<Point3D>? ReferencePath { get; set; } = null;

        /// <summary>
        /// How much easier the medium is along a channel following the spine than far from it. One
        /// leaves the medium uniform.
        /// <para>
        /// This is the scalar way of shaping a corridor and it is not the same thing as
        /// <see cref="SpineGuideContrast"/>. A guide makes a direction cheap and so collimates the flow;
        /// a channel makes a neighbourhood cheap and leaves the flow free inside it. A source and a sink
        /// in a uniform medium give a dipole, whose streamlines are a family of bulging loops with only
        /// the axial one straight, and a channel is what breaks that: the flow concentrates where the
        /// resistance is low, and since the streamlines carry equal shares of the flux they concentrate
        /// with it.
        /// </para>
        /// </summary>
        public double ChannelContrast { get; set; } = 1.0;

        /// <summary>
        /// how far the channel reaches at the slot and at the target, m
        /// </summary>
        public double ChannelNarrowWidth { get; set; } = 25.0;

        /// <summary>
        /// how far it reaches midway between them, m, where there is a choice worth leaving open
        /// </summary>
        public double ChannelWideWidth { get; set; } = 150.0;

        /// <summary>
        /// the curves the channel follows, or null to use a single spine between the two ends
        /// </summary>
        public IReadOnlyList<IReadOnlyList<Point3D>>? ChannelSpines { get; set; } = null;

        /// <summary>
        /// how the spines are pushed clear of the obstacles before a channel is built on them
        /// </summary>
        public SpineRelaxerOptions Relaxer { get; set; } = new SpineRelaxerOptions();

        /// <summary>
        /// How much easier the medium is made along the way the conduit is pointing, just past its
        /// mouth. One leaves it out.
        /// <para>
        /// A conduit gives out through one cell, so whatever a channel does with the route as a whole,
        /// the flow leaves that cell as a point source and fans over the full solid angle: measured on
        /// Ullrigg the median attitude a hundred metres past the outlet is forty to fifty degrees off the
        /// way the conduit was pointing, whatever length the conduit is. This is the one place a
        /// directional bias is wanted, and it is wanted only just past the mouth and only downstream of
        /// it.
        /// </para>
        /// </summary>
        public double OutletGuideContrast { get; set; } = 1.0;

        /// <summary>
        /// how far past the mouth the outlet guide reaches, m
        /// </summary>
        public double OutletGuideLength { get; set; } = 100.0;

        /// <summary>
        /// how far across it reaches, m
        /// </summary>
        public double OutletGuideWidth { get; set; } = 30.0;

        /// <summary>
        /// How far in from the edge of the outlet face the streamlines are launched, as a fraction of
        /// the cell, between zero and a half. The launches fill the disc that leaves.
        /// <para>
        /// The face is bounded by the conduit's own walls, and a wall is where the flow separates: a
        /// streamline released a few centimetres from one whips around the rim of the tube and reports a
        /// turn that says more about where it was put than about the corridor. Measured with nothing in
        /// the way at all, the same field gave a sharpest turn of 70.5 deg per 30 m over thirty six
        /// launches spanning eight to ninety two per cent of the face, and 24.9 over four launches at
        /// twenty five and seventy five per cent.
        /// </para>
        /// </summary>
        public double LaunchInset { get; set; } = 0.25;
    }

    /// <summary>
    /// what came out of a generation
    /// </summary>
    public class StreamlineGenerationResult
    {
        /// <summary>
        /// the streamlines produced, in the order they were launched
        /// </summary>
        public List<Streamline> Streamlines { get; internal set; } = new List<Streamline>();

        /// <summary>
        /// the octree they were produced on
        /// </summary>
        public StreamlineGrid? Grid { get; internal set; } = null;

        /// <summary>
        /// the field they follow, or null when none was solved
        /// </summary>
        public FlowField? Field { get; internal set; } = null;

        /// <summary>
        /// whether the geometry allowed anything to be produced
        /// </summary>
        public StreamlineGridStatus Status { get; internal set; } = StreamlineGridStatus.NotConnected;

        /// <summary>
        /// how many streamlines ended each way, indexed by <see cref="TraceOutcome"/>
        /// </summary>
        public int[] Outcomes { get; internal set; } = new int[5];

        /// <summary>
        /// how each produced streamline ended, in the same order as <see cref="Streamlines"/>. Only the
        /// ones that reached the sink are candidate well paths; the rest stopped somewhere and are kept
        /// because where they stopped is worth seeing.
        /// </summary>
        public List<TraceOutcome> StreamlineOutcomes { get; internal set; } = new List<TraceOutcome>();

        /// <summary>
        /// how many cells of the target the arrival constraint left reachable. Below the number the grid
        /// found, the constraint has shut part of the target off.
        /// </summary>
        public int ServedTargetCellCount { get; internal set; }

        /// <summary>
        /// The sharpest turn of the least sharply turning streamline, rad per 30 m.
        /// <para>
        /// A corridor is a set of alternatives, so the one that turns least is the one worth drilling, and
        /// this is the dogleg a bit would have to build to follow it. It is not a property the flow solve
        /// controls, which is exactly why it is worth reporting: a corridor can be perfectly conservative,
        /// clear of every obstacle, and still turn faster than anything can be drilled.
        /// </para>
        /// </summary>
        public double BestWorstDogleg { get; internal set; } = double.NaN;

        /// <summary>
        /// how many cells each source's conduit took, and how far it reached, m
        /// </summary>
        public List<(int Cells, double Reach)> Conduits { get; internal set; }
            = new List<(int, double)>();

        /// <summary>
        /// the centres of the cells each source's conduit runs through, which is the stretch of the well
        /// path that the flow solve did not produce and the streamlines therefore do not show
        /// </summary>
        public List<List<Point3D>> ConduitPaths { get; internal set; } = new List<List<Point3D>>();

        /// <summary>
        /// where a two pass run got its spine from: the gentlest dogleg of the first pass, rad per 30 m,
        /// or NaN when only one pass was run
        /// </summary>
        public double ScoutDogleg { get; internal set; } = double.NaN;

        /// <summary>
        /// the curves a channel was built on, one per way round the obstacles that the scout found
        /// </summary>
        public List<List<Point3D>> Spines { get; internal set; } = new List<List<Point3D>>();

        /// <summary>
        /// what the relaxation made of each of them, in the same order
        /// </summary>
        public List<SpineRelaxerResult> SpineQuality { get; internal set; }
            = new List<SpineRelaxerResult>();

        /// <summary>
        /// measures <see cref="BestWorstDogleg"/> over the streamlines produced
        /// </summary>
        internal void MeasureCurvature()
        {
            double best = double.NaN;
            for (int i = 0; i < Streamlines.Count; i++)
            {
                // a path that stopped short is not a candidate, and since a streamline now carries its
                // conduit a stub is mostly straight hole and would otherwise report the gentlest turn
                // of the lot
                if (i < StreamlineOutcomes.Count && StreamlineOutcomes[i] != TraceOutcome.ReachedSink)
                {
                    continue;
                }
                double worst = StreamlineCurvature.GetWorst(Streamlines[i]);
                if (worst > 0 && (double.IsNaN(best) || worst < best))
                {
                    best = worst;
                }
            }
            BestWorstDogleg = best;
        }

        /// <summary>
        /// how many of them reached the target
        /// </summary>
        public int ArrivedCount
        {
            get
            {
                return Outcomes[(int)TraceOutcome.ReachedSink];
            }
        }

        /// <summary>
        /// a one line account, for a log or an assertion message
        /// </summary>
        public string Describe()
        {
            List<string> parts = new List<string>();
            foreach (TraceOutcome outcome in Enum.GetValues<TraceOutcome>())
            {
                if (Outcomes[(int)outcome] > 0)
                {
                    parts.Add($"{Outcomes[(int)outcome]} {outcome}");
                }
            }
            string flow = Field == null ? "no solve"
                : $"{Field.UnknownCount} unknowns, {Field.IterationCount} iterations, "
                  + $"residual {Field.Residual:E1}, imbalance {Field.WorstCellImbalance:E1}";
            string curvature = double.IsNaN(BestWorstDogleg) ? ""
                : $", gentlest path turns {BestWorstDogleg * 180.0 / System.Math.PI:F1} deg/30m";
            return $"{Status}; {flow}; {ServedTargetCellCount} target cells served; "
                   + $"{Streamlines.Count} streamlines [{string.Join(", ", parts)}]{curvature}";
        }
    }

    /// <summary>
    /// Produces candidate well paths from a slot or a tie-in to a target, avoiding the uncertainty volumes
    /// of the wells already there.
    /// <para>
    /// The whole chain in one call: lay out an octree that follows the passages between the obstacles,
    /// close the cells that touch one, check that the target can be reached at all, solve the flow with
    /// the source direction held by a conduit and the target drawing at a fixed rate, and trace equal
    /// shares of the flow through the result. What comes out is non-crossing by construction and feeds
    /// <see cref="StreamlineBundler"/> directly.
    /// </para>
    /// <para>
    /// A streamline is a corridor rather than a well path. It has no limit on its curvature and it kinks
    /// wherever the medium changes, so a drillable path is what comes of fitting analytic sections to it
    /// afterwards. The clearance proved here belongs to the streamline, and rounding a corner moves a path
    /// toward the inside of it, so it has to be verified again after that fit.
    /// </para>
    /// </summary>
    public static class StreamlineGenerator
    {
        /// <summary>
        /// Runs the chain twice, with a channel of easier medium between the passes.
        /// <para>
        /// The first pass is the plain problem, whose streamlines are the dipole family between a source
        /// and a sink: a set of bulging loops, only the axial one of which is straight. What that family
        /// is good for is that its members go round the obstacles in genuinely different ways, and
        /// <see cref="StreamlineBundler"/> was built to tell those ways apart. So each bundle is one way
        /// round, its gentlest member is pushed clear of the volumes and smoothed into a spine, and the
        /// second pass runs in a medium made easier along all of those spines at once.
        /// </para>
        /// <para>
        /// The division of labour is the point. Curvature is a geometric property and the relaxation
        /// handles it; which ways round exist is a question about the domain and the flow answers it;
        /// clearance is guaranteed by the cells, as it always was.
        /// </para>
        /// </summary>
        /// <param name="obstacles"></param>
        /// <param name="sources"></param>
        /// <param name="target"></param>
        /// <param name="options"></param>
        /// <param name="maximumSpines">how many ways round to keep, the gentlest first</param>
        /// <returns></returns>
        public static StreamlineGenerationResult GenerateChannelled(
                                                    ObstacleField obstacles,
                                                    IReadOnlyList<StreamlineSource> sources,
                                                    TargetPolygon target,
                                                    StreamlineGeneratorOptions? options = null,
                                                    int maximumSpines = 4)
        {
            options ??= new StreamlineGeneratorOptions();
            StreamlineGenerationResult scout = Scout(obstacles, sources, target, options);
            if (scout.Status != StreamlineGridStatus.Connected || scout.Streamlines.Count == 0)
            {
                return scout;
            }

            // one way round per bundle, taking the gentlest member of each as its representative
            StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(scout.Streamlines);
            List<(double Dogleg, int Index)> pick = new List<(double, int)>();
            foreach (StreamlineBundle bundle in bundling.Bundles)
            {
                int best = -1;
                double gentlest = double.MaxValue;
                foreach (int at in bundle.StreamlineIndices)
                {
                    if (at < scout.StreamlineOutcomes.Count
                        && scout.StreamlineOutcomes[at] != TraceOutcome.ReachedSink)
                    {
                        continue;
                    }
                    double worst = StreamlineCurvature.GetWorst(scout.Streamlines[at]);
                    if (worst > 0 && worst < gentlest)
                    {
                        gentlest = worst;
                        best = at;
                    }
                }
                if (best >= 0)
                {
                    pick.Add((gentlest, best));
                }
            }
            pick.Sort((a, b) => a.Dogleg.CompareTo(b.Dogleg));

            target.GetArrivalUnit(out double an, out double ae, out double av);
            Vector3D arrival = new Vector3D(an, ae, av);
            SpineRelaxerOptions relaxing = options.Relaxer;
            relaxing.CeilingVertical = options.Grid.CeilingVertical;
            relaxing.FloorVertical = options.Grid.FloorVertical;

            List<IReadOnlyList<Point3D>> spines = new List<IReadOnlyList<Point3D>>();
            StreamlineGenerationResult result = new StreamlineGenerationResult();
            for (int k = 0; k < pick.Count && spines.Count < maximumSpines; k++)
            {
                List<Point3D> seed = new List<Point3D> { sources[0].Position! };
                if (scout.ConduitPaths.Count > 0)
                {
                    seed.AddRange(scout.ConduitPaths[0]);
                }
                seed.AddRange(scout.Streamlines[pick[k].Index].Positions!);
                if (seed.Count < 3)
                {
                    continue;
                }
                SpineRelaxerResult relaxed = SpineRelaxer.Relax(seed, obstacles, sources[0].Direction!,
                                                                arrival, relaxing);
                spines.Add(relaxed.Spine);
                result.Spines.Add(relaxed.Spine);
                result.SpineQuality.Add(relaxed);
            }
            if (spines.Count == 0)
            {
                return scout;
            }

            options.ChannelSpines = spines;
            if (!(options.ChannelContrast > 1.0))
            {
                options.ChannelContrast = 100.0;
            }
            StreamlineGenerationResult final = Generate(obstacles, sources, target, options);
            final.Spines = result.Spines;
            final.SpineQuality = result.SpineQuality;
            final.ScoutDogleg = pick.Count > 0 ? pick[0].Dogleg : double.NaN;
            return final;
        }

        /// <summary>
        /// one pass with the arrival left free, so that nothing forces a corner into what comes back
        /// </summary>
        private static StreamlineGenerationResult Scout(ObstacleField obstacles,
                                                        IReadOnlyList<StreamlineSource> sources,
                                                        TargetPolygon target,
                                                        StreamlineGeneratorOptions options)
        {
            TargetIncidence incidence = target.Incidence;
            TargetSides sides = target.Sides;
            try
            {
                target.Incidence = TargetIncidence.Free;
                target.Sides = TargetSides.Both;
                return Generate(obstacles, sources, target, new StreamlineGeneratorOptions
                {
                    Grid = options.Grid,
                    Flow = options.Flow,
                    Tracer = options.Tracer,
                    ConduitLength = options.ConduitLength,
                    StreamlineCount = options.StreamlineCount
                });
            }
            finally
            {
                target.Incidence = incidence;
                target.Sides = sides;
            }
        }

        /// <summary>
        /// Runs the chain twice: once to find a path that clears the obstacles, then again with the
        /// conduits at both ends shaped to it.
        /// <para>
        /// The first pass is deliberately unconstrained at the target, so that nothing forces a corner
        /// into the path the second pass will follow. Its gentlest streamline becomes the spine, and
        /// because a streamline is traced through open cells it cannot lead a conduit into a wall, which
        /// a curve drawn straight between the two ends does.
        /// </para>
        /// </summary>
        /// <param name="obstacles"></param>
        /// <param name="sources"></param>
        /// <param name="target"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static StreamlineGenerationResult GenerateShaped(ObstacleField obstacles,
                                                                IReadOnlyList<StreamlineSource> sources,
                                                                TargetPolygon target,
                                                                StreamlineGeneratorOptions? options = null)
        {
            options ??= new StreamlineGeneratorOptions();
            TargetIncidence incidence = target.Incidence;
            TargetSides sides = target.Sides;
            StreamlineGenerationResult scout;
            try
            {
                target.Incidence = TargetIncidence.Free;
                target.Sides = TargetSides.Both;
                scout = Generate(obstacles, sources, target, new StreamlineGeneratorOptions
                {
                    Grid = options.Grid,
                    Flow = options.Flow,
                    Tracer = options.Tracer,
                    ConduitLength = options.ConduitLength,
                    StreamlineCount = options.StreamlineCount
                });
            }
            finally
            {
                target.Incidence = incidence;
                target.Sides = sides;
            }
            if (scout.Status != StreamlineGridStatus.Connected)
            {
                return scout;
            }
            Streamline? best = null;
            double gentlest = double.MaxValue;
            foreach (Streamline line in scout.Streamlines)
            {
                double worst = StreamlineCurvature.GetWorst(line);
                if (worst > 0 && worst < gentlest)
                {
                    gentlest = worst;
                    best = line;
                }
            }
            if (best == null || best.Positions == null || best.Positions.Count < 2)
            {
                return scout;
            }
            // A streamline starts where it was launched, which is the outlet of the scout's own conduit
            // and not the slot. Shaping to it alone would start the real conduit there too, and because
            // the rate is injected at the head of a conduit that quietly moves the well off its slot.
            // The scout's conduit is the missing stretch, so the spine is that followed by the path.
            List<Point3D> reference = new List<Point3D> { sources[0].Position! };
            if (scout.ConduitPaths.Count > 0)
            {
                reference.AddRange(scout.ConduitPaths[0]);
            }
            reference.AddRange(best.Positions);
            for (int i = reference.Count - 1; i > 0; i--)
            {
                if (Distance(reference[i], reference[i - 1]) < 1.0e-9)
                {
                    reference.RemoveAt(i);
                }
            }
            options.ReferencePath = reference;
            StreamlineGenerationResult result = Generate(obstacles, sources, target, options);
            result.ScoutDogleg = gentlest;
            return result;
        }

        /// <summary>
        /// runs the whole chain
        /// </summary>
        /// <param name="obstacles"></param>
        /// <param name="sources"></param>
        /// <param name="target"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static StreamlineGenerationResult Generate(ObstacleField obstacles,
                                                          IReadOnlyList<StreamlineSource> sources,
                                                          TargetPolygon target,
                                                          StreamlineGeneratorOptions? options = null)
        {
            options ??= new StreamlineGeneratorOptions();
            StreamlineGenerationResult result = new StreamlineGenerationResult();

            if (!target.IsValid(out string? reason))
            {
                throw new ArgumentException(reason, nameof(target));
            }

            StreamlineGrid grid = StreamlineGrid.Build(obstacles, sources, target, options.Grid);
            result.Grid = grid;
            result.Status = grid.Status;
            if (grid.Status != StreamlineGridStatus.Connected)
            {
                return result;
            }

            FlowProblem problem = new FlowProblem(grid);
            double rate = 1.0;
            List<int[]> conduits = new List<int[]>();
            for (int s = 0; s < sources.Count; s++)
            {
                double length = sources[s].ConduitLength ?? options.ConduitLength;
                int[] chain = options.ShapeDepartureToSpine
                              ? BuildShapedConduit(grid, sources[s], target, length, options)
                              : BuildConduit(grid, sources[s], length);
                conduits.Add(chain);
                result.Conduits.Add((chain.Length, MeasureChain(grid, chain)));
                List<Point3D> drawn = new List<Point3D>(chain.Length);
                foreach (int cell in chain)
                {
                    OctreeCell box = grid.Tree.GetCell(cell);
                    drawn.Add(new Point3D(box.CentreNorth, box.CentreEast, box.CentreVertical));
                }
                result.ConduitPaths.Add(drawn);
                if (chain.Length >= 2)
                {
                    problem.AddConduit(chain);
                    problem.AddRate(chain[0], rate / sources.Count);
                }
                else if (grid.SourceLeaves[s] >= 0)
                {
                    problem.AddRate(grid.SourceLeaves[s], rate / sources.Count);
                }
            }

            problem.Mobility = BuildMedium(sources, target, options, grid, conduits);

            IReadOnlyList<Point3D>? spine = options.ShapeLandingToSpine && sources.Count > 0
                ? GetSpine(sources[0], target, options) : null;
            List<int> served = ConstrainArrival(grid, problem, target, spine);
            result.ServedTargetCellCount = served.Count;
            if (served.Count == 0)
            {
                result.Status = StreamlineGridStatus.TargetBlocked;
                return result;
            }
            // How the target draws: a weight per cell, scaled so the weights sum to the rate injected,
            // the problem being pure Neumann. The weight is the cell's own volume times whatever the
            // target asks for at that position. The volume matters on its own: the earlier rule gave
            // every cell the same share whatever its size, so a refined patch of a target drew more per
            // unit area than a coarse one without anyone saying so. Uniform weighting therefore now
            // means uniform per unit area rather than per cell, and on a target of uniformly sized cells
            // the two are the same thing.
            double[] shares = new double[served.Count];
            double totalShare = 0;
            for (int i = 0; i < served.Count; i++)
            {
                OctreeCell box = grid.Tree.GetCell(served[i]);
                double volume = box.Size * box.Size * box.Size;
                shares[i] = volume * target.GetArrivalWeight(box.CentreNorth, box.CentreEast,
                                                             box.CentreVertical);
                if (shares[i] < 0) { shares[i] = 0; }
                totalShare += shares[i];
            }
            if (!(totalShare > 0))
            {
                // a weighting that leaves every served cell at zero says nothing about where to arrive,
                // so it is treated as saying nothing at all rather than as an unsolvable problem
                for (int i = 0; i < served.Count; i++) { shares[i] = 1.0; }
                totalShare = served.Count;
            }
            for (int i = 0; i < served.Count; i++)
            {
                problem.AddRate(served[i], -rate * shares[i] / totalShare);
            }

            // the conduits and the shut faces can cut a way through that the grid alone thought was open
            grid.Reassess(problem);
            result.Status = grid.Status;
            if (grid.Status != StreamlineGridStatus.Connected)
            {
                return result;
            }

            FlowField field = FlowField.Solve(problem, options.Flow);
            result.Field = field;
            // Streamlines traced through a field that has not converged are not conservative and not
            // non-crossing, so they are not corridors at all. Measured once, with a conduit reaching
            // most of the way to the target: residual 7.4e7, imbalance 2.7e6, and thirty six paths
            // produced and reported as if they meant something. Nothing is produced from that now.
            if (!field.Converged)
            {
                return result;
            }

            StreamlineTracer tracer = new StreamlineTracer(options.Tracer);
            foreach ((int from, double[] start) in GetLaunchPositions(grid, conduits, sources,
                                                                      options.StreamlineCount, options))
            {
                Streamline line = tracer.Trace(field, problem, start[0], start[1], start[2]);
                result.Outcomes[(int)tracer.Outcome]++;
                if (line.Count > 1)
                {
                    result.Streamlines.Add(WithConduit(grid, conduits[from], sources[from], line));
                    result.StreamlineOutcomes.Add(tracer.Outcome);
                }
            }
            result.MeasureCurvature();
            return result;
        }


        /// <summary>
        /// The medium the flow runs in: a background of one, plus a guide along each source's direction
        /// and, for a guided arrival, one along the normal of the target.
        /// </summary>
        private static IFaceMobility? BuildMedium(IReadOnlyList<StreamlineSource> sources,
                                                  TargetPolygon target,
                                                  StreamlineGeneratorOptions options,
                                                  StreamlineGrid grid,
                                                  List<int[]> conduits)
        {
            // a guide at the mouth of each conduit, pointing the way that conduit was going
            List<IFaceMobility?> layers = new List<IFaceMobility?>();
            if (options.OutletGuideContrast > 1.0)
            {
                List<DirectionGuide> mouths = new List<DirectionGuide>();
                for (int s = 0; s < conduits.Count; s++)
                {
                    int[] chain = conduits[s];
                    if (chain.Length < 2)
                    {
                        continue;
                    }
                    OctreeCell last = grid.Tree.GetCell(chain[chain.Length - 1]);
                    OctreeCell before = grid.Tree.GetCell(chain[chain.Length - 2]);
                    if (!GetSharedFace(in before, in last, out int axis, out int sense))
                    {
                        continue;
                    }
                    mouths.Add(new DirectionGuide
                    {
                        Anchor = new Point3D(last.CentreNorth, last.CentreEast, last.CentreVertical),
                        Direction = new Vector3D(axis == 0 ? sense : 0, axis == 1 ? sense : 0,
                                                 axis == 2 ? sense : 0),
                        AlongLength = options.OutletGuideLength,
                        AcrossLength = options.OutletGuideWidth,
                        Contrast = options.OutletGuideContrast,
                        DownstreamOnly = true
                    });
                }
                if (mouths.Count > 0)
                {
                    DirectionGuideField atMouths = new DirectionGuideField(mouths);
                    if (atMouths.Count > 0)
                    {
                        layers.Add(atMouths);
                    }
                }
            }
            layers.Add(BuildRoute(sources, target, options));
            List<IFaceMobility?> real = layers.FindAll(one => one != null);
            if (real.Count == 0)
            {
                return null;
            }
            return real.Count == 1 ? real[0] : new CombinedMobilityField(real);
        }

        /// <summary>
        /// the medium that shapes the route as a whole, as against the one at a conduit's mouth
        /// </summary>
        private static IFaceMobility? BuildRoute(IReadOnlyList<StreamlineSource> sources,
                                                 TargetPolygon target,
                                                 StreamlineGeneratorOptions options)
        {
            // a channel along the spines, which unlike a guide leaves the medium isotropic
            if (options.ChannelContrast > 1.0 && sources.Count > 0)
            {
                if (options.ChannelSpines != null && options.ChannelSpines.Count > 0)
                {
                    return new ChannelMobilityField(options.ChannelSpines, options.ChannelNarrowWidth,
                                                    options.ChannelWideWidth, options.ChannelContrast);
                }
                return new ChannelMobilityField(GetSpine(sources[0], target, options),
                                                options.ChannelNarrowWidth, options.ChannelWideWidth,
                                                options.ChannelContrast);
            }

            // a spine runs the whole way and turns as it goes, so it stands in place of the end guides
            if (options.SpineGuideContrast > 1.0 && sources.Count > 0)
            {
                target.GetArrivalUnit(out double sn, out double se, out double sv);
                List<Point3D> spine = ReferenceSpine.Sample(sources[0].Position!, sources[0].Direction!,
                                                            target.GetCentre(), new Vector3D(sn, se, sv));
                return new PathGuideField(spine, options.SpineGuideWidth, options.SpineGuideContrast);
            }

            List<DirectionGuide> guides = new List<DirectionGuide>();
            if (options.SourceGuideContrast > 1.0)
            {
                foreach (StreamlineSource source in sources)
                {
                    guides.Add(new DirectionGuide
                    {
                        Anchor = source.Position,
                        Direction = source.Direction,
                        AlongLength = options.SourceGuideLength,
                        AcrossLength = options.SourceGuideWidth,
                        Contrast = options.SourceGuideContrast
                    });
                }
            }
            if (target.Incidence == TargetIncidence.Guided)
            {
                target.GetArrivalUnit(out double dn, out double de, out double dv);
                double width = target.GuideWidth;
                if (!(width > 0))
                {
                    // the target's own size, so the guide covers what it has to reach and no more
                    width = 0.5 * System.Math.Max(
                        target.BoundingBoxMaximum[0] - target.BoundingBoxMinimum[0],
                        System.Math.Max(target.BoundingBoxMaximum[1] - target.BoundingBoxMinimum[1],
                                        target.BoundingBoxMaximum[2] - target.BoundingBoxMinimum[2]));
                }
                guides.Add(new DirectionGuide
                {
                    Anchor = target.GetCentre(),
                    Direction = new Vector3D(dn, de, dv),
                    AlongLength = target.LandingLength,
                    AcrossLength = width,
                    Contrast = target.GuideContrast,
                    UpstreamOnly = target.Sides == TargetSides.One
                });
            }
            if (guides.Count == 0)
            {
                return null;
            }
            DirectionGuideField field = new DirectionGuideField(guides);
            return field.Count > 0 ? field : null;
        }

        /// <summary>
        /// Imposes the way the streamlines are allowed to arrive at the target, and returns the cells of
        /// it that can still be served.
        /// <para>
        /// Free arrival through both faces leaves the target as it is. One face becomes a box open only
        /// on the admitted side: the far face and the rim are shut, which says nothing about the angle
        /// but leaves only one way in. A perpendicular arrival is the source conduit run at the other
        /// end, one tube per column of target cells, laid along the normal: inside a tube the velocity
        /// has nowhere to point but along it, so the last <see cref="TargetPolygon.LandingLength"/>
        /// metres are on the normal exactly rather than approximately.
        /// </para>
        /// </summary>
        private static List<int> ConstrainArrival(StreamlineGrid grid, FlowProblem problem,
                                                  TargetPolygon target,
                                                  IReadOnlyList<Point3D>? shaped)
        {
            // a guided arrival puts nothing in the way: the medium does the work, and the only walls
            // left are the ones that shut a face the caller did not admit
            bool walled = target.Incidence == TargetIncidence.Perpendicular;
            if (!walled && target.Sides == TargetSides.Both)
            {
                return new List<int>(grid.SinkLeaves);
            }
            target.GetArrivalUnit(out double dn, out double de, out double dv);
            if (!walled)
            {
                return AdmitOneFace(grid, problem, dn, de, dv);
            }
            return shaped != null
                   ? BuildShapedLandings(grid, problem, target, shaped, dn, de, dv)
                   : BuildLandingConduits(grid, problem, target, dn, de, dv);
        }

        /// <summary>
        /// Shuts every outward face of the target region except those a streamline travelling the
        /// approach direction would enter by, the rim included.
        /// </summary>
        private static List<int> AdmitOneFace(StreamlineGrid grid, FlowProblem problem,
                                              double dn, double de, double dv)
        {
            HashSet<int> sinks = new HashSet<int>(grid.SinkLeaves);
            double[] direction = { dn, de, dv };
            List<int> neighbours = new List<int>();
            foreach (int leaf in grid.SinkLeaves)
            {
                for (int axis = 0; axis < 3; axis++)
                {
                    for (int step = -1; step <= 1; step += 2)
                    {
                        // an outward face whose normal opposes the travel is where a streamline comes in
                        if (step * direction[axis] < -1.0e-9)
                        {
                            continue;
                        }
                        neighbours.Clear();
                        grid.Tree.GetFaceNeighbours(leaf, axis, step, neighbours);
                        bool interior = neighbours.Count > 0;
                        foreach (int other in neighbours)
                        {
                            if (!sinks.Contains(other))
                            {
                                interior = false;
                            }
                        }
                        if (!interior)
                        {
                            problem.CloseFace(leaf, axis, step);
                        }
                    }
                }
            }
            return new List<int>(grid.SinkLeaves);
        }

        /// <summary>
        /// One conduit per column of target cells, laid along the normal and reaching a landing length
        /// out into the medium, so that whatever arrives does so along the normal.
        /// </summary>
        private static List<int> BuildLandingConduits(StreamlineGrid grid, FlowProblem problem,
                                                      TargetPolygon target,
                                                      double dn, double de, double dv)
        {
            HashSet<int> sinks = new HashSet<int>(grid.SinkLeaves);
            HashSet<int> claimed = new HashSet<int>();
            List<int> served = new List<int>();
            bool both = target.Sides == TargetSides.Both;

            foreach (int leaf in grid.SinkLeaves)
            {
                // only the cell furthest downstream in its column seeds a tube, or the column would be
                // walked once for every cell in it
                if (StepTo(grid, leaf, dn, de, dv, out int behind) && sinks.Contains(behind))
                {
                    continue;
                }
                List<int> upstream = Walk(grid, leaf, -dn, -de, -dv, target.LandingLength, sinks, claimed);
                if (upstream.Count < 1)
                {
                    continue;
                }
                List<int> chain = new List<int>();
                if (both)
                {
                    List<int> downstream = Walk(grid, leaf, dn, de, dv, target.LandingLength,
                                                sinks, claimed);
                    for (int k = downstream.Count - 1; k >= 0; k--)
                    {
                        chain.Add(downstream[k]);
                    }
                }
                bool openAtHead = both && chain.Count > 0;
                chain.Add(leaf);
                chain.AddRange(upstream);
                if (chain.Count < 2)
                {
                    continue;
                }
                foreach (int cell in chain)
                {
                    claimed.Add(cell);
                    if (sinks.Contains(cell))
                    {
                        served.Add(cell);
                    }
                }
                problem.AddConduit(chain, openAtHead);
            }
            return served;
        }

        /// <summary>
        /// the leaf immediately across the face the given direction leaves the cell by
        /// </summary>
        private static bool StepTo(StreamlineGrid grid, int leaf, double dn, double de, double dv,
                                   out int next)
        {
            OctreeCell cell = grid.Tree.GetCell(leaf);
            double reach = 0.6 * cell.Size;
            next = grid.Tree.FindLeaf(cell.CentreNorth + dn * reach, cell.CentreEast + de * reach,
                                      cell.CentreVertical + dv * reach);
            return next >= 0;
        }

        /// <summary>
        /// The cells a tube passes through, leaving the given cell in the given direction: the rest of
        /// its column, and then a landing length of medium beyond it.
        /// </summary>
        private static List<int> Walk(StreamlineGrid grid, int leaf, double dn, double de, double dv,
                                      double landing, HashSet<int> sinks, HashSet<int> claimed)
        {
            List<int> walked = new List<int>();
            OctreeCell start = grid.Tree.GetCell(leaf);
            double n = start.CentreNorth;
            double e = start.CentreEast;
            double v = start.CentreVertical;
            double step = 0.25 * grid.Tree.Frame.GetCellSize(grid.Tree.DeepestDepth);
            if (!(step > 0))
            {
                return walked;
            }
            int at = leaf;
            double beyond = 0;
            while (beyond <= landing)
            {
                n += dn * step;
                e += de * step;
                v += dv * step;
                int found = grid.Tree.FindLeaf(n, e, v);
                if (found < 0 || grid.States[found] != CellState.Open || claimed.Contains(found))
                {
                    break;
                }
                if (found != at)
                {
                    at = found;
                    walked.Add(found);
                }
                if (!sinks.Contains(at))
                {
                    // the column is behind us, so this is the landing section proper
                    beyond += step;
                }
            }
            return walked;
        }



        /// <summary>
        /// Which face of the first cell the second lies across.
        /// <para>
        /// Taken from the bound the two share rather than from the line between their centres: where a
        /// coarse cell meets a fine one the centres are offset sideways as well, so the difference
        /// between them points nowhere in particular and can name the wrong face — a face that in a
        /// conduit is closed, which leaves anything launched against it with nowhere to go.
        /// </para>
        /// </summary>
        private static bool GetSharedFace(in OctreeCell from, in OctreeCell to, out int axis,
                                          out int step)
        {
            double[] low = { from.MinimumNorth, from.MinimumEast, from.MinimumVertical };
            double[] farLow = { to.MinimumNorth, to.MinimumEast, to.MinimumVertical };
            double tolerance = 1.0e-6 * System.Math.Min(from.Size, to.Size);
            for (axis = 0; axis < 3; axis++)
            {
                if (System.Math.Abs(low[axis] + from.Size - farLow[axis]) <= tolerance)
                {
                    step = 1;
                    return true;
                }
                if (System.Math.Abs(farLow[axis] + to.Size - low[axis]) <= tolerance)
                {
                    step = -1;
                    return true;
                }
            }
            axis = -1;
            step = 0;
            return false;
        }


        /// <summary>
        /// A streamline with the conduit it was launched out of put back on the front of it.
        /// <para>
        /// A trace starts where it was launched, which is the outlet of the conduit and not the slot, so
        /// what comes back from the tracer is the second part of a well path and not the whole of it.
        /// The conduit is every bit as much of the path — it is the stretch whose direction was
        /// prescribed rather than solved for — and leaving it off would say the well started hundreds of
        /// metres underground. Every candidate from one source shares that stretch, because they share
        /// the hole.
        /// </para>
        /// </summary>
        private static Streamline WithConduit(StreamlineGrid grid, int[] chain, StreamlineSource source,
                                              Streamline traced)
        {
            List<Point3D> positions = new List<Point3D> { source.Position! };
            // up to the cell before the one the launch sits in, which the trace itself then covers
            for (int i = 0; i + 2 < chain.Length; i++)
            {
                OctreeCell cell = grid.Tree.GetCell(chain[i]);
                positions.Add(new Point3D(cell.CentreNorth, cell.CentreEast, cell.CentreVertical));
            }
            positions.AddRange(traced.Positions!);
            for (int i = positions.Count - 1; i > 0; i--)
            {
                if (Distance(positions[i], positions[i - 1]) < 1.0e-9)
                {
                    positions.RemoveAt(i);
                }
            }
            return new Streamline(positions) { ID = traced.ID, Name = traced.Name };
        }

        /// <summary>
        /// how far a chain of cells reaches, centre to centre, m
        /// </summary>
        private static double MeasureChain(StreamlineGrid grid, int[] chain)
        {
            double reach = 0;
            for (int i = 1; i < chain.Length; i++)
            {
                OctreeCell a = grid.Tree.GetCell(chain[i - 1]);
                OctreeCell b = grid.Tree.GetCell(chain[i]);
                reach += System.Math.Sqrt(Squared(a.CentreNorth - b.CentreNorth)
                                          + Squared(a.CentreEast - b.CentreEast)
                                          + Squared(a.CentreVertical - b.CentreVertical));
            }
            return reach;
        }

        /// <summary>
        /// the reference curve from a source to the target, honouring the direction at both ends
        /// </summary>
        private static IReadOnlyList<Point3D> GetSpine(StreamlineSource source, TargetPolygon target,
                                                      StreamlineGeneratorOptions options)
        {
            if (options.ReferencePath != null && options.ReferencePath.Count >= 2)
            {
                return options.ReferencePath;
            }
            target.GetArrivalUnit(out double dn, out double de, out double dv);
            return ReferenceSpine.Sample(source.Position!, source.Direction!, target.GetCentre(),
                                         new Vector3D(dn, de, dv), 513);
        }

        /// <summary>
        /// the stretch of a polyline within a given length of one of its ends
        /// </summary>
        private static List<Point3D> Trim(IReadOnlyList<Point3D> path, double length, bool fromEnd)
        {
            List<Point3D> kept = new List<Point3D>();
            double travelled = 0;
            if (fromEnd)
            {
                kept.Add(path[path.Count - 1]);
                for (int i = path.Count - 1; i > 0 && travelled <= length; i--)
                {
                    travelled += Distance(path[i], path[i - 1]);
                    kept.Add(path[i - 1]);
                }
            }
            else
            {
                kept.Add(path[0]);
                for (int i = 0; i + 1 < path.Count && travelled <= length; i++)
                {
                    travelled += Distance(path[i], path[i + 1]);
                    kept.Add(path[i + 1]);
                }
            }
            return kept;
        }

        private static double Distance(Point3D a, Point3D b)
        {
            return System.Math.Sqrt(Squared(a.X!.Value - b.X!.Value) + Squared(a.Y!.Value - b.Y!.Value)
                                    + Squared(a.Z!.Value - b.Z!.Value));
        }

        private static double Squared(double value)
        {
            return value * value;
        }

        /// <summary>
        /// The cells a curve passes through, as a chain in which each cell shares a face with the next.
        /// <para>
        /// Stepping by position and asking which leaf holds it is what a straight axis aligned conduit
        /// can afford; a curve cannot, because two cells a small step apart on a diagonal may meet only
        /// at an edge, and a conduit restricts each cell to the two that continue the chain by their
        /// identity. A cell with no face between it and its successor would be sealed on every side. So
        /// the walk is taken one face at a time, always to the neighbour that gets nearest the curve.
        /// </para>
        /// </summary>
        private static int[] FollowPath(StreamlineGrid grid, IReadOnlyList<Point3D> path,
                                        HashSet<int>? claimed, int maximumCells)
        {
            if (path.Count < 2)
            {
                return Array.Empty<int>();
            }
            int leaf = grid.Tree.FindLeaf(path[0].X!.Value, path[0].Y!.Value, path[0].Z!.Value);
            if (leaf < 0 || grid.States[leaf] != CellState.Open
                || (claimed != null && claimed.Contains(leaf)))
            {
                return Array.Empty<int>();
            }
            List<int> chain = new List<int> { leaf };
            HashSet<int> visited = new HashSet<int> { leaf };
            List<int> neighbours = new List<int>();
            int waypoint = 1;
            while (chain.Count < maximumCells)
            {
                OctreeCell cell = grid.Tree.GetCell(leaf);
                // Steer by a waypoint at least a cell away. Aiming at the next sample of the curve
                // works only while the cells are smaller than the spacing between samples: in a coarse
                // cell the next sample is already inside it, no neighbour is any nearer, and the walk
                // stops where it stands. Measured before this: a conduit asked for four hundred metres
                // gave up after a hundred and eighty three.
                double wn = 0, we = 0, wv = 0, here = 0;
                bool steering = false;
                while (waypoint < path.Count)
                {
                    wn = path[waypoint].X!.Value;
                    we = path[waypoint].Y!.Value;
                    wv = path[waypoint].Z!.Value;
                    here = System.Math.Sqrt(Squared(cell.CentreNorth - wn)
                                            + Squared(cell.CentreEast - we)
                                            + Squared(cell.CentreVertical - wv));
                    if (here >= 1.2 * cell.Size)
                    {
                        steering = true;
                        break;
                    }
                    waypoint++;
                }
                if (!steering)
                {
                    break;
                }
                int best = -1;
                double nearest = here;
                for (int axis = 0; axis < 3; axis++)
                {
                    for (int step = -1; step <= 1; step += 2)
                    {
                        neighbours.Clear();
                        grid.Tree.GetFaceNeighbours(leaf, axis, step, neighbours);
                        foreach (int other in neighbours)
                        {
                            if (grid.States[other] != CellState.Open || visited.Contains(other)
                                || (claimed != null && claimed.Contains(other)))
                            {
                                continue;
                            }
                            OctreeCell far = grid.Tree.GetCell(other);
                            double distance = System.Math.Sqrt(Squared(far.CentreNorth - wn)
                                                               + Squared(far.CentreEast - we)
                                                               + Squared(far.CentreVertical - wv));
                            if (distance < nearest)
                            {
                                nearest = distance;
                                best = other;
                            }
                        }
                    }
                }
                if (best < 0)
                {
                    break;
                }
                chain.Add(best);
                visited.Add(best);
                leaf = best;
            }
            return chain.Count >= 2 ? chain.ToArray() : Array.Empty<int>();
        }

        /// <summary>
        /// the departure conduit, shaped to the first stretch of the reference spine so that it turns
        /// while it holds rather than holding straight and turning all at once at its mouth
        /// </summary>
        private static int[] BuildShapedConduit(StreamlineGrid grid, StreamlineSource source,
                                                TargetPolygon target, double length,
                                                StreamlineGeneratorOptions options)
        {
            if (!(length > 0))
            {
                return Array.Empty<int>();
            }
            List<Point3D> head = Trim(GetSpine(source, target, options), length, false);
            // whatever curve was handed in, a conduit is where the well starts, so it starts at the slot
            if (head.Count == 0 || Distance(head[0], source.Position!) > 1.0e-9)
            {
                head.Insert(0, source.Position!);
            }
            int[] chain = FollowPath(grid, head, null, 100000);
            GetEndTangent(head, out double dn, out double de, out double dv);
            return EndAlong(grid, chain, dn, de, dv);
        }


        /// <summary>
        /// the direction a curve is heading in where it stops
        /// </summary>
        private static void GetEndTangent(IReadOnlyList<Point3D> path, out double north, out double east,
                                          out double vertical)
        {
            north = 0;
            east = 0;
            vertical = 0;
            for (int i = path.Count - 1; i > 0; i--)
            {
                north = path[i].X!.Value - path[i - 1].X!.Value;
                east = path[i].Y!.Value - path[i - 1].Y!.Value;
                vertical = path[i].Z!.Value - path[i - 1].Z!.Value;
                if (north * north + east * east + vertical * vertical > 1.0e-18)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Trims a chain back so that its last step is along the axis the path is mostly heading in.
        /// <para>
        /// A chain of cells can only step along an axis, so a chain following anything but an axis is a
        /// staircase and its last step is whichever one the staircase happened to land on. Since the
        /// streamlines are launched across the face the chain gives out by, a chain that happens to end
        /// on a sideways step sends every one of them out sideways — a right angle at the very place the
        /// conduit exists to stop one. Ending on the step that matches where the well is going costs at
        /// most a couple of cells.
        /// </para>
        /// </summary>
        private static int[] EndAlong(StreamlineGrid grid, int[] chain, double dn, double de, double dv)
        {
            if (chain.Length < 3)
            {
                return chain;
            }
            double[] direction = { dn, de, dv };
            int wanted = 0;
            for (int a = 1; a < 3; a++)
            {
                if (System.Math.Abs(direction[a]) > System.Math.Abs(direction[wanted]))
                {
                    wanted = a;
                }
            }
            if (!(System.Math.Abs(direction[wanted]) > 0))
            {
                return chain;
            }
            int sense = direction[wanted] > 0 ? 1 : -1;
            for (int end = chain.Length - 1; end >= 2; end--)
            {
                OctreeCell before = grid.Tree.GetCell(chain[end - 1]);
                OctreeCell last = grid.Tree.GetCell(chain[end]);
                if (GetSharedFace(in before, in last, out int axis, out int step)
                    && axis == wanted && step == sense)
                {
                    int[] trimmed = new int[end + 1];
                    Array.Copy(chain, trimmed, end + 1);
                    return trimmed;
                }
            }
            return chain;
        }

        /// <summary>
        /// The landing conduits, each shaped to the last stretch of the reference spine and carried
        /// sideways onto its own column of the target, so that the same length of hole turns instead of
        /// standing straight above the target.
        /// </summary>
        private static List<int> BuildShapedLandings(StreamlineGrid grid, FlowProblem problem,
                                                     TargetPolygon target,
                                                     IReadOnlyList<Point3D> spine,
                                                     double dn, double de, double dv)
        {
            List<Point3D> tail = Trim(spine, target.LandingLength, true);
            Point3D centre = target.GetCentre();
            HashSet<int> sinks = new HashSet<int>(grid.SinkLeaves);
            HashSet<int> claimed = new HashSet<int>();
            List<int> served = new List<int>();

            foreach (int leaf in grid.SinkLeaves)
            {
                // one tube per column, seeded from the cell furthest downstream in it
                if (StepTo(grid, leaf, dn, de, dv, out int behind) && sinks.Contains(behind))
                {
                    continue;
                }
                OctreeCell cell = grid.Tree.GetCell(leaf);
                double shiftNorth = cell.CentreNorth - centre.X!.Value;
                double shiftEast = cell.CentreEast - centre.Y!.Value;
                double shiftVertical = cell.CentreVertical - centre.Z!.Value;
                // only the part of the offset that lies in the plane of the target, so every tube keeps
                // the same arrival direction and they stay parallel where it matters
                double along = shiftNorth * dn + shiftEast * de + shiftVertical * dv;
                shiftNorth -= along * dn;
                shiftEast -= along * de;
                shiftVertical -= along * dv;

                List<Point3D> carried = new List<Point3D>(tail.Count);
                foreach (Point3D point in tail)
                {
                    carried.Add(new Point3D(point.X!.Value + shiftNorth, point.Y!.Value + shiftEast,
                                            point.Z!.Value + shiftVertical));
                }
                int[] chain = FollowPath(grid, carried, claimed, 100000);
                if (chain.Length < 2)
                {
                    continue;
                }
                foreach (int cellOfChain in chain)
                {
                    claimed.Add(cellOfChain);
                    if (sinks.Contains(cellOfChain))
                    {
                        served.Add(cellOfChain);
                    }
                }
                problem.AddConduit(chain);
            }
            return served;
        }

        /// <summary>
        /// the chain of cells from the source along the direction it leaves in
        /// </summary>
        private static int[] BuildConduit(StreamlineGrid grid, StreamlineSource source, double length)
        {
            source.GetUnitDirection(out double dn, out double de, out double dv);
            double n = source.Position!.X!.Value;
            double e = source.Position.Y!.Value;
            double v = source.Position.Z!.Value;
            List<int> chain = new List<int>();
            double travelled = 0;
            double step = 0.25 * grid.Tree.Frame.GetCellSize(grid.Tree.DeepestDepth);
            if (!(step > 0))
            {
                return Array.Empty<int>();
            }
            while (travelled <= length)
            {
                int leaf = grid.Tree.FindLeaf(n, e, v);
                if (leaf < 0 || grid.States[leaf] != CellState.Open)
                {
                    break;
                }
                if (chain.Count == 0 || chain[chain.Count - 1] != leaf)
                {
                    chain.Add(leaf);
                }
                n += dn * step;
                e += de * step;
                v += dv * step;
                travelled += step;
            }
            return chain.Count >= 2 ? chain.ToArray() : Array.Empty<int>();
        }

        /// <summary>
        /// Where the streamlines start.
        /// <para>
        /// They are spread over the face where the conduit gives out onto the medium, evenly by area. A
        /// face carries one flux and so a uniform velocity across it, which is what makes an even spread
        /// an equal share of the flow for each streamline; that in turn is what makes the density of
        /// streamlines mean something rather than merely their count.
        /// </para>
        /// </summary>
        private static IEnumerable<(int Source, double[] Position)> GetLaunchPositions(
                                                                StreamlineGrid grid, List<int[]> conduits,
                                                                IReadOnlyList<StreamlineSource> sources,
                                                                int count,
                                                                StreamlineGeneratorOptions options)
        {
            int perSource = System.Math.Max(1, count / System.Math.Max(1, sources.Count));
            for (int s = 0; s < sources.Count; s++)
            {
                sources[s].GetUnitDirection(out double dn, out double de, out double dv);

                int leaf;
                if (s < conduits.Count && conduits[s].Length >= 2)
                {
                    leaf = conduits[s][conduits[s].Length - 2];
                    // A conduit shaped to a curve does not give out along the direction the source
                    // leaves in, so the face it gives out by has to come from the chain itself. Reading
                    // it off the source would put the launches on a face that is closed, and they would
                    // never start.
                    OctreeCell last = grid.Tree.GetCell(conduits[s][conduits[s].Length - 1]);
                    OctreeCell before = grid.Tree.GetCell(leaf);
                    if (GetSharedFace(in before, in last, out int outgoing, out int sense))
                    {
                        dn = outgoing == 0 ? sense : 0;
                        de = outgoing == 1 ? sense : 0;
                        dv = outgoing == 2 ? sense : 0;
                    }
                }
                else
                {
                    leaf = grid.SourceLeaves[s];
                }
                int axis = System.Math.Abs(dn) >= System.Math.Abs(de)
                           ? (System.Math.Abs(dn) >= System.Math.Abs(dv) ? 0 : 2)
                           : (System.Math.Abs(de) >= System.Math.Abs(dv) ? 1 : 2);
                double along = axis == 0 ? dn : axis == 1 ? de : dv;
                if (leaf < 0)
                {
                    continue;
                }
                OctreeCell cell = grid.Tree.GetCell(leaf);
                double[] low = { cell.MinimumNorth, cell.MinimumEast, cell.MinimumVertical };
                // just inside the outgoing face, so the launch is in the cell rather than on its boundary
                double face = along > 0 ? low[axis] + 0.99 * cell.Size : low[axis] + 0.01 * cell.Size;

                // A disc inscribed in the face, filled by the sunflower arrangement: the radius goes
                // as the square root of the index so every launch carries the same area and therefore,
                // the velocity across a face being uniform, the same share of the flux. A square grid
                // over the whole face would put its outermost launches against the conduit's own walls,
                // which is where the flow separates round the rim of the tube, and what those report is
                // where they were put rather than anything about the corridor.
                int first = (axis + 1) % 3;
                int second = (axis + 2) % 3;
                double inset = System.Math.Max(0, System.Math.Min(0.45, options.LaunchInset));
                double radius = (0.5 - inset) * cell.Size;
                double centreFirst = low[first] + 0.5 * cell.Size;
                double centreSecond = low[second] + 0.5 * cell.Size;
                double golden = System.Math.PI * (3.0 - System.Math.Sqrt(5.0));
                for (int k = 0; k < perSource; k++)
                {
                    double reach = radius * System.Math.Sqrt((k + 0.5) / perSource);
                    double angle = k * golden;
                    double[] position = new double[3];
                    position[axis] = face;
                    position[first] = centreFirst + reach * System.Math.Cos(angle);
                    position[second] = centreSecond + reach * System.Math.Sin(angle);
                    yield return (s, position);
                }
            }
        }
    }
}
