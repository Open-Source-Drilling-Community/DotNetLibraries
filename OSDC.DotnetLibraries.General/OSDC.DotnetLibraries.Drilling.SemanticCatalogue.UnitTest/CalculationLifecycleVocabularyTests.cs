using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class CalculationLifecycleVocabularyTests
{
    private sealed class OperationBindings
    {
        [Semantic(Concepts.CalculationCase, Role = Concepts.QueuedCalculationSubmission)]
        public void Submit() { }

        [Semantic(Concepts.CalculationCase, Role = Concepts.CalculationCaseDeletion)]
        public void Delete() { }
    }

    [Test]
    public void CalculationLifecycleIncrementIsReviewedAndPreservesPreviousIncrements()
    {
        var catalogue = Catalogue.Default;

        Assert.Multiple(() =>
        {
            Assert.That(catalogue.Document.Version, Is.EqualTo("0.19.0"));
            Assert.That(catalogue.Document.Concepts, Has.Count.EqualTo(650));
            Assert.That(catalogue.Document.Concepts.Skip(577).Take(20), Has.Count.EqualTo(20));
            Assert.That(catalogue.Document.Concepts.Skip(577).Take(20).All(x => x.Status == CurationStatus.Reviewed), Is.True);
            Assert.That(catalogue.Document.Concepts.Skip(577).Take(20).All(x => x.Evidence.Count > 0), Is.True);
            Assert.That(catalogue.Document.Concepts.Count(x => x.Status == CurationStatus.Reviewed), Is.EqualTo(647));
        });
    }

    [Test]
    public void CalculationCaseSeparatesSpecificationLifecycleAndResult()
    {
        var catalogue = Catalogue.Default;
        SemanticDefinition calculationCase = catalogue.Get(Concepts.CalculationCase);

        Assert.Multiple(() =>
        {
            Assert.That(catalogue.IsA(Concepts.CalculationCase, Concepts.Resource), Is.True);
            Assert.That(calculationCase.Relations,
                Does.Contain(new SemanticRelation(SemanticRelationKind.HasInput, Concepts.CalculationSpecification)));
            Assert.That(calculationCase.Relations,
                Does.Contain(new SemanticRelation(SemanticRelationKind.Produces, Concepts.CalculationResult)));
            Assert.That(calculationCase.Relations,
                Does.Contain(new SemanticRelation(SemanticRelationKind.HasPart, Concepts.CalculationState)));
            Assert.That(calculationCase.Relations,
                Does.Contain(new SemanticRelation(SemanticRelationKind.HasPart, Concepts.CalculationProgress)));
            Assert.That(calculationCase.Relations,
                Does.Contain(new SemanticRelation(SemanticRelationKind.HasPart, Concepts.CalculationDiagnosticMessage)));
            Assert.That(catalogue.Quantity(Concepts.CalculationProgress)!.Name, Is.EqualTo("ProportionStandard"));
            Assert.That(catalogue.SiUnit(Concepts.CalculationProgress), Is.EqualTo("1"));
        });
    }

    [Test]
    public void LightweightStatusAndChunkedResultsRemainProjectionsOfTheirAggregates()
    {
        var catalogue = Catalogue.Default;

        Assert.Multiple(() =>
        {
            Assert.That(catalogue.Get(Concepts.CalculationStatusSnapshot).Relations,
                Does.Contain(new SemanticRelation(SemanticRelationKind.ProjectionOf, Concepts.CalculationCase)));
            Assert.That(catalogue.Get(Concepts.CalculationResultManifest).Relations,
                Does.Contain(new SemanticRelation(SemanticRelationKind.ProjectionOf, Concepts.CalculationResult)));
            Assert.That(catalogue.Get(Concepts.CalculationResultManifest).Relations,
                Does.Contain(new SemanticRelation(SemanticRelationKind.HasPart, Concepts.CalculationResultChunk)));
            Assert.That(catalogue.Get(Concepts.CalculationResultChunk).Relations,
                Does.Contain(new SemanticRelation(SemanticRelationKind.ProjectionOf, Concepts.CalculationResult)));
        });
    }

    [Test]
    public void ImmediateAndQueuedOperationsSpecializeSharedOperationRoles()
    {
        var catalogue = Catalogue.Default;

        Assert.Multiple(() =>
        {
            Assert.That(catalogue.IsA(Concepts.ImmediateCalculationSubmission, Concepts.CalculationSubmission), Is.True);
            Assert.That(catalogue.IsA(Concepts.QueuedCalculationSubmission, Concepts.CalculationSubmission), Is.True);
            Assert.That(catalogue.IsA(Concepts.ImmediateCalculationReplacement, Concepts.CalculationReplacement), Is.True);
            Assert.That(catalogue.IsA(Concepts.QueuedCalculationReplacement, Concepts.CalculationReplacement), Is.True);
            Assert.That(catalogue.IsA(Concepts.CalculationResultChunkRetrieval, Concepts.CalculationResultRetrieval), Is.True);
            Assert.That(catalogue.Get(Concepts.CalculationSubmission).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(catalogue.Get(Concepts.CalculationStatusRetrieval).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(catalogue.Get(Concepts.CalculationCaseDeletion).Kind, Is.EqualTo(SemanticKind.Role));
        });
    }

    [Test]
    public void CalculationCaseDeletionIsDistinctFromReplacementAndCancellation()
    {
        var catalogue = Catalogue.Default;
        SemanticDefinition deletion = catalogue.Get(Concepts.CalculationCaseDeletion);
        var method = typeof(OperationBindings).GetMethod(nameof(OperationBindings.Delete))!;
        var metadata = SemanticMetadata.For(method)!;

        Assert.Multiple(() =>
        {
            Assert.That(deletion.Label, Is.EqualTo("Calculation case deletion"));
            Assert.That(deletion.Parents, Is.EquivalentTo(new[]
            {
                Concepts.CalculationCaseOperation,
                Concepts.ResourceDeletion
            }));
            Assert.That(catalogue.IsA(Concepts.CalculationCaseDeletion, Concepts.CalculationCancellation), Is.False);
            Assert.That(catalogue.IsA(Concepts.CalculationCaseDeletion, Concepts.CalculationReplacement), Is.False);
            Assert.That(deletion.Constraints, Has.Some.Contains("destructive operation"));
            Assert.That(deletion.Constraints, Has.Some.Contains("does not mean cancelling"));
            Assert.That(deletion.Constraints, Has.Some.Contains("exact case created"));
            Assert.That(deletion.Evidence, Is.Not.Empty);
            Assert.That(metadata["concept"]!.GetValue<string>(), Is.EqualTo(Concepts.CalculationCase));
            Assert.That(metadata["role"]!.GetValue<string>(), Is.EqualTo(Concepts.CalculationCaseDeletion));
        });
    }

    [Test]
    public void MethodsCanPublishOperationSemanticBindings()
    {
        var method = typeof(OperationBindings).GetMethod(nameof(OperationBindings.Submit))!;
        var metadata = SemanticMetadata.For(method)!;

        Assert.Multiple(() =>
        {
            Assert.That(metadata["catalogueVersion"]!.GetValue<string>(), Is.EqualTo("0.19.0"));
            Assert.That(metadata["concept"]!.GetValue<string>(), Is.EqualTo(Concepts.CalculationCase));
            Assert.That(metadata["role"]!.GetValue<string>(), Is.EqualTo(Concepts.QueuedCalculationSubmission));
        });
    }
}
