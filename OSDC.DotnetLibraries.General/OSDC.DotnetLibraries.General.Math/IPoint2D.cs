using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IPoint2D: IEquatable<IPoint2D>
    {
        /// <summary>
        /// the X coordinate
        /// </summary>
        public double? X { get; set; }
        /// <summary>
        /// the Y coordinate
        /// </summary>
        public double? Y { get; set; }
        /// <summary>
        /// equality test at numeric accuracty
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(IPoint2D cmp);
    }
}
