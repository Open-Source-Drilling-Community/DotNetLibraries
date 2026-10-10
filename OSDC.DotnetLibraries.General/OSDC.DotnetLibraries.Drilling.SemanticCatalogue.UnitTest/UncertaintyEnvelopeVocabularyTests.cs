using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public sealed class UncertaintyEnvelopeVocabularyTests
{
    [Test]
    public void CircularEnvelopeCompositionIsReviewedAndExplicit()
    {
        var catalogue = Catalogue.Default;

        Assert.Multiple(() =>
        {
            Assert.That(catalogue.Document.Version, Is.EqualTo("0.19.0"));
            Assert.That(catalogue.Document.Concepts, Has.Count.EqualTo(650));
            Assert.That(catalogue.IsA(Concepts.BoreholeDiameterAtAbscissaEvaluation, Concepts.StatelessEvaluation), Is.True);
            Assert.That(catalogue.Get(Concepts.BoreholeDiameterAtAbscissaResult).Relations.Select(r => r.Target),
                Does.Contain(Concepts.BoreholeDiameter));
            Assert.That(catalogue.Get(Concepts.ProjectedBoreholeCrossSection).Constraints,
                Has.Some.Contains("orthogonal projection"));
            Assert.That(catalogue.Get(Concepts.UncertaintyProjectionPlane).Constraints,
                Has.Some.Contains("Horizontal, Vertical or Perpendicular"));
            Assert.That(catalogue.Get(Concepts.IntervalStartCoordinate).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(catalogue.Get(Concepts.IntervalEndCoordinate).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(catalogue.Get(Concepts.MinimumDeterminantEllipsoidalOuterBoundConvention).Kind,
                Is.EqualTo(SemanticKind.Reference));
            Assert.That(catalogue.IsA(Concepts.CircularUncertaintyEnvelopeDilation, Concepts.StatelessEvaluation), Is.True);
            Assert.That(catalogue.IsA(Concepts.CircularlyDilatedUncertaintyEnvelope, Concepts.SurveyStationUncertaintyEllipse), Is.True);
            Assert.That(catalogue.Get(Concepts.CircularUncertaintyEnvelopeDilation).Constraints,
                Has.Some.Contains("not a confidence transformation"));
            Assert.That(catalogue.Get(Concepts.CircularlyDilatedUncertaintyEnvelope).Constraints,
                Has.Some.Contains("approximation convention"));
        });
    }
}
