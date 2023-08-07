using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IVector3D : IVector2D, IEquivalent<IVector3D>
    {
        /// <summary>
        /// the z-coordinate
        /// </summary>
        double? Z { get; set; }
    }
}
