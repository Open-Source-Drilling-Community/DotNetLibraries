using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// One cross-section of a <see cref="StreamlineBundleFactory"/>: a plane perpendicular to the median
    /// curve, the frame that gives the polar angle its meaning in that plane, and the transform that
    /// carries the normalized cross-section onto it.
    /// </summary>
    public class CrossSectionStation
    {
        /// <summary>
        /// the curvilinear abscissa of the cross-section along the median curve
        /// </summary>
        public double Abscissa { get; internal set; } = 0;

        /// <summary>
        /// where the median curve passes through the cross-section
        /// </summary>
        public Point3D? Position { get; internal set; } = null;

        /// <summary>
        /// the unit tangent of the median curve, normal to the plane of the cross-section
        /// </summary>
        public Vector3D? Tangent { get; internal set; } = null;

        /// <summary>
        /// the in-plane direction of the polar angle zero. Carried along the median curve by a rotation
        /// minimising frame, so it turns no more than the curve obliges it to, and it stays defined where
        /// the curve is straight, which the Frenet normal does not.
        /// </summary>
        public Vector3D? FirstNormal { get; internal set; } = null;

        /// <summary>
        /// the in-plane direction of the polar angle of a quarter turn, completing a right handed frame
        /// with <see cref="Tangent"/> and <see cref="FirstNormal"/>
        /// </summary>
        public Vector3D? SecondNormal { get; internal set; } = null;

        /// <summary>
        /// The outline of the bundle in this cross-section, about the median. It replaces the affine
        /// transform the station used to carry: the outline states where the bundle ends, which a least
        /// squares fit through the middle of the crossings never did.
        /// </summary>
        public CrossSectionPolygon? Polygon { get; internal set; } = null;

        /// <summary>
        /// How far the bundle has turned about the median by this cross-section, rad.
        /// <para>
        /// A radial map carries an angle straight through, so on its own it cannot express a bundle that
        /// rotates about its own axis as it goes; the old affine transform could. One angle per station
        /// restores that much of it. What neither the twist nor the outline can express is shear, which
        /// therefore lands in the residual.
        /// </para>
        /// </summary>
        public double Twist { get; internal set; } = 0;

        /// <summary>
        /// how many of the streamlines of the bundle reached this cross-section
        /// </summary>
        public int StreamlineCount { get; internal set; } = 0;

        /// <summary>
        /// the curvature of the median curve here. The cross-sections stop being a valid description of
        /// the bundle where the radius of the bundle exceeds the inverse of this, because the planes then
        /// cut into one another.
        /// </summary>
        public double Curvature { get; internal set; } = 0;

        /// <summary>
        /// how far, on average, the streamlines of the bundle sit from where the transform puts them.
        /// What the surrogate leaves out.
        /// </summary>
        public double ResidualDeviation { get; internal set; } = 0;

        /// <summary>
        /// how far the streamlines of the bundle sit from the median curve here, on average
        /// </summary>
        public double Radius { get; internal set; } = 0;

        /// <summary>
        /// Carries a normalized polar draw onto this cross-section, in its own plane.
        /// <para>
        /// The angle passes through, turned by the twist of the station, and the radius is taken as a
        /// fraction of how far the outline reaches in that direction. A draw of radius one therefore
        /// lands exactly on the boundary and anything less lands inside it, so a produced streamline is
        /// contained by construction rather than by a fit that happened to be close.
        /// </para>
        /// </summary>
        /// <param name="r">the normalized radius, one being the boundary</param>
        /// <param name="theta">the normalized angle, rad</param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        public void Apply(double r, double theta, out double u, out double v)
        {
            // The outline is held in the untwisted frame, which is the frame the normalized angle is
            // measured in, so the boundary is looked up at theta and the twist is applied only when the
            // point is placed. Looking it up at the twisted angle instead reads the outline of a
            // different direction, and the further the cross-section has turned the further out it reads.
            double reach = Polygon != null ? r * Polygon.GetBoundaryRadius(theta) : 0;
            double turned = theta + Twist;
            u = reach * System.Math.Cos(turned);
            v = reach * System.Math.Sin(turned);
        }

        /// <summary>
        /// whether an in-plane position of this cross-section lies inside the outline. The position is
        /// given in the frame of the cross-section, so the twist is taken off before the outline is asked.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool Contains(double u, double v)
        {
            if (Polygon == null)
            {
                return false;
            }
            double reach = System.Math.Sqrt(u * u + v * v);
            return reach <= Polygon.GetBoundaryRadius(System.Math.Atan2(v, u) - Twist);
        }

        /// <summary>
        /// how far the outline reaches in the direction of the given in-plane position, m
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public double GetBoundaryRadius(double u, double v)
        {
            return Polygon == null
                ? 0 : Polygon.GetBoundaryRadius(System.Math.Atan2(v, u) - Twist);
        }

        /// <summary>
        /// where a normalized polar draw falls in space
        /// </summary>
        /// <param name="r"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        public Point3D? GetPosition(double r, double theta)
        {
            if (Position == null || FirstNormal == null || SecondNormal == null)
            {
                return null;
            }
            Apply(r, theta, out double u, out double v);
            return new Point3D(Position.X!.Value + u * FirstNormal.X!.Value + v * SecondNormal.X!.Value,
                               Position.Y!.Value + u * FirstNormal.Y!.Value + v * SecondNormal.Y!.Value,
                               Position.Z!.Value + u * FirstNormal.Z!.Value + v * SecondNormal.Z!.Value);
        }
    }
}
