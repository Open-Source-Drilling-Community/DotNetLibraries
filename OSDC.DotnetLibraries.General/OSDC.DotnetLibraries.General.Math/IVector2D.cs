using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IVector2D : IEquatable<IVector2D>
    {
        /// <summary>
        /// the x-coordinate
        /// </summary>
        double? X { get; set; }
        /// <summary>
        /// the y-coordinate
        /// </summary>
        double? Y { get; set; }
        /// <summary>
        /// equals at numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(IVector2D cmp);
    }
}
