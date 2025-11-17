using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public static class CovarianceCalculatorWolffDeWardt
    {
        public static bool Calculate(List<SurveyStation>? surveyStationList, List<int>? surveyStationsIndices = null)
        {
            bool ok = false;
            if (surveyStationList != null && surveyStationList.Count > 0)
            {
                ok = SurveyStation.CompleteSIA(surveyStationList); // make sure that all survey stations member variables are complete
                if (ok)
                {
                    double[,]? A = new double[6, 3];
                    for (int i = 0; i < A.GetLength(0); i++)
                    {
                        for (int j = 0; j < A.GetLength(1); j++)
                        {
                            A[i, j] = 0;
                        }
                    }
                    int startIdx = surveyStationsIndices is null ? 0 : surveyStationsIndices[0];
                    int endIdx = surveyStationsIndices is null ? surveyStationList.Count - 1 : surveyStationsIndices[^1];
                    // The error at first survey station is assumed to be 0
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            surveyStationList[startIdx].Covariance![j, k] = 0.0;
                        }
                    }
                    for (int i = 1 + startIdx; i <= endIdx; i++)
                    {
                        A = CalculateCovariance(surveyStationList[i - 1], surveyStationList[i], A);
                    }
                    ok = true;
                }
            }
            return ok;
        }

        public static double[,]? CalculateCovariance(SurveyStation? previousStation, SurveyStation? surveyStation, double[,]? A)
        {
            if (A is { } &&
                previousStation is { } &&
                surveyStation is { } &&
                surveyStation.SurveyTool is { } instrument &&
                surveyStation.Inclination is { } incl &&
                surveyStation.Azimuth is { } azim &&
                previousStation.MD is { } mdPrev &&
                surveyStation.MD is { } md &&
                surveyStation.TVD is { } tvd &&
                previousStation.TVD is { } tvdPrev &&
                surveyStation.RiemannianNorth is { } rNorth &&
                previousStation.RiemannianNorth is { } rNorthPrev &&
                surveyStation.RiemannianEast is { } rEast &&
                previousStation.RiemannianEast is { } rEastPrev)
            {
                double sinI = System.Math.Sin(incl);
                double cosI = System.Math.Cos(incl);
                double sinA = System.Math.Sin(azim);
                double cosA = System.Math.Cos(azim);
                double deltaSk = md - mdPrev;
                double deltaC10 = instrument.ReferenceError is { } refErr ? refErr : 0;
                double deltaC20 = instrument.DrillStringMag is { } dMag ? dMag : 0;
                double deltaC30 = instrument.GyroCompassError is { } gyrErr ? gyrErr : 0;
                double deltaIt0 = instrument.TrueInclination is { } trueIncl ? trueIncl : 0;
                double deltaIm = instrument.Misalignment is { } mis ? mis : 0;
                double epsilon = instrument.RelDepthError is { } relErr ? relErr : 0;
                double deltaZ = tvd - tvdPrev;
                double deltaX = rNorth - rNorthPrev;
                double deltaY = rEast - rEastPrev;

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
                    surveyStation.Covariance ??= new SymmetricMatrix3x3();
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            surveyStation.Covariance[i, j] = A[0, i] * A[0, j] + A[1, i] * A[1, j] + A[2, i] * A[2, j] + A[3, i] * A[3, j] + A[4, i] * A[4, j] + A[5, i] * A[5, j] + ((i == j) ? tmp : 0.0) - A[4, i] * A[4, j];
                        }
                    }

                    // apply horizontal magnetic deviations
                    surveyStation.Bias ??= new Vector3D();
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
