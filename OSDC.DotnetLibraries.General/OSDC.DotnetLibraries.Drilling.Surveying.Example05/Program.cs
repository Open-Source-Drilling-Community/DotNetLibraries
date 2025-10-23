using NORCE.Drilling.SurveyInstrument.ModelShared;
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

            List<Tuple<double, SurveyInstrument>> silist = new()
            {
                new Tuple<double, SurveyInstrument>(0.0, instrument)
            };
            // an underground position at Norce, Stavanger, Norway
            SurveyPoint survey1 = new() { TVD = 100, Latitude = 58.93438 * System.Math.PI / 180.0, Longitude = 5.70725 * System.Math.PI / 180.0, MD = 100.0, Inclination = 0, Azimuth = 0 };
            SurveyPoint survey2 = new SurveyPoint() { MD = 130.0, Inclination = 2.0 * System.Math.PI / 180.0, Azimuth = 30.0 * System.Math.PI / 180.0 };
            if (survey1.CompleteSIA(survey2))
            {
                SurveyStationList ssList = new()
                {
                    new SurveyStation() {
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
                        Abscissa = survey2.MD,
                        Inclination = survey2.Inclination,
                        Longitude = survey2.Longitude,
                        Latitude = survey2.Latitude,
                        RiemannianNorth = survey2.RiemannianNorth,
                        RiemannianEast = survey2.RiemannianEast,
                        Azimuth = survey2.Azimuth,
                        TVD = survey2.TVD
                    }
                };
                bool success = WolffDeWardtCalculator.CalculateCovariances(ssList, silist);
                for (int i = 1; i < ssList.Count; ++i)
                {
                    if (ssList[i] != null && ssList[i].Covariance != null)
                    {
                        Console.WriteLine(
                        $"C00 = {ssList[i].Covariance[0, 0].Value}\n" +
                        $"C01 = {ssList[i].Covariance[0, 1].Value}\n" +
                        $"C02 = {ssList[i].Covariance[0, 2].Value}\n" +
                        $"C10 = {ssList[i].Covariance[1, 0].Value}\n" +
                        $"C11 = {ssList[i].Covariance[1, 1].Value}\n" +
                        $"C12 = {ssList[i].Covariance[1, 2].Value}\n" +
                        $"C20 = {ssList[i].Covariance[2, 0].Value}\n" +
                        $"C21 = {ssList[i].Covariance[2, 1].Value}\n" +
                        $"C22 = {ssList[i].Covariance[2, 2].Value}");
                    }
                    else
                    {
                        Console.WriteLine("Problem while computing the covariance for survey station");
                    }
                }
            }
            else
            {
                Console.WriteLine("Problem while completing the trajectory");
            }
        }
    }
}