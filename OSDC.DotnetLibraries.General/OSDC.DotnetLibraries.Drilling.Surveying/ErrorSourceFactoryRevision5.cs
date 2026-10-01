using OSDC.DotnetLibraries.General.DataManagement;
using static OSDC.DotnetLibraries.Drilling.Surveying.ErrorSource;

namespace OSDC.DotnetLibraries.Drilling.Surveying;

public static partial class ErrorSourceFactory
{
    public static ErrorSource Create_ABXY_TI1(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.ABXY_TI1, "Accelerometer bias term 1", "AccelerationDrilling", mode, false, magnitude,
            a => -Math.Cos(I(a)) / G(a),
            a => Math.Tan(Dip(a)) * Math.Cos(I(a)) * Math.Sin(Am(a)) / G(a));

    public static ErrorSource Create_ABXY_TI2(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.ABXY_TI2, "Accelerometer bias term 2", "AccelerationDrilling", mode, true, magnitude,
            _ => 0.0,
            a => (Cot(I(a)) - Math.Tan(Dip(a)) * Math.Cos(Am(a))) / G(a));

    public static ErrorSource Create_ASXY_TI1(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.ASXY_TI1, "Accelerometer scale factor term 1", "ProportionSmall", ErrorPropagationMode.Systematic, false, magnitude,
            a => Math.Sin(I(a)) * Math.Cos(I(a)) / Math.Sqrt(2.0),
            a => -Math.Tan(Dip(a)) * Math.Sin(I(a)) * Math.Cos(I(a)) * Math.Sin(Am(a)) / Math.Sqrt(2.0));

    public static ErrorSource Create_ASXY_TI2(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.ASXY_TI2, "Accelerometer scale factor term 2", "ProportionSmall", mode, false, magnitude,
            a => Math.Sin(I(a)) * Math.Cos(I(a)) / 2.0,
            a => -Math.Tan(Dip(a)) * Math.Sin(I(a)) * Math.Cos(I(a)) * Math.Sin(Am(a)) / 2.0);

    public static ErrorSource Create_ASXY_TI3(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.ASXY_TI3, "Accelerometer scale factor term 3", "ProportionSmall", mode, false, magnitude,
            _ => 0.0,
            a => (Math.Tan(Dip(a)) * Math.Sin(I(a)) * Math.Cos(Am(a)) - Math.Cos(I(a))) / 2.0);

    public static ErrorSource Create_ABIXY_TI1(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.ABIXY_TI1, "Accelerometer bias with axial-interference correction term 1", "AccelerationDrilling", mode, false, magnitude,
            a => -Math.Cos(I(a)) / G(a),
            a => Math.Pow(Math.Cos(I(a)), 2) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / (G(a) * Den(a)));

    public static ErrorSource Create_ABIXY_TI2(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.ABIXY_TI2, "Accelerometer bias with axial-interference correction term 2", "AccelerationDrilling", mode, true, magnitude,
            _ => 0.0,
            a => -(Math.Tan(Dip(a)) * Math.Cos(Am(a)) - Cot(I(a))) / (G(a) * Den(a)));

    public static ErrorSource Create_ABIZ(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.ABIZ, "Accelerometer z bias with axial-interference correction", "AccelerationDrilling", ErrorPropagationMode.Systematic, false, magnitude,
            a => -Math.Sin(I(a)) / G(a),
            a => Math.Sin(I(a)) * Math.Cos(I(a)) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / (G(a) * Den(a)));

    public static ErrorSource Create_ASIXY_TI1(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.ASIXY_TI1, "Accelerometer scale factor with axial-interference correction term 1", "ProportionSmall", ErrorPropagationMode.Systematic, false, magnitude,
            a => Math.Sin(I(a)) * Math.Cos(I(a)) / Math.Sqrt(2.0),
            a => -Math.Sin(I(a)) * Math.Pow(Math.Cos(I(a)), 2) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / (Math.Sqrt(2.0) * Den(a)));

    public static ErrorSource Create_ASIXY_TI2(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.ASIXY_TI2, "Accelerometer scale factor with axial-interference correction term 2", "ProportionSmall", mode, false, magnitude,
            a => Math.Sin(I(a)) * Math.Cos(I(a)) / 2.0,
            a => -Math.Sin(I(a)) * Math.Pow(Math.Cos(I(a)), 2) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / (2.0 * Den(a)));

    public static ErrorSource Create_ASIXY_TI3(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.ASIXY_TI3, "Accelerometer scale factor with axial-interference correction term 3", "ProportionSmall", mode, false, magnitude,
            _ => 0.0,
            a => (Math.Tan(Dip(a)) * Math.Sin(I(a)) * Math.Cos(Am(a)) - Math.Cos(I(a))) / (2.0 * Den(a)));

    public static ErrorSource Create_ASIXY_TI1S(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.ASIXY_TI1S, "MWD TF Ind: X and Y accelerometer scale factor with Z-axis correction, term 1", "ProportionSmall", ErrorPropagationMode.Systematic, false, magnitude,
            a => Math.Sin(I(a)) * Math.Cos(I(a)) / Math.Sqrt(2.0),
            a => -Math.Sin(I(a)) * Math.Pow(Math.Cos(I(a)), 2) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / (Math.Sqrt(2.0) * Den(a)));

    public static ErrorSource Create_ASIXY_TI2S(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.ASIXY_TI2S, "MWD TF Ind: X and Y accelerometer scale factor with Z-axis correction, term 2", "ProportionSmall", ErrorPropagationMode.Systematic, false, magnitude,
            a => Math.Sin(I(a)) * Math.Cos(I(a)) / 2.0,
            a => -Math.Sin(I(a)) * Math.Pow(Math.Cos(I(a)), 2) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / (2.0 * Den(a)));

    public static ErrorSource Create_ASIXY_TI3S(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.ASIXY_TI3S, "MWD TF Ind: X and Y accelerometer scale factor with Z-axis correction, term 3", "ProportionSmall", ErrorPropagationMode.Systematic, false, magnitude,
            _ => 0.0,
            a => (Math.Tan(Dip(a)) * Math.Sin(I(a)) * Math.Cos(Am(a)) - Math.Cos(I(a))) / (2.0 * Den(a)));

    public static ErrorSource Create_ASIZ(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.ASIZ, "Accelerometer z scale factor with axial-interference correction", "ProportionSmall", ErrorPropagationMode.Systematic, false, magnitude,
            a => -Math.Sin(I(a)) * Math.Cos(I(a)),
            a => Math.Sin(I(a)) * Math.Pow(Math.Cos(I(a)), 2) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / Den(a));

    public static ErrorSource Create_MBIXY_TI1(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.MBIXY_TI1, "Magnetometer bias with axial-interference correction term 1", "EarthMagneticFluxDensity", mode, false, magnitude,
            _ => 0.0,
            a => -Math.Cos(I(a)) * Math.Sin(Am(a)) / (B(a) * Math.Cos(Dip(a)) * Den(a)));

    public static ErrorSource Create_MBIXY_TI2(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.MBIXY_TI2, "Magnetometer bias with axial-interference correction term 2", "EarthMagneticFluxDensity", mode, false, magnitude,
            _ => 0.0,
            a => Math.Cos(Am(a)) / (B(a) * Math.Cos(Dip(a)) * Den(a)));

    public static ErrorSource Create_MBIXY_TI1S(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.MBIXY_TI1S, "MWD TF Ind: X and Y magnetometer bias with Z-axis correction, term 1", "EarthMagneticFluxDensity", ErrorPropagationMode.Systematic, false, magnitude,
            _ => 0.0,
            a => -Math.Cos(I(a)) * Math.Sin(Am(a)) / (B(a) * Math.Cos(Dip(a)) * Den(a)));

    public static ErrorSource Create_MBIXY_TI2S(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.MBIXY_TI2S, "MWD TF Ind: X and Y magnetometer bias with Z-axis correction, term 2", "EarthMagneticFluxDensity", ErrorPropagationMode.Systematic, false, magnitude,
            _ => 0.0,
            a => Math.Cos(Am(a)) / (B(a) * Math.Cos(Dip(a)) * Den(a)));

    public static ErrorSource Create_MSIXY_TI1(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.MSIXY_TI1, "Magnetometer scale factor with axial-interference correction term 1", "ProportionSmall", ErrorPropagationMode.Systematic, false, magnitude,
            _ => 0.0,
            a => Math.Sin(I(a)) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / (Math.Sqrt(2.0) * Den(a)));

    public static ErrorSource Create_MSIXY_TI2(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.MSIXY_TI2, "Magnetometer scale factor with axial-interference correction term 2", "ProportionSmall", mode, false, magnitude,
            _ => 0.0,
            a => Math.Sin(Am(a)) * (Math.Tan(Dip(a)) * Math.Sin(I(a)) * Math.Cos(I(a)) -
                 Math.Pow(Math.Cos(I(a)), 2) * Math.Cos(Am(a)) - Math.Cos(Am(a))) / (2.0 * Den(a)));

    public static ErrorSource Create_MSIXY_TI3(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Systematic) =>
        CreateRevision5Mwd(ErrorCode.MSIXY_TI3, "Magnetometer scale factor with axial-interference correction term 3", "ProportionSmall", mode, false, magnitude,
            _ => 0.0,
            a => (Math.Cos(I(a)) * Math.Pow(Math.Cos(Am(a)), 2) - Math.Cos(I(a)) * Math.Pow(Math.Sin(Am(a)), 2) -
                 Math.Tan(Dip(a)) * Math.Sin(I(a)) * Math.Cos(Am(a))) / (2.0 * Den(a)));

    public static ErrorSource Create_MSIXY_TI1S(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.MSIXY_TI1S, "MWD TF Ind: X and Y magnetometer scale factor with Z-axis correction, term 1", "ProportionSmall", ErrorPropagationMode.Systematic, false, magnitude,
            _ => 0.0,
            a => Math.Sin(I(a)) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / (Math.Sqrt(2.0) * Den(a)));

    public static ErrorSource Create_MSIXY_TI2S(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.MSIXY_TI2S, "MWD TF Ind: X and Y magnetometer scale factor with Z-axis correction, term 2", "ProportionSmall", ErrorPropagationMode.Systematic, false, magnitude,
            _ => 0.0,
            a => Math.Sin(Am(a)) * (Math.Tan(Dip(a)) * Math.Sin(I(a)) * Math.Cos(I(a)) -
                 Math.Pow(Math.Cos(I(a)), 2) * Math.Cos(Am(a)) - Math.Cos(Am(a))) / (2.0 * Den(a)));

    public static ErrorSource Create_MSIXY_TI3S(double? magnitude = null) =>
        CreateRevision5Mwd(ErrorCode.MSIXY_TI3S, "MWD TF Ind: X and Y magnetometer scale factor with Z-axis correction, term 3", "ProportionSmall", ErrorPropagationMode.Systematic, false, magnitude,
            _ => 0.0,
            a => (Math.Cos(I(a)) * Math.Pow(Math.Cos(Am(a)), 2) - Math.Cos(I(a)) * Math.Pow(Math.Sin(Am(a)), 2) -
                 Math.Tan(Dip(a)) * Math.Sin(I(a)) * Math.Cos(Am(a))) / (2.0 * Den(a)));

    public static ErrorSource Create_DEC(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Global) =>
        CreateRevision5Mwd(ErrorCode.DEC, "Constant declination error", "PlaneAngleDrilling", mode, false, magnitude, _ => 0.0, _ => 1.0);

    public static ErrorSource Create_DBH(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Global) =>
        CreateRevision5Mwd(ErrorCode.DBH, "Declination error dependent on horizontal magnetic field", "AngleMagneticFluxDensitySurveyInstrumentDrilling", mode, false, magnitude,
            _ => 0.0, a => 1.0 / (B(a) * Math.Cos(Dip(a))));

    public static ErrorSource Create_MFI(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Global) =>
        CreateRevision5Mwd(ErrorCode.MFI, "Total magnetic-field error with axial-interference correction", "EarthMagneticFluxDensity", mode, false, magnitude,
            _ => 0.0,
            a => -Math.Sin(I(a)) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / (B(a) * Den(a)));

    public static ErrorSource Create_MDI(double? magnitude = null, ErrorPropagationMode mode = ErrorPropagationMode.Global) =>
        CreateMdi(ErrorCode.MDI, "Dip-angle error with axial-interference correction", magnitude, mode);

    public static ErrorSource Create_MDIR(double? magnitude = null) =>
        CreateMdi(ErrorCode.MDIR, "MWD: magnetic dip with Z-axis correction - random", magnitude, ErrorPropagationMode.Random);

    public static ErrorSource Create_MFIR(double? magnitude = null) =>
        CreateMfi(ErrorCode.MFIR, "MWD: total magnetic field with Z-axis correction - random", magnitude, ErrorPropagationMode.Random);

    public static ErrorSource Create_MFI_U(double? magnitude = null) =>
        CreateMfi(ErrorCode.MFI_U, "MWD: total magnetic field with Z-axis correction - uncorrelated errors", magnitude, ErrorPropagationMode.WellByWell);

    public static ErrorSource Create_MFI_OS(double? magnitude = null) =>
        CreateMfi(ErrorCode.MFI_OS, "MWD: total magnetic field with Z-axis correction - crustal omission standard models", magnitude, ErrorPropagationMode.Global);

    public static ErrorSource Create_MFI_OH(double? magnitude = null) =>
        CreateMfi(ErrorCode.MFI_OH, "MWD: total magnetic field with Z-axis correction - crustal omission HD models", magnitude, ErrorPropagationMode.Global);

    public static ErrorSource Create_MFI_OI(double? magnitude = null) =>
        CreateMfi(ErrorCode.MFI_OI, "MWD: total magnetic field with Z-axis correction - crustal omission IFR models", magnitude, ErrorPropagationMode.Global);

    public static ErrorSource Create_MDI_U(double? magnitude = null) =>
        CreateMdi(ErrorCode.MDI_U, "MWD: magnetic dip with Z-axis correction - uncorrelated errors", magnitude, ErrorPropagationMode.WellByWell);

    public static ErrorSource Create_MDI_OS(double? magnitude = null) =>
        CreateMdi(ErrorCode.MDI_OS, "MWD: magnetic dip with Z-axis correction - crustal omission standard models", magnitude, ErrorPropagationMode.Global);

    public static ErrorSource Create_MDI_OH(double? magnitude = null) =>
        CreateMdi(ErrorCode.MDI_OH, "MWD: magnetic dip with Z-axis correction - crustal omission HD models", magnitude, ErrorPropagationMode.Global);

    public static ErrorSource Create_MDI_OI(double? magnitude = null) =>
        CreateMdi(ErrorCode.MDI_OI, "MWD: magnetic dip with Z-axis correction - crustal omission IFR models", magnitude, ErrorPropagationMode.Global);

    private static ErrorSource CreateMfi(ErrorCode code, string description, double? magnitude, ErrorPropagationMode mode) =>
        CreateRevision5Mwd(code, description, "EarthMagneticFluxDensity", mode, false, magnitude,
            _ => 0.0,
            a => -Math.Sin(I(a)) * Math.Sin(Am(a)) *
                 (Math.Tan(Dip(a)) * Math.Cos(I(a)) + Math.Sin(I(a)) * Math.Cos(Am(a))) / (B(a) * Den(a)));

    private static ErrorSource CreateMdi(ErrorCode code, string description, double? magnitude, ErrorPropagationMode mode) =>
        CreateRevision5Mwd(code, description, "PlaneAngleDrilling", mode, false, magnitude,
            _ => 0.0,
            a => -Math.Sin(I(a)) * Math.Sin(Am(a)) *
                 (Math.Cos(I(a)) - Math.Tan(Dip(a)) * Math.Sin(I(a)) * Math.Cos(Am(a))) / Den(a));

    private static ErrorSource CreateRevision5Mwd(
        ErrorCode code, string description, string quantity, ErrorPropagationMode mode, bool singular, double? magnitude,
        Func<KeyValuePair<ParameterType, double>?[], double?> inclination,
        Func<KeyValuePair<ParameterType, double>?[], double?> azimuth,
        Func<KeyValuePair<ParameterType, double>?[], double?>? verticalNorth = null,
        Func<KeyValuePair<ParameterType, double>?[], double?>? verticalEast = null,
        Func<KeyValuePair<ParameterType, double>?[], double?>? vertical = null) => new ErrorSource
    {
        MetaInfo = new MetaInfo
        {
            HttpHostName = "https://app.digiwells.no/", HttpHostBasePath = "SurveyInstrument/api/",
            HttpEndPoint = "ErrorSource/", ID = Revision5Id(code)
        },
        ErrorCode = code,
        Description = description,
        Index = 100 + (int)code,
        PropagationMode = mode,
        IsRandom = mode == ErrorPropagationMode.Random,
        IsSystematic = mode is ErrorPropagationMode.Systematic or ErrorPropagationMode.WellByWell,
        IsGlobal = mode == ErrorPropagationMode.Global,
        SingularIssues = singular,
        Magnitude = magnitude,
        MagnitudeQuantity = quantity,
        WeightingFunctionMD = _ => 0.0,
        WeightingFunctionIncl = inclination,
        WeightingFunctionAzim = azimuth,
        VerticalHoleWeightingFunctionNorth = verticalNorth ?? (_ => 0.0),
        VerticalHoleWeightingFunctionEast = verticalEast ?? (_ => 0.0),
        VerticalHoleWeightingFunctionVertical = vertical ?? (_ => 0.0)
    };

    private static Guid Revision5Id(ErrorCode code)
    {
        byte[] bytes = new byte[16];
        BitConverter.GetBytes(0x5A130000 + (int)code).CopyTo(bytes, 0);
        BitConverter.GetBytes(0x4953435753410000L + (int)code).CopyTo(bytes, 8);
        return new Guid(bytes);
    }

    private static double I(KeyValuePair<ParameterType, double>?[] a) => Value(a, ParameterType.Inclination);
    private static double Dip(KeyValuePair<ParameterType, double>?[] a) => OptionalValue(a, ParameterType.Dip, SurveyInstrument.DEFAULT_DIP);
    private static double G(KeyValuePair<ParameterType, double>?[] a) => OptionalValue(a, ParameterType.GField, SurveyInstrument.DEFAULT_GFIELD);
    private static double B(KeyValuePair<ParameterType, double>?[] a) => OptionalValue(a, ParameterType.BField, SurveyInstrument.DEFAULT_BFIELD);
    private static double Am(KeyValuePair<ParameterType, double>?[] a) =>
        Value(a, ParameterType.Azimuth) - OptionalValue(a, ParameterType.Declination, SurveyInstrument.DEFAULT_DECLINATION);
    private static double Den(KeyValuePair<ParameterType, double>?[] a) =>
        1.0 - Math.Pow(Math.Sin(I(a)), 2) * Math.Pow(Math.Sin(Am(a)), 2);
    private static double Cot(double value) => Math.Cos(value) / Math.Sin(value);
}
