using NORCE.Drilling.SurveyInstrument.Model;
using OSDC.DotnetLibraries.Drilling.Surveying;
using OSDC.DotnetLibraries.General.DataManagement;

namespace DrillingProperties
{
    class Example
    {
        private static readonly double DEG2RAD = Math.PI / 180.0;

        static void Main()
        {
            SurveyInstrument gyro_ISCWSA = new()
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "SurveyInstrument/", ID = new Guid("8ee3d202-47d3-40b2-a8e1-29a4605a025f") },
                Name = "Gyro_ISCWSA",
                Description = "Default Gyro_ISCWSA survey instrument",
                CreationDate = DateTimeOffset.UtcNow,
                LastModificationDate = DateTimeOffset.UtcNow,
                ModelType = SurveyInstrumentModelType.Gyro_ISCWSA,
                UseRelDepthError = false,
                UseMisalignment = false,
                UseTrueInclination = false,
                UseReferenceError = false,
                UseDrillStringMag = false,
                UseGyroCompassError = false,
                CantAngle = 0.0 * DEG2RAD,
                GyroSwitching = 1,
                GyroNoiseRed = 1.0,
                GyroMinDist = 9999,
                ErrorSourceList = [
                    ErrorSourceFactory.Create_AXY_B(magnitude:0.005),
                    ErrorSourceFactory.Create_AXY_SF(magnitude:0.0005),
                    ErrorSourceFactory.Create_AXY_MS(magnitude:0.05 * DEG2RAD),
                    ErrorSourceFactory.Create_AXY_GB(magnitude:0.005),
                    ErrorSourceFactory.Create_GXY_B1(magnitude:0.1 * DEG2RAD / 3600.0, startInclination:0.0 * DEG2RAD, endInclination:150.0 * DEG2RAD, initInclination:0.0 * DEG2RAD),
                    ErrorSourceFactory.Create_GXY_B2(magnitude:0.1 * DEG2RAD / 3600.0, startInclination:0.0 * DEG2RAD, endInclination:150.0 * DEG2RAD, initInclination:0.0 * DEG2RAD),
                    ErrorSourceFactory.Create_GXY_RN(magnitude:0.1 * DEG2RAD / 3600.0, startInclination:0.0 * DEG2RAD, endInclination:150.0 * DEG2RAD, initInclination:0.0 * DEG2RAD),
                    ErrorSourceFactory.Create_GXY_G1(magnitude:0.5 * DEG2RAD / 3600.0, startInclination:0.0 * DEG2RAD, endInclination:150.0 * DEG2RAD, initInclination:0.0 * DEG2RAD),
                    ErrorSourceFactory.Create_GXY_G2(magnitude:0.5 * DEG2RAD / 3600.0, startInclination:0.0 * DEG2RAD, endInclination:150.0 * DEG2RAD, initInclination:0.0 * DEG2RAD),
                    ErrorSourceFactory.Create_GXY_G3(magnitude:0.5 * DEG2RAD / 3600.0, startInclination:0.0 * DEG2RAD, endInclination:150.0 * DEG2RAD, initInclination:0.0 * DEG2RAD),
                    ErrorSourceFactory.Create_GXY_G4(magnitude:0.5 * DEG2RAD / 3600.0, startInclination:0.0 * DEG2RAD, endInclination:150.0 * DEG2RAD, initInclination:0.0 * DEG2RAD),
                    ErrorSourceFactory.Create_GXY_SF(magnitude:0.001, startInclination:0.0 * DEG2RAD, endInclination:150.0 * DEG2RAD, initInclination:0.0 * DEG2RAD),
                    ErrorSourceFactory.Create_GXY_MIS(magnitude:0.05 * DEG2RAD, startInclination:0.0 * DEG2RAD, endInclination:150.0 * DEG2RAD, initInclination:0.0 * DEG2RAD),
                    ErrorSourceFactory.Create_XYM1(magnitude:0.1 * DEG2RAD),
                    ErrorSourceFactory.Create_XYM2(magnitude:0.1 * DEG2RAD),
                    ErrorSourceFactory.Create_XYM3(magnitude:0.2 * DEG2RAD),
                    ErrorSourceFactory.Create_XYM4(magnitude:0.2 * DEG2RAD),
                    ErrorSourceFactory.Create_SAG(magnitude:0.1 * DEG2RAD),
                    ErrorSourceFactory.Create_DRFR(magnitude:0.5),
                    ErrorSourceFactory.Create_DSFS(magnitude:0.5),
                    ErrorSourceFactory.Create_DSFS(magnitude:0.001),
                    ErrorSourceFactory.Create_DSTG(magnitude:5.0e-7),
                ]
            };
            // an underground position at Norce, Stavanger, Norway
            List<SurveyPoint> surveyPointList = [
                new()
                {
                    TVD = 100,
                    Latitude = 58.93438 * System.Math.PI / 180.0,
                    Longitude = 5.70725 * System.Math.PI / 180.0,
                    MD = 100.0,
                    Inclination = 0,
                    Azimuth = 0
                },
                new()
                {
                    MD = 130.0,
                    Inclination = 2.0 * System.Math.PI / 180.0,
                    Azimuth = 30.0 * System.Math.PI / 180.0
                },
                new()
                {
                    MD = 1000.0,
                    Inclination = 10.0 * System.Math.PI / 180.0,
                    Azimuth = 30.0 * System.Math.PI / 180.0
                },
                new()
                {
                    MD = 2000.0,
                    Inclination = 90.0 * System.Math.PI / 180.0,
                    Azimuth = 30.0 * System.Math.PI / 180.0
                }
            ];
            SurveyStationList surveyStationList = new();
            foreach (var sp in surveyPointList)
            {
                surveyStationList.Add(new()
                {
                    SurveyTool = gyro_ISCWSA,
                    Abscissa = sp.MD,
                    Inclination = sp.Inclination,
                    Longitude = sp.Longitude,
                    Latitude = sp.Latitude,
                    RiemannianNorth = sp.RiemannianNorth,
                    RiemannianEast = sp.RiemannianEast,
                    Azimuth = sp.Azimuth,
                    TVD = sp.TVD
                });
            }
            // Trajectory completion is done by the covariance calculators to be sure
            bool ok = CovarianceCalculatorISCWSA.Calculate(surveyStationList);
            if (ok)
            {
                for (int i = 1; i < surveyStationList.Count; ++i)
                {
                    if (surveyStationList[i] is { } s &&
                        s.Covariance is { } cov &&
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
                        $"\nSurveystation: MD={s.MD}, incl={s.Inclination / DEG2RAD}, azim={s.Azimuth / DEG2RAD}\n" +
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
    }
}