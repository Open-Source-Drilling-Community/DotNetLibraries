using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// an interface for Global Cordinate 3D point
    /// </summary>
    public interface IPoint3DGlobalCoordinates : IPoint3D
    {
        double? LatitudeWGS84 { get; set; }

        double? LongitudeWGS84 { get; set; }

        double? TvdWGS84 { get; set; }
    }
}
