using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class OperationProfileVocabularyTests
{
    private sealed class OperationBindings
    {
        [Semantic(Concepts.Resource, Role = Concepts.ResourceCollectionRetrieval)]
        public void List() { }

        [Semantic(Concepts.GeodeticEvaluationPoint, Role = Concepts.StatelessEvaluation)]
        public void Evaluate() { }

        [Semantic(Concepts.CalculationCase, Role = Concepts.CalculationCancellation)]
        public void Cancel() { }
    }

    [Test]
    public void IncrementIsReviewedAndPreservesPublishedVocabulary()
    {
        var catalogue = Catalogue.Default;
        var increment = catalogue.Document.Concepts.Skip(606).Take(35).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(catalogue.Document.Version, Is.EqualTo("0.20.0"));
            Assert.That(catalogue.Document.Concepts, Has.Count.EqualTo(675));
            Assert.That(increment, Has.Length.EqualTo(35));
            Assert.That(increment.All(x => x.Status == CurationStatus.Reviewed), Is.True);
            Assert.That(increment.All(x => x.Evidence.Count > 0), Is.True);
        });
    }

    [Test]
    public void OperationProfilesClassifyMechanicsWithoutReplacingDomainConcepts()
    {
        var catalogue = Catalogue.Default;

        Assert.Multiple(() =>
        {
            Assert.That(catalogue.IsA(Concepts.ResourceCollectionRetrieval, Concepts.ResourceOperation), Is.True);
            Assert.That(catalogue.IsA(Concepts.ResourcePartialUpdate, Concepts.ResourceOperation), Is.True);
            Assert.That(catalogue.IsA(Concepts.StatelessEvaluation, Concepts.OperationRole), Is.True);
            Assert.That(catalogue.IsA(Concepts.PageRetrieval, Concepts.StreamOperation), Is.True);
            Assert.That(catalogue.IsA(Concepts.PageRetrieval, Concepts.ResourceCollectionRetrieval), Is.True);
            Assert.That(catalogue.IsA(Concepts.CalculationResultChunkRetrieval, Concepts.ChunkRetrieval), Is.True);
            Assert.That(catalogue.Get(Concepts.ResourceCreation).Kind, Is.EqualTo(SemanticKind.Role));
        });
    }

    [Test]
    public void CalculationOperationsRetainLifecycleAndResourceSemantics()
    {
        var catalogue = Catalogue.Default;

        Assert.Multiple(() =>
        {
            Assert.That(catalogue.IsA(Concepts.CalculationSubmission, Concepts.CalculationCaseOperation), Is.True);
            Assert.That(catalogue.IsA(Concepts.CalculationSubmission, Concepts.ResourceCreation), Is.True);
            Assert.That(catalogue.IsA(Concepts.CalculationReplacement, Concepts.ResourceReplacement), Is.True);
            Assert.That(catalogue.IsA(Concepts.CalculationCaseRetrieval, Concepts.ResourceRetrieval), Is.True);
            Assert.That(catalogue.IsA(Concepts.CalculationCaseDeletion, Concepts.ResourceDeletion), Is.True);
            Assert.That(catalogue.IsA(Concepts.CalculationCancellation, Concepts.CalculationCaseOperation), Is.True);
            Assert.That(catalogue.IsA(Concepts.CalculationCancellation, Concepts.ResourceDeletion), Is.False);
        });
    }

    [Test]
    public void CalculationStatesHaveSharedMachineReadableMeanings()
    {
        var catalogue = Catalogue.Default;
        string[] states =
        [
            Concepts.CalculationQueuedState,
            Concepts.CalculationRunningState,
            Concepts.CalculationCompletedState,
            Concepts.CalculationFailedState,
            Concepts.CalculationCancelledState
        ];

        Assert.Multiple(() =>
        {
            Assert.That(states.All(x => catalogue.IsA(x, Concepts.CalculationState)), Is.True);
            Assert.That(catalogue.Get(Concepts.CalculationCompletedState).Constraints,
                Has.Some.Contains("result is obtained"));
            Assert.That(catalogue.Get(Concepts.CalculationCancelledState).Constraints,
                Has.Some.Contains("does not imply deletion"));
        });
    }

    [Test]
    public void ExecutionProvenanceSeparatesCallsFromPlansAndResultContribution()
    {
        var catalogue = Catalogue.Default;
        var provenance = catalogue.Get(Concepts.ExecutionProvenance);
        var invocation = catalogue.Get(Concepts.OperationInvocation);

        Assert.Multiple(() =>
        {
            Assert.That(provenance.Relations,
                Does.Contain(new SemanticRelation(SemanticRelationKind.HasPart, Concepts.OperationInvocation)));
            Assert.That(catalogue.IsA(Concepts.ToolInvocation, Concepts.OperationInvocation), Is.True);
            Assert.That(invocation.Constraints, Has.Some.Contains("undispatched"));
            Assert.That(catalogue.Get(Concepts.ResultContribution).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(catalogue.Get(Concepts.OperationContractRevision).Constraints,
                Has.Some.Contains("display name"));
        });
    }

    [Test]
    public void SemanticMetadataPublishesTheSharedRoles()
    {
        var list = SemanticMetadata.For(typeof(OperationBindings).GetMethod(nameof(OperationBindings.List))!)!;
        var evaluate = SemanticMetadata.For(typeof(OperationBindings).GetMethod(nameof(OperationBindings.Evaluate))!)!;
        var cancel = SemanticMetadata.For(typeof(OperationBindings).GetMethod(nameof(OperationBindings.Cancel))!)!;

        Assert.Multiple(() =>
        {
            Assert.That(list["catalogueVersion"]!.GetValue<string>(), Is.EqualTo("0.20.0"));
            Assert.That(list["role"]!.GetValue<string>(), Is.EqualTo(Concepts.ResourceCollectionRetrieval));
            Assert.That(evaluate["role"]!.GetValue<string>(), Is.EqualTo(Concepts.StatelessEvaluation));
            Assert.That(cancel["role"]!.GetValue<string>(), Is.EqualTo(Concepts.CalculationCancellation));
        });
    }
}
