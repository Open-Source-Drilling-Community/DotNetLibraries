using OSDC.DotnetLibraries.Drilling.Surveying;
using OSDC.DotnetLibraries.General.Math;
using System.Globalization;

namespace DrillingProperties
{
    class Example
    {
        private static readonly double DEG2RAD = Math.PI / 180.0;

        static void Main()
        {
            // a underground position at Norce, Stavanger, Norway
            SurveyPoint survey = new SurveyPoint() { TVD = 500, Latitude = 58.93438 * System.Math.PI / 180.0, Longitude = 5.70725 * System.Math.PI / 180.0 };
            Console.WriteLine("RiemannianNorth: " + survey.RiemannianNorth?.ToString("F3", CultureInfo.InvariantCulture) + " m, RiemannianEast: " + survey.RiemannianEast?.ToString("F3", CultureInfo.InvariantCulture) + " m");

            SphericalPoint3D? sphericalPoint3D = survey.GetSphericalPoint();
            if (sphericalPoint3D != null)
            {
                Console.WriteLine("CartesianX: " + sphericalPoint3D.X?.ToString("F3", CultureInfo.InvariantCulture) + " m, CartesianY: " + sphericalPoint3D.Y?.ToString("F3", CultureInfo.InvariantCulture) + " m, CartesianZ: " + sphericalPoint3D.Z?.ToString("F3", CultureInfo.InvariantCulture) + " m");
            }
        }
    }
}