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
            double groundLevel = 39.7;
            SurveyPoint survey1 = new SurveyPoint() { TVD = -groundLevel, Latitude = 58.93414 * System.Math.PI / 180.0, Longitude = 5.7085 * System.Math.PI / 180.0, MD = -groundLevel, Inclination = 0, Azimuth = 0 };
            SurveyList traj = new SurveyList() { survey1,
                new SurveyPoint() { MD = 50.0 - groundLevel, Inclination = 0.9 * Numeric.PI / 180.0, Azimuth = -29.7 * Numeric.PI / 180.0 },
                new SurveyPoint() { MD = 100.0 - groundLevel, Inclination = 0.4 * Numeric.PI / 180.0, Azimuth = -95.1 * Numeric.PI / 180.0 },
                new SurveyPoint() { MD = 150.0 - groundLevel, Inclination = 0.7 * Numeric.PI / 180.0, Azimuth = 142.5 * Numeric.PI / 180.0 },
                new SurveyPoint() { MD = 200.0 - groundLevel, Inclination = 0.9 * Numeric.PI / 180.0, Azimuth = 58.2 * Numeric.PI / 180.0 },
                new SurveyPoint() { MD = 250.0 - groundLevel, Inclination = 0.6 * Numeric.PI / 180.0, Azimuth = 173.2 * Numeric.PI / 180.0 },
                new SurveyPoint() { MD = 300.0 - groundLevel, Inclination = 1.3 * Numeric.PI / 180.0, Azimuth = 143.6 * Numeric.PI / 180.0 },
                new SurveyPoint() { MD = 350.0 - groundLevel, Inclination = 6.6 * Numeric.PI / 180.0, Azimuth = 155.6 * Numeric.PI / 180.0 },
                new SurveyPoint() { MD = 400.0 - groundLevel, Inclination = 11.2 * Numeric.PI / 180.0, Azimuth = 143.7 * Numeric.PI / 180.0 },
            };
            if (traj.Calculate())
            {
                Console.WriteLine("Calculated Trajectory");
                PrintTrajectory(traj);
                SurveyList interpolatedTraj = traj.Interpolate(10.0, new List<double>() { 229.0- groundLevel });
                if (interpolatedTraj != null)
                {
                    Console.WriteLine("Interpolated Trajectory");
                    PrintTrajectory(interpolatedTraj);
                }
            }
        }
        static void PrintTrajectory(SurveyList? traj)
        {
            if (traj != null)
            {
                Console.WriteLine("MD (m)\tIncl (°)\tAz (°)\tTVD (m)\tRiem. North (m)\tRiem. East (m)\tDLS (°/30m)\tToolface (°)\tBUR (°/30m)\tTUR (°/30m)");
                foreach (var sv in traj)
                {
                    if (sv != null)
                    {
                        if (sv.Curvature != null && sv.Toolface != null && sv.BUR != null && sv.TUR != null)
                        {
                            Console.WriteLine(sv.MD.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Inclination.Value * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Azimuth.Value * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t\t" +
                                sv.TVD.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                sv.RiemannianNorth.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                sv.RiemannianEast.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Curvature.Value * 30.0 * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Toolface.Value * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.BUR.Value * 30.0 * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.TUR.Value * 30.0 * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            Console.WriteLine(sv.MD.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Inclination.Value * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Azimuth.Value * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t\t" +
                                sv.TVD.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                sv.RiemannianNorth.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                sv.RiemannianEast.Value.ToString("F3", CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
        }
    }
}