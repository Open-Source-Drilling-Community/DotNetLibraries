using System;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// a section has a starting and ending point and a curvilinear curve in between
    /// </summary>
    [Serializable]
    public abstract class ArcSection
    {
        public CurvilinearPoint3D Start { get; set; } = new CurvilinearPoint3D();
        public CurvilinearPoint3D End { get; set; } = new CurvilinearPoint3D();

        public abstract NonLocalizedCurve Curve { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public ArcSection()
        {
        }

        public abstract bool Calculate();

        public abstract CurvilinearPoint3D InterpolateAtMD(double md);

        public CurvilinearPoint3D InterpolateAtTVD(double tvd)
        {
            if (Start == null || End == null)
            {
                return null;
            }
            if (Start.Abscissa == null || End.Abscissa == null)
            {
                return null;
            }
            if (Start.Z == null || End.Z == null)
            {
                return null;
            }
            double z = (double)tvd;
            double z0 = (double)Start.Z;
            double z1 = (double)End.Z;
            if (!Numeric.IsBetween(z, z0, z1))
            {
                return null;
            }
            double s0 = (double)Start.Abscissa;
            double s1 = (double)End.Abscissa;
            if (Numeric.EQ(s0, s1))
            {
                return new CurvilinearPoint3D(Start);
            }
            CurvilinearPoint3D inter = null;
            int c = 0;
            do
            {
                double alpha = (z - z0) / (z1 - z0);
                double s = s0 + alpha * (s1 - s0);
                inter = InterpolateAtMD(s);
                if (inter != null && inter.Z != null)
                {
                    if (z0 <= z1)
                    {
                        if ((double)inter.Z <= z)
                        {
                            s0 = s;
                        }
                        else
                        {
                            s1 = s;
                        }
                    }
                    else
                    {
                        if ((double)inter.Z <= z)
                        {
                            s1 = s;
                        }
                        else
                        {
                            s0 = s;
                        }

                    }
                }
            } while (inter != null && inter.Z != null && System.Math.Abs((double)inter.Z - z) > 0.001 && c++ < 50);
            if (inter != null && inter.Z != null && System.Math.Abs((double)inter.Z - z) > 0.011)
            {
                return null;
            }
            else
            {
                return inter;
            }
        }
    }
}
