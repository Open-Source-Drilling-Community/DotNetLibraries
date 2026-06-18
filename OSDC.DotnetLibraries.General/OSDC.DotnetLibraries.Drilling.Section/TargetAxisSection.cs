using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    public class TargetAxisSection : IList<CircularArcSection>
    {
        private List<CircularArcSection> inputSections_ = new List<CircularArcSection>();
        private List<CircularArcSection> calculatedSections_ = null;

        /// <summary>
        /// 
        /// </summary>
        public List<CircularArcSection> CalculatedSections { get => calculatedSections_; }
        /// <summary>
        ///  calculate the target axis
        /// </summary>
        /// <returns></returns>
        public bool Calculate()
        {
            bool check = true;
            if (calculatedSections_ != null)
            {
                calculatedSections_.Clear();
            }
            if (calculatedSections_ == null)
            {
                calculatedSections_ = new List<CircularArcSection>();
            }
            for (int i = 1; i < inputSections_.Count; i++)
            {
                if (inputSections_[i].Start != inputSections_[i - 1].End)
                {
                    check = false;
                    break;
                }
            }
            if (check)
            {
                if (inputSections_.Count > 0)
                {
                    CurvilinearPoint3D start = new CurvilinearPoint3D();
                    start.Set(inputSections_[0].Start);
                    if (Numeric.IsUndefined(start.Inclination) || Numeric.IsUndefined(start.Azimuth))
                    {
                        if (Numeric.IsUndefined(inputSections_[0].End.Inclination) || Numeric.IsUndefined(inputSections_[0].End.Azimuth))
                        {
                            Vector3D v0 = new Vector3D(start, inputSections_[0].End);
                            start.Inclination = v0.GetIncl();
                            start.Azimuth = v0.GetAz();
                        }
                        else
                        {
                            start.Inclination = inputSections_[0].End.Inclination;
                            start.Azimuth = inputSections_[0].End.Azimuth;
                        }
                        start.Abscissa = 0.0;
                    }
                    for (int i = 0; i < inputSections_.Count; i++)
                    {
                        if (Numeric.IsDefined(inputSections_[i].End.Inclination) && Numeric.IsDefined(inputSections_[i].End.Azimuth))
                        {
                            //double arc case
                            DoubleArcs doubleArcSection = new DoubleArcs();
                            doubleArcSection.Start.Set(start);
                            doubleArcSection.End.Set(inputSections_[i].End);
                            if (doubleArcSection.CalculateXYZ())
                            {
                                CircularArcSection upstreamSection = new CircularArcSection(doubleArcSection.Start, doubleArcSection.Intermediate);
                                upstreamSection.Circle.Curvature = doubleArcSection.DoubleArcCurve.Curvature;
                                upstreamSection.Circle.ReferenceToolface = doubleArcSection.DoubleArcCurve.UpstreamReferenceToolface;
                                upstreamSection.Circle.Length = doubleArcSection.Intermediate.Abscissa - doubleArcSection.Start.Abscissa;
                                calculatedSections_.Add(upstreamSection);
                                CircularArcSection downstreamSection = new CircularArcSection(doubleArcSection.Intermediate, doubleArcSection.End);
                                downstreamSection.Circle.Curvature = doubleArcSection.DoubleArcCurve.Curvature;
                                downstreamSection.Circle.ReferenceToolface = doubleArcSection.DoubleArcCurve.DownstreamReferenceToolface;
                                downstreamSection.Circle.Length = doubleArcSection.End.Abscissa - doubleArcSection.Intermediate.Abscissa;
                                calculatedSections_.Add(downstreamSection);
                                start = new CurvilinearPoint3D(downstreamSection.End);
                            }
                            else
                            {
                                check = false;
                                break;
                            }
                        }
                        else
                        {
                            CircularArcSection section = new CircularArcSection(start, inputSections_[i].End);
                            if (section.CalculateXYZ())
                            {
                                calculatedSections_.Add(section);
                                start = new CurvilinearPoint3D(section.End);
                            }
                            else
                            {
                                check = false;
                                break;
                            }
                        }
                    }
                }
            }
            return check;
        }
        /// <summary>
        /// index accessor. It makes sure that the starte of the section is actually the end of the previous section (if any).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CircularArcSection this[int index]
        {
            get => inputSections_[index];
            set
            {
                inputSections_[index] = value;
                if (index > 0)
                {
                    inputSections_[index].Start = inputSections_[index - 1].End;
                }
            }
        }

        public int Count => inputSections_.Count;

        public bool IsReadOnly => false;

        /// <summary>
        /// ensure to link start to the end of the previous section
        /// </summary>
        /// <param name="item"></param>
        public void Add(CircularArcSection item)
        {
            inputSections_.Add(item);
            if (inputSections_.Count > 1)
            {
                item.Start = inputSections_[inputSections_.Count - 2].End;
            }
        }

        public void Clear()
        {
            inputSections_.Clear();
        }

        public bool Contains(CircularArcSection item)
        {
            return inputSections_.Contains(item);
        }

        public void CopyTo(CircularArcSection[] array, int arrayIndex)
        {
            inputSections_.CopyTo(array, arrayIndex);
        }

        public IEnumerator<CircularArcSection> GetEnumerator()
        {
            return inputSections_.GetEnumerator();
        }

        public int IndexOf(CircularArcSection item)
        {
            return inputSections_.IndexOf(item);
        }

        /// <summary>
        /// ensure that the start points to the end of previous section and the next section start points to the end of item
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, CircularArcSection item)
        {
            inputSections_.Insert(index, item);
            if (inputSections_.Count > 1)
            {
                item.Start = inputSections_[index - 1].End;
            }
            if (index < inputSections_.Count)
            {
                inputSections_[index + 1].Start = item.End;
            }
        }

        public bool Remove(CircularArcSection item)
        {
            return inputSections_.Remove(item);
        }

        public void RemoveAt(int index)
        {
            inputSections_.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
