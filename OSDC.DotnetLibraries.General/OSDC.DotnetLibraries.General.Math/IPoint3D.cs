using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IPoint3D : IPoint2D, IEquatable<IPoint3D>
    {
        /// <summary>
        /// the Z coordinate
        /// </summary>
        public double? Z { get; set; }
        /// <summary>
        /// equality test at numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(IPoint3D cmp);
    }
}
