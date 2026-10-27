using System.Text.Json;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Surveying;

namespace OSDC.DotnetLibraries.Drilling.Surveying.UnitTest;

public sealed class SurveyStationBoreholeRadiusTests
{
    [Test]
    public void UnspecifiedRadiusIsNullForNewCopiedAndDeserializedStations()
    {
        var station = new SurveyStation();
        Assert.That(station.BoreholeRadius, Is.Null);
        Assert.That(new SurveyStation(station).BoreholeRadius, Is.Null);
        Assert.That(new SurveyStation(new SurveyPoint()).BoreholeRadius, Is.Null);
        Assert.That(JsonSerializer.Deserialize<SurveyStation>("{}")!.BoreholeRadius, Is.Null);
        Assert.That(JsonSerializer.Deserialize<SurveyStation>("{\"BoreholeRadius\":null}")!.BoreholeRadius, Is.Null);
    }

    [TestCase(0.0)]
    [TestCase(0.125)]
    public void ExplicitRadiusSurvivesCopyAndJsonRoundTrip(double radius)
    {
        var station = new SurveyStation { BoreholeRadius = radius };
        Assert.That(new SurveyStation(station).BoreholeRadius, Is.EqualTo(radius));
        Assert.That(JsonSerializer.Deserialize<SurveyStation>(JsonSerializer.Serialize(station))!.BoreholeRadius, Is.EqualTo(radius));
    }
}
