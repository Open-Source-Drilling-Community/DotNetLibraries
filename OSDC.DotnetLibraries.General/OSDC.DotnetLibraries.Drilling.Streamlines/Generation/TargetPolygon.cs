using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// how free a streamline is in the direction it arrives at the target
    /// </summary>
    public enum TargetIncidence
    {
        /// <summary>
        /// it may arrive at any angle
        /// </summary>
        Free,

        /// <summary>
        /// it must arrive along the normal of the plane of the polygon, held over
        /// <see cref="TargetPolygon.LandingLength"/> before the target by a tube of closed faces. Exact,
        /// and abrupt: the tube's mouth is where the whole turn into it has to happen.
        /// </summary>
        Perpendicular,

        /// <summary>
        /// it should arrive near the normal, the medium being made easier along it over
        /// <see cref="TargetPolygon.LandingLength"/>. Approximate, and smooth: the turn is spread over
        /// that length instead of taken at a face.
        /// </summary>
        Guided,

        /// <summary>
        /// It must arrive along the normal, because the flow is drawn to a sink laid
        /// <see cref="TargetPolygon.ThroughLength"/> <em>beyond</em> the target and reachable only by a
        /// tube running out from the target along the normal.
        /// <para>
        /// The difference from <see cref="Perpendicular"/> is where the rate is drawn, and it decides
        /// everything. A walled landing puts the tube on the approach side and the sink at the target, so
        /// the flow wants to go one way and may only enter another, and it resolves the disagreement by
        /// turning hard at the tube's mouth. Here the sink is past the target, so the flow is already
        /// travelling the wanted direction before it meets any wall, and the tube has almost nothing left
        /// to turn.
        /// </para>
        /// <para>
        /// The stretch beyond the target is a construction and not a well: it is where the rate is drawn
        /// so that the direction through the target is the one asked for, and the produced streamlines
        /// are cut at the target plane before anyone sees them.
        /// </para>
        /// </summary>
        Through
    }

    /// <summary>
    /// how the flow the target draws is spread over it
    /// </summary>
    public enum TargetArrivalWeighting
    {
        /// <summary>
        /// every part of the target draws the same rate per unit area, so the arrivals are spread evenly
        /// over it
        /// </summary>
        Uniform,

        /// <summary>
        /// the rate falls off as a Gaussian away from <see cref="TargetPolygon.ArrivalCentre"/>, so the
        /// arrivals concentrate there. Reaching the middle of a target is usually worth more than
        /// reaching its rim, and this is how that preference is stated.
        /// </summary>
        Gaussian
    }

    /// <summary>
    /// which faces of the target a streamline may arrive through
    /// </summary>
    public enum TargetSides
    {
        /// <summary>
        /// either face of the polygon, and, when the incidence is free, its rim as well
        /// </summary>
        Both,

        /// <summary>
        /// only the face the approach direction points into. The other face and the rim are shut.
        /// </summary>
        One
    }

    /// <summary>
    /// The region the streamlines have to reach: a planar polygon, given a thickness so that it becomes a
    /// thin prism of cells rather than a surface of zero volume that no cell could ever lie in.
    /// <para>
    /// The plane is taken from the vertices by Newell's method, which gives the area weighted normal and
    /// so does not care that a polygon digitised from a map is only approximately planar.
    /// </para>
    /// </summary>
    public class TargetPolygon
    {
        private readonly double[] normal_ = new double[3];
        private readonly double[] centre_ = new double[3];
        private readonly double[] firstAxis_ = new double[3];
        private readonly double[] secondAxis_ = new double[3];
        private readonly double[] flatNorth_;
        private readonly double[] flatEast_;
        private readonly double[] halfExtent_ = new double[2];

        /// <summary>
        /// the vertices, in order around the polygon
        /// </summary>
        public IReadOnlyList<Point3D> Vertices { get; }

        /// <summary>
        /// how thick the prism is, m
        /// </summary>
        public double Thickness { get; }

        /// <summary>
        /// whether a streamline may arrive at any angle or only along the normal
        /// </summary>
        public TargetIncidence Incidence { get; set; } = TargetIncidence.Free;

        /// <summary>
        /// which faces may be arrived through
        /// </summary>
        public TargetSides Sides { get; set; } = TargetSides.Both;

        /// <summary>
        /// Which way a streamline is travelling when it arrives, used only for its sign.
        /// <para>
        /// It selects a face; it does not set the angle. The geometry always uses the normal of the
        /// plane, so this may be given loosely, and only the sign of its component along the normal is
        /// read. Naming the face by the winding order of the vertices instead would be silently reversed
        /// by anyone who re-entered the polygon the other way round.
        /// </para>
        /// </summary>
        public Vector3D? ApproachDirection { get; set; } = null;

        /// <summary>
        /// How far before the target the arrival direction is held, m. This is the landing section: the
        /// last stretch that runs along the normal, whether that is imposed by walls or by the medium.
        /// <para>
        /// For a guided arrival it has to be long enough for the turn to fit at a drillable rate. Turning
        /// through an angle at a dogleg limit takes <c>angle / limit</c> of hole and no less, which
        /// <see cref="StreamlineCurvature.GetLeastTurn"/> makes it possible to work out beforehand: thirty
        /// or sixty metres is a landing that can be imposed but not drilled.
        /// </para>
        /// </summary>
        public double LandingLength { get; set; } = 30.0;

        /// <summary>
        /// How far beyond the target the sink is laid, m, used only when the incidence is
        /// <see cref="TargetIncidence.Through"/>.
        /// <para>
        /// It plays no part in the well: nothing of it is reported. It only has to be long enough for the
        /// flow to be heading the right way as it crosses the target, and short enough not to run into
        /// something. Sixty metres is a starting point on a field where the ground beyond a target is not
        /// known to be clear.
        /// </para>
        /// </summary>
        public double ThroughLength { get; set; } = 60.0;

        /// <summary>
        /// how much easier the medium is made along the normal than across it, used only when the
        /// incidence is guided. One is isotropic, and the useful range starts around five.
        /// </summary>
        public double GuideContrast { get; set; } = 10.0;

        /// <summary>
        /// how far across the normal the guide reaches, m, used only when the incidence is guided.
        /// Defaults to the size of the target when left at zero.
        /// </summary>
        public double GuideWidth { get; set; } = 0;

        /// <summary>
        /// how the rate the target draws is spread over it. Uniform keeps every part of it equally
        /// served, which is the earlier behaviour.
        /// </summary>
        public TargetArrivalWeighting ArrivalWeighting { get; set; } = TargetArrivalWeighting.Uniform;

        /// <summary>
        /// Where the arrivals concentrate under <see cref="TargetArrivalWeighting.Gaussian"/>, or null for
        /// the centroid of the vertices.
        /// <para>
        /// It does not have to be the middle. A target is often worth more at one end than the other, and
        /// an eccentric point states that without redrawing the polygon.
        /// </para>
        /// </summary>
        public Point3D? ArrivalCentre { get; set; } = null;

        /// <summary>
        /// The standard deviation of the arrival weighting along the first in-plane axis, m, or zero for a
        /// third of the half extent of the polygon along that axis, which puts the rim at about three
        /// standard deviations.
        /// <para>
        /// The two spreads are separate because a target is usually longer one way than the other, and one
        /// circular spread over an elongated polygon wastes the long axis.
        /// </para>
        /// </summary>
        public double ArrivalSpreadFirst { get; set; } = 0;

        /// <summary>
        /// the same along the second in-plane axis
        /// </summary>
        public double ArrivalSpreadSecond { get; set; } = 0;

        /// <summary>
        /// the lowest corner of the box holding the prism
        /// </summary>
        public double[] BoundingBoxMinimum { get; } = { double.MaxValue, double.MaxValue, double.MaxValue };

        /// <summary>
        /// the highest corner of the box holding the prism
        /// </summary>
        public double[] BoundingBoxMaximum { get; } = { double.MinValue, double.MinValue, double.MinValue };

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="vertices">at least three, in order around the polygon</param>
        /// <param name="thickness"></param>
        public TargetPolygon(IReadOnlyList<Point3D> vertices, double thickness)
        {
            if (vertices == null)
            {
                throw new ArgumentNullException(nameof(vertices));
            }
            if (vertices.Count < 3)
            {
                throw new ArgumentException("at least three vertices are needed", nameof(vertices));
            }
            if (!(thickness > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(thickness));
            }
            Vertices = vertices;
            Thickness = thickness;

            int count = vertices.Count;
            for (int v = 0; v < count; v++)
            {
                Point3D here = vertices[v];
                Point3D next = vertices[(v + 1) % count];
                double hn = here.X!.Value, he = here.Y!.Value, hv = here.Z!.Value;
                double nn = next.X!.Value, ne = next.Y!.Value, nv = next.Z!.Value;
                // Newell: the area weighted normal, which survives vertices that are not quite coplanar
                normal_[0] += (he - ne) * (hv + nv);
                normal_[1] += (hv - nv) * (hn + nn);
                normal_[2] += (hn - nn) * (he + ne);
                centre_[0] += hn;
                centre_[1] += he;
                centre_[2] += hv;
            }
            for (int a = 0; a < 3; a++)
            {
                centre_[a] /= count;
            }
            double length = System.Math.Sqrt(normal_[0] * normal_[0] + normal_[1] * normal_[1]
                                             + normal_[2] * normal_[2]);
            if (!(length > 0))
            {
                throw new ArgumentException("the vertices are collinear", nameof(vertices));
            }
            for (int a = 0; a < 3; a++)
            {
                normal_[a] /= length;
            }

            // any pair of directions in the plane will do for the inside test
            int smallest = 0;
            for (int a = 1; a < 3; a++)
            {
                if (System.Math.Abs(normal_[a]) < System.Math.Abs(normal_[smallest]))
                {
                    smallest = a;
                }
            }
            double[] helper = new double[3];
            helper[smallest] = 1.0;
            Cross(helper, normal_, firstAxis_);
            Normalise(firstAxis_);
            Cross(normal_, firstAxis_, secondAxis_);
            Normalise(secondAxis_);

            flatNorth_ = new double[count];
            flatEast_ = new double[count];
            for (int v = 0; v < count; v++)
            {
                Project(vertices[v].X!.Value, vertices[v].Y!.Value, vertices[v].Z!.Value,
                        out flatNorth_[v], out flatEast_[v], out double _);
                for (int a = 0; a < 3; a++)
                {
                    double value = a == 0 ? vertices[v].X!.Value
                                 : a == 1 ? vertices[v].Y!.Value : vertices[v].Z!.Value;
                    double half = 0.5 * thickness;
                    if (value - half < BoundingBoxMinimum[a]) { BoundingBoxMinimum[a] = value - half; }
                    if (value + half > BoundingBoxMaximum[a]) { BoundingBoxMaximum[a] = value + half; }
                }
            }
            for (int v = 0; v < count; v++)
            {
                double reachFirst = System.Math.Abs(flatNorth_[v]);
                double reachSecond = System.Math.Abs(flatEast_[v]);
                if (reachFirst > halfExtent_[0]) { halfExtent_[0] = reachFirst; }
                if (reachSecond > halfExtent_[1]) { halfExtent_[1] = reachSecond; }
            }
        }

        /// <summary>
        /// How much of the rate the target draws belongs at the given position, as a relative weight.
        /// <para>
        /// It is a weight and not a rate: the caller scales the weights of the cells it actually has so
        /// that they sum to the rate injected, the problem being pure Neumann.
        /// </para>
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <returns></returns>
        public double GetArrivalWeight(double north, double east, double vertical)
        {
            if (ArrivalWeighting != TargetArrivalWeighting.Gaussian)
            {
                return 1.0;
            }
            Project(north, east, vertical, out double u, out double v, out double _);
            double centreFirst = 0, centreSecond = 0;
            if (ArrivalCentre != null)
            {
                Project(ArrivalCentre.X!.Value, ArrivalCentre.Y!.Value, ArrivalCentre.Z!.Value,
                        out centreFirst, out centreSecond, out double _);
            }
            double spreadFirst = ArrivalSpreadFirst > 0
                ? ArrivalSpreadFirst : halfExtent_[0] / 3.0;
            double spreadSecond = ArrivalSpreadSecond > 0
                ? ArrivalSpreadSecond : halfExtent_[1] / 3.0;
            if (!(spreadFirst > 0) || !(spreadSecond > 0))
            {
                return 1.0;
            }
            double first = (u - centreFirst) / spreadFirst;
            double second = (v - centreSecond) / spreadSecond;
            return System.Math.Exp(-0.5 * (first * first + second * second));
        }

        /// <summary>
        /// the unit normal of the plane of the polygon
        /// </summary>
        public Vector3D GetNormal()
        {
            return new Vector3D(normal_[0], normal_[1], normal_[2]);
        }

        /// <summary>
        /// Which way a streamline travels as it arrives, as a unit vector: the normal of the plane,
        /// turned to point the way the approach direction does.
        /// <para>
        /// With both faces admitted there is no preferred side, and the normal is returned as it comes
        /// out of Newell's formula.
        /// </para>
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        public void GetArrivalUnit(out double north, out double east, out double vertical)
        {
            north = normal_[0];
            east = normal_[1];
            vertical = normal_[2];
            if (ApproachDirection == null)
            {
                return;
            }
            double along = north * (ApproachDirection.X ?? 0) + east * (ApproachDirection.Y ?? 0)
                           + vertical * (ApproachDirection.Z ?? 0);
            if (along < 0)
            {
                north = -north;
                east = -east;
                vertical = -vertical;
            }
        }

        /// <summary>
        /// The box the target needs together with the room its landing sections take, so that the octree
        /// can carry the same cell size along them. A conduit that changed cell size partway would keep
        /// only one of the four sub-faces at the change and so pinch itself.
        /// </summary>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        public void GetLandingBox(double[] minimum, double[] maximum)
        {
            double reach = Incidence == TargetIncidence.Perpendicular ? LandingLength
                           : Incidence == TargetIncidence.Through ? ThroughLength : 0;
            // a guided landing needs no uniform cells, the medium having no faces to pinch
            double[] axis = { normal_[0], normal_[1], normal_[2] };
            for (int a = 0; a < 3; a++)
            {
                double grown = reach * System.Math.Abs(axis[a]);
                minimum[a] = BoundingBoxMinimum[a] - grown;
                maximum[a] = BoundingBoxMaximum[a] + grown;
            }
        }

        /// <summary>
        /// checks that the arrival constraint is usable
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool IsValid(out string? reason)
        {
            reason = null;
            if (Sides == TargetSides.One
                && (ApproachDirection == null || (ApproachDirection.GetLength() ?? 0) <= 0))
            {
                reason = "admitting one face needs an ApproachDirection to say which face it is.";
            }
            else if (Sides == TargetSides.One && ApproachDirection != null)
            {
                double along = normal_[0] * (ApproachDirection.X ?? 0)
                               + normal_[1] * (ApproachDirection.Y ?? 0)
                               + normal_[2] * (ApproachDirection.Z ?? 0);
                if (System.Math.Abs(along) < 1.0e-9)
                {
                    reason = "the ApproachDirection lies in the plane of the polygon, so it names no face.";
                }
            }
            if (reason == null && Incidence != TargetIncidence.Free && !(LandingLength > 0))
            {
                reason = "a constrained arrival needs a strictly positive LandingLength.";
            }
            if (reason == null && Incidence == TargetIncidence.Guided && !(GuideContrast > 1.0))
            {
                reason = "a guided arrival needs a GuideContrast greater than one.";
            }
            else if (ArrivalSpreadFirst < 0 || ArrivalSpreadSecond < 0)
            {
                reason = "an arrival spread cannot be negative.";
            }
            else if (ArrivalWeighting == TargetArrivalWeighting.Gaussian && ArrivalCentre != null
                     && !Contains(ArrivalCentre.X!.Value, ArrivalCentre.Y!.Value,
                                  ArrivalCentre.Z!.Value))
            {
                // an eccentric centre outside a re-entrant polygon would weight the whole target by its
                // tail, which is a silent way of drawing a different target than the one given
                reason = "the arrival centre lies outside the target.";
            }
            return reason == null;
        }

        /// <summary>
        /// the middle of the vertices
        /// </summary>
        public Point3D GetCentre()
        {
            return new Point3D(centre_[0], centre_[1], centre_[2]);
        }

        /// <summary>
        /// whether the given position lies inside the prism
        /// </summary>
        public bool Contains(double north, double east, double vertical)
        {
            Project(north, east, vertical, out double u, out double v, out double away);
            if (System.Math.Abs(away) > 0.5 * Thickness)
            {
                return false;
            }
            return IsInsidePolygon(u, v);
        }

        private void Project(double north, double east, double vertical,
                             out double u, out double v, out double away)
        {
            double dn = north - centre_[0];
            double de = east - centre_[1];
            double dv = vertical - centre_[2];
            u = dn * firstAxis_[0] + de * firstAxis_[1] + dv * firstAxis_[2];
            v = dn * secondAxis_[0] + de * secondAxis_[1] + dv * secondAxis_[2];
            away = dn * normal_[0] + de * normal_[1] + dv * normal_[2];
        }

        /// <summary>
        /// the crossing number test, in the plane of the polygon
        /// </summary>
        private bool IsInsidePolygon(double u, double v)
        {
            bool inside = false;
            int count = flatNorth_.Length;
            for (int a = 0, b = count - 1; a < count; b = a++)
            {
                double ua = flatNorth_[a], va = flatEast_[a];
                double ub = flatNorth_[b], vb = flatEast_[b];
                if (va > v != vb > v)
                {
                    double at = ua + (v - va) / (vb - va) * (ub - ua);
                    if (u < at)
                    {
                        inside = !inside;
                    }
                }
            }
            return inside;
        }

        private static void Cross(double[] first, double[] second, double[] result)
        {
            result[0] = first[1] * second[2] - first[2] * second[1];
            result[1] = first[2] * second[0] - first[0] * second[2];
            result[2] = first[0] * second[1] - first[1] * second[0];
        }

        private static void Normalise(double[] vector)
        {
            double length = System.Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1]
                                             + vector[2] * vector[2]);
            if (length > 0)
            {
                for (int a = 0; a < 3; a++)
                {
                    vector[a] /= length;
                }
            }
        }
    }
}
