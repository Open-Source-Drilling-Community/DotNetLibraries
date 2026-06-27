using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// an interface for a 3D point carrying both Riemannian coordinates and geodetic coordinates.
    /// </summary>
    public interface IPoint3DGlobalCoordinates : IPoint3D, IGeodeticPoint3D
    {
        /// <summary>
        /// Synonym of X. Arc length from the equator to the latitude of the point.
        /// </summary>
        double? RiemannianNorth { get; set; }

        /// <summary>
        /// Synonym of Y. Arc length from the Greenwich meridian to the longitude of the point along the latitude parallel.
        /// </summary>
        double? RiemannianEast { get; set; }
    }
}
