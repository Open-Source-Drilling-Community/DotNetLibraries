using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// a section based on a constant build and turn curve in between the start and end of the section
    /// </summary>
    [Serializable]
    public class BuildAndTurnArcSection : ArcSection
    {
        public NonLocalizedBuildAndTurnCurve BuildAndTurn { get; set; } = new NonLocalizedBuildAndTurnCurve();
        /// <summary>
        /// 
        /// </summary>
        public override NonLocalizedCurve Curve
        {
            get => BuildAndTurn;
            set
            {
                if (value is NonLocalizedBuildAndTurnCurve)
                {
                    BuildAndTurn = (NonLocalizedBuildAndTurnCurve)value;
                }
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public BuildAndTurnArcSection()
        {
        }

        public override CurvilinearPoint3D InterpolateAtMD(double md)
        {
            BuildAndTurnArcSection sec = new BuildAndTurnArcSection();
            sec.Start = Start;
            sec.BuildAndTurn = new NonLocalizedBuildAndTurnCurve(BuildAndTurn);
            sec.BuildAndTurn.Length = (double?)md - Start.Abscissa;
            sec.End = new CurvilinearPoint3D();
            sec.End.Abscissa = md;
            sec.CalculateLBT();
            return sec.End;
        }

        public override bool Calculate()
        {
            if (End != null && Curve != null)
            {
                if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                    Numeric.IsDefined(BuildAndTurn.TR) &&
                    Numeric.IsDefined(BuildAndTurn.Length))
                {
                    return CalculateLBT();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Abscissa))
                {
                    return CalculateBTS();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateBTZ();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateBTA();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateBTI();
                }
                else if (Numeric.IsDefined(End.Abscissa) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateSIA();
                }
                else if (Numeric.IsDefined(BuildAndTurn.Length) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateLIA();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateBIA();
                }
                else if (Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateTIA();
                }
                else if (Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Abscissa) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateTSI();
                }
                else if (Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(BuildAndTurn.Length) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateTLI();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(End.Abscissa) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateBSI();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(BuildAndTurn.Length) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateBLI();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(End.Azimuth) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateBAZ();
                }
                else if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateBIZ();
                }
                else if (Numeric.IsDefined(BuildAndTurn.TR) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateTIZ();
                }
                else if (Numeric.IsDefined(End.Abscissa) &&
                         Numeric.IsDefined(End.Azimuth) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateSAZ();
                }
                else if (Numeric.IsDefined(BuildAndTurn.Length) &&
                         Numeric.IsDefined(End.Azimuth) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateLAZ();
                }
                else if (Numeric.IsDefined(End.X) &&
                         Numeric.IsDefined(End.Y) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateXYZ();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool CalculateLBT()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(BuildAndTurn.Length))
            {
                End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                return CalculateBTS();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBTS()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Abscissa))
            {
                BuildAndTurn.Length = End.Abscissa - Start.Abscissa;
                double dm = (double)BuildAndTurn.Length;
                double incl = (double)Start.Inclination;
                double az = (double)Start.Azimuth;
                double p =  (double)(BuildAndTurn.BUR + BuildAndTurn.TR);
                double s = (double)(BuildAndTurn.BUR - BuildAndTurn.TR);
                double pdm = p * dm;
                double sdm = s * dm;
                End.Inclination = Start.Inclination + BuildAndTurn.BUR * dm;
                End.Azimuth = Start.Azimuth + BuildAndTurn.TR * dm;
                if (End.Inclination < 0)
                {
                    End.Inclination = -End.Inclination;
                    End.Azimuth += Numeric.PI;
                }
                if (Numeric.EQ(BuildAndTurn.BUR, 0))
                {
                    End.Z = Start.Z + System.Math.Cos(incl) * dm;
                }
                else
                {
                    End.Z = Start.Z + (System.Math.Sin((double)End.Inclination) - System.Math.Sin(incl)) / BuildAndTurn.BUR;
                }
                double E = System.Math.Sin(incl);
                double F = System.Math.Cos(incl);
                double G = System.Math.Sin(az);
                double H = System.Math.Cos(az);
                if (Numeric.EQ(System.Math.Abs((double)BuildAndTurn.BUR), System.Math.Abs((double)BuildAndTurn.TR)))
                {
                    if (Numeric.EQ(BuildAndTurn.BUR,0))
                    {
                        End.X = Start.X + E * H * dm;
                        End.Y = Start.Y = E * G * dm;
                        return true;
                    }
                    else
                    {
                        double epsilon = (BuildAndTurn.BUR * BuildAndTurn.TR >= 0) ? 1 : -1;
                        double s2b = System.Math.Sin(2.0 * (double)BuildAndTurn.BUR * dm);
                        double c2b = System.Math.Cos(2.0 * (double)BuildAndTurn.BUR * dm);
                        double _2b = 2.0 * (double)BuildAndTurn.BUR;
                        End.X = Start.X + (0.5 * E * H * (dm + s2b / _2b) - 0.5 * E * G * epsilon * (1.0 - c2b) / _2b) + (0.5 * F * H * (1.0 - c2b) / _2b - 0.5 * F * G * epsilon * (dm - s2b / _2b));
                        End.Y = Start.Y + (0.5 * E * G * (dm + s2b / _2b) + 0.5 * E * H * epsilon * (1.0 - c2b) / _2b) + (0.5 * F * G * (1.0 - c2b) / _2b + 0.5 * F * H * epsilon * (dm - s2b / _2b));
                        return true;
                    }
                }
                else
                {
                    double cosp = System.Math.Cos(pdm) / p;
                    double sinp = System.Math.Sin(pdm) / p;
                    double coss = System.Math.Cos(sdm) / s;
                    double sins = System.Math.Sin(sdm) / s;
                    double _1p = 1.0 / p;
                    double _1s = 1.0 / s;
                    End.X = Start.X + (0.5 * E * H * (sinp + sins) - 0.5 * E * G * ((coss - cosp) - (_1s - _1p))) - (0.5 * F * G * (sins - sinp) + 0.5 * F * H * (cosp + coss - (_1p + _1s)));
                    End.Y = Start.Y + (0.5 * E * G * (sinp + sins) + 0.5 * E * H * ((coss - cosp) - (_1s - _1p))) + (0.5 * F * H * (sins - sinp) - 0.5 * F * G * (cosp + coss - (_1p + _1s)));
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBTZ()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Z))
            {
                if (Numeric.EQ(BuildAndTurn.BUR, 0))
                {
                    if (Numeric.EQ(System.Math.Cos((double)Start.Inclination), 0))
                    {
                        if (Numeric.EQ(End.Z, Start.Z))
                        {
                            CalculateID();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        BuildAndTurn.Length = (End.Z - Start.Z) / System.Math.Cos((double)Start.Inclination);
                        End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                        return CalculateBTS();
                    }
                }
                else
                {
                    End.Inclination = Numeric.AsinEqual(System.Math.Sin((double)Start.Inclination) + (End.Z - Start.Z) * BuildAndTurn.BUR);
                    return CalculateBTI();
                }
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBTA()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Azimuth))
            {
                if (!Numeric.EQ(BuildAndTurn.TR, 0))
                {
                    BuildAndTurn.Length = (End.Azimuth - Start.Azimuth) / BuildAndTurn.TR;
                    End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                    return CalculateBTS();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool CalculateBTI()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Inclination))
            {
                if (!Numeric.EQ(BuildAndTurn.BUR, 0))
                {
                    if ((End.Inclination - Start.Inclination) * BuildAndTurn.BUR >= 0)
                    {
                        BuildAndTurn.Length = (End.Inclination - Start.Inclination) / BuildAndTurn.BUR;
                    }
                    else
                    {
                        BuildAndTurn.Length = 2.0 * Numeric.PI / System.Math.Abs((double)BuildAndTurn.BUR) + (End.Inclination - Start.Inclination) / BuildAndTurn.BUR;
                    }
                    return CalculateBTS();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool CalculateSIA()
        {
            if (Numeric.IsDefined(End.Abscissa) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                BuildAndTurn.Length = End.Abscissa - Start.Abscissa;
                if (Numeric.EQ(Start.Abscissa, End.Abscissa))
                {
                    return CalculateID();
                }
                else
                {
                    BuildAndTurn.BUR = (End.Inclination - Start.Inclination) / BuildAndTurn.Length;
                    BuildAndTurn.TR = (End.Azimuth - Start.Azimuth) / BuildAndTurn.Length;
                    return CalculateBTS();
                }
            }
            else
            {
                return false;
            }
        }

        public bool CalculateLIA()
        {
            if (Numeric.IsDefined(BuildAndTurn.Length) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                return CalculateSIA();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBIA()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                throw new Exception();
                /*
                if (!Numeric.EQ(BuildAndTurn.BUR, 0) &&
                    !Numeric.EQ(Start.Inclination, End.Inclination))
                {
                    BuildAndTurn.Length = (End.Inclination - Start.Inclination) / BuildAndTurn.BUR;
                    End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                    
                }
                else
                {
                    return false;
                }
                */
            }
            else
            {
                return false;
            }
        }

        public bool CalculateTIA()
        {
            if (Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                throw new Exception();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateTSI()
        {
            if (Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Abscissa) &&
                Numeric.IsDefined(End.Inclination))
            {
                throw new Exception();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateTLI()
        {
            if (Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(BuildAndTurn.Length) &&
                Numeric.IsDefined(End.Inclination))
            {
                End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                return CalculateTSI();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBSI()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(End.Abscissa) &&
                Numeric.IsDefined(End.Inclination))
            {
                throw new Exception();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBLI()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(BuildAndTurn.Length) &&
                Numeric.IsDefined(End.Inclination))
            {
                End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                return CalculateBSI();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBAZ()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(End.Azimuth) &&
                Numeric.IsDefined(End.Z))
            {
                throw new Exception();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateBIZ()
        {
            if (Numeric.IsDefined(BuildAndTurn.BUR) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Z))
            {
                throw new Exception();
            }
            else
            {
                return false;
            }
        }
        public bool CalculateTIZ()
        {
            if (Numeric.IsDefined(BuildAndTurn.TR) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Z))
            {
                throw new Exception();
            }
            else
            {
                return false;
            }
        }
        public bool CalculateSAZ()
        {
            if (Numeric.IsDefined(End.Abscissa) &&
                Numeric.IsDefined(End.Azimuth) &&
                Numeric.IsDefined(End.Z))
            {
                throw new Exception();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateLAZ()
        {
            if (Numeric.IsDefined(BuildAndTurn.Length) &&
                Numeric.IsDefined(End.Azimuth) &&
                Numeric.IsDefined(End.Z))
            {
                End.Abscissa = Start.Abscissa + BuildAndTurn.Length;
                return CalculateSAZ();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateXYZ()
        {
            if (Numeric.IsDefined(End.X) &&
                Numeric.IsDefined(End.Y) &&
                Numeric.IsDefined(End.Z))
            {
                throw new Exception();
            }
            else
            {
                return false;
            }
        }

        public bool CalculateID()
        {
            End.X = Start.X;
            End.Y = Start.Y;
            End.Z = Start.Z;
            End.Inclination = Start.Inclination;
            End.Azimuth = Start.Azimuth;
            End.Abscissa = Start.Abscissa;
            BuildAndTurn.BUR = 0.0;
            BuildAndTurn.TR = 0.0;
            BuildAndTurn.Length = 0.0;
            return true;
        }
    }
}
