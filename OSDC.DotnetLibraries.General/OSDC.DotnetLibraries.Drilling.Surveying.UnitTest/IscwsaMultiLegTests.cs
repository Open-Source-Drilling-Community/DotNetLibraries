using NUnit.Framework;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying.UnitTest;

[TestFixture]
public sealed class IscwsaMultiLegTests
{
    [Test]
    public void One_leg_matches_existing_calculation()
    {
        SurveyInstrument tool = MwdTool();
        List<SurveyStation> existing = Stations(tool, tool);
        List<SurveyStation> multiLeg = Stations(tool, tool);

        Assert.That(CovarianceCalculatorISCWSA.Calculate(existing), Is.True);
        Assert.That(CovarianceCalculatorISCWSA.CalculateMultiLeg(multiLeg,
        [
            new(0, multiLeg.Count - 1, tool)
        ]), Is.True);

        for (int station = 0; station < existing.Count; station++)
        {
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    Assert.That(multiLeg[station].Covariance![row, column],
                        Is.EqualTo(existing[station].Covariance![row, column]).Within(1e-12));
                }
            }
        }
    }

    [Test]
    public void Gyro_to_mwd_tie_on_supports_different_error_source_sets()
    {
        SurveyInstrument gyro = GyroTool();
        SurveyInstrument mwd = MwdTool();
        List<SurveyStation> stations = Stations(gyro, mwd);

        Assert.That(CovarianceCalculatorISCWSA.CalculateMultiLeg(stations,
        [
            new(0, 2, gyro),
            new(3, 4, mwd)
        ]), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(stations, Is.All.Matches<SurveyStation>(station => IsFinite(station.Covariance)));
            Assert.That(Trace(stations[2].Covariance!), Is.GreaterThan(0.0));
            Assert.That(Trace(stations[4].Covariance!), Is.GreaterThan(Trace(stations[2].Covariance!)));
        });
    }

    [Test]
    public void Error_source_order_does_not_change_multi_leg_covariance()
    {
        SurveyInstrument gyro = GyroTool();
        SurveyInstrument mwd = MwdTool();
        List<SurveyStation> first = Stations(gyro, mwd);
        Assert.That(CovarianceCalculatorISCWSA.CalculateMultiLeg(first,
        [
            new(0, 2, gyro),
            new(3, 4, mwd)
        ]), Is.True);

        SurveyInstrument reordered = MwdTool();
        reordered.ErrorSourceList!.Reverse();
        List<SurveyStation> second = Stations(gyro, reordered);
        Assert.That(CovarianceCalculatorISCWSA.CalculateMultiLeg(second,
        [
            new(0, 2, gyro),
            new(3, 4, reordered)
        ]), Is.True);

        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                Assert.That(second[^1].Covariance![row, column],
                    Is.EqualTo(first[^1].Covariance![row, column]).Within(1e-12));
            }
        }
    }

    [Test]
    public void Conflicting_propagation_modes_for_one_error_code_fail_closed()
    {
        SurveyInstrument gyro = GyroTool();
        SurveyInstrument mwd = MwdTool();
        ErrorSource original = mwd.ErrorSourceList!.Single(source => source.ErrorCode == ErrorCode.DSTG);
        mwd.ErrorSourceList[mwd.ErrorSourceList.IndexOf(original)] = new ErrorSource
        {
            ErrorCode = original.ErrorCode,
            PropagationMode = ErrorPropagationMode.Systematic,
            Magnitude = original.Magnitude,
            MagnitudeQuantity = original.MagnitudeQuantity,
            WeightingFunctionMD = original.WeightingFunctionMD,
            WeightingFunctionIncl = original.WeightingFunctionIncl,
            WeightingFunctionAzim = original.WeightingFunctionAzim
        };

        Assert.That(CovarianceCalculatorISCWSA.CalculateMultiLeg(Stations(gyro, mwd),
        [
            new(0, 2, gyro),
            new(3, 4, mwd)
        ]), Is.False);
    }

    private static SurveyInstrument GyroTool() => new()
    {
        Name = "Gyro ISCWSA leg",
        ModelType = SurveyInstrumentModelType.Gyro_ISCWSA,
        BField = 50_000e-9,
        Dip = 72.0 * Math.PI / 180.0,
        Gravity = 9.80665,
        ErrorSourceList =
        [
            ErrorSourceFactory.Create_DRFR(magnitude: 0.35),
            ErrorSourceFactory.Create_DSTG(magnitude: 2.5e-7),
            ErrorSourceFactory.Create_ABXY_TI1S(magnitude: 0.004)
        ]
    };

    private static SurveyInstrument MwdTool() => new()
    {
        Name = "MWD ISCWSA leg",
        ModelType = SurveyInstrumentModelType.MWD_ISCWSA,
        BField = 50_000e-9,
        Dip = 72.0 * Math.PI / 180.0,
        Gravity = 9.80665,
        ErrorSourceList =
        [
            ErrorSourceFactory.Create_DRFR(magnitude: 0.35),
            ErrorSourceFactory.Create_DSFS(magnitude: 0.00056),
            ErrorSourceFactory.Create_DSTG(magnitude: 2.5e-7),
            ErrorSourceFactory.Create_ABXY_TI1S(magnitude: 0.004),
            ErrorSourceFactory.Create_MBXY_TI1(magnitude: 70e-9)
        ]
    };

    private static List<SurveyStation> Stations(SurveyInstrument gyro, SurveyInstrument mwd) =>
    [
        Station(0.0, 0.20, 0.30, 0.0, 0.0, 0.0, gyro),
        Station(30.0, 0.25, 0.32, 29.0, 5.0, 2.0, gyro),
        Station(60.0, 0.30, 0.34, 57.0, 12.0, 4.0, gyro),
        Station(90.0, 0.35, 0.36, 85.0, 20.0, 6.0, mwd),
        Station(120.0, 0.40, 0.38, 112.0, 30.0, 8.0, mwd)
    ];

    private static SurveyStation Station(
        double md,
        double inclination,
        double azimuth,
        double tvd,
        double north,
        double east,
        SurveyInstrument tool) => new()
    {
        MD = md,
        Abscissa = md,
        Inclination = inclination,
        Azimuth = azimuth,
        TVD = tvd,
        RiemannianNorth = north,
        RiemannianEast = east,
        SurveyTool = tool
    };

    private static bool IsFinite(SymmetricMatrix3x3? covariance)
    {
        if (covariance == null) return false;
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                if (covariance[row, column] is not double value || !double.IsFinite(value)) return false;
            }
        }
        return true;
    }

    private static double Trace(SymmetricMatrix3x3 covariance) =>
        covariance[0, 0]!.Value + covariance[1, 1]!.Value + covariance[2, 2]!.Value;
}
