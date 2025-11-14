using DWIS.API.DTO;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Linq;
using static NORCE.Drilling.SurveyInstrument.Model.ErrorSource;

namespace NORCE.Drilling.SurveyInstrument.Model
{
    public static class ErrorSourceFactory
    {
        public static ErrorSource Create_XYM1(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("cc8fbca4-d168-49d2-8f7a-96ea408b9c1c") },
                ErrorCode = ErrorCode.XYM1,
                Description = "Error due to the Misalignment: XY Misalignment 1 error source",
                Index = 30,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                        return Math.Sin(incl); // NB! Make configurable
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                }
            };
            return src;
        }

        public static ErrorSource Create_XYM2(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("fc0968eb-321a-4b8e-83bc-37667c63efcd") },
                ErrorCode = ErrorCode.XYM2,
                Description = "Error due to the Misalignment: XY Misalignment 2 error source",
                Index = 31,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return -1.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_XYM3(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("c3c797bb-d5db-4aff-9fdf-8d9c1eb28f9e") },
                ErrorCode = ErrorCode.XYM3,
                Description = "Error due to the Misalignment: XY Misalignment 3 error source",
                Index = 31,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = true,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                            args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                        {
                            double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? 0.0;
                            return Math.Abs(Math.Cos(incl)) * Math.Cos(az + converg);
                        }
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? 0.0;
                        return incl < 0.0001 * Math.PI / 180.0 ? (double?)null : -Math.Abs(Math.Cos(incl)) * Math.Sin(az + converg) / Math.Sin(incl);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 1.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_XYM4(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("8f6ab35e-1bc7-496f-967a-ca15c38a7aa9") },
                ErrorCode = ErrorCode.XYM4,
                Description = "Error due to the Misalignment: XY Misalignment 4 error source",
                Index = 31,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = true,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                            args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? 0.0;
                        return Math.Abs(Math.Cos(incl)) * Math.Sin(az + converg);
                    }
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                            args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? 0.0;
                        return incl < 0.0001 * Math.PI / 180.0 ? (double?)null : Math.Abs(Math.Cos(incl)) * Math.Cos(az + converg) / Math.Sin(incl);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = az => 1.0,
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_SAG(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("8e22c56b-5fc9-4b82-8501-af31590530a8") },
                ErrorCode = ErrorCode.SAG,
                Description = "Error due to the Vertical Sag error source",
                Index = 31,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                        return Math.Sin(incl); // NB! Make configurable
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DRFR(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("e6320bc9-fbc1-42ad-b7a1-43fd6758833d") },
                ErrorCode = ErrorCode.DRFR,
                Description = "Depth Reference - Random error source",
                Index = 1,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "DepthDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 1.0;
                },
                WeightingFunctionIncl = args =>
                {
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DRFS(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("853bd46d-798d-4ad6-80e2-e83c0789bbd0") },
                ErrorCode = ErrorCode.DRFS,
                Description = "Depth Reference - Systematic error source",
                Index = 1,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "DepthDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DSFS(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("0047535a-4931-4ebb-9240-3df37145e96d") },
                ErrorCode = ErrorCode.DSFS,
                Description = "Depth Scale Factor - Systematic error source",
                Index = 2,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.MD)?.Value is double md)
                        return md;
                    return 0.0;
                },
                WeightingFunctionDepthGyro = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.MD)?.Value is double md &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.MDPrev)?.Value is double mdPrev)
                        return md - mdPrev;
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DSTG(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("5915d643-9eb9-45c7-801b-e7f9384bef6f") },
                ErrorCode = ErrorCode.DSTG,
                Description = "Depth Stretch - Global error source",
                Index = 3,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = true,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ReciprocalLengthSurveyInstrumentDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.MD)?.Value is double md &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.TVD)?.Value is double tvd)
                        return md * tvd;
                    return 0.0;
                },
                WeightingFunctionDepthGyro = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.MD)?.Value is double md &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.MDPrev)?.Value is double mdPrev &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.TVD)?.Value is double tvd &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                        return (md * Math.Cos(incl) + tvd) * (md - mdPrev);
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_XYM3E(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("a8305b27-a47e-44aa-9f61-86883bdcac38") },
                ErrorCode = ErrorCode.XYM3E,
                Description = "XY Misalignment 3E error source",
                Index = 32,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = true,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                            args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                        {
                            double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? 0.0;
                            return Math.Abs(Math.Cos(incl)) * Math.Cos(az + converg);
                        }
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? 0.0;
                        return incl < 0.0001 * Math.PI / 180.0 ? (double?)null : -(Math.Abs(Math.Cos(incl)) * Math.Sin(az + converg)) / Math.Sin(incl);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 1.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_XYM4E(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("8c0b58ed-9c3d-4379-b9a4-4456d8848af1") },
                ErrorCode = ErrorCode.XYM4E,
                Description = "XY Misalignment 4E error source",
                Index = 33,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = true,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                            args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                        {
                            double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? 0.0;
                            return Math.Abs(Math.Cos(incl)) * Math.Sin(az + converg);
                        }
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? 0.0;
                        return incl < 0.0001 * Math.PI / 180.0 ? (double?)null : (Math.Abs(Math.Cos(incl)) * Math.Cos(az + converg)) / Math.Sin(incl);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = az => 1.0,
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_SAGE(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("2326af01-5fe5-4615-8ab8-5dd33cb586c8") },
                ErrorCode = ErrorCode.SAGE,
                Description = "MWD: Sag Enhanced error source",
                Index = 29,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = true,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                            return Math.Pow(Math.Sin(incl), 0.25);
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_XCLH(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("703462b6-daa1-443f-b23f-471a4917f196") },
                ErrorCode = ErrorCode.XCLH,
                Description = "Depth: Long Course Length High Side XCL",
                Index = 35,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "DepthDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_XCLL(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("025ac751-b194-424d-9528-6b5462d1573f") },
                ErrorCode = ErrorCode.XCLL,
                Description = "Depth: Long Course Length Low Side XCL",
                Index = 35,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "DepthDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_XCLA(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("31a07b36-9d62-4f49-b440-567e0a7a3b3a") },
                ErrorCode = ErrorCode.XCLA,
                Description = " Depth: Long Course Length Azimuth XCL",
                Index = 34,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "DepthDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_ABXY_TI1S(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("bac97f7c-bd87-4a4c-8d74-f2aad5f9e994") },
                ErrorCode = ErrorCode.ABXY_TI1S,
                Description = "MWD TF Ind: X and Y Accelerometer Bias error",
                Index = 4,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AccelerationDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                        {
                            double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                            return -System.Math.Cos(incl) / gField;
                        }
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        return (tanDip * cosI * sinAm) / gField;
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_ABXY_TI2S(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("d3a8730a-6489-40ac-8215-d85515e84dcb") },
                ErrorCode = ErrorCode.ABXY_TI2S,
                Description = "MWD TF Ind: X and Y Accelerometer Bias error",
                Index = 5,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = true,
                Magnitude = magnitude,
                MagnitudeQuantity = "AccelerationDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        if (incl < 0.0001 * Math.PI / 180.0)
                        {
                            return null;
                        }
                        else
                        {
                            return (System.Math.Tan(Math.PI / 2 - incl) - tanDip * cosAm) / gField;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                        return -Math.Sin(az) / gField;
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                        return Math.Cos(az) / gField;
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_ABIXY_TI1S(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("de56cc32-cb33-4d36-a709-4ff41c238284") },
                ErrorCode = ErrorCode.ABIXY_TI1S,
                Description = "MWD TF Ind: X and Y Accelerometer Bias axial interference correction - term 1 [m/s2]",
                Index = 4,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AccelerationDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_ABIXY_TI2S(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("e3f3f423-8bd7-473d-a6b3-395e44d094ec") },
                ErrorCode = ErrorCode.ABIXY_TI2S,
                Description = "MWD TF Ind: X and Y Accelerometer Bias - axial interference correction - term 2 [m/s2]",
                Index = 5,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = true,
                Magnitude = magnitude,
                MagnitudeQuantity = "AccelerationDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_ABZ(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("3e836647-cf32-419d-bf83-b38a4afc5b5d") },
                ErrorCode = ErrorCode.ABZ,
                Description = "MWD: Z-Accelerometer Bias error",
                Index = 6,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AccelerationDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                        {
                            double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                            return -System.Math.Sin(incl) / gField;
                        }
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        return tanDip * sinI * sinAm / gField;
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_ASXY_TI1S(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("f10bb03e-d6e8-4bac-95c0-66dd1ffa6c7f") },
                ErrorCode = ErrorCode.ASXY_TI1S,
                Description = "MWD TF Ind: X and Y Accelerometer Scale Factor error",
                Index = 7,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                            return System.Math.Sin(incl) * System.Math.Cos(incl) / Math.Sqrt(2);
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        return (-tanDip * sinI * cosI * sinAm) / Math.Sqrt(2);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_ASXY_TI2S(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("4bdf5134-e742-4ee5-a07f-4157fcec543e") },
                ErrorCode = ErrorCode.ASXY_TI2S,
                Description = "MWD TF Ind: X and Y Accelerometer Scale Factor error",
                Index = 8,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                            return System.Math.Sin(incl) * System.Math.Cos(incl) / 2;
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        return (-tanDip * sinI * cosI * sinAm) / 2;
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_ASXY_TI3S(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("421e40f3-586c-41b8-bb35-8735a353f56f") },
                ErrorCode = ErrorCode.ASXY_TI3S,
                Description = "MWD TF Ind: X and Y Accelerometer Scale Factor error",
                Index = 9,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        return (tanDip * sinI * cosAm - cosI) / 2;
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_ASZ(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("05cdcccf-6036-4903-8a10-45579b0cd1c0") },
                ErrorCode = ErrorCode.ASXY_TI3S,
                Description = "MWD: Z-Accelerometer Scale Factor error",
                Index = 10,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                            return -System.Math.Sin(incl) * System.Math.Cos(incl);
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        return tanDip * sinI * cosI * sinAm;
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_MBXY_TI1(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("8f9f9af9-5e4e-4df7-9f53-366306cc1dcc") },
                ErrorCode = ErrorCode.MBXY_TI1,
                Description = " MWD TF Ind: X and Y Magnetometer Bias error",
                Index = 11,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "EarthMagneticFluxDensity",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double bField = args.FirstOrDefault(p => p?.Key == ParameterType.BField)?.Value ?? SurveyInstrument.DEFAULT_BFIELD;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double cosDip = System.Math.Cos(dip);
                        return -cosI * sinAm / (bField * cosDip);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_MBXY_TI2(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("8aeebd78-28fe-443b-933f-83d753ec52a3") },
                ErrorCode = ErrorCode.MBXY_TI2,
                Description = " MWD TF Ind: X and Y Magnetometer Bias error",
                Index = 12,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "EarthMagneticFluxDensity",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                    args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double bField = args.FirstOrDefault(p => p?.Key == ParameterType.BField)?.Value ?? SurveyInstrument.DEFAULT_BFIELD;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double cosDip = System.Math.Cos(dip);
                        return cosAm / (bField * cosDip);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_MBZ(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("6770a9a5-b184-4b13-a470-e5149c1f5082") },
                ErrorCode = ErrorCode.MBZ,
                Description = "MWD: Z-Magnetometer Bias error",
                Index = 13,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "EarthMagneticFluxDensity",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double bField = args.FirstOrDefault(p => p?.Key == ParameterType.BField)?.Value ?? SurveyInstrument.DEFAULT_BFIELD;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double cosDip = System.Math.Cos(dip);
                        return -sinI * sinAm / (bField * cosDip);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_MSXY_TI1(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("8f55c64b-28d8-4fd0-b989-adc080b89dee") },
                ErrorCode = ErrorCode.MSXY_TI1,
                Description = "MWD TF Ind: X and Y Magnetometer Scale Factor error",
                Index = 14,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        return sinI * sinAm * (tanDip * cosI + sinI * cosAm) / Math.Sqrt(2);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_MSXY_TI2(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("ec3de795-f6ff-4123-9f5a-7495cd7ee7f5") },
                ErrorCode = ErrorCode.MSXY_TI2,
                Description = "MWD TF Ind: X and Y Magnetometer Scale Factor error",
                Index = 15,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        return sinAm * (tanDip * sinI * cosI - cosI * cosI * cosAm - cosAm) / 2;
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_MSXY_TI3(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("5aff380f-8562-4cca-be15-9327cb7db6df") },
                ErrorCode = ErrorCode.MSXY_TI3,
                Description = "MWD TF Ind: X and Y Magnetometer Scale Factor error",
                Index = 16,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        return (cosI * cosAm * cosAm - cosI * sinAm * sinAm - tanDip * sinI * cosAm) / 2;
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_MSZ(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("1b7d3e3d-4694-43a7-97ba-5952f396a1fb") },
                ErrorCode = ErrorCode.MSZ,
                Description = "MWD: Z-Magnetometer Scale Factor error",
                Index = 17,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double cosI = System.Math.Cos(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosAm = System.Math.Cos(az - declin);
                        double tanDip = System.Math.Tan(dip);
                        return -(sinI * cosAm + tanDip * cosI) * sinI * sinAm;
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_AMIL(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("f2527672-1226-45d0-ae3a-9b52591aaed8") },
                ErrorCode = ErrorCode.MSZ,
                Description = "MWD: Axial Interference - SinI.SinA error",
                Index = 28,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "MagneticFlux",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                        double bField = args.FirstOrDefault(p => p?.Key == ParameterType.BField)?.Value ?? SurveyInstrument.DEFAULT_BFIELD;
                        double declin = args.FirstOrDefault(p => p?.Key == ParameterType.Declination)?.Value ?? SurveyInstrument.DEFAULT_DECLINATION;
                        double sinI = System.Math.Sin(incl);
                        double sinAm = System.Math.Sin(az - declin);
                        double cosDip = System.Math.Cos(dip);
                        return sinI * sinAm / (bField * cosDip);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DEC_U(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("8c0b58ed-9c3d-4379-b9a4-4456d8848af1") },
                ErrorCode = ErrorCode.DEC_U,
                Description = "MWD: Declination - Uncorrelated error",
                Index = 18,
                IsSystematic = false,
                IsRandom = false,
                IsGlobal = true,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 1.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DEC_OS(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("2326af01-5fe5-4615-8ab8-5dd33cb586c8") },
                ErrorCode = ErrorCode.DEC_OS,
                Description = "MWD: Declination - Crustal Omission SD Ref Models error",
                Index = 19,
                IsSystematic = false,
                IsRandom = false,
                IsGlobal = true,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 1.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DEC_OH(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("703462b6-daa1-443f-b23f-471a4917f196") },
                ErrorCode = ErrorCode.DEC_OH,
                Description = "MWD: Declination - Crustal Omission HD Ref Models error",
                Index = 20,
                IsSystematic = false,
                IsRandom = false,
                IsGlobal = true,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 1.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DEC_OI(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("025ac751-b194-424d-9528-6b5462d1573f") },
                ErrorCode = ErrorCode.DEC_OI,
                Description = "MWD: Declination - Crustal Omission IFR Ref Models error",
                Index = 21,
                IsSystematic = false,
                IsRandom = false,
                IsGlobal = true,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 1.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DECR(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("31a07b36-9d62-4f49-b440-567e0a7a3b3a") },
                ErrorCode = ErrorCode.DECR,
                Description = "MWD: Declination - Random error",
                Index = 22,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 1.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DBH_U(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("bac97f7c-bd87-4a4c-8d74-f2aad5f9e994") },
                ErrorCode = ErrorCode.DBH_U,
                Description = "MWD: BH-Dependent Declination - Uncorrelated error",
                Index = 23,
                IsSystematic = false,
                IsRandom = false,
                IsGlobal = true,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngleMagneticFluxDensitySurveyInstrumentDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                    double bField = args.FirstOrDefault(p => p?.Key == ParameterType.BField)?.Value ?? SurveyInstrument.DEFAULT_BFIELD;
                    return 1 / (bField * Math.Cos(dip));
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DBH_OS(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("d3a8730a-6489-40ac-8215-d85515e84dcb") },
                ErrorCode = ErrorCode.DBH_OS,
                Description = "MWD: BH-Dependent Declination - Crustal Omission SD Ref Models  error",
                Index = 24,
                IsSystematic = false,
                IsRandom = false,
                IsGlobal = true,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngleMagneticFluxDensitySurveyInstrumentDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                    double bField = args.FirstOrDefault(p => p?.Key == ParameterType.BField)?.Value ?? SurveyInstrument.DEFAULT_BFIELD;
                    return 1 / (bField * Math.Cos(dip));
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DBH_OH(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("f10bb03e-d6e8-4bac-95c0-66dd1ffa6c7f") },
                ErrorCode = ErrorCode.DBH_OH,
                Description = "MWD: BH-Dependent Declination - Crustal Omission HD Ref Models  error",
                Index = 25,
                IsSystematic = false,
                IsRandom = false,
                IsGlobal = true,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngleMagneticFluxDensitySurveyInstrumentDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                    double bField = args.FirstOrDefault(p => p?.Key == ParameterType.BField)?.Value ?? SurveyInstrument.DEFAULT_BFIELD;
                    return 1 / (bField * Math.Cos(dip));
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DBH_OI(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("4bdf5134-e742-4ee5-a07f-4157fcec543e") },
                ErrorCode = ErrorCode.DBH_OI,
                Description = "MWD: BH-Dependent Declination - Crustal Omission IFR Ref Models error",
                Index = 26,
                IsSystematic = false,
                IsRandom = false,
                IsGlobal = true,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngleMagneticFluxDensitySurveyInstrumentDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                    double bField = args.FirstOrDefault(p => p?.Key == ParameterType.BField)?.Value ?? SurveyInstrument.DEFAULT_BFIELD;
                    return 1 / (bField * Math.Cos(dip));
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_DBHR(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("421e40f3-586c-41b8-bb35-8735a353f56f") },
                ErrorCode = ErrorCode.DBHR,
                Description = "MWD: BH-Dependent Declination - Random error",
                Index = 27,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngleMagneticFluxDensitySurveyInstrumentDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    double dip = args.FirstOrDefault(p => p?.Key == ParameterType.Dip)?.Value ?? SurveyInstrument.DEFAULT_DIP;
                    double bField = args.FirstOrDefault(p => p?.Key == ParameterType.BField)?.Value ?? SurveyInstrument.DEFAULT_BFIELD;
                    return 1 / (bField * Math.Cos(dip));
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_AXYZ_XYB(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("05cdcccf-6036-4903-8a10-45579b0cd1c0") },
                ErrorCode = ErrorCode.AXYZ_XYB,
                Description = "Gyro 3-axis: xy accelerometer bias  error",
                Index = 36,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AccelerationDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                    {
                        double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                        return Math.Cos(incl) / gField;
                    }
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_AXYZ_ZB(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("8f9f9af9-5e4e-4df7-9f53-366306cc1dcc") },
                ErrorCode = ErrorCode.AXYZ_ZB,
                Description = "Gyro 3-axis: z accelerometer bias  error",
                Index = 37,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AccelerationDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                    {
                        double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                        return Math.Sin(incl) / gField;
                    }
                        
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_AXYZ_SF(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("8aeebd78-28fe-443b-933f-83d753ec52a3") },
                ErrorCode = ErrorCode.AXYZ_SF,
                Description = "Gyro 3-axis: accelerometer scale factor error",
                Index = 38,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                        return 1.3 * Math.Sin(incl) * Math.Cos(incl);
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_AXYZ_MIS(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("6770a9a5-b184-4b13-a470-e5149c1f5082") },
                ErrorCode = ErrorCode.AXYZ_MIS,
                Description = "Gyro 3-axis: accelerometer misalignment error",
                Index = 39,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    return 1.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_AXY_B(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("ec3de795-f6ff-4123-9f5a-7495cd7ee7f5") },
                ErrorCode = ErrorCode.AXY_B,
                Description = "Gyro 2-axis: accelerometer bias error",
                Index = 40,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = true,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AccelerationDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                    {
                        double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                        double cantAngle = args.FirstOrDefault(p => p?.Key == ParameterType.CantAngle)?.Value ?? SurveyInstrument.DEFAULT_CANT_ANGLE;
                        double kOperator = args.FirstOrDefault(p => p?.Key == ParameterType.KOperator)?.Value ?? 1;
                        if (incl > 90.0 * Math.PI / 180.0)
                            kOperator = -1;
                        return 1 / (gField * Math.Cos(incl - kOperator * cantAngle));
                    }
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_AXY_SF(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("8f55c64b-28d8-4fd0-b989-adc080b89dee") },
                ErrorCode = ErrorCode.AXY_SF,
                Description = "Gyro 2-axis: accelerometer scale factor error",
                Index = 41,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = true,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                    {
                        double cantAngle = args.FirstOrDefault(p => p?.Key == ParameterType.CantAngle)?.Value ?? SurveyInstrument.DEFAULT_CANT_ANGLE;
                        double kOperator = args.FirstOrDefault(p => p?.Key == ParameterType.KOperator)?.Value ?? 1;
                        if (incl > 90.0 * Math.PI / 180.0)
                            kOperator = -1;
                        return Math.Tan(incl - kOperator * cantAngle);
                    }
                    return 0.0;
                },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_AXY_MS(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("5aff380f-8562-4cca-be15-9327cb7db6df") },
                ErrorCode = ErrorCode.AXY_MS,
                Description = "Gyro 2-axis: accelerometer misalignment error",
                Index = 42,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = true,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AccelerationDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 1.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_AXY_GB(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("6ffccd89-4890-4087-b753-01ba3e67deb5") },
                ErrorCode = ErrorCode.AXY_GB,
                Description = "Gyro 2-axis: Gravity bias error",
                Index = 43,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = false,
                KOperatorImposed = true,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AccelerationDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl)
                        {
                            double gField = args.FirstOrDefault(p => p?.Key == ParameterType.GField)?.Value ?? SurveyInstrument.DEFAULT_GFIELD;
                            double cantAngle = args.FirstOrDefault(p => p?.Key == ParameterType.CantAngle)?.Value ?? SurveyInstrument.DEFAULT_CANT_ANGLE;
                            double kOperator = args.FirstOrDefault(p => p?.Key == ParameterType.KOperator)?.Value ?? 1;
                            if (incl > 90.0 * Math.PI / 180.0)
                                kOperator = -1;
                            return Math.Tan(incl - kOperator * cantAngle) / gField;
                        }
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_XYB1(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("322d747c-4f23-4336-b87a-230438e192cf") },
                ErrorCode = ErrorCode.GXYZ_XYB1,
                Description = "Gyro 3-axis: stationary: xy bias 1 error",
                Index = 44,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Sin(AzT) * Math.Cos(incl) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_XYB2(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("4206760f-dec8-48cd-ac2d-7f57a20448f3") },
                ErrorCode = ErrorCode.GXYZ_XYB2,
                Description = "Gyro 3-axis: stationary: xy bias 2 error",
                Index = 45,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Cos(AzT) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_XYRN(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("a4d3018e-871a-4469-8c02-b07fed7c25b7") },
                ErrorCode = ErrorCode.GXYZ_XYRN,
                Description = "Gyro 3-axis: stationary: xy random noise error",
                Index = 44,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Sqrt(1 - (Math.Sin(AzT) * Math.Sin(AzT) * Math.Sin(incl) * Math.Sin(incl))) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_XYG1(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("29a65a94-a0bf-463a-823e-93afb4a46f05") },
                ErrorCode = ErrorCode.GXYZ_XYG1,
                Description = "Gyro 3-axis: stationary: xy gyro g-dependent",
                Index = 47,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        endInclination is double)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Cos(AzT) * Math.Sin(incl) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_XYG2(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("48740348-045b-4f15-9a42-cfceefb50422") },
                ErrorCode = ErrorCode.GXYZ_XYG2,
                Description = "Gyro 3-axis: stationary: xy gyro g-dependent",
                Index = 48,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        endInclination is double)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Cos(AzT) * Math.Cos(incl) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_XYG3(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("59f6dd1b-152d-4bda-8da5-23ad4500e276") },
                ErrorCode = ErrorCode.GXYZ_XYG3,
                Description = "Gyro 3-axis: stationary: xy gyro g-dependent",
                Index = 49,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        endInclination is double)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Sin(AzT) * Math.Cos(incl) * Math.Cos(incl) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_XYG4(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("f7b15ceb-6c62-46e9-b3df-5c485d2d4ddf") },
                ErrorCode = ErrorCode.GXYZ_XYG4,
                Description = "Gyro 3-axis: stationary: xy gyro g-dependent",
                Index = 50,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        endInclination is double)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Sin(AzT) * Math.Sin(incl) * Math.Cos(incl) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_ZB(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("6aa7e81b-dba2-4be3-af60-b88428923124") },
                ErrorCode = ErrorCode.GXYZ_ZB,
                Description = "Gyro 3-axis: stationary: z gyro bias error",
                Index = 51,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        endInclination is double)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Sin(AzT) * Math.Sin(incl) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_ZRN(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("ddd11e67-7e87-4542-8da2-44c7b7b0b7df") },
                ErrorCode = ErrorCode.GXYZ_ZRN,
                Description = "Gyro 3-axis: stationary: z gyro random noise error",
                Index = 52,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        endInclination is double)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Sin(AzT) * Math.Sin(incl) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_ZG1(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("35ce9aea-81cb-42d6-9a52-1e2faefea478") },
                ErrorCode = ErrorCode.GXYZ_ZG1,
                Description = "Gyro 3-axis: stationary: z gyro g-dependent",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        endInclination is double)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Sin(AzT) * Math.Sin(incl) * Math.Sin(incl) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_ZG2(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("1889d9e1-c8ea-433e-b320-d2b08408492b") },
                ErrorCode = ErrorCode.GXYZ_ZG2,
                Description = "Gyro 3-axis: stationary: z gyro g-dependent",
                Index = 54,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        endInclination is double)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Sin(AzT) * Math.Sin(incl) * Math.Cos(incl) / (earthRotRate * Math.Cos(lat));
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_SF(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("2236974f-5345-487d-a84d-aecc64fbc621") },
                ErrorCode = ErrorCode.GXYZ_SF,
                Description = "Gyro 3-axis: stationary: z gyro scalefactor",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        endInclination is double)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return Math.Tan(lat) * Math.Sin(AzT) * Math.Sin(incl) * Math.Cos(incl);
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_MIS(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("aebc073a-35b9-4a60-92c3-e80628ea67aa") },
                ErrorCode = ErrorCode.GXYZ_MIS,
                Description = "Gyro 3-axis: stationary: gyro misalignment",
                Index = 56,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        endInclination is double)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        if (true || incl < endInclination)
                        {
                            double AzT = az + converg;
                            return 1 / Math.Cos(lat);
                        }
                        else
                        {
                            return 0.0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_B1(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("d86c0b1b-b6de-4fb4-b4ad-9c24998ba0fb") },
                ErrorCode = ErrorCode.GXY_B1,
                Description = "Gyro 2-axis: stationary: xy gyro bias",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        double AzT = az + converg;
                        return Math.Sin(AzT) / (earthRotRate * Math.Cos(lat) * Math.Cos(incl));
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_B2(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("2d846046-0809-4f31-bcf9-c07ea78a7a1e") },
                ErrorCode = ErrorCode.GXY_B2,
                Description = "Gyro 2-axis: stationary: xy gyro bias",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        double AzT = az + converg;
                        return Math.Cos(AzT) / (earthRotRate * Math.Cos(lat));
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_RN(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("ec425eb7-5991-4a54-8661-868b9271ebd8") },
                ErrorCode = ErrorCode.GXY_RN,
                Description = "Gyro 2-axis: stationary: xy gyro random noise",
                Index = 53,
                IsSystematic = false,
                IsRandom = true,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        double AzT = az + converg;
                        //return NoiseRedFactor * Math.Sqrt((1 - Math.Cos(AzT) * Math.Cos(AzT) * Math.Sin(incl) * Math.Sin(incl))) / (EarthRotRate * Math.Cos(Latitude) * Math.Cos(incl));
                        return Math.Sqrt((1 - Math.Cos(AzT) * Math.Cos(AzT) * Math.Sin(incl) * Math.Sin(incl))) / (earthRotRate * Math.Cos(lat) * Math.Cos(incl));
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_G1(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("439f2707-be81-4974-b8d6-7c121ecb7ca7") },
                ErrorCode = ErrorCode.GXY_G1,
                Description = "Gyro 2-axis: stationary: xy gyro g-dependent",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        double AzT = az + converg;
                        return Math.Cos(AzT) * Math.Sin(incl) / (earthRotRate * Math.Cos(lat));
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_G2(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("82d6ad3b-06da-4b12-98e0-a9d4eea2f47e") },
                ErrorCode = ErrorCode.GXY_G2,
                Description = "Gyro 2-axis: stationary: xy gyro g-dependent",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        double AzT = az + converg;
                        return Math.Cos(AzT) * Math.Cos(incl) / (earthRotRate * Math.Cos(lat));
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_G3(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("cd367ec4-7eb8-4108-8a94-298574d6aacd") },
                ErrorCode = ErrorCode.GXY_G3,
                Description = "Gyro 2-axis: stationary: xy gyro g-dependent",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        double AzT = az + converg;
                        return Math.Sin(AzT) / (earthRotRate * Math.Cos(lat));
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_G4(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("44592b9e-a62b-479a-8ac3-4fec7bcc5c71") },
                ErrorCode = ErrorCode.GXY_G4,
                Description = "Gyro 2-axis: stationary: xy gyro g-dependent",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az)
                    {
                        double converg = args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value ?? SurveyInstrument.DEFAULT_CONVERGENCE;
                        double lat = args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value ?? SurveyInstrument.DEFAULT_LATITUDE;
                        double earthRotRate = args.FirstOrDefault(p => p?.Key == ParameterType.EarthRotRate)?.Value ?? SurveyInstrument.DEFAULT_EARTH_ROT_RATE;
                        double AzT = az + converg;
                        return Math.Sin(AzT) * Math.Tan(incl) / (earthRotRate * Math.Cos(lat));
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_SF(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("e9b717ab-3208-4052-b616-efedb3e48c20") },
                ErrorCode = ErrorCode.GXY_SF,
                Description = "Gyro 2-axis: stationary: gyro scalefactor",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "ProportionSmall",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value is double converg &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value is double lat)
                    {
                        double AzT = az + converg;
                        return Math.Tan(lat) * Math.Sin(AzT) * Math.Tan(incl);
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_MIS(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("dc0c3d6c-49cd-4aef-9f8e-bbb8c6c340a0") },
                ErrorCode = ErrorCode.GXY_MIS,
                Description = "Gyro 2-axis: stationary:gyro misalignment",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Azimuth)?.Value is double az &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Convergence)?.Value is double converg &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Latitude)?.Value is double lat)
                    {
                        double AzT = az + converg;
                        return 1 / (Math.Cos(lat) * Math.Cos(incl));
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_EXT_REF(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("cb8948aa-b684-4d75-86f5-0a9f911d1ae1") },
                ErrorCode = ErrorCode.EXT_REF,
                Description = "Gyro external reference error",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 1.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_EXT_TIE(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("ea2a00b4-5d44-42fd-9314-5f0c287a2e82") },
                ErrorCode = ErrorCode.EXT_TIE,
                Description = "Gyro Un-modelled random azimuth error in tie-ontool",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    return 1.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_EXT_MIS(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("ef67ab18-41f3-4a8c-9859-f5daf5be5cc1") },
                ErrorCode = ErrorCode.EXT_MIS,
                Description = "Gyro Misalignment effect at tie-on",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = false,
                IsStationary = true,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "PlaneAngleDrilling",
                UseInclinationInterval = false,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.Magnitude)?.Value is double magnitude)
                    {
                        if (magnitude > 0) //NB!
                        {
                            return 1.0 / Math.Sin(incl);
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_GD(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("51e9dc26-3dcb-46ce-b36a-06579958eaf5") },
                ErrorCode = ErrorCode.GXYZ_GD,
                Description = "Gyro 3-axis, continuous: xyz gyro drift",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = true,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.h_gyroPrev)?.Value is double h_gyroPrev &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.DeltaMD)?.Value is double deltaD &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.c_gyro)?.Value is double c_gyro)
                    {
                        if (incl >= startInclination && incl < endInclination)
                        {
                            return h_gyroPrev + deltaD / c_gyro;
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXYZ_RW(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("c64e0fc1-486a-4945-ad0d-c23118bb9a94") },
                ErrorCode = ErrorCode.GXYZ_RW,
                Description = "Gyro 3-axis, continuous: xyz gyro random walk error",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = true,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "RandomWalkDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.h_gyroPrev)?.Value is double h_gyroPrev &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.DeltaMD)?.Value is double deltaD &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.c_gyro)?.Value is double c_gyro)
                    {
                        if (incl >= startInclination && incl < endInclination)
                        {
                            return Math.Sqrt((h_gyroPrev * h_gyroPrev) + deltaD / c_gyro);
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_GD(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("1b7d3e3d-4694-43a7-97ba-5952f396a1fb") },
                ErrorCode = ErrorCode.GXY_GD,
                Description = "Gyro 3-axis, continuous: xy gyro drift",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = true,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.h_gyroPrev)?.Value is double h_gyroPrev &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.DeltaMD)?.Value is double deltaD &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.c_gyro)?.Value is double c_gyro &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.InclinationPrev)?.Value is double inclPrev)
                    {
                        if (incl >= startInclination && incl < endInclination)
                        {
                            return h_gyroPrev + ((1 / Math.Sin((inclPrev + incl) / 2)) * deltaD / c_gyro);
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GXY_RW(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("f2527672-1226-45d0-ae3a-9b52591aaed8") },
                ErrorCode = ErrorCode.GXY_RW,
                Description = "Gyro 3-axis, continuous: xy gyro random walk",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = true,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "RandomWalkDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.h_gyroPrev)?.Value is double h_gyroPrev &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.DeltaMD)?.Value is double deltaD &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.c_gyro)?.Value is double c_gyro &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.InclinationPrev)?.Value is double inclPrev)
                    {
                        if (incl >= startInclination && incl < endInclination)
                        {
                            return Math.Sqrt((h_gyroPrev * h_gyroPrev) + ((1 / (Math.Sin((inclPrev + incl) / 2) * Math.Sin((inclPrev + incl) / 2))) * deltaD / c_gyro));
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GZ_GD(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("43bf2b62-1efa-4357-9fc6-c2e548c45cd7") },
                ErrorCode = ErrorCode.GZ_GD,
                Description = "Gyro z-axis, continuous: z gyro drift",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = true,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.h_gyroPrev)?.Value is double h_gyroPrev &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.DeltaMD)?.Value is double deltaD &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.c_gyro)?.Value is double c_gyro &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.InclinationPrev)?.Value is double inclPrev)
                    {
                        if (incl >= startInclination && incl < endInclination)
                        {
                            return h_gyroPrev + ((1 / Math.Cos((inclPrev + incl) / 2)) * deltaD / c_gyro);
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }

        public static ErrorSource Create_GZ_RW(double? startInclination = null, double? endInclination = null, double? initInclination = null, double? magnitude = null)
        {
            var src = new ErrorSource
            {
                MetaInfo = new MetaInfo() { HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/", HttpEndPoint = "ErrorSource/", ID = new Guid("9acbbbc5-d04f-41dd-a3f6-4293b2db3e14") },
                ErrorCode = ErrorCode.GZ_RW,
                Description = "Gyro z-axis, continuous: z gyro random walk",
                Index = 53,
                IsSystematic = true,
                IsRandom = false,
                IsGlobal = false,
                IsContinuous = true,
                IsStationary = false,
                KOperatorImposed = false,
                SingularIssues = false,
                Magnitude = magnitude,
                MagnitudeQuantity = "RandomWalkDrilling",
                UseInclinationInterval = true,
                StartInclination = startInclination,
                EndInclination = endInclination,
                InitInclination = initInclination,
                WeightingFunctionMD = args =>
                {
                    return 0.0;
                },
                WeightingFunctionIncl = args =>
                    {
                        return 0.0;
                    },
                WeightingFunctionAzim = args =>
                {
                    if (args.FirstOrDefault(p => p?.Key == ParameterType.Inclination)?.Value is double incl &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.h_gyroPrev)?.Value is double h_gyroPrev &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.DeltaMD)?.Value is double deltaD &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.c_gyro)?.Value is double c_gyro &&
                        args.FirstOrDefault(p => p?.Key == ParameterType.InclinationPrev)?.Value is double inclPrev)
                    {
                        if (incl >= startInclination && incl < endInclination)
                        {
                            return Math.Sqrt((h_gyroPrev * h_gyroPrev) + ((1 / (Math.Cos((inclPrev + incl) / 2) * Math.Cos((inclPrev + incl) / 2))) * deltaD / c_gyro));
                        }
                    }
                    return 0.0;
                },
                VerticalHoleWeightingFunctionNorth = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionEast = args =>
                {
                    return 0.0;
                },
                VerticalHoleWeightingFunctionVertical = args =>
                {
                    return 0.0;
                },
            };
            return src;
        }
    }
}
