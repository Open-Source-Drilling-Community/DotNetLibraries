using NUnit.Framework;
using static OSDC.DotnetLibraries.Drilling.Surveying.ErrorSource;

namespace OSDC.DotnetLibraries.Drilling.Surveying.UnitTest;

public class IscwsaRevision5Tests
{
    [Test]
    public void AmilUsesFluxDensityAndHistoricAmidIsRejectedForRevision5()
    {
        ErrorSource amil = ErrorSourceFactory.Create_AMIL(220e-6);
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
        ErrorSource source = ErrorSourceFactory.Create_ABXY_TI1(0.004, ErrorPropagationMode.WellByWell);
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
            ErrorCode = ErrorCode.ABXY_TI1,
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
        ErrorSource source = ErrorSourceFactory.Create_AMIL(220e-6);
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
}
