using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public class WolffDeWardtCalculator
    {

        public bool CalculateCovariances(SurveyStationList? traj, List<Tuple<double, WolffDeWardtSurveyInstrument>> instrumentList)
        {
            if (traj == null || instrumentList == null || instrumentList.Count == 0) return false;
            List<Tuple<double, WolffDeWardtSurveyInstrument>> sorted = new List<Tuple<double, WolffDeWardtSurveyInstrument>>();
            foreach (var item in instrumentList)
            {
                sorted.Add(item);
            }
            sorted.Sort();
            double[,]? A = new double[3, 3];
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    A[i, j] = 0;
                }
            }
            for (int i = 1; i < traj.Count; i++)
            {
                WolffDeWardtSurveyInstrument instrument;
                while (sorted.Count > 1 && Numeric.GE(traj[i].MD, sorted[1].Item1))
                {
                    sorted.RemoveAt(0);
                }
                instrument = sorted[0].Item2;
                A = CalculateCovariances(instrument, traj[i - 1], traj[i], A);
            }
            return false;
        }
        public double[,]? CalculateCovariances(WolffDeWardtSurveyInstrument instrument, SurveyStation? previousStation, SurveyStation? surveyStation, double[,]? A)
        {
            if (A != null &&
                instrument != null &&
                previousStation != null &&
                surveyStation != null && 
                A != null &&
                surveyStation.Inclination != null &&
                surveyStation.Azimuth != null &&
                previousStation.MD != null &&
                surveyStation.MD != null &&
                surveyStation.TVD != null &&
                previousStation.TVD != null &&
                surveyStation.RiemannianNorth != null &&
                previousStation.RiemannianNorth != null &&
                surveyStation.RiemannianEast != null &&
                previousStation.RiemannianEast != null)
            {
                double sinI = System.Math.Sin(surveyStation.Inclination.Value);
                double cosI = System.Math.Cos(surveyStation.Inclination.Value);
                double sinA = System.Math.Sin(surveyStation.Azimuth.Value);
                double cosA = System.Math.Cos(surveyStation.Azimuth.Value);
                double deltaSk = surveyStation.MD.Value - previousStation.MD.Value;
                double deltaC10 = instrument.ReferenceError != null ? instrument.ReferenceError.Value : 0;
                double deltaC20 = instrument.DrillStringMag != null ? instrument.DrillStringMag.Value : 0;
                double deltaC30 = instrument.GyroCompassError != null ? instrument.GyroCompassError.Value : 0;
                double deltaIt0 = instrument.TrueInclination != null ? instrument.TrueInclination.Value : 0;
                double deltaIm = instrument.Misalignment != null ? instrument.Misalignment.Value : 0;
                double epsilon = instrument.RelDepthError != null ? instrument.RelDepthError.Value : 0;
                double deltaZ = surveyStation.TVD.Value - previousStation.TVD.Value;
                double deltaX = surveyStation.RiemannianNorth.Value - previousStation.RiemannianNorth.Value;
                double deltaY = surveyStation.RiemannianEast.Value - previousStation.RiemannianEast.Value;

                if ((Numeric.EQ(cosI, 0.0) && Numeric.IsDefined(deltaC30) && !Numeric.EQ(deltaC30, 0.0)) || Numeric.IsUndefined(A[0, 0]))
                {
                    for (int i = 0; i < A.GetLength(0); i++)
                    {
                        for (int j = 0; j < A.GetLength(1); j++)
                        {
                            A[i, j] = Numeric.UNDEF_DOUBLE;
                        }
                    }
                }
                else
                {
                    // calculate Transfer vectors
                    double tmp = deltaC10 * sinI * deltaSk;
                    A[0, 0] = A[0, 0] - tmp * sinA;
                    A[0, 1] = A[0, 1] + tmp * cosA;
                    tmp = deltaC20 * sinI * sinI * sinA * deltaSk;
                    A[1, 0] = A[1, 0] - tmp * sinA;
                    A[1, 1] = A[1, 1] + tmp * cosA;
                    if (!Numeric.EQ(cosI, 0.0))
                    {
                        tmp = deltaC30 * deltaSk * sinI / cosI;
                        A[2, 0] = A[2, 0] - tmp * sinA;
                        A[2, 1] = A[2, 1] + tmp * cosA;
                    }
                    else
                    {
                        A[2, 0] = 0;
                        A[2, 1] = 0;
                    }
                    tmp = deltaIt0 * sinI * deltaSk;
                    A[3, 0] = A[3, 0] + tmp * cosI * cosA;
                    A[3, 1] = A[3, 1] + tmp * cosI * sinA;
                    A[3, 2] = A[3, 2] - tmp * sinI;
                    tmp = deltaIm;
                    A[4, 0] = A[4, 0] + tmp * deltaX;
                    A[4, 1] = A[4, 1] + tmp * deltaY;
                    A[4, 2] = A[4, 2] + tmp * deltaZ;
                    tmp = epsilon;
                    A[5, 0] = A[5, 0] + tmp * deltaX;
                    A[5, 1] = A[5, 1] + tmp * deltaY;
                    A[5, 2] = A[5, 2] + tmp * deltaZ;

                    //calculate covariance matrix
                    tmp = A[4, 0] * A[4, 0] + A[4, 1] * A[4, 1] + A[4, 2] * A[4, 2];
                    if (surveyStation.Covariance == null)
                    {
                        surveyStation.Covariance = new SymmetricMatrix3x3();
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            surveyStation.Covariance[i, j] = A[0, i] * A[0, j] + A[1, i] * A[1, j] + A[2, i] * A[2, j] + A[3, i] * A[3, j] + A[4, i] * A[4, j] + A[5, i] * A[5, j] + ((i == j) ? tmp : 0.0) - A[4, i] * A[4, j];
                        }
                    }

                    // apply horizontal magnetic deviations
                    if (surveyStation.Bias == null)
                    {
                        surveyStation.Bias = new Vector3D();
                    }
                    surveyStation.Bias.X = A[1, 1];
                    surveyStation.Bias.Y = A[1, 0];
                    surveyStation.Bias.Z = 0.0; 
                }
                return A;
            }
            else
            {
                return null;
            }
        }
    }
}
