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

        public CircularArcSection(TrajectoryPoint3D start, TrajectoryPoint3D end)
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
                // The circular arc geometry is defined once, in
                // OSDC.DotnetLibraries.General.Math, on TrajectoryPoint3D, and used here rather than restated. It
                // reports the curvature and the toolface angle at the start of the arc exactly, so
                // neither has to be recomputed afterwards.
                if (!Start.CompleteCASIA(End))
                {
                    return false;
                }
                Circle.Curvature = End.Curvature;
                Circle.ReferenceToolface = End.Toolface;
                Circle.Length = End.Abscissa - Start.Abscissa;
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
                    double dl = TrajectoryPoint3D.DoglegAngle(
                        (double)Start.Inclination, (double)Start.Azimuth,
                        (double)End.Inclination, (double)End.Azimuth);
                    Circle.Length = dl / Circle.Curvature;
                    End.Abscissa = Start.Abscissa + Circle.Length;
                    bool ok = CalculateSIA();
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
                // A circular arc leaving a known tangent and passing through a known point is closed
                // form: the chord bisects the angle between the tangents at its two ends.
                if (!Start.CompleteCAXYZ(End))
                {
                    return false;
                }
                Circle.Curvature = End.Curvature;
                Circle.ReferenceToolface = End.Toolface;
                Circle.Length = End.Abscissa - Start.Abscissa;
                return true;
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
                            return ok;
                        }
                    }
                }
                else
                {
                    // Along a circular arc of curvature k and reference toolface f, the tangent turns in
                    // the plane spanned by the start tangent t0 and the arc normal n, so after an arc
                    // angle w the depth gained is
                    //
                    //     dz(w) = ( t0z*sin(w) + nz*(1 - cos(w)) ) / k
                    //
                    // with t0z = cos(i0) and nz = -cos(f)*sin(i0). Collecting the harmonics turns this
                    // into a single sinusoid, which is solved exactly. That covers the toolface aligned
                    // with the build plane as one case among the rest, with no special branch, and it
                    // keeps both roots of the sine so that arcs turning past the horizontal are reached.
                    double curvature = (double)Circle.Curvature;
                    double tangentZ = System.Math.Cos((double)Start.Inclination);
                    double normalZ = -System.Math.Cos((double)Circle.ReferenceToolface) * System.Math.Sin((double)Start.Inclination);
                    double target = (double)(End.Z - Start.Z) * curvature;

                    // tangentZ*sin(w) - normalZ*cos(w) = target - normalZ
                    double amplitude = System.Math.Sqrt(tangentZ * tangentZ + normalZ * normalZ);
                    double offset = target - normalZ;
                    if (Numeric.EQ(amplitude, 0.0))
                    {
                        // The arc lies in a horizontal plane, so no depth other than the present one is
                        // ever reached and no length is singled out when it is the one requested.
                        return false;
                    }
                    double ratio = offset / amplitude;
                    if (ratio < -1.0 || ratio > 1.0)
                    {
                        if (Numeric.EQ(System.Math.Abs(ratio), 1.0))
                        {
                            ratio = System.Math.Sign(ratio);
                        }
                        else
                        {
                            // The requested depth lies outside the range the arc ever spans.
                            return false;
                        }
                    }
                    double phase = System.Math.Atan2(-normalZ, tangentZ);
                    double principal = System.Math.Asin(ratio);

                    // Both roots of the sine, each brought into [0, 2pi) as an arc angle.
                    double best = double.PositiveInfinity;
                    foreach (double root in new double[] { principal, Numeric.PI - principal })
                    {
                        double angle = root - phase;
                        angle -= 2.0 * Numeric.PI * System.Math.Floor(angle / (2.0 * Numeric.PI));
                        if (Numeric.EQ(angle, 2.0 * Numeric.PI))
                        {
                            angle = 0.0;
                        }
                        if (useMax)
                        {
                            if (angle > best || double.IsInfinity(best))
                            {
                                best = angle;
                            }
                        }
                        else if (angle < best)
                        {
                            best = angle;
                        }
                    }
                    if (double.IsInfinity(best))
                    {
                        return false;
                    }
                    Circle.Length = best / curvature;
                    End.Abscissa = Start.Abscissa + (double)Circle.Length;
                    return CalculateSDT();
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
                    // A straight line holds its inclination, so nothing fixes the length.
                    return false;
                }

                // Along a circular arc of curvature k and reference toolface f, the tangent after an
                // arc angle w is t0*cos(w) + n*sin(w), and reading its vertical component gives the
                // inclination directly:
                //
                //     cos(i(w)) = cos(i0)*cos(w) - cos(f)*sin(i0)*sin(w)
                //
                // Collecting the two harmonics into a single cosine solves this exactly for w. The
                // arc angle, not the length, is what the equation yields, so the length follows by
                // division by the curvature - the earlier formulation added a bare 2*pi to a length.
                double curvature = (double)Circle.Curvature;
                double tangentZ = System.Math.Cos((double)Start.Inclination);
                double normalZ = -System.Math.Cos((double)Circle.ReferenceToolface) * System.Math.Sin((double)Start.Inclination);

                // tangentZ*cos(w) + normalZ*sin(w) = cos(i1)
                double amplitude = System.Math.Sqrt(tangentZ * tangentZ + normalZ * normalZ);
                if (Numeric.EQ(amplitude, 0.0))
                {
                    // The arc lies in a horizontal plane and holds a horizontal inclination throughout.
                    return false;
                }
                double ratio = System.Math.Cos((double)End.Inclination) / amplitude;
                if (ratio < -1.0 || ratio > 1.0)
                {
                    if (Numeric.EQ(System.Math.Abs(ratio), 1.0))
                    {
                        ratio = System.Math.Sign(ratio);
                    }
                    else
                    {
                        // The requested inclination lies outside the range the arc ever spans.
                        return false;
                    }
                }
                double phase = System.Math.Atan2(normalZ, tangentZ);
                double principal = System.Math.Acos(ratio);

                // Both roots of the cosine, each brought into [0, 2pi) as an arc angle.
                double best = double.PositiveInfinity;
                foreach (double root in new double[] { principal, -principal })
                {
                    double angle = root + phase;
                    angle -= 2.0 * Numeric.PI * System.Math.Floor(angle / (2.0 * Numeric.PI));
                    if (Numeric.EQ(angle, 2.0 * Numeric.PI))
                    {
                        angle = 0.0;
                    }
                    if (useMax)
                    {
                        if (double.IsInfinity(best) || angle > best)
                        {
                            best = angle;
                        }
                    }
                    else if (angle < best)
                    {
                        best = angle;
                    }
                }
                if (double.IsInfinity(best))
                {
                    return false;
                }
                Circle.Length = best / curvature;
                End.Abscissa = Start.Abscissa + (double)Circle.Length;
                return CalculateSDT();
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
                Circle.Length = End.Abscissa - Start.Abscissa;
                return Start.CompleteCASDT(End, (double)Circle.Curvature, (double)Circle.ReferenceToolface);
            }
            else
            {
                return false;
            }
        }
        public override CurvilinearPoint3D InterpolateAtMD(double md)
        {
            CurvilinearPoint3D point = new TrajectoryPoint3D();
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
            if (point == null)
            {
                return false;
            }
            if (Numeric.EQ(Start.Abscissa, md, Numeric.DEPTH_ACCURACY))
            {
                point.Set(Start);
                return true;
            }
            if (Numeric.EQ(End.Abscissa, md, Numeric.DEPTH_ACCURACY))
            {
                point.Set(End);
                return true;
            }
            if (!Numeric.IsBetween(md, (double)Start.Abscissa, (double)End.Abscissa))
            {
                return false;
            }
            // Interpolation along a circular arc is defined once, in
            // OSDC.DotnetLibraries.General.Math, on TrajectoryPoint3D, and used here rather than restated.
            return Start.InterpolateAtAbscissaCA(End, md, point);
        }

    }
}
