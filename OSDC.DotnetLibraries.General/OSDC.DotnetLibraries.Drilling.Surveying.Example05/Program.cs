using OSDC.DotnetLibraries.Drilling.Surveying;

namespace DrillingProperties
{
    class Example
    {
        private static readonly double DEG2RAD = Math.PI / 180.0;

        static void Main()
        {
            SurveyInstrument instrument = new()
            {
                RelDepthError = 0.001,
                Misalignment = 0.1 * DEG2RAD,
                TrueInclination = 0.5 * DEG2RAD,
                ReferenceError = 1.5 * DEG2RAD,
                DrillStringMag = 0.25 * DEG2RAD,
            };
            // an underground position at Norce, Stavanger, Norway
            SurveyPoint survey1 = new()
            {
                TVD = 100,
                Latitude = 58.93438 * System.Math.PI / 180.0,
                Longitude = 5.70725 * System.Math.PI / 180.0,
                MD = 100.0,
                Inclination = 0,
                Azimuth = 0
            };
            SurveyPoint survey2 = new()
            {
                MD = 130.0,
                Inclination = 2.0 * System.Math.PI / 180.0,
                Azimuth = 30.0 * System.Math.PI / 180.0
            };
            List<SurveyStation> surveyStationList =
            [
                new SurveyStation() {
                        SurveyTool = instrument,
                        Abscissa = survey1.MD,
                        Inclination = survey1.Inclination,
                        Longitude = survey1.Longitude,
                        Latitude = survey1.Latitude,
                        RiemannianNorth = survey1.RiemannianNorth,
                        RiemannianEast = survey1.RiemannianEast,
                        Azimuth = survey1.Azimuth,
                        TVD = survey1.TVD
                    },
                    new SurveyStation() {
                        SurveyTool = instrument,
                        Abscissa = survey2.MD,
                        Inclination = survey2.Inclination,
                        Longitude = survey2.Longitude,
                        Latitude = survey2.Latitude,
                        RiemannianNorth = survey2.RiemannianNorth,
                        RiemannianEast = survey2.RiemannianEast,
                        Azimuth = survey2.Azimuth,
                        TVD = survey2.TVD
                    }
            ];
            if (SurveyPoint.CompleteSurvey(surveyStationList))
            {
                bool ok = CovarianceCalculatorWolffDeWardt.Calculate(surveyStationList);
                if (ok)
                {
                    for (int i = 1; i < surveyStationList.Count; ++i)
                    {
                        if (surveyStationList[i] is { } &&
                            surveyStationList[i].Covariance is { } cov &&
                            cov[0, 0] is double &&
                            cov[0, 1] is double &&
                            cov[0, 2] is double &&
                            cov[1, 0] is double &&
                            cov[1, 1] is double &&
                            cov[1, 2] is double &&
                            cov[2, 0] is double &&
                            cov[2, 1] is double &&
                            cov[2, 2] is double)
                        {
                            Console.WriteLine(
                            $"C00 = {cov[0, 0]}\n" +
                            $"C01 = {cov[0, 1]}\n" +
                            $"C02 = {cov[0, 2]}\n" +
                            $"C10 = {cov[1, 0]}\n" +
                            $"C11 = {cov[1, 1]}\n" +
                            $"C12 = {cov[1, 2]}\n" +
                            $"C20 = {cov[2, 0]}\n" +
                            $"C21 = {cov[2, 1]}\n" +
                            $"C22 = {cov[2, 2]}");
                        }
                        else
                        {
                            Console.WriteLine($"Problem while computing the covariance for survey station {i}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Problem while calculating covariances");
                }
            }
            else
            {
                Console.WriteLine("Problem while completing the trajectory");
            }
        }
    }
}