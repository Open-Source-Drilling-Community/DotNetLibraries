using System;
using System.Collections;
using System.Collections.Generic;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Statistics;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    public class ComplexSection : IList<CircularArcSection>
    {
        private List<CircularArcSection> sections_ = new List<CircularArcSection>();

        /// <summary>
        ///  calculate the complex section
        /// </summary>
        /// <returns></returns>
        public bool Calculate()
        {
            return CalculateStart(sections_);
        }

        private bool CalculateStart(List<CircularArcSection> sections)
        {
            bool check = true;
            if (sections != null && sections.Count > 0 && sections[0] != null && sections[0].Start != null)
            {
                // check that the start is fully defined
                check &= !sections[0].Start.IsUndefined();
                List<int> parameterCounts = new List<int>();
                List<bool> controlPoints = new List<bool>();
                for (int i = 0; i < sections.Count; i++)
                {
                    if (sections[i] == null)
                    {
                        return false;
                    }
                    if (sections[i].Start == null)
                    {
                        return false;
                    }
                    if (sections[i].End == null)
                    {
                        return false;
                    }
                    if (sections[i].Curve == null)
                    {
                        return false;
                    }
                    if (i > 0 && sections[i].Start != sections[i - 1].End)
                    {
                        return false;
                    }
                    int count = 0;
                    bool controlPoint = sections[i].End.X != null && sections[i].End.Y != null && sections[i].End.Z != null;
                    if (sections[i].End.Abscissa != null || sections[i].Curve.Length != null)
                    {
                        count++;
                    }
                    if (sections[i].End.Inclination != null)
                    {
                        count++;
                    }
                    if (sections[i].End.Azimuth != null)
                    {
                        count++;
                    }
                    if (sections[i].End.X != null)
                    {
                        count++;
                    }
                    if (sections[i].End.Y != null)
                    {
                        count++;
                    }
                    if (sections[i].End.Z != null)
                    {
                        count++;
                    }
                    if (sections[i].Circle != null && sections[i].Circle.Curvature != null)
                    {
                        count++;
                    }
                    if (sections[i].Circle != null && sections[i].Circle.ReferenceToolface != null)
                    {
                        count++;
                    }
                    parameterCounts.Add(count);
                    controlPoints.Add(controlPoint);
                }
                int parameterCount = 0;
                foreach (int count in parameterCounts)
                {
                    parameterCount += count;
                }
                if (parameterCount != 3 * sections.Count)
                {
                    return false;
                }
                // calculate the start as deep as possible
                int j = 0;
                while (j < parameterCounts.Count && parameterCounts[j] == 3)
                {
                    check &= sections[j].Calculate();
                    if (!check)
                    {
                        return false;
                    }
                    j++;
                }
                if (j < sections.Count)
                {
                    List<CircularArcSection> subSetSections = new List<CircularArcSection>();
                    List<int> subSetParameterCounts = new List<int>();
                    List<bool> subSetControlPoints = new List<bool>();
                    for (int k = j; k < sections.Count; k++)
                    {
                        subSetSections.Add(sections[k]);
                        subSetParameterCounts.Add(parameterCounts[k]);
                        subSetControlPoints.Add(controlPoints[k]);
                    }
                    // calculation is not completed
                    check = CalculateFrom(subSetSections, subSetParameterCounts, subSetControlPoints);
                }
            }
            return check;
        }

        private bool CalculateFrom(List<CircularArcSection> sections, List<int> parameterCounts, List<bool> controlPoints)
        {
            bool check = true;
            if (sections != null && parameterCounts != null && controlPoints != null && sections.Count > 0)
            {
                if (sections[0].Start == null || sections[0].Start.IsUndefined())
                {
                    return false;
                }
                // find the first overdetermined section
                int next = -1;
                for (int i = 0; i < sections.Count; i++)
                {
                    if (parameterCounts[i] > 3)
                    {
                        next = i;
                        break;
                    }
                }
                if (next < 0)
                {
                    return false;
                }
                List<CircularArcSection> subSet = new List<CircularArcSection>();
                List<int> subSetParameterCounts = new List<int>();
                for (int i = 0; i <= next; i++)
                {
                    subSet.Add(sections[i]);
                    subSetParameterCounts.Add(parameterCounts[i]);
                }
                if (!CalculateComplex(subSet, subSetParameterCounts))
                {
                    return false;
                }
                List<CircularArcSection> subSetSections = new List<CircularArcSection>();
                for (int k = next+1; k < sections.Count; k++)
                {
                    subSetSections.Add(sections[k]);
                }
                check = CalculateStart(subSetSections);
            }
            return check;
        }



        private bool CalculateComplex(List<CircularArcSection> sections, List<int> parameterCounts)
        {
            bool check = true;
            if (sections != null && sections.Count > 0 && parameterCounts.Count > 0)
            {
                if (sections[0].Start == null || sections[0].Start.IsUndefined())
                {
                    return false;
                }
                // choose testing variables that are of the same physical dimension and such that the associated function to calculate the last section is "deterministic" (if possible)
                List<CircularArcSection.VariableType> targetDefinedVariables = sections[sections.Count - 1].GetDefinedVariables();
                List<List<CircularArcSection.VariableType>> combinations = new List<List<CircularArcSection.VariableType>>();
                CircularArcSection.GetCombinations(targetDefinedVariables, parameterCounts[parameterCounts.Count-1]-3, combinations);
                List<List<CircularArcSection.VariableType>> filteredCombinations = CircularArcSection.FilterSamePhysicalDimensions(combinations);
                List<Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType>> candidates = CircularArcSection.FilterHasAFunctionType(filteredCombinations, targetDefinedVariables);
                if (candidates == null || candidates.Count == 0)
                {
                    return false;
                }
                Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType> candidate = candidates[candidates.Count - 1];
                List<Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType>> intermediates = new List<Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType>>();
                for (int i = 0; i < sections.Count - 1; i++)
                {
                    List<CircularArcSection.VariableType> definedVariables = sections[i].GetDefinedVariables();
                    if (definedVariables != null)
                    {
                        List<CircularArcSection.FunctionType> functionTypes = CircularArcSection.GetFunctionTypes(definedVariables);
                        if (functionTypes != null && functionTypes.Count > 0)
                        {
                            CircularArcSection.FunctionType selectedFunctionType = functionTypes[functionTypes.Count - 1];
                            List<CircularArcSection.VariableType> missingVariables = CircularArcSection.GetMissingVariables(definedVariables, selectedFunctionType);
                            intermediates.Add(new Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType>(missingVariables, selectedFunctionType));
                        }
                    }
                }
                if (intermediates.Count != sections.Count-1)
                {
                    return false;
                }
                double[] Xs = new double[candidate.Item1.Count];
                double[] Ys = new double[candidate.Item1.Count];
                double[] sigs = new double[candidate.Item1.Count];
                double[] a = null;
                bool[] ia = new bool[candidate.Item1.Count];
                for (int i = 0; i < Xs.Length; i++)
                {
                    Xs[i] = (double)candidate.Item1[i];
                    Ys[i] = (double)sections[sections.Count-1].Get(candidate.Item1[i]);
                    sigs[i] = 0.0001;
                    ia[i] = true;
                }
                if (a == null &&
                    targetDefinedVariables.Contains(CircularArcSection.VariableType.X) &&
                    targetDefinedVariables.Contains(CircularArcSection.VariableType.Y) &&
                    targetDefinedVariables.Contains(CircularArcSection.VariableType.Z) &&
                    targetDefinedVariables.Contains(CircularArcSection.VariableType.Incl) &&
                    targetDefinedVariables.Contains(CircularArcSection.VariableType.Az))
                {
                    // guess using a cubic spline
                    CubicSection cubicSection = new CubicSection();
                    cubicSection.Start = sections[0].Start;
                    cubicSection.End = new CurvilinearPoint3D(sections[sections.Count - 1].End);
                    if (cubicSection.Calculate())
                    {
                        //if (false)
                        //{
                        //    using (System.IO.StreamWriter writer = new System.IO.StreamWriter("c:\\temp\\cubicspline.txt"))
                        //    {
                        //        for (double s = (double)cubicSection.Start.Abscissa; s <= cubicSection.End.Abscissa; s += 30.0)
                        //        {
                        //            CurvilinearPoint3D p = cubicSection.InterpolateAtMD(s);
                        //            writer.WriteLine(p.X + "\t" + p.Y + "\t" + p.Z);
                        //        }
                        //    }
                        //}
                        List<double> MDs = GenerateMDs(sections, intermediates, cubicSection);
                        a = GetDefaultsForLevenbergMarquardt(candidate.Item1.Count, sections, intermediates, MDs, cubicSection);
                    }
                 }
                if (a == null &&
                    targetDefinedVariables.Contains(CircularArcSection.VariableType.X) &&
                    targetDefinedVariables.Contains(CircularArcSection.VariableType.Y) &&
                    targetDefinedVariables.Contains(CircularArcSection.VariableType.Z))
                {
                    double incl1 = sections[0].Start.GetIncl(sections[sections.Count - 1].End);
                    double az1 = sections[0].Start.GetAz(sections[sections.Count - 1].End);
                    if (Numeric.IsDefined(incl1) && Numeric.IsDefined(az1))
                    {
                        double? incl2 = null;
                        double? az2 = null;
                        CircularArcSection arc = new CircularArcSection();
                        arc.Start = sections[0].Start;
                        arc.End = new CurvilinearPoint3D(sections[sections.Count - 1].End);
                        if (arc.CalculateXYZ())
                        {
                            incl2 = arc.End.Inclination;
                            az2 = arc.End.Azimuth;
                        }
                        if (!Numeric.IsDefined(sections[sections.Count - 1].End.Inclination))
                        {
                            if (incl2 != null)
                            {
                                incl1 = (incl1 + (double)incl2) / 2.0;
                            }
                            if (incl1 > Math.PI / 2.0)
                            {
                                incl1 = Math.PI / 2.0;
                            }
                        }
                        if (!Numeric.IsDefined(sections[sections.Count - 1].End.Azimuth))
                        {
                            if (az2 != null)
                            {
                                az1 = (az1 + (double)az2) / 2.0;
                            }
                            while (az1 < 0)
                            {
                                az1 += 2.0 * Math.PI;
                            }
                            az1 = az1 % 2.0 * Math.PI;
                        }
                        if (Numeric.IsDefined(incl1) && Numeric.IsDefined(az1))
                        {
                            // guess using a cubic spline
                            CubicSection cubicSection = new CubicSection();
                            cubicSection.Start = sections[0].Start;
                            cubicSection.End = new CurvilinearPoint3D(sections[sections.Count - 1].End);
                            cubicSection.End.Inclination = incl1;
                            cubicSection.End.Azimuth = az1;
                            if (cubicSection.Calculate())
                            {
                                List<double> MDs = GenerateMDs(sections, intermediates, cubicSection);
                                a = GetDefaultsForLevenbergMarquardt(candidate.Item1.Count, sections, intermediates, MDs, cubicSection);
                            }
                        }
                    }
                }
                if (a == null)
                {
                    // guess using an arc
                    List<CircularArcSection.FunctionType> possibilities = CircularArcSection.GetFunctionTypes(targetDefinedVariables);
                    if (possibilities == null || possibilities.Count == 0)
                    {
                        return false;
                    }
                    CircularArcSection.FunctionType chosen = possibilities[possibilities.Count - 1];
                    CircularArcSection arc = new CircularArcSection();
                    arc.Start = sections[0].Start;
                    arc.End = new CurvilinearPoint3D(sections[sections.Count - 1].End);
                    if (arc.Calculate(chosen))
                    {
                        List<double> MDs = GenerateMDs(sections, intermediates, arc);
                        a = GetDefaultsForLevenbergMarquardt(candidate.Item1.Count, sections, intermediates, MDs, arc);
                    }
                }
                if (candidate.Item1.Contains(CircularArcSection.VariableType.X) && candidate.Item1.Contains(CircularArcSection.VariableType.Y))
                {
                    int TFIndex = -1;
                    int k = 0;
                    foreach (var inter in intermediates)
                    {
                        foreach (var v in inter.Item1)
                        {
                            if (v == CircularArcSection.VariableType.TF)
                            {
                                TFIndex = k;
                                break;
                            }
                            k++;
                        }
                    }
                    if (TFIndex >= 0)
                    {
                        CurvilinearPoint3D target = new CurvilinearPoint3D(sections[sections.Count - 1].End);
                        double bestTF = 0;
                        double min = double.MaxValue;
                        for (double tf = 0; tf < 2.0 * Math.PI; tf += 10.0 * Math.PI / 180.0)
                        {
                            a[TFIndex] = tf;
                            F((double)candidate.Item1[0], a, sections, intermediates, candidate);
                            double d = sections[sections.Count - 1].End.GetDistance(target) ?? double.MaxValue;
                            if (d < min)
                            {
                                min = d;
                                bestTF = tf;
                            }
                        }
                        a[TFIndex] = bestTF;
                        //if (false)
                        //{
                        //    F((double)candidate.Item1[0], a, sections, intermediates, candidate);
                        //    using (System.IO.StreamWriter writer = new System.IO.StreamWriter("c:\\temp\\initial.txt"))
                        //    {
                        //        foreach (var section in sections)
                        //        {
                        //            for (double s = (double)section.Start.Abscissa; s <= section.End.Abscissa; s += 30.0)
                        //            {
                        //                CurvilinearPoint3D p = section.InterpolateAtMD(s);
                        //                writer.WriteLine(p.X + "\t" + p.Y + "\t" + p.Z + "\t" + p.Abscissa + "\t" + (p.Inclination * 180.0 / Math.PI).ToString() + "\t" + (p.Azimuth * 180.0 / Math.PI).ToString());
                        //            }
                        //        }
                        //    }
                        //}
                    }
                }
                double chisq;
                DataModelling.NonLinearFitting(Xs, Ys, sigs, a, ia, (x, param) => F(x, param, sections, intermediates, candidate), out chisq);
                F((double)candidate.Item1[0], a, sections, intermediates, candidate);
                List<double> rx = new List<double>();
                foreach (var c in candidate.Item1)
                {
                    rx.Add((double)sections[sections.Count - 1].Get(c));
                }
                double sum = 0;
                double sum2 = 0;
                for (int i = 0; i < rx.Count; i++)
                {
                    sum += (Ys[i] - rx[i]) * (Ys[i] - rx[i]);
                    sum2 += Ys[i] * Ys[i];
                }
                if (Math.Sqrt(sum) > 1)
                {
                    check = false;
                }
                else
                {
                    foreach (var section in sections)
                    {
                        if (section.Circle != null && section.Circle.Length < 0)
                        {
                            check = false;
                            break;
                        }
                    }
                    if (check)
                    {
                        foreach (CircularArcSection section in sections)
                        {
                            section.End.Azimuth = section.End.Azimuth % (2.0 * Numeric.PI);
                        }
                    }
                }
            }
            return check;
        }

        private double F(double x, double[] a, List<CircularArcSection> sections, List<Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType>> intermediates, Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType> target)
        {
            int idx = (int)x;
            int k = 0;
            for (int i = 0; i < intermediates.Count; i++)
            {
                for (int j = 0; j < intermediates[i].Item1.Count; j++)
                {
                    sections[i].Set(intermediates[i].Item1[j], a[k++]);
                }
                sections[i].Calculate(intermediates[i].Item2);
            }
            sections[sections.Count - 1].Calculate(target.Item2);
            return (double)sections[sections.Count - 1].Get((CircularArcSection.VariableType)idx);
        }

        private double[] GetDefaultsForLevenbergMarquardt(int dim, List<CircularArcSection> sections, List<Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType>> intermediates, List<double> MDs, ArcSection arcSection)
        {
            double[] a = new double[dim];
            int k = 0;
            CurvilinearPoint3D prevPoint = arcSection.Start;
            for (int i = 0; i < MDs.Count; i++)
            {
                CurvilinearPoint3D interpolatedAtMD = arcSection.InterpolateAtMD(MDs[i]);
                if (interpolatedAtMD == null)
                {
                    a = null;
                    break;
                }
                foreach (var acc in intermediates[i].Item1)
                {
                    switch (acc)
                    {
                        case CircularArcSection.VariableType.X:
                            a[k++] = (double)interpolatedAtMD.X;
                            break;
                        case CircularArcSection.VariableType.Y:
                            a[k++] = (double)interpolatedAtMD.Y;
                            break;
                        case CircularArcSection.VariableType.Z:
                            a[k++] = (double)interpolatedAtMD.Z;
                            break;
                        case CircularArcSection.VariableType.Incl:
                            a[k++] = (double)interpolatedAtMD.Inclination;
                            break;
                        case CircularArcSection.VariableType.Az:
                            a[k++] = (double)interpolatedAtMD.Azimuth;
                            break;
                        case CircularArcSection.VariableType.MD:
                            a[k++] = (double)interpolatedAtMD.Abscissa;
                            break;
                        case CircularArcSection.VariableType.Length:
                            a[k++] = (double)(interpolatedAtMD.Abscissa - prevPoint.Abscissa);
                            break;
                        case CircularArcSection.VariableType.TF:
                            double? TF = prevPoint.GetToolface(interpolatedAtMD);
                            if (TF != null)
                            {
                                a[k++] = (double)TF;
                            }
                            else
                            {
                                a = null;
                                break;
                            }
                            break;
                        case CircularArcSection.VariableType.DLS:
                            double? DLS = prevPoint.GetDLS(interpolatedAtMD);
                            if (DLS != null)
                            {
                                a[k++] = (double)DLS;
                            }
                            else
                            {
                                a = null;
                                break;
                            }
                            break;
                        default:
                            a = null;
                            break;
                    }
                    if (a == null)
                    {
                        break;
                    }
                }
                prevPoint = interpolatedAtMD;
                if (a == null)
                {
                    break;
                }
            }
            return a;
        }
        private List<double> GenerateMDs(List<CircularArcSection> sections, List<Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType>> intermediates, ArcSection arc)
        {
            List<double> MDs = new List<double>();
            if (sections != null && sections.Count > 0 && sections[0].Start != null && sections[0].Start.Abscissa != null)
            {
                List<CircularArcSection> secs = new List<CircularArcSection>();
                List<Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType>> inters = new List<Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType>>();
                double startMD = (double)sections[0].Start.Abscissa;
                for (int i = 0; i < intermediates.Count; i++)
                {
                    Tuple<CircularArcSection.VariableType, CircularArcSection.VariableType, CircularArcSection.VariableType> sig = null;
                    if (CircularArcSection.FunctionTypeVariables.TryGetValue(intermediates[i].Item2, out sig))
                    {
                        if ((sig.Item1 == CircularArcSection.VariableType.Z ||
                             sig.Item2 == CircularArcSection.VariableType.Z ||
                             sig.Item3 == CircularArcSection.VariableType.Z) &&
                            !intermediates[i].Item1.Contains(CircularArcSection.VariableType.Z))
                        {
                            if (sections[i] != null && sections[i].End != null && sections[i].End.Z != null)
                            {
                                CurvilinearPoint3D interpolatedAtTVD = arc.InterpolateAtTVD((double)sections[i].End.Z);
                                if (interpolatedAtTVD != null && interpolatedAtTVD.Abscissa != null)
                                {
                                    GenerateMDs(secs, inters, MDs, startMD, (double)interpolatedAtTVD.Abscissa);
                                    secs.Clear();
                                    inters.Clear();
                                    startMD = (double)interpolatedAtTVD.Abscissa;
                                    MDs.Add(startMD);
                                }
                                else
                                {
                                    secs.Add(sections[i]);
                                    inters.Add(intermediates[i]);
                                }
                            }
                            else
                            {
                                secs.Add(sections[i]);
                                inters.Add(intermediates[i]);
                            }
                        }
                        else if ((sig.Item1 == CircularArcSection.VariableType.DLS ||
                                  sig.Item2 == CircularArcSection.VariableType.DLS ||
                                  sig.Item3 == CircularArcSection.VariableType.DLS) &&
                                !intermediates[i].Item1.Contains(CircularArcSection.VariableType.DLS))
                        {
                            if (sections[i] != null && 
                                sections[i].Circle != null && sections[i].Circle.Curvature != null && !Numeric.EQ(sections[i].Circle.Curvature, 0.0) &&
                                sections[i].Start != null && sections[i].Start.Inclination != null)
                            {
                                double R = 1.0 / (double)sections[i].Circle.Curvature;
                                double L = 0.9 * R * (Math.PI / 2.0 - (double)sections[i].Start.Inclination);
                                if (Numeric.GT(L, 0, 1.0) && L < (arc.End.Abscissa-startMD))
                                {
                                    GenerateMDs(secs, inters, MDs, startMD, startMD + L);
                                    secs.Clear();
                                    inters.Clear();
                                    startMD = startMD + L;
                                    MDs.Add(startMD);
                                }
                                else
                                {
                                    secs.Add(sections[i]);
                                    inters.Add(intermediates[i]);
                                }
                            }
                            else
                            {
                                secs.Add(sections[i]);
                                inters.Add(intermediates[i]);
                            }
                        }
                        else
                        {
                            secs.Add(sections[i]);
                            inters.Add(intermediates[i]);
                        }
                    }
                    else
                    {
                        secs.Add(sections[i]);
                        inters.Add(intermediates[i]);
                    }
                }
                if (secs.Count > 0)
                {
                    GenerateMDs(secs, inters, MDs, startMD, (double)arc.End.Abscissa);
                }
            }
            return MDs;
        }

        private void GenerateMDs(List<CircularArcSection> sections, List<Tuple<List<CircularArcSection.VariableType>, CircularArcSection.FunctionType>> intermediates, List<double> MDs, double startMD, double finalMD)
        {
            if (sections != null && intermediates != null && intermediates.Count == sections.Count && sections.Count > 0)
            {
                double definedLengths = 0;
                int count = 0;
                for (int i = 0; i < sections.Count; i++)
                {
                    if (intermediates[i].Item1.Contains(CircularArcSection.VariableType.Length) && sections[i].Circle.Length != null)
                    {
                        definedLengths += (double)sections[i].Circle.Length;
                    }
                    else
                    {
                        count++;
                    }
                }
                double length = finalMD - startMD - definedLengths;
                double step = 0;
                if (count > 0)
                {
                    step = length / (count + 1);
                }
                double currentMD = startMD;
                for (int i = 0; i < sections.Count; i++)
                {
                    if (intermediates[i].Item1.Contains(CircularArcSection.VariableType.Length) && sections[i].Circle.Length != null)
                    {
                        currentMD += (double)sections[i].Circle.Length;
                    }
                    else
                    {
                        currentMD += step;
                    }
                    MDs.Add(currentMD);
                }
            }
        }
        
        /// <summary>
        /// index accessor. It makes sure that the start of the section is actually the end of the previous section (if any).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CircularArcSection this[int index]
        {
            get => sections_[index];
            set
            {
                sections_[index] = value;
                if (index > 0)
                {
                    sections_[index].Start = sections_[index - 1].End;
                }
            }
        }

        public int Count => sections_.Count;

        public bool IsReadOnly => false;

        /// <summary>
        /// ensure to link start to the end of the previous section
        /// </summary>
        /// <param name="item"></param>
        public void Add(CircularArcSection item)
        {
            sections_.Add(item);
            if (sections_.Count > 1)
            {
                item.Start = sections_[sections_.Count - 2].End;
            }
        }

        public void Clear()
        {
            sections_.Clear();
        }

        public bool Contains(CircularArcSection item)
        {
            return sections_.Contains(item);
        }

        public void CopyTo(CircularArcSection[] array, int arrayIndex)
        {
            sections_.CopyTo(array, arrayIndex);
        }

        public IEnumerator<CircularArcSection> GetEnumerator()
        {
            return sections_.GetEnumerator();
        }

        public int IndexOf(CircularArcSection item)
        {
            return sections_.IndexOf(item);
        }

        /// <summary>
        /// ensure that the start points to the end of previous section and the next section start points to the end of item
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, CircularArcSection item)
        {
            sections_.Insert(index, item);
            if (sections_.Count > 1)
            {
                item.Start = sections_[index - 1].End;
            }
            if (index < sections_.Count)
            {
                sections_[index + 1].Start = item.End;
            }
        }

        public bool Remove(CircularArcSection item)
        {
            return sections_.Remove(item);
        }

        public void RemoveAt(int index)
        {
            sections_.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

    }
}
