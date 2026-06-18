using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// a section based on a circular arc in between the start and end of the section
    /// </summary>
    [Serializable]
    public class CircularArcSection : ArcSection
    {
        public enum VariableType { MD, Length, Incl, Az, Z, X, Y, DLS, TF };
        public enum PhysicalDimensionType { Length, Angle, Curvature };

        public static Dictionary<VariableType, PhysicalDimensionType> VariablePhysicalDimension = new Dictionary<VariableType, PhysicalDimensionType>()
        {
            { VariableType.MD, PhysicalDimensionType.Length },
            { VariableType.Length, PhysicalDimensionType.Length },
            { VariableType.Incl, PhysicalDimensionType.Angle },
            { VariableType.Az, PhysicalDimensionType.Angle },
            { VariableType.Z, PhysicalDimensionType.Length },
            { VariableType.X, PhysicalDimensionType.Length },
            { VariableType.Y, PhysicalDimensionType.Length },
            { VariableType.DLS, PhysicalDimensionType.Curvature },
            { VariableType.TF, PhysicalDimensionType.Angle }
         };

        public enum FunctionType { LIA, SIA, DIA, XYZ, DTZ, DTI, LDT, SDT}

        public static Dictionary<FunctionType, Tuple<VariableType, VariableType, VariableType>> FunctionTypeVariables = new Dictionary<FunctionType, Tuple<VariableType, VariableType, VariableType>>()
        {
            { FunctionType.LIA, new Tuple<VariableType, VariableType, VariableType>(VariableType.Length, VariableType.Incl, VariableType.Az) },
            { FunctionType.SIA, new Tuple<VariableType, VariableType, VariableType>(VariableType.MD, VariableType.Incl, VariableType.Az) },
            { FunctionType.DIA, new Tuple<VariableType, VariableType, VariableType>(VariableType.DLS, VariableType.Incl, VariableType.Az) },
            { FunctionType.XYZ, new Tuple<VariableType, VariableType, VariableType>(VariableType.X, VariableType.Y, VariableType.Z) },
            { FunctionType.DTZ, new Tuple<VariableType, VariableType, VariableType>(VariableType.DLS, VariableType.TF, VariableType.Z) },
            { FunctionType.DTI, new Tuple<VariableType, VariableType, VariableType>(VariableType.DLS, VariableType.TF, VariableType.Incl) },
            { FunctionType.LDT, new Tuple<VariableType, VariableType, VariableType>(VariableType.Length, VariableType.DLS, VariableType.TF) },
            { FunctionType.SDT, new Tuple<VariableType, VariableType, VariableType>(VariableType.MD, VariableType.DLS, VariableType.TF) }
        };

        public static Dictionary<FunctionType, bool> DeterministicFunctions = new Dictionary<FunctionType, bool>()
        {
            {FunctionType.LIA, true },
            {FunctionType.SIA, true },
            {FunctionType.DIA, false },
            {FunctionType.XYZ, true },
            {FunctionType.DTZ, false },
            {FunctionType.DTI, false },
            {FunctionType.LDT, true },
            {FunctionType.SDT, true }
      };

        public NonLocalizedCircle3D Circle { get; set; } = new NonLocalizedCircle3D();
        public override NonLocalizedCurve Curve
        {
            get => Circle;
            set
            {
                if (value is NonLocalizedCircle3D)
                {
                    Circle = (NonLocalizedCircle3D)value;
                }
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public CircularArcSection()
        {
        }

        public CircularArcSection(CurvilinearPoint3D start, CurvilinearPoint3D end)
        {
            if (start != null)
            {
                Start.Set(start);
            }
            if (end != null)
            {
                End.Set(end);
            }
        }

        public List<VariableType> GetDefinedVariables()
        {
            List<VariableType> variables = new List<VariableType>();
            if (Numeric.IsDefined(End.Abscissa))
            {
                variables.Add(VariableType.MD);
            }
            if (Numeric.IsDefined(End.Inclination))
            {
                variables.Add(VariableType.Incl);
            }
            if (Numeric.IsDefined(End.Azimuth))
            {
                variables.Add(VariableType.Az);
            }
            if (Numeric.IsDefined(End.X))
            {
                variables.Add(VariableType.X);
            }
            if (Numeric.IsDefined(End.Y))
            {
                variables.Add(VariableType.Y);
            }
            if (Numeric.IsDefined(End.Z))
            {
                variables.Add(VariableType.Z);
            }
            if (Numeric.IsDefined(Circle.Length))
            {
                variables.Add(VariableType.Length);
            }
            if (Numeric.IsDefined(Circle.Curvature))
            {
                variables.Add(VariableType.DLS);
            }
            if (Numeric.IsDefined(Circle.ReferenceToolface))
            {
                variables.Add(VariableType.TF);
            }
            return variables;
        }

        public double? Get(VariableType var)
        {
            switch (var)
            {
                case VariableType.MD:
                    return End.Abscissa;
                case VariableType.Incl:
                    return End.Inclination;
                case VariableType.Az:
                    return End.Azimuth;
                case VariableType.X:
                    return End.X;
                case VariableType.Y:
                    return End.Y;
                case VariableType.Z:
                    return End.Z;
                case VariableType.Length:
                    return Circle.Length;
                case VariableType.DLS:
                    return Circle.Curvature;
                default:
                    return Circle.ReferenceToolface;
            }
        }

        public void Set(VariableType var, double val)
        {
            switch(var)
            {
                case VariableType.MD:
                    End.Abscissa = val;
                    break;
                case VariableType.Incl:
                    End.Inclination = val;
                    break;
                case VariableType.Az:
                    End.Azimuth = val;
                    break;
                case VariableType.X:
                    End.X = val;
                    break;
                case VariableType.Y:
                    End.Y = val;
                    break;
                case VariableType.Z:
                    End.Z = val;
                    break;
                case VariableType.Length:
                    Circle.Length = val;
                    break;
                case VariableType.DLS:
                    Circle.Curvature = val;
                    break;
                default:
                    Circle.ReferenceToolface = val;
                    break;
            }
        }

        public static void GetCombinations(List<VariableType> variables, int n, List<List<VariableType>> combinations)
        {
            if (combinations == null)
            {
                combinations = new List<List<VariableType>>();
            }
            for (int i = 0; i < variables.Count; i++)
            {
                List<VariableType> rest = new List<VariableType>();
                for (int j = i+1; j < variables.Count; j++)
                {
                    rest.Add(variables[j]);
                }
                List<VariableType> combination = new List<VariableType>();
                combination.Add(variables[i]);
                GetCombinations(combination, n-1, rest, combinations);
            }
        }
        private static void GetCombinations(List<VariableType> combination, int n, List<VariableType> variables, List<List<VariableType>> combinations)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                List<VariableType> augmentedCombination = new List<VariableType>();
                for (int j = 0; j < combination.Count; j++)
                {
                    augmentedCombination.Add(combination[j]);
                }
                augmentedCombination.Add(variables[i]);
                List<VariableType> rest = new List<VariableType>();
                for (int j = i + 1; j < variables.Count; j++)
                {
                    rest.Add(variables[j]);
                }
                if (n == 1)
                {
                    combinations.Add(augmentedCombination);
                }
                else
                {
                    GetCombinations(augmentedCombination, n - 1, rest, combinations);
                }
            }
        }

        public static List<List<VariableType>> FilterSamePhysicalDimensions(List<List<VariableType>> combinations)
        {
            List<List<VariableType>> filteredCombinations = new List<List<VariableType>>(); 
            foreach (var combination in combinations)
            {
                if (combination != null && combination.Count > 0)
                {
                    PhysicalDimensionType dimensionType;
                    if (VariablePhysicalDimension.TryGetValue(combination[0], out dimensionType))
                    {
                        bool isSameDimension = true;
                        for (int j = 1; j < combination.Count; j++)
                        {
                            PhysicalDimensionType nextDimensionType;
                            if (VariablePhysicalDimension.TryGetValue(combination[j], out nextDimensionType))
                            {
                                if (nextDimensionType != dimensionType)
                                {
                                    isSameDimension = false;
                                    break;
                                }
                            }
                        }
                        if (isSameDimension)
                        {
                            filteredCombinations.Add(combination);
                        }
                    }
                }
            }
            return filteredCombinations;
        }

        public static List<VariableType> RemainingVariables(List<VariableType> initialList, List<VariableType> selectedVariables)
        {
            List<VariableType> remainingList = new List<VariableType>();
            foreach (var var in initialList)
            {
                if (!selectedVariables.Contains(var))
                {
                    remainingList.Add(var);
                }
            }
            return remainingList;
        }

        public static FunctionType? GetFunctionType(List<VariableType> vars)
        {
            FunctionType? functionType = null;
            if (vars != null)
            {
                foreach (FunctionType typ in FunctionTypeVariables.Keys)
                {
                    Tuple<VariableType, VariableType, VariableType> sig;
                    if (FunctionTypeVariables.TryGetValue(typ, out sig))
                    {
                        if (vars.Contains(sig.Item1) && vars.Contains(sig.Item2) && vars.Contains(sig.Item3))
                        {
                            functionType = typ;
                            break;
                        }
                    }
                }
            }
            return functionType;
        }

        public static List<Tuple<List<VariableType>, FunctionType>> FilterHasAFunctionType(List<List<VariableType>> combinations, List<VariableType> vars)
        {
            List<Tuple<List<VariableType>, FunctionType>> filteredList = new List<Tuple<List<VariableType>, FunctionType>>();
            if (combinations != null && vars != null)
            {
                foreach (var combination in combinations)
                {
                    List<VariableType> rest = RemainingVariables(vars, combination);
                    if (rest != null && rest.Count == 3)
                    {
                        FunctionType? typ = GetFunctionType(rest);
                        if (typ != null)
                        {
                            filteredList.Add(new Tuple<List<VariableType>, FunctionType>(combination, (FunctionType)typ));
                        }
                    }
                }
            }
            filteredList.Sort(Compare);
            return filteredList;
        }

        public static List<FunctionType> GetFunctionTypes(List<VariableType> vars)
        {
            List<FunctionType> types = new List<FunctionType>();
            foreach (var typ in FunctionTypeVariables.Keys)
            {
                Tuple<VariableType, VariableType, VariableType> sig;
                if (FunctionTypeVariables.TryGetValue(typ, out sig))
                {
                    if (sig.Item1 != VariableType.MD && sig.Item2 != VariableType.MD && sig.Item3 != VariableType.MD)
                    {
                        List<VariableType> list = new List<VariableType>() { sig.Item1, sig.Item2, sig.Item3 };
                        bool defined = true;
                        foreach (VariableType t in vars)
                        {
                            if (!list.Contains(t))
                            {
                                defined = false;
                                break;
                            }
                        }
                        if (defined)
                        {
                            types.Add(typ);
                        }
                    }
                }
            }
            types.Sort(Compare);
            return types;
        }

        public static List<VariableType> GetMissingVariables(List<VariableType> definedVars, FunctionType typ)
        {
            List<VariableType> missingVariables = new List<VariableType>();
            if (definedVars != null)
            {
                Tuple<VariableType, VariableType, VariableType> sig;
                if (FunctionTypeVariables.TryGetValue(typ, out sig))
                {
                    if (!definedVars.Contains(sig.Item1))
                    {
                        missingVariables.Add(sig.Item1);
                    }
                    if (!definedVars.Contains(sig.Item2))
                    {
                        missingVariables.Add(sig.Item2);
                    }
                    if (!definedVars.Contains(sig.Item3))
                    {
                        missingVariables.Add(sig.Item3);
                    }
                }
            }
            return missingVariables;
        }

        public static int Compare(FunctionType x1, FunctionType x2)
        {
            bool isDeterministic1, isDeterminstic2;
            if (DeterministicFunctions.TryGetValue(x1, out isDeterministic1) && DeterministicFunctions.TryGetValue(x2, out isDeterminstic2))
            {
                if (isDeterministic1)
                {
                    if (isDeterminstic2)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
        }

        public static int Compare(Tuple<List<VariableType>, FunctionType> x1, Tuple<List<VariableType>, FunctionType> x2)
        {
            if (x1 != null && x2 != null)
            {
                return Compare(x1.Item2, x2.Item2);
            }
            else
            {
                return -1;
            }
        }

        public bool Calculate(FunctionType typ)
        {
            switch (typ)
            {
                case FunctionType.DIA:
                    return CalculateDIA();
                case FunctionType.DTI:
                    return CalculateDTI();
                case FunctionType.DTZ:
                    return CalculateDTZ();
                case FunctionType.LDT:
                    return CalculateLDT();
                case FunctionType.LIA:
                    return CalculateLIA();
                case FunctionType.SDT:
                    return CalculateSDT();
                case FunctionType.SIA:
                    return CalculateSIA();
                default:
                    return CalculateXYZ();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Calculate()
        {
            if (End != null && Curve != null)
            {
                if (Numeric.IsDefined(End.Inclination) &&
                    Numeric.IsDefined(End.Azimuth) &&
                    Numeric.IsDefined(Circle.Length))
                {
                    return CalculateLIA();
                }
                else if (Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Azimuth) &&
                         Numeric.IsDefined(End.Abscissa))
                {
                    return CalculateSIA();
                }
                else if (Numeric.IsDefined(Circle.Curvature) &&
                         Numeric.IsDefined(End.Inclination) &&
                         Numeric.IsDefined(End.Azimuth))
                {
                    return CalculateDIA();
                }
                else if (Numeric.IsDefined(End.X) &&
                         Numeric.IsDefined(End.Y) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateXYZ();
                }
                else if (Numeric.IsDefined(Circle.Curvature) &&
                         Numeric.IsDefined(Circle.ReferenceToolface) &&
                         Numeric.IsDefined(End.Z))
                {
                    return CalculateDTZ();
                }
                else if (Numeric.IsDefined(Circle.Curvature) &&
                         Numeric.IsDefined(Circle.ReferenceToolface) &&
                         Numeric.IsDefined(End.Inclination))
                {
                    return CalculateDTI();
                }
                else if (Numeric.IsDefined(Circle.Curvature) &&
                         Numeric.IsDefined(Circle.ReferenceToolface) &&
                         Numeric.IsDefined(Circle.Length))
                {
                    return CalculateLDT();
                }
                else if (Numeric.IsDefined(Circle.Curvature) &&
                         Numeric.IsDefined(Circle.ReferenceToolface) &&
                         Numeric.IsDefined(End.Abscissa))
                {
                    return CalculateSDT();
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

        public bool CalculateLIA()
        {
            if (Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth) &&
                Numeric.IsDefined(Curve.Length))
            {
                End.Abscissa = Start.Abscissa + Curve.Length;
                return CalculateSIA();
            }
            else
            {
                return false;
            }
        }
        public bool CalculateSIA()
        {
            if (Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth) &&
                Numeric.IsDefined(End.Abscissa))
            {
                Start.MinimumCurvatureMethod(End);
                Circle.Curvature = Start.GetDLS(End);
                Circle.ReferenceToolface = Start.GetToolface(End);
                return true;
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
            Circle.Curvature = 0.0;
            Circle.ReferenceToolface = 0.0;
            Circle.Length = 0.0;
            return true;
        }
        public bool CalculateDIA()
        {
            if (Numeric.IsDefined(Circle.Curvature) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                if (Numeric.EQ(Circle.Curvature, 0))
                {
                   // if the curvature is 0 either the incl and az are not parallel to the start tangent or
                   // if they are parallel, it is not possible to determine the length as the point can
                   // be anywhere along that straight line.
                   return false;
                }
                else
                {
                    double dl = Numeric.AcosEqual(System.Math.Cos((double)(End.Inclination - Start.Inclination)) - (1.0 - System.Math.Cos((double)(End.Azimuth - Start.Azimuth))) * System.Math.Sin((double)End.Inclination) * System.Math.Sin((double)Start.Inclination));
                    Circle.Length = dl / Circle.Curvature;
                    End.Abscissa = Start.Abscissa + Circle.Length;
                    bool ok = CalculateSIA();
                    //if (false)
                    //{
                    //    using (System.IO.StreamWriter writer = new System.IO.StreamWriter("c:\\temp\\LDT.txt", true))
                    //    {
                    //        writer.WriteLine("DLS=" + (Circle.Curvature * 180.0 * 30.0 / Math.PI).ToString() + "\tTF=" + (Circle.ReferenceToolface * 180.0 / Math.PI).ToString() + "\tL=" + Circle.Length + "\tIncl=" + (End.Inclination * 180.0 / Math.PI).ToString() + "\tAz=" + (End.Azimuth * 180.0 / Math.PI).ToString() + "\tZ=" + End.Z + "\tX=" + End.X + "\tY=" + End.Y);
                    //    }
                    //}
                    return ok;
                }
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
                double ci = System.Math.Cos((double)Start.Inclination);
                double si = System.Math.Sin((double)Start.Inclination);
                double ca = System.Math.Cos((double)Start.Azimuth);
                double sa = System.Math.Sin((double)Start.Azimuth);
                Vector3D v1 = Vector3D.CreateSpheric(1.0, Start.Inclination, Start.Azimuth);
                Vector3D v2 = new Vector3D(Start, End);
                if (Numeric.EQ(End.Distance(Start), 0.0) || v1.IsParallel(v2, 1e-4))
                {
                    Circle.Curvature = 0.0;
                    Circle.ReferenceToolface = 0.0;
                    Circle.Length = Start.Distance(End);
                    End.Inclination = Start.Inclination;
                    End.Azimuth = Start.Azimuth;
                    End.Abscissa = Start.Abscissa + Circle.Length;
                    return true;
                }
                else
                {
                    double dls = 0;
                    double a = 0;
                    Point3D p1 = Start.TransCoord2PtsTg(End, End);
                    if (!Numeric.EQ(p1.Z, 0))
                    {
                        dls = (double)(2.0 * p1.Y / (p1.Y * p1.Y + p1.Z * p1.Z));
                        a = 2.0 * System.Math.Atan(System.Math.Abs((double)p1.Y) / System.Math.Abs((double)p1.Z));
                        if (Numeric.LE(p1.Z, 0))
                        {
                            a = 2.0 * Numeric.PI - (a - Numeric.PI * System.Math.Floor(a / Numeric.PI));
                        }
                        else
                        {
                            a = a - Numeric.PI * System.Math.Floor(a / Numeric.PI);
                        }
                    }
                    Point3D p3 = new Point3D
                    {
                        X = 0.0,
                        Y = System.Math.Sin(a),
                        Z = System.Math.Cos(a)
                    };
                    Point3D pt2 = Start.TransCoord2PtsTgReversed(End, p3);
                    double c1 = (double)(pt2.Z - Start.Z);
                    double b1 = (double)(pt2.Y - Start.Y);
                    double a1 = (double)(pt2.X - Start.X);
                    if (Numeric.EQ(dls, 0))
                    {
                        Circle.Length = Start.Distance(End);
                    }
                    else
                    {
                        Circle.Length = a / dls;
                    }
                    End.Abscissa = Start.Abscissa + Circle.Length;
                    End.Inclination = Numeric.AcosEqual(c1);
                    if (Numeric.EQ(a1, 0))
                    {
                        if (Numeric.GE(b1, 0))
                        {
                            End.Azimuth = Numeric.PI / 2.0;
                        }
                        else
                        {
                            End.Azimuth = 3.0 * Numeric.PI / 2.0;
                        }
                    }
                    else
                    {
                        End.Azimuth = System.Math.Atan(b1 / a1);
                    }
                    if (Numeric.LT(a1, 0))
                    {
                        End.Azimuth = End.Azimuth + Numeric.PI;
                    }
                    if (End.Azimuth < 0)
                    {
                        End.Azimuth = End.Azimuth + 2.0 * Numeric.PI;
                    }
                    Circle.Curvature = dls;
                    Circle.ReferenceToolface = Start.GetAngle(End, ci, si, ca, sa);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        public bool CalculateDTZ(bool useMax = false)
        {
            if (Numeric.IsDefined(Circle.Curvature) &&
                Numeric.IsDefined(Circle.ReferenceToolface) &&
                Numeric.IsDefined(End.Z))
            {
                if (Numeric.EQ(Circle.Curvature, 0))
                {
                    if (Numeric.EQ(Start.Inclination, Numeric.PI / 2.0))
                    {
                        if (Numeric.EQ(Start.Z, End.Z))
                        {
                            End.X = Start.X;
                            End.Y = Start.Y;
                            End.Z = Start.Z;
                            End.Inclination = Start.Inclination;
                            End.Azimuth = Start.Azimuth;
                            End.Abscissa = Start.Abscissa;
                            Circle.Length = 0;
                            //if (false)
                            //{
                            //    using (System.IO.StreamWriter writer = new System.IO.StreamWriter("c:\\temp\\DTZ.txt", true))
                            //    {
                            //        writer.WriteLine("A#\tDLS=" + (Circle.Curvature * 180.0 * 30.0 / Math.PI).ToString() + "\tTF=" + (Circle.ReferenceToolface * 180.0 / Math.PI).ToString() + "\tL=" + Circle.Length + "\tIncl=" + (End.Inclination * 180.0 / Math.PI).ToString() + "\tAz=" + (End.Azimuth * 180.0 / Math.PI).ToString() + "\tZ=" + End.Z + "\tX=" + End.X + "\tY=" + End.Y);
                            //    }
                            //}
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        double dm = (double)(End.Z - Start.Z) / System.Math.Cos((double)Start.Inclination);
                        Circle.Length = dm;
                        if (Numeric.LT(dm, 0))
                        {
                            return false;
                        }
                        else
                        {
                            End.Abscissa = Start.Abscissa + dm;
                            bool ok = CalculateSDT();
                            //if (false)
                            //{
                            //    using (System.IO.StreamWriter writer = new System.IO.StreamWriter("c:\\temp\\DTZ.txt", true))
                            //    {
                            //        writer.WriteLine("B#\tDLS=" + (Circle.Curvature * 180.0 * 30.0 / Math.PI).ToString() + "\tTF=" + (Circle.ReferenceToolface * 180.0 / Math.PI).ToString() + "\tL=" + Circle.Length + "\tIncl=" + (End.Inclination * 180.0 / Math.PI).ToString() + "\tAz=" + (End.Azimuth * 180.0 / Math.PI).ToString() + "\tZ=" + End.Z + "\tX=" + End.X + "\tY=" + End.Y);
                            //    }
                            //}
                            return ok;
                        }
                    }
                }
                else if (Numeric.EQ(Circle.ReferenceToolface, 0) || Numeric.EQ(Circle.ReferenceToolface, Numeric.PI))
                {
                    double centerZ = (double)(Start.Z + ((Numeric.EQ(Circle.ReferenceToolface, 0)) ? -1 : 1) * System.Math.Sin((double)Start.Inclination) / (double)Circle.Curvature);
                    if (End.Z > centerZ+ 1.0/Circle.Curvature || End.Z < centerZ - 1.0/Circle.Curvature)
                    {
                        return false;
                    }
                    else
                    {
                        double incl2 = (double)(Numeric.AsinEqual(((Numeric.EQ(Circle.ReferenceToolface, Numeric.PI)) ? -1 : 1) * (End.Z - centerZ) * Circle.Curvature));
                        double length = System.Math.Abs((double)(incl2 - Start.Inclination) / (double)Circle.Curvature);
                        if (End.Z < Start.Z)
                        {
                            //length = 2.0 * Numeric.PI / (double)Circle.Curvature - length;
                            length = Numeric.PI / (double)Circle.Curvature + length;
                        }
                        Circle.Length = length;
                        End.Abscissa = Start.Abscissa + length;
                        bool ok = CalculateSDT();
                        //if (false)
                        //{
                        //    using (System.IO.StreamWriter writer = new System.IO.StreamWriter("c:\\temp\\DTZ.txt", true))
                        //    {
                        //        writer.WriteLine("C#\tDLS=" + (Circle.Curvature * 180.0 * 30.0 / Math.PI).ToString() + "\tTF=" + (Circle.ReferenceToolface * 180.0 / Math.PI).ToString() + "\tL=" + Circle.Length + "\tIncl=" + (End.Inclination * 180.0 / Math.PI).ToString() + "\tAz=" + (End.Azimuth * 180.0 / Math.PI).ToString() + "\tZ=" + End.Z + "\tX=" + End.X + "\tY=" + End.Y);
                        //    }
                        //}
                        return ok;
                    }
                }
                else
                {
                    double ci = System.Math.Cos((double)Start.Inclination);
                    double si = System.Math.Sin((double)Start.Inclination);
                    double ca = System.Math.Cos((double)Start.Azimuth);
                    double sa = System.Math.Sin((double)Start.Azimuth);
                    double ct = System.Math.Cos((double)Circle.ReferenceToolface);
                    double st = System.Math.Sin((double)Circle.ReferenceToolface);
                    double dz = (double)(End.Z - Start.Z);
                    double R = 1.0 / (double)Circle.Curvature;
                    double a1 = ct * ci * ca - st * sa;
                    double b1 = ct * ci * sa + st * ca;
                    double c1 = -ct * si;
                    double a2 = -st * ci * ca - ct * sa;
                    double b2 = ct * ca - st * ci * sa;
                    double c2 = st * si;
                    double a3 = si * ca;
                    double b3 = si * sa;
                    double c3 = ci;
                    double p = 0;
                    double q = 0;
                    double r = 0;
                    double s = 0;
                    if (!Numeric.EQ(a2, 0, 100.0*Numeric.DOUBLE_ACCURACY))
                    {
                        p = b1 - a1 * b2 / a2;
                        q = dz * (c1 - a1 * c2 / a2);
                        r = b3 - a3 * b2 / a2;
                        s = dz * (c3 - a3 * c2 / a2);
                    }
                    else
                    {
                        if (!Numeric.EQ(b2, 0, 100.0*Numeric.DOUBLE_ACCURACY))
                        {
                            p = a1 - b1 * a2 / b2;
                            q = dz * (c1 - b1 * c2 / b2);
                            r = a3 - b3 * a2 / b2;
                            s = dz * (c3 - b3 * c2 / b2);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    double x, y;
                    int sol = Numeric.SolveRealQuadraticEquation(p * p + r * r, 2 * (p * (q - R) + r * s), s * s + (q - R) * (q - R) - R * R, out x, out y);
                    if (sol > 0)
                    {
                        if (sol == 1)
                        {
                            y = x;
                        }
                        double teta1 = Numeric.AsinEqual((r * x + s) / R);
                        double teta2 = Numeric.AsinEqual((r * y + s) / R);
                        if (Numeric.GT(teta1, 0) && Numeric.GT(teta2, 0))
                        {
                            if (useMax)
                            {
                                teta1 = System.Math.Max(teta1, teta2);
                            }
                            else
                            {
                                teta1 = System.Math.Min(teta1, teta2);
                            }
                        }
                        else if (Numeric.GT(teta1, 0))
                        {

                        }
                        else if (Numeric.GT(teta2, 0))
                        {
                            teta1 = teta2;
                        }
                        else
                        {
                            teta1 = -1.0;
                        }
                        if (Numeric.GT(teta1, 0))
                        {
                            Circle.Length = R * teta1;
                            End.Abscissa = Start.Abscissa + R * teta1;
                            bool ok =  CalculateSDT();
                            //if (false)
                            //{
                            //    using (System.IO.StreamWriter writer = new System.IO.StreamWriter("c:\\temp\\DTZ.txt", true))
                            //    {
                            //        writer.WriteLine("D#\tDLS=" + (Circle.Curvature * 180.0 * 30.0 / Math.PI).ToString() + "\tTF=" + (Circle.ReferenceToolface * 180.0 / Math.PI).ToString() + "\tL=" + Circle.Length + "\tIncl=" + (End.Inclination * 180.0 / Math.PI).ToString() + "\tAz=" + (End.Azimuth * 180.0 / Math.PI).ToString() + "\tZ=" + End.Z + "\tX=" + End.X + "\tY=" + End.Y);
                            //    }
                            //}
                            return ok;
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
            }
            else
            {
                return false;
            }
        }
        public bool CalculateDTI(bool useMax = false)
        {
            if (Numeric.IsDefined(Circle.Curvature) &&
                Numeric.IsDefined(Circle.ReferenceToolface) &&
                Numeric.IsDefined(End.Inclination))
            {
                if (Numeric.EQ(Circle.Curvature, 0))
                {
                    return false;
                }
                else
                {
                    if (Numeric.EQ(Start.Inclination, 0))
                    {
                        Circle.Length = (End.Inclination - Start.Inclination) / Circle.Curvature;
                        End.Abscissa = Start.Abscissa + Circle.Length;
                        return CalculateSDT();
                    }
                    else
                    {
                        if (Numeric.EQ(Circle.ReferenceToolface, Numeric.PI /2.0) ||
                            Numeric.EQ(Circle.ReferenceToolface, 3.0*Numeric.PI/ 2.0))
                        {
                            return false;
                        }
                        else
                        {
                            if (Numeric.EQ(Circle.ReferenceToolface, 0))
                            {
                                if (End.Inclination >= Start.Inclination)
                                {
                                    Circle.Length = (End.Inclination - Start.Inclination) / Circle.Curvature;
                                }
                                else
                                {
                                    Circle.Length = 2.0 * Numeric.PI + (End.Inclination - Start.Inclination) / Circle.Curvature;
                                }
                                End.Abscissa = Start.Abscissa + Circle.Length;
                                return CalculateSDT();
                            }
                            else
                            {
                                if (Numeric.EQ(Circle.ReferenceToolface, Numeric.PI) || Numeric.EQ(Circle.ReferenceToolface, -Numeric.PI))
                                {
                                    if (End.Inclination < Start.Inclination)
                                    {
                                        Circle.Length = - (End.Inclination - Start.Inclination) / Circle.Curvature;
                                    }
                                    else
                                    {
                                        Circle.Length = - 2.0 * Numeric.PI + (End.Inclination - Start.Inclination) / Circle.Curvature;
                                    }
                                    End.Abscissa = Start.Abscissa + Circle.Length;
                                    return CalculateSDT();
                                }
                                else
                                {
                                    double ci = System.Math.Cos((double)Start.Inclination);
                                    double si = System.Math.Sin((double)Start.Inclination);
                                    double ct = System.Math.Cos((double)Circle.ReferenceToolface);
                                    double cif = System.Math.Cos((double)End.Inclination);
                                    double r = 1.0 / (double)Circle.Curvature;
                                    double a = ci + cif;
                                    double b = 2.0 * ct * si;
                                    double c = cif - ci;
                                    double x, y;
                                    int sol = Numeric.SolveRealQuadraticEquation(a, b, c, out x, out y);
                                    if (sol > 0)
                                    {
                                        if (sol == 1)
                                        {
                                            y = x;
                                        }
                                        double teta1 = System.Math.Atan(x);
                                        double teta2 = System.Math.Atan(y);
                                        if (Numeric.GT(teta1, 0) && Numeric.GT(teta2, 0))
                                        {
                                            if (useMax)
                                            {
                                                teta1 = Math.Max(teta1, teta2);
                                            }
                                            else
                                            {
                                                teta1 = Math.Min(teta1, teta2);
                                            }
                                        } 
                                        else if (Numeric.GT(teta1, 0))
                                        {

                                        }
                                        else if (Numeric.GT(teta2, 0))
                                        {
                                            teta1 = teta2;
                                        }
                                        else
                                        {
                                            teta1 = -1.0;
                                        }
                                        if (Numeric.GT(teta1, 0))
                                        {
                                            Circle.Length = 2.0 * r * teta1;
                                            End.Abscissa = Start.Abscissa + Circle.Length;
                                            return CalculateSDT();
                                        }
                                        return false;
                                    }
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }
        public bool CalculateLDT()
        {
            if (Numeric.IsDefined(Circle.Curvature) &&
                Numeric.IsDefined(Circle.ReferenceToolface) &&
                Numeric.IsDefined(Circle.Length))
            {
                End.Abscissa = Start.Abscissa + Curve.Length;
                bool ok= CalculateSDT();
                //if (false)
                //{
                //    using (System.IO.StreamWriter writer = new System.IO.StreamWriter("c:\\temp\\LDT.txt", true))
                //    {
                //        writer.WriteLine("DLS=" + (Circle.Curvature * 180.0 * 30.0 / Math.PI).ToString() + "\tTF=" + (Circle.ReferenceToolface * 180.0 / Math.PI).ToString() + "\tL=" + Circle.Length + "\tIncl=" + (End.Inclination * 180.0 / Math.PI).ToString() + "\tAz=" + (End.Azimuth * 180.0 / Math.PI).ToString() + "\tZ=" + End.Z + "\tX=" + End.X + "\tY=" + End.Y);
                //    }
                //}
                return ok;
            }
            else
            {
                return false;
            }
        }
        public bool CalculateSDT()
        {
            if (Numeric.IsDefined(Circle.Curvature) &&
                Numeric.IsDefined(Circle.ReferenceToolface) &&
                Numeric.IsDefined(End.Abscissa))
            {
                if (Numeric.EQ(Circle.Curvature, 0))
                {
                    double dm = (double)(End.Abscissa - Start.Abscissa);
                    Circle.Length = dm;
                    double si = System.Math.Sin((double)Start.Inclination);
                    End.Inclination = Start.Inclination;
                    End.Azimuth = Start.Azimuth;
                    End.X = Start.X + dm * si * System.Math.Cos((double)Start.Azimuth);
                    End.Y = Start.Y + dm * si * System.Math.Sin((double)Start.Azimuth);
                    End.Z = Start.Z + dm * System.Math.Cos((double)Start.Inclination);
                    return true;
                }
                else if (Numeric.EQ(Start.Inclination, 0)) 
                {
                    double dm = (double)(End.Abscissa - Start.Abscissa);
                    Circle.Length = dm;
                    End.Inclination = (dm * Circle.Curvature) % Numeric.PI;
                    End.Azimuth = Circle.ReferenceToolface;
                    int halfCircleSign = 1;
                    if (Numeric.GT(dm * Circle.Curvature, Numeric.PI))
                    {
                        End.Azimuth += Numeric.PI;
                        End.Inclination = Numeric.PI - End.Inclination;
                        // Note that the InterpolateAtMD method does not take into account sections with more than half the circle, so there will still be problems with WellPath Calculator
                        halfCircleSign = -1;
                    }
                    double ci = System.Math.Cos((double)End.Inclination);
                    End.X = Start.X + halfCircleSign * System.Math.Cos((double)End.Azimuth) * (1.0 - ci) / Circle.Curvature;
                    End.Y = Start.Y + halfCircleSign * System.Math.Sin((double)End.Azimuth) * (1.0 - ci) / Circle.Curvature;
                    End.Z = Start.Z + halfCircleSign * System.Math.Sin((double)End.Inclination) / Circle.Curvature;
                    return true;
                }
                else
                {
                    double dm = (double)(End.Abscissa - Start.Abscissa);
                    Circle.Length = dm;
                    Point3D p1 = new Point3D(0, 0, 0);
                    Point3D p2 = new Point3D(0, 0, 0);
                    CurvilinearPoint3D s = Start;
                    CurvilinearPoint3D f = End;
                    double dls = (double)Circle.Curvature;
                    double teta = dm * dls;
                    double st = System.Math.Sin(teta);
                    double ct = System.Math.Cos(teta);
                    p1.X = (1 - ct) / dls;
                    p1.Y = 0.0;
                    p1.Z = st / dls;
                    Point3D pt = s.TransCoord3RotsReversed((double)Circle.ReferenceToolface, p1);
                    f.X = s.X + pt.X;
                    f.Y = s.Y + pt.Y;
                    f.Z = s.Z + pt.Z;
                    p1.X = st;
                    p1.Y = 0;
                    p1.Z = ct;
                    pt = s.TransCoord3RotsReversed((double)Circle.ReferenceToolface, p1);
                    f.Inclination = Numeric.AcosEqual(pt.Z);
                    if (Numeric.EQ(pt.Z, 1.0))
                    {
                        f.Azimuth = s.Azimuth;
                    }
                    else
                    {
                        if (Numeric.EQ(pt.X, 0.0) && Numeric.EQ(pt.Y, 0))
                        {
                            f.Azimuth = Numeric.UNDEF_DOUBLE;
                        }
                        else
                        {
                            double teta2 = (double)(Numeric.AcosEqual(pt.X / System.Math.Sqrt((double)(pt.X * pt.X + pt.Y * pt.Y))));
                            if (pt.Y >= 0.0)
                            {
                                f.Azimuth = teta2;
                            }
                            else
                            {
                                f.Azimuth = 2.0 * Numeric.PI - teta2;
                            }
                        }
                    }
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public override CurvilinearPoint3D InterpolateAtMD(double md)
        {
            CurvilinearPoint3D point = new CurvilinearPoint3D();
            point.SetUndefined();
            bool success = InterpolateAtMD(md, point);
            if (success)
            {
                return point;
            }
            else
            {
                return null;
            }
        }
        public bool InterpolateAtMD(double md, CurvilinearPoint3D point)
        {
            if (point != null)
            {
                CurvilinearPoint3D point1 = Start;
                CurvilinearPoint3D point2 = End;
                if (Numeric.EQ(point1.Abscissa, md, Numeric.DEPTH_ACCURACY))
                {
                    point.Set(point1);
                    return true;
                }
                else if (!Numeric.IsBetween(md, (double)point1.Abscissa, (double)point2.Abscissa))
                {
                    return false;
                }
                double x1 = (double)point1.X;
                double y1 = (double)point1.Y;
                double z1 = (double)point1.Z;
                double i2 = (double)point2.Inclination;
                double i1 = (double)point1.Inclination;
                double a2 = (double)point2.Azimuth;
                double a1 = (double)point1.Azimuth;
                double sini1 = System.Math.Sin(i1);
                double sini2 = System.Math.Sin(i2);
                double DL = System.Math.Acos(System.Math.Cos(i2 - i1) - (1 - System.Math.Cos(a2 - a1)) * sini2 * sini1);
                double DM = (double)(point2.Abscissa - point1.Abscissa);
                if (Numeric.EQ(DM, 0, Numeric.DEPTH_ACCURACY))
                {
                    if (Numeric.EQ(point1.Abscissa, md))
                    {
                        point.Set(point1);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    point.Abscissa = md;
                    double dx = (double)(point2.X - x1);
                    double dy = (double)(point2.Y - y1);
                    double dz = (double)(point2.Z - z1);
                    double Ci = System.Math.Cos(i1);
                    double Si = System.Math.Sin(i1);
                    double Ca = System.Math.Cos(a1);
                    double Sa = System.Math.Sin(a1);
                    double X = dx * Ci * Ca + dy * Sa * Ci - dz * Si;
                    double Y = dy * Ca - dx * Sa;
                    double L = System.Math.Sqrt(X * X + Y * Y);
                    double TF = 0;
                    if (!Numeric.EQ(L, 0))
                    {
                        if (Numeric.GE(Y, 0))
                        {
                            TF = System.Math.Acos(X / L);
                        }
                        else
                        {
                            TF = 2 * Numeric.PI - System.Math.Acos(X / L);
                        }
                    }
                    double DLS = DL / DM;
                    double dm = md - (double)point1.Abscissa;
                    if (Numeric.EQ(DLS, 0))
                    {
                        point.Inclination = i1;
                        point.Azimuth = a1;
                        point.X = x1 + dm * System.Math.Cos(a1) * sini1;
                        point.Y = y1 + dm * System.Math.Sin(a1) * sini1;
                        point.Z = z1 + dm * System.Math.Cos(i1);
                    }
                    else
                    {
                        if (Numeric.EQ(i1, 0))
                        {
                            point.Inclination = dm * DLS;
                            if (Numeric.IsUndefined(TF))
                            {
                                point.Azimuth = point2.Azimuth;
                            }
                            else
                            {
                                point.Azimuth = TF;
                            }
                            double ci = System.Math.Cos((double)point.Inclination);
                            point.X = x1 + System.Math.Cos((double)point.Azimuth) * (1 - ci) / DLS;
                            point.Y = y1 + System.Math.Sin((double)point.Azimuth) * (1 - ci) / DLS;
                            point.Z = z1 + System.Math.Sin((double)point.Inclination) / DLS;
                        }
                        else
                        {
                            double teta = dm * DLS;
                            double deltaXp = (1 - System.Math.Cos(teta)) / DLS;
                            double deltaZp = System.Math.Sin(teta) / DLS;
                            double ctf = System.Math.Cos(TF);
                            double stf = System.Math.Sin(TF);
                            double cosi1 = System.Math.Cos(i1);
                            double cosa1 = System.Math.Cos(a1);
                            double sina1 = System.Math.Sin(a1);
                            point.X = x1 + deltaXp * (ctf * cosa1 * cosi1 - sina1 * stf) + deltaZp * sini1 * cosa1;
                            point.Y = y1 + deltaXp * (stf * cosa1 + ctf * sina1 * cosi1) + deltaZp * sini1 * sina1;
                            point.Z = z1 - deltaXp * ctf * sini1 + deltaZp * cosi1;
                            double deltaXt = System.Math.Sin(teta);
                            double deltaZt = System.Math.Cos(teta);
                            double xt = deltaXt * (ctf * cosa1 * cosi1 - sina1 * stf) + deltaZt * sini1 * cosa1;
                            double yt = deltaXt * (stf * cosa1 + ctf * sina1 * cosi1) + deltaZt * sini1 * sina1;
                            double zt = -deltaXt * ctf * sini1 + deltaZt * cosi1;
                            point.Inclination = System.Math.Acos(zt);
                            if (Numeric.EQ(zt, 1))
                            {
                                point.Azimuth = point1.Azimuth;
                            }
                            else
                            {
                                double omega = System.Math.Acos(xt / System.Math.Sqrt(xt * xt + yt * yt));
                                if (Numeric.GE(yt, 0))
                                {
                                    point.Azimuth = omega;
                                }
                                else
                                {
                                    point.Azimuth = 2 * Numeric.PI - omega;
                                }
                            }
                        }
                    }
                    return true;
                }
            }else
            {
                return false;
            }
        }
    }
}
