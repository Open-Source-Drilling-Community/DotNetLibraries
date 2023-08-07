using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// a non localized constant build and turn curve
    /// </summary>
    [Serializable]
    public class NonLocalizedBuildAndTurnCurve : NonLocalizedCurve
    {
        /// <summary>
        /// build up rate when positive, drop-off rate when negative, i.e., inclination gradien
        /// </summary>
        public double? BUR { get; set; }
        /// <summary>
        /// turn rate, i.e., azimuth gradient
        /// </summary>
        public double? TR { get; set; }
        /// <summary>
        ///  default constructor
        /// </summary>
        public NonLocalizedBuildAndTurnCurve()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public NonLocalizedBuildAndTurnCurve(NonLocalizedBuildAndTurnCurve src)
        {
            if (src != null)
            {
                Length = src.Length;
                BUR = src.BUR;
                TR = src.TR;
            }
        }
    }
}
