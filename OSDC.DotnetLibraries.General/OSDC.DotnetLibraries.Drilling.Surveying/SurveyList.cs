using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    /// <summary>
    /// a list of survey points
    /// </summary>
    public class SurveyList : List<Survey>
    {
        /// <summary>
        /// calculate the whole survey list from the starting survey.
        /// The first survey shall be complete
        /// </summary>
        /// <returns></returns>
        public bool Calculate()
        {
            return Survey.Calculate(this);
        }

        /// <summary>
        /// interpolate a Survey at a given abscissa. The abscissa must be between the first and last Survey of the SurveyList.
        /// </summary>
        /// <param name="MD"></param>
        /// <param name="interpolatedPoint"></param>
        /// <returns></returns>
        public bool InterpolateAtAbscissa(double MD, ICurvilinear3D interpolatedPoint)
        {
            if (interpolatedPoint == null || Numeric.IsUndefined(MD) || Count < 2 || Numeric.LT(MD, this.First<Survey>().MD) || Numeric.GT(MD, this.Last<Survey>().MD))
            {
                return false;
            }
            else
            {
                for (int i = 1; i < Count; i++)
                {
                    if (Numeric.GE(MD, this[i - 1]?.MD) && Numeric.LE(MD, this[i]?.MD))
                    {
                        return this[i - 1].InterpolateAtAbscissa(this[i], MD, interpolatedPoint);
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// interpolate a Survey at a given abscissa. The abscissa must be between the first and last Survey of the SurveyList.
        /// </summary>
        /// <param name="MD"></param>
        /// <param name="interpolatedPoint"></param>
        /// <returns></returns>
        public bool InterpolateAtAbscissa(double MD, Survey interpolatedPoint)
        {
            if (interpolatedPoint == null || Numeric.IsUndefined(MD) || Count < 2 || Numeric.LT(MD, this.First<Survey>().MD) || Numeric.GT(MD, this.Last<Survey>().MD))
            {
                return false;
            }
            else
            {
                for (int i = 1; i < Count; i++)
                {
                    if (Numeric.GE(MD, this[i - 1]?.MD) && Numeric.LE(MD, this[i]?.MD))
                    {
                        return this[i - 1].InterpolateAtAbscissa(this[i], MD, interpolatedPoint);
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// Return an interpolated SurveyList. The interpolation step is passed in argument. In addition
        /// interpolations are made at the abscissas given in a list. The interpolation uses the minimum curvature method.
        /// </summary>
        /// <param name="step"></param>
        /// <param name="abscissas"></param>
        /// <returns></returns>
        public SurveyList? Interpolate(double step, List<double>? abscissas = null)
        {
            if (Count < 2 || Numeric.IsUndefined(step) || Numeric.LE(step, 0) || this[0].MD == null)
            {
                return null;
            }
            List<double> list = new List<double>();
            if (abscissas != null)
            {
                foreach (double s in abscissas)
                {
                    if (Numeric.GE(s, this[0].MD) && Numeric.LE(s, this.Last<Survey>().MD))
                    {
                        list.Add(s);
                    }
                }
            }
            list.Sort();
            SurveyList result = new SurveyList() { this[0] };
            for (double s = this[0].MD.Value + step; Numeric.LE(s, this.Last<Survey>().MD); s += step)
            {
                double lastS = double.MinValue;
                if (list.Count > 0)
                {
                    while (list.Count > 0 && Numeric.LE(list.First<double>(), s))
                    {
                        Survey sv = new Survey();
                        if (InterpolateAtAbscissa(list.First<double>(), sv))
                        {
                            result.Add(sv);
                            lastS = list.First<double>();
                        }
                        list.RemoveAt(0);
                    }
                }
                if (!Numeric.EQ(s, lastS))
                {
                    Survey sv = new Survey();
                    if (InterpolateAtAbscissa(s, sv))
                    {
                        result.Add(sv);
                    }
                }
            }
            return result;
        }
    }
}
