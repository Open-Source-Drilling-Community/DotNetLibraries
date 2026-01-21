using OSDC.DotnetLibraries.Drilling.Surveying;
using OSDC.DotnetLibraries.General.Common;
using System.Globalization;

namespace DrillingProperties
{
    class Example
    {
        static void Main()
        {
            // an underground position at Norce, Stavanger, Norway
            SurveyPoint survey1 = new SurveyPoint() {
                TVD = 100, 
                Latitude = 58.93438 * System.Math.PI / 180.0, 
                Longitude = 5.70725 * System.Math.PI / 180.0, 
                MD = 100.0, 
                Inclination = 0,
                Azimuth = 0 
            };
            SurveyPoint survey2 = new SurveyPoint() { 
                MD = 130.0, 
                Inclination = 2.0 * Numeric.PI / 180.0, 
                Azimuth = 30.0 * Numeric.PI / 180.0 
            };
            if (survey1.CompleteFromSIA(survey2))
            {
                Console.WriteLine("Calculated displacements: dZ= " + (survey2.TVD - survey1.TVD)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " m, dNorth= " + (survey2.RiemannianNorth - survey1.RiemannianNorth)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " m, dEast= " + (survey2.RiemannianEast - survey1.RiemannianEast)?.ToString("F3", CultureInfo.InvariantCulture) + " m");
                SurveyPoint survey3 = new SurveyPoint();
                survey1.InterpolateAtAbscissa(survey2, 110.0, survey3);
                Console.WriteLine("Interpolated survey: dZ= " + (survey3.TVD - survey1.TVD)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " m, dNorth= " + (survey3.RiemannianNorth - survey1.RiemannianNorth)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " m, dEast= " + (survey3.RiemannianEast - survey1.RiemannianEast)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " m, Inclination= " + (survey3.Inclination * 180.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °, Azimuth= " + (survey3.Azimuth * 180.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °, Curvature= " + (survey3.Curvature * 180.0 * 30.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °/30m, Toolface= " + (survey3.Toolface * 180.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °, BUR= " + (survey3.BUR * 180.0 * 30.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °/30m, TUR= " + (survey3.TUR * 180.0 * 30.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °/30m");
            }
        }
    }
}