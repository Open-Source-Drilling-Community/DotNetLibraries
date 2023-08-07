using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IPoint3D : IPoint2D, IEquivalent<IPoint3D>
    {
        /// <summary>
        /// the Z coordinate
        /// </summary>
        public double? Z { get; set; }

        void Set(IPoint3D point);

        void Set(double? x, double? y, double? z);

    }
}
