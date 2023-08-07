using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// a generic non localized curve
    /// </summary>
    [Serializable]
    public class NonLocalizedCurve
    {
        public double? Length { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public NonLocalizedCurve() : base()
        {

        }
    }
}
