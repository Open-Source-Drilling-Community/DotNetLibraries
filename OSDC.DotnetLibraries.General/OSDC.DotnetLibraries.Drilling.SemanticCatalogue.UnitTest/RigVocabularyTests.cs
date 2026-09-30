using NUnit.Framework;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class RigVocabularyTests
{
    [Test]
    public void RigIncrementIsReviewedAndPreservesThePreviousBaseline()
    {
        var c = Catalogue.Default;
        Assert.That(c.Document.Concepts.Take(370), Has.Count.EqualTo(370));
        Assert.That(c.Document.Concepts.Skip(292).Take(78), Has.Count.EqualTo(78));
        Assert.That(c.Document.Concepts.Skip(292).Take(78).All(x => x.Status == CurationStatus.Reviewed), Is.True);
    }

    [Test]
    public void ElevationAndDistanceToBitRequireExplicitUpwardOrigins()
    {
        var c = Catalogue.Default;
        Assert.That(c.IsA(Concepts.Elevation, Concepts.Coordinate), Is.True);
        Assert.That(c.Quantity(Concepts.Elevation)!.Name, Is.EqualTo("LengthStandard"));
        Assert.That(c.SiUnit(Concepts.Elevation), Is.EqualTo("m"));
        Assert.That(c.CanonicalReference(Concepts.Elevation), Is.Null);

        Assert.That(c.IsA(Concepts.DistanceToBit, Concepts.Elevation), Is.True);
        var elevation = SemanticMetadata.Create(Concepts.Elevation, reference: Concepts.DrillFloorUpward);
        var distanceToBit = SemanticMetadata.Create(Concepts.DistanceToBit, reference: Concepts.BitFrontFaceUpward);
        Assert.That(elevation["reference"]!.GetValue<string>(), Is.EqualTo(Concepts.DrillFloorUpward));
        Assert.That(distanceToBit["reference"]!.GetValue<string>(), Is.EqualTo(Concepts.BitFrontFaceUpward));
        Assert.That(c.Get(Concepts.DrillFloorUpward).Kind, Is.EqualTo(SemanticKind.Reference));
        Assert.That(c.Get(Concepts.BitFrontFaceUpward).Kind, Is.EqualTo(SemanticKind.Reference));
    }

    [Test]
    public void PressureRatingsAreAbsoluteButDifferentialPressureIsNot()
    {
        var c = Catalogue.Default;
        Assert.That(c.IsA(Concepts.AbsolutePressureRating, Concepts.AbsolutePressure), Is.True);
        Assert.That(c.Quantity(Concepts.AbsolutePressureRating)!.Name, Is.EqualTo("PressureDrilling"));
        Assert.That(c.SiUnit(Concepts.AbsolutePressureRating), Is.EqualTo("Pa"));
        Assert.That(c.CanonicalReference(Concepts.AbsolutePressureRating), Is.EqualTo(Concepts.Vacuum));
        Assert.That(c.IsA(Concepts.PressureDifference, Concepts.AbsolutePressureRating), Is.False);
        Assert.That(c.CanonicalReference(Concepts.PressureDifference), Is.Null);
    }

    [Test]
    public void RigEquipmentTaxonomyIsExplicit()
    {
        var c = Catalogue.Default;
        Assert.That(c.IsA(Concepts.RigIdentification, Concepts.Rig), Is.True);
        Assert.That(c.IsA(Concepts.RigEquipment, Concepts.RigComponent), Is.True);
        foreach (var equipment in new[]
                 {
                     Concepts.MudPump, Concepts.CementPump, Concepts.MudTank, Concepts.ShaleShaker,
                     Concepts.Generator, Concepts.TopDrive, Concepts.RotaryTable, Concepts.Drawworks,
                     Concepts.BlowoutPreventerStack, Concepts.ManagedPressureDrillingEquipment,
                     Concepts.DrillingMarineRiser, Concepts.HeaveCompensator, Concepts.FlowRoutingManifold
                 })
            Assert.That(c.IsA(equipment, Concepts.RigEquipment), Is.True, equipment);
    }

    [TestCase(Concepts.EquipmentMass, "MassDrilling", "kg")]
    [TestCase(Concepts.ForceCapacity, "ForceDrilling", "N")]
    [TestCase(Concepts.PowerRating, "PowerDrilling", "W")]
    [TestCase(Concepts.VolumetricFlowCapacity, "VolumetricFlowrateDrilling", "m³/s")]
    [TestCase(Concepts.VolumeCapacity, "VolumeDrilling", "m³")]
    [TestCase(Concepts.MassCapacity, "MassDrilling", "kg")]
    [TestCase(Concepts.RotationalAngularVelocity, "AngularVelocityDrilling", "rad/s")]
    [TestCase(Concepts.LinearVelocity, "Velocity", "m/s")]
    [TestCase(Concepts.OperatingTemperature, "TemperatureDrilling", "K")]
    [TestCase(Concepts.ElectricalPotential, "ElectricTension", "V")]
    [TestCase(Concepts.SignalFrequency, "Frequency", "Hz")]
    [TestCase(Concepts.ControllerGain, "ProportionStandard", "1")]
    [TestCase(Concepts.RateOfPenetrationLimit, "RateOfPenetrationDrilling", "m/s")]
    [TestCase(Concepts.WeightOnBitLimit, "WeightOnBitDrilling", "N")]
    [TestCase(Concepts.ChokeOpeningRate, "ChokeOpeningRateDrilling", "1/s")]
    [TestCase(Concepts.PowerRateOfChange, "PowerRateOfChangeDrilling", "W/s")]
    [TestCase(Concepts.RotationalFrequencyRateOfChange, "RotationalFrequencyRateOfChangeDrilling", "Hz/s")]
    [TestCase(Concepts.StrokeFrequency, "StrokeFrequency", "Hz")]
    [TestCase(Concepts.EquipmentDuration, "DurationDrilling", "s")]
    [TestCase(Concepts.EquipmentEfficiency, "ProportionStandard", "1")]
    public void RigEngineeringConceptsUseAuthoritativeQuantities(string concept, string quantity, string unit)
    {
        var c = Catalogue.Default;
        Assert.That(c.Quantity(concept)!.Name, Is.EqualTo(quantity));
        Assert.That(c.SiUnit(concept), Is.EqualTo(unit));
        Assert.That(c.CanonicalReference(concept), Is.Null);
    }

    [Test]
    public void DepthCapacitiesAreExtentsAndDynamicMeasurementFieldsStayContextual()
    {
        var c = Catalogue.Default;
        foreach (var capacity in new[] { Concepts.DrillingDepthCapacity, Concepts.WaterDepthCapacity })
        {
            Assert.That(c.Quantity(capacity)!.Name, Is.EqualTo("DepthDrilling"));
            Assert.That(c.IsA(capacity, Concepts.Coordinate), Is.False);
            Assert.That(c.CanonicalReference(capacity), Is.Null);
        }

        foreach (var contextual in new[] { Concepts.MeasurementRangeValue, Concepts.AbsoluteMeasurementAccuracy })
        {
            Assert.That(c.Quantity(contextual), Is.Null);
            Assert.That(c.RequiredContext(contextual), Does.Contain("sibling PhysicalQuantity declaration"));
        }
        Assert.That(c.Quantity(Concepts.RelativeMeasurementAccuracy)!.Name, Is.EqualTo("ProportionStandard"));
    }

    [Test]
    public void LimitAndDutyQualifiersAreRoles()
    {
        var c = Catalogue.Default;
        foreach (var role in new[]
                 {
                     Concepts.RatedValue, Concepts.NominalValue, Concepts.MaximumDesignLimit,
                     Concepts.MaximumOperatingLimit, Concepts.MinimumOperatingLimit, Concepts.MaximumTestLimit,
                     Concepts.ContinuousRating, Concepts.IntermittentRating, Concepts.MakeUpLimit,
                     Concepts.BreakoutLimit, Concepts.StaticPressureLimit, Concepts.DynamicPressureLimit,
                     Concepts.ActivationPressureLimit, Concepts.DischargePressureLimit, Concepts.SurvivalLimit,
                     Concepts.MeasurementRangeMinimum, Concepts.MeasurementRangeMaximum,
                     Concepts.ColdStartDuration, Concepts.WarmStartDuration
                 })
            Assert.That(c.Get(role).Kind, Is.EqualTo(SemanticKind.Role), role);
    }
}
