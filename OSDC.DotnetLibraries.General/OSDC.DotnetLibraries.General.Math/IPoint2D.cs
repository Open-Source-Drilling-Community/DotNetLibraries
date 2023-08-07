using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IPoint2D: IEquivalent<IPoint2D>, ICloneable, IUndefinable, IZeroeable
    {
        /// <summary>
        /// the X coordinate
        /// </summary>
        public double? X { get; set; }
        /// <summary>
        /// the Y coordinate
        /// </summary>
        public double? Y { get; set; }

        void Set(IPoint2D point);

        void Set(double? x, double? y);

        /// <summary>
        /// calculate the cross product between the vector P1P2 and P1P3 where P1 is this.
        /// </summary>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        double? CrossProduct(IPoint2D p2, IPoint2D p3);
    }
}
