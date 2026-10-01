using NUnit.Framework;
using static OSDC.DotnetLibraries.Drilling.Surveying.ErrorSource;

namespace OSDC.DotnetLibraries.Drilling.Surveying.UnitTest;

public class IscwsaRevision5Tests
{
    private const double DegreesToRadians = Math.PI / 180.0;

    [Test]
    public void AmilUsesFluxDensityAndHistoricAmidIsRejectedForRevision5()
    {
        ErrorSource amil = ErrorSourceFactory.Create_AMIL(220e-9);
        Assert.That(amil.ErrorCode, Is.EqualTo(ErrorCode.AMIL));
        Assert.That(amil.MagnitudeQuantity, Is.EqualTo("EarthMagneticFluxDensity"));
        Assert.That(amil.PropagationMode, Is.EqualTo(ErrorPropagationMode.Systematic));
        Assert.That(ErrorSourceRevision5.TryValidate(amil, true, out _), Is.True);

        ErrorSource amid = ErrorSourceFactory.Create_AMID(magnitude: 0.1);
        Assert.That(amid.MagnitudeQuantity, Is.EqualTo("PlaneAngleDrilling"));
        Assert.That(ErrorSourceRevision5.TryValidate(amid, false, out _), Is.True);
        Assert.That(ErrorSourceRevision5.TryValidate(amid, true, out string? error), Is.False);
        Assert.That(error, Does.Contain("historic"));
    }

    [Test]
    public void Revision5PropagationIsOneClosedModeAndSupportsWellByWell()
    {
        ErrorSource source = ErrorSourceFactory.Create_MFI_U(61.15e-9);
        Assert.That(source.EffectivePropagationMode, Is.EqualTo(ErrorPropagationMode.WellByWell));
        Assert.That(ErrorSourceRevision5.TryValidate(source, true, out _), Is.True);

        ErrorSource contradictoryLegacy = new()
        {
            ErrorCode = ErrorCode.XYM1,
            IsRandom = true,
            IsSystematic = true,
            Magnitude = 0.01,
            MagnitudeQuantity = "PlaneAngleDrilling"
        };
        Assert.That(ErrorSourceRevision5.TryValidate(contradictoryLegacy, true, out _), Is.False);

        ErrorSource contradictoryExplicit = new()
        {
            ErrorCode = ErrorCode.ABXY_TI1S,
            PropagationMode = ErrorPropagationMode.Systematic,
            IsRandom = true,
            Magnitude = 0.004,
            MagnitudeQuantity = "AccelerationDrilling"
        };
        Assert.That(ErrorSourceRevision5.TryValidate(contradictoryExplicit, true, out _), Is.False);
    }

    [Test]
    public void CurrentAxialInterferenceWeightingMatchesRevision5Equation()
    {
        ErrorSource source = ErrorSourceFactory.Create_AMIL(220e-9);
        const double inclination = 0.7;
        const double azimuth = 1.1;
        const double declination = 0.1;
        const double dip = 0.9;
        const double field = 48e-6;
        KeyValuePair<ParameterType, double>?[] args =
        [
            new(ParameterType.Inclination, inclination),
            new(ParameterType.Azimuth, azimuth),
            new(ParameterType.Declination, declination),
            new(ParameterType.Dip, dip),
            new(ParameterType.BField, field)
        ];
        double expected = Math.Sin(inclination) * Math.Sin(azimuth - declination) /
                          (field * Math.Cos(dip));
        Assert.That(source.WeightingFunctionAzim!(args), Is.EqualTo(expected).Within(1e-12));
    }

    [Test]
    public void ExactAxialRevision5CodesHaveOfficialQuantitiesAndPropagationModes()
    {
        (ErrorSource Source, string Quantity, ErrorPropagationMode Mode)[] sources =
        [
            (ErrorSourceFactory.Create_ABIXY_TI1S(), "AccelerationDrilling", ErrorPropagationMode.Systematic),
            (ErrorSourceFactory.Create_ABIXY_TI2S(), "AccelerationDrilling", ErrorPropagationMode.Systematic),
            (ErrorSourceFactory.Create_ASIXY_TI1S(), "ProportionSmall", ErrorPropagationMode.Systematic),
            (ErrorSourceFactory.Create_ASIXY_TI2S(), "ProportionSmall", ErrorPropagationMode.Systematic),
            (ErrorSourceFactory.Create_ASIXY_TI3S(), "ProportionSmall", ErrorPropagationMode.Systematic),
            (ErrorSourceFactory.Create_MBIXY_TI1S(), "EarthMagneticFluxDensity", ErrorPropagationMode.Systematic),
            (ErrorSourceFactory.Create_MBIXY_TI2S(), "EarthMagneticFluxDensity", ErrorPropagationMode.Systematic),
            (ErrorSourceFactory.Create_MSIXY_TI1S(), "ProportionSmall", ErrorPropagationMode.Systematic),
            (ErrorSourceFactory.Create_MSIXY_TI2S(), "ProportionSmall", ErrorPropagationMode.Systematic),
            (ErrorSourceFactory.Create_MSIXY_TI3S(), "ProportionSmall", ErrorPropagationMode.Systematic),
            (ErrorSourceFactory.Create_MDIR(), "PlaneAngleDrilling", ErrorPropagationMode.Random),
            (ErrorSourceFactory.Create_MFIR(), "EarthMagneticFluxDensity", ErrorPropagationMode.Random),
            (ErrorSourceFactory.Create_MFI_U(), "EarthMagneticFluxDensity", ErrorPropagationMode.WellByWell),
            (ErrorSourceFactory.Create_MFI_OS(), "EarthMagneticFluxDensity", ErrorPropagationMode.Global),
            (ErrorSourceFactory.Create_MFI_OH(), "EarthMagneticFluxDensity", ErrorPropagationMode.Global),
            (ErrorSourceFactory.Create_MFI_OI(), "EarthMagneticFluxDensity", ErrorPropagationMode.Global),
            (ErrorSourceFactory.Create_MDI_U(), "PlaneAngleDrilling", ErrorPropagationMode.WellByWell),
            (ErrorSourceFactory.Create_MDI_OS(), "PlaneAngleDrilling", ErrorPropagationMode.Global),
            (ErrorSourceFactory.Create_MDI_OH(), "PlaneAngleDrilling", ErrorPropagationMode.Global),
            (ErrorSourceFactory.Create_MDI_OI(), "PlaneAngleDrilling", ErrorPropagationMode.Global)
        ];

        Assert.That(sources.Select(item => item.Source.ErrorCode), Is.Unique);
        foreach ((ErrorSource source, string quantity, ErrorPropagationMode mode) in sources)
        {
            Assert.Multiple(() =>
            {
                Assert.That(source.MagnitudeQuantity, Is.EqualTo(quantity), source.ErrorCode.ToString());
                Assert.That(source.PropagationMode, Is.EqualTo(mode), source.ErrorCode.ToString());
                Assert.That(ErrorSourceRevision5.TryValidate(source, true, out _), Is.True, source.ErrorCode.ToString());
            });
        }

        Assert.That(ErrorSourceRevision5.TryValidate(ErrorSourceFactory.Create_MFI(), true, out _), Is.False);
        Assert.That(ErrorSourceRevision5.TryValidate(ErrorSourceFactory.Create_MDI(), true, out _), Is.False);
    }

    [Test]
    public void AxialCorrectionWeightingFunctionsMatchToolgroupRevision5Equations()
    {
        const double inclination = 0.7;
        const double azimuth = 1.1;
        const double declination = 0.1;
        const double dip = 0.9;
        const double magneticField = 48e-6;
        const double gravity = 9.80665;
        double magneticAzimuth = azimuth - declination;
        double denominator = 1.0 - Math.Pow(Math.Sin(inclination), 2) * Math.Pow(Math.Sin(magneticAzimuth), 2);
        KeyValuePair<ParameterType, double>?[] args =
        [
            new(ParameterType.Inclination, inclination),
            new(ParameterType.Azimuth, azimuth),
            new(ParameterType.Declination, declination),
            new(ParameterType.Dip, dip),
            new(ParameterType.BField, magneticField),
            new(ParameterType.GField, gravity)
        ];

        ErrorSource abi1 = ErrorSourceFactory.Create_ABIXY_TI1S();
        double abi1Inclination = -Math.Cos(inclination) / gravity;
        double abi1Azimuth = Math.Pow(Math.Cos(inclination), 2) * Math.Sin(magneticAzimuth) *
            (Math.Tan(dip) * Math.Cos(inclination) + Math.Sin(inclination) * Math.Cos(magneticAzimuth)) /
            (gravity * denominator);

        ErrorSource mfi = ErrorSourceFactory.Create_MFI_U();
        double mfiAzimuth = -Math.Sin(inclination) * Math.Sin(magneticAzimuth) *
            (Math.Tan(dip) * Math.Cos(inclination) + Math.Sin(inclination) * Math.Cos(magneticAzimuth)) /
            (magneticField * denominator);

        ErrorSource mdi = ErrorSourceFactory.Create_MDI_U();
        double mdiAzimuth = -Math.Sin(inclination) * Math.Sin(magneticAzimuth) *
            (Math.Cos(inclination) - Math.Tan(dip) * Math.Sin(inclination) * Math.Cos(magneticAzimuth)) /
            denominator;

        Assert.Multiple(() =>
        {
            Assert.That(abi1.WeightingFunctionIncl!(args), Is.EqualTo(abi1Inclination).Within(1e-12));
            Assert.That(abi1.WeightingFunctionAzim!(args), Is.EqualTo(abi1Azimuth).Within(1e-12));
            Assert.That(mfi.WeightingFunctionAzim!(args), Is.EqualTo(mfiAzimuth).Within(1e-9));
            Assert.That(mdi.WeightingFunctionAzim!(args), Is.EqualTo(mdiAzimuth).Within(1e-12));
        });

        ErrorSource abi2 = ErrorSourceFactory.Create_ABIXY_TI2S();
        Assert.Multiple(() =>
        {
            Assert.That(abi2.VerticalHoleWeightingFunctionNorth!(args), Is.EqualTo(-Math.Sin(azimuth) / gravity).Within(1e-12));
            Assert.That(abi2.VerticalHoleWeightingFunctionEast!(args), Is.EqualTo(Math.Cos(azimuth) / gravity).Within(1e-12));
        });
    }

    [Test]
    public void InvalidMagnitudeQuantityAndInclinationIntervalAreRejected()
    {
        ErrorSource wrongQuantity = new()
        {
            ErrorCode = ErrorCode.AMIL,
            PropagationMode = ErrorPropagationMode.Systematic,
            Magnitude = 1.0,
            MagnitudeQuantity = "MagneticFlux"
        };
        Assert.That(ErrorSourceRevision5.TryValidate(wrongQuantity, true, out _), Is.False);

        ErrorSource invalidInterval = new()
        {
            ErrorCode = ErrorCode.GXYZ_GD,
            PropagationMode = ErrorPropagationMode.Systematic,
            Magnitude = 1e-6,
            MagnitudeQuantity = "AngularVelocitySurveyInstrumentDrilling",
            UseInclinationInterval = true,
            StartInclination = 1.0,
            EndInclination = 0.5
        };
        Assert.That(ErrorSourceRevision5.TryValidate(invalidInterval, true, out _), Is.False);
    }

    [Test]
    public void MwdRev5MatchesIscwsaDiagnosticWellOneAtFinalStation()
    {
        // ISCWSA Error Model Diagnostics Rev5.1, ISCWSA#1, published at
        // https://www.iscwsa.net/committees/error-model/ (download 806).
        SurveyInstrument tool = CreateOfficialMwdRev5Tool();
        List<SurveyStation> stations = CreateIscwsaWellOne(tool);

        Assert.That(CovarianceCalculatorISCWSA.Calculate(stations), Is.True);
        Assert.That(stations[0].Covariance![2, 2], Is.EqualTo(0.1225).Within(1e-12),
            "ISCWSA#1 starts with the 0.35 m random depth-reference variance");

        var covariance = stations[^1].Covariance!;
        Assert.Multiple(() =>
        {
            AssertIscwsaDiagnosticValue(covariance[0, 0], 8518.6721, "NN");
            AssertIscwsaDiagnosticValue(covariance[1, 1], 711.2004, "EE");
            AssertIscwsaDiagnosticValue(covariance[2, 2], 513.2943, "VV");
            AssertIscwsaDiagnosticValue(covariance[0, 1], -2244.3268, "NE");
            AssertIscwsaDiagnosticValue(covariance[0, 2], -58.6686, "NV");
            AssertIscwsaDiagnosticValue(covariance[1, 2], -146.0403, "EV");
        });
    }

    private static void AssertIscwsaDiagnosticValue(double? actual, double expected, string component)
    {
        // The diagnostic file reports four decimals and OSDC uses its established minimum-curvature
        // geometry implementation. A one-percent envelope still detects unit, propagation-mode and
        // missing-covariance defects while allowing the documented interpolation-method difference.
        Assert.That(actual, Is.EqualTo(expected).Within(Math.Max(0.1, Math.Abs(expected) * 0.01)), component);
    }

    private static SurveyInstrument CreateOfficialMwdRev5Tool() => new()
    {
        Name = "ISCWSA MWD (Fixed Rig) Rev5",
        ModelType = SurveyInstrumentModelType.MWD_ISCWSA,
        BField = 50_000e-9,
        Dip = 72 * DegreesToRadians,
        Declination = -4 * DegreesToRadians,
        Gravity = 9.80665,
        ErrorSourceList =
        [
            ErrorSourceFactory.Create_DRFR(magnitude: 0.35),
            ErrorSourceFactory.Create_DSFS(magnitude: 0.00056),
            ErrorSourceFactory.Create_DSTG(magnitude: 2.5e-7),
            ErrorSourceFactory.Create_ABXY_TI1S(magnitude: 0.004),
            ErrorSourceFactory.Create_ABXY_TI2S(magnitude: 0.004),
            ErrorSourceFactory.Create_ABZ(magnitude: 0.004),
            ErrorSourceFactory.Create_ASXY_TI1S(magnitude: 0.0005),
            ErrorSourceFactory.Create_ASXY_TI2S(magnitude: 0.0005),
            ErrorSourceFactory.Create_ASXY_TI3S(magnitude: 0.0005),
            ErrorSourceFactory.Create_ASZ(magnitude: 0.0005),
            ErrorSourceFactory.Create_MBXY_TI1(magnitude: 70e-9),
            ErrorSourceFactory.Create_MBXY_TI2(magnitude: 70e-9),
            ErrorSourceFactory.Create_MBZ(magnitude: 70e-9),
            ErrorSourceFactory.Create_MSXY_TI1(magnitude: 0.0016),
            ErrorSourceFactory.Create_MSXY_TI2(magnitude: 0.0016),
            ErrorSourceFactory.Create_MSXY_TI3(magnitude: 0.0016),
            ErrorSourceFactory.Create_MSZ(magnitude: 0.0016),
            ErrorSourceFactory.Create_DECR(magnitude: 0.1 * DegreesToRadians),
            ErrorSourceFactory.Create_DBHR(magnitude: 3000e-9 * DegreesToRadians),
            ErrorSourceFactory.Create_AMIL(magnitude: 220e-9),
            ErrorSourceFactory.Create_XYM1(magnitude: 0.1 * DegreesToRadians),
            ErrorSourceFactory.Create_XYM2(magnitude: 0.1 * DegreesToRadians),
            ErrorSourceFactory.Create_XCLA(magnitude: 0.167),
            ErrorSourceFactory.Create_XCLH(magnitude: 0.167),
            ErrorSourceFactory.Create_DEC_U(magnitude: 0.16 * DegreesToRadians),
            ErrorSourceFactory.Create_DEC_OS(magnitude: 0.24 * DegreesToRadians),
            ErrorSourceFactory.Create_DBH_U(magnitude: 2350.33e-9 * DegreesToRadians),
            ErrorSourceFactory.Create_DBH_OS(magnitude: 3359.1e-9 * DegreesToRadians),
            ErrorSourceFactory.Create_SAGE(magnitude: 0.2 * DegreesToRadians),
            ErrorSourceFactory.Create_XYM3E(magnitude: 0.3 * DegreesToRadians),
            ErrorSourceFactory.Create_XYM4E(magnitude: 0.3 * DegreesToRadians),
            ErrorSourceFactory.Create_DEC_OH(magnitude: 0.2 * DegreesToRadians),
            ErrorSourceFactory.Create_DEC_OI(magnitude: 0.05 * DegreesToRadians),
            ErrorSourceFactory.Create_DBH_OH(magnitude: 2839.77e-9 * DegreesToRadians),
            ErrorSourceFactory.Create_DBH_OI(magnitude: 356e-9 * DegreesToRadians)
        ]
    };

    private static List<SurveyStation> CreateIscwsaWellOne(SurveyInstrument tool)
    {
        List<SurveyStation> stations = [];
        for (int index = 0; index <= 267; index++)
        {
            double md = index == 267 ? 8000 : index * 30.0;
            double inclination = index switch
            {
                <= 40 => 0,
                <= 70 => (index - 40) * 2.0,
                <= 170 => 60,
                <= 180 => Math.Min(90, 60 + (index - 170) * 3.0),
                _ => 90
            };
            double azimuth = index <= 40 ? 0 : 75;
            stations.Add(new SurveyStation
            {
                MD = md,
                Inclination = inclination * DegreesToRadians,
                Azimuth = azimuth * DegreesToRadians,
                X = index == 0 ? 0 : null,
                Y = index == 0 ? 0 : null,
                Z = index == 0 ? 0 : null,
                SurveyTool = tool
            });
        }
        return stations;
    }
}
