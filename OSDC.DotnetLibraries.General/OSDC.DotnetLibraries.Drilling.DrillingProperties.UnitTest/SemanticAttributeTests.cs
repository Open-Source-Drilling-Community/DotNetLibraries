using System.Reflection;
using System.Text.Json;
using DWIS.API.DTO;
using DWIS.Vocabulary.Schemas;
using NUnit.Framework;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties.UnitTest;

public class SemanticAttributeTests
{
    private static ManifestFile Manifest(string property, params int[] options) => GeneratorSparQLManifestFile.GetManifestFile(
        typeof(Signals).Assembly, typeof(Signals).FullName, property, "attributes", "tests", "", options.ToList())!;
    private static List<QuerySpecification> Queries(string property) => GeneratorSparQLManifestFile.GetSparQLQueries(
        typeof(Signals).Assembly, typeof(Signals).FullName, property)!.Values.ToList();
    private static Field Field(ManifestFile manifest, string node, string name) => manifest.InjectedNodes.Single(n => n.UniqueName == node).Fields.Single(f => f.FieldName == name);
    private const string Xsd = "http://www.w3.org/2001/XMLSchema#";

    [Test]
    public void MandatoryDeclarationAndRelationshipAttributesArePreservedAndTyped()
    {
        var manifest = Manifest(nameof(Signals.Mandatory));
        Assert.That(Field(manifest, "telemetry", "NumberOfRepeaters").FieldValue, Is.EqualTo("3"));
        Assert.That(Field(manifest, "telemetry", "AverageDelayByRepeater").FieldValue, Is.EqualTo("0.25"));
        Assert.That(Field(manifest, "measurement", "IsValid").FieldValue, Is.EqualTo("true"));
        string query = Queries(nameof(Signals.Mandatory)).Single().SparQL!;
        Assert.That(query, Does.Contain($"?telemetry ddhub:NumberOfRepeaters \"3\"^^<{Xsd}int>"));
        Assert.That(query, Does.Contain($"?telemetry ddhub:AverageDelayByRepeater \"0.25\"^^<{Xsd}double>"));
        Assert.That(query, Does.Contain($"?measurement ddhub:IsValid \"true\"^^<{Xsd}boolean>"));
    }

    [Test]
    public void StringsAreRawInManifestAndEscapedInQuery()
    {
        var manifest = Manifest(nameof(Signals.Escaped));
        Assert.That(Field(manifest, "provider", "ProviderName").FieldValue, Is.EqualTo(Signals.Special));
        var roundtrip = JsonSerializer.Deserialize<ManifestFile>(JsonSerializer.Serialize(manifest))!;
        Assert.That(Field(roundtrip, "provider", "ProviderName").FieldValue, Is.EqualTo(Signals.Special));
        string query = Queries(nameof(Signals.Escaped)).Single().SparQL!;
        Assert.That(query, Does.Contain("\"a\\\"\\\\\\n\\r\\t\\u0001 Ω\"^^<" + Xsd + "string>"));
    }

    [Test]
    public void OptionalAttributesFollowSelectedGroupInManifestAndQueries()
    {
        Assert.That(Manifest(nameof(Signals.Optional)).InjectedNodes.Single(n => n.UniqueName == "telemetry").Fields, Is.Null.Or.Empty);
        Assert.That(Field(Manifest(nameof(Signals.Optional), 1), "telemetry", "NumberOfRepeaters").FieldValue, Is.EqualTo("4"));
        var queries = Queries(nameof(Signals.Optional));
        Assert.That(queries.Any(q => q.Options!.Contains(1) && q.SparQL!.Contains("ddhub:NumberOfRepeaters")), Is.True);
        Assert.That(queries.Where(q => !q.Options!.Contains(1)).All(q => !q.SparQL!.Contains("ddhub:NumberOfRepeaters")), Is.True);
    }

    [Test]
    public void ExcludedAttributesAppearOnlyInsideNegativePatternAndNeverInManifest()
    {
        Assert.That(Manifest(nameof(Signals.Excluded)).InjectedNodes.Single(n => n.UniqueName == "telemetry").Fields, Is.Null.Or.Empty);
        string query = Queries(nameof(Signals.Excluded)).Single().SparQL!;
        int filter = query.IndexOf("FILTER NOT EXISTS", StringComparison.Ordinal);
        Assert.That(filter, Is.GreaterThan(0));
        Assert.That(query[..filter], Does.Not.Contain("ddhub:NumberOfRepeaters"));
        Assert.That(query[filter..], Does.Contain($"?telemetry ddhub:NumberOfRepeaters \"9\"^^<{Xsd}int>"));
    }

    [Test]
    public void OptionalExclusionsAreNegativeOnlyWhenSelected()
    {
        var queries = Queries(nameof(Signals.OptionalExcluded));
        Assert.That(queries.Where(q => q.Options!.Contains(1)).All(q => q.SparQL!.Contains("FILTER NOT EXISTS") && q.SparQL.IndexOf("ddhub:NumberOfRepeaters") > q.SparQL.IndexOf("FILTER NOT EXISTS")), Is.True);
        Assert.That(queries.Where(q => !q.Options!.Contains(1)).All(q => !q.SparQL!.Contains("ddhub:NumberOfRepeaters")), Is.True);
        Assert.That(Manifest(nameof(Signals.OptionalExcluded), 1).InjectedNodes.Single(n => n.UniqueName == "telemetry").Fields, Is.Null.Or.Empty);
    }

    [TestCase(nameof(Signals.InvalidInteger))]
    [TestCase(nameof(Signals.Unknown))]
    [TestCase(nameof(Signals.NonFinite))]
    [TestCase(nameof(Signals.Overflow))]
    [TestCase(nameof(Signals.InvalidBoolean))]
    [TestCase(nameof(Signals.CommaDecimal))]
    [TestCase(nameof(Signals.InvalidDate))]
    public void InvalidAttributesFailBothGenerators(string property)
    {
        Assert.Throws<ArgumentException>(() => Manifest(property));
        Assert.Throws<ArgumentException>(() => Queries(property));
    }

    [Test]
    public void ConflictingAssignmentsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Manifest(nameof(Signals.Conflict)));
        Assert.Throws<ArgumentException>(() => Queries(nameof(Signals.Conflict)));
    }

    [Test]
    public void AttributeOnlyDeclarationsAndDuplicateValuesArePreserved()
    {
        Assert.That(Field(Manifest(nameof(Signals.Standalone)), "telemetry", "NumberOfRepeaters").FieldValue, Is.EqualTo("3"));
        Assert.That(Manifest(nameof(Signals.Standalone)).InjectedNodes.Single(n => n.UniqueName == "telemetry").Fields.Count, Is.EqualTo(1));
    }

    [Test]
    public void NumericFormattingIsCultureIndependentAndDatesAreTyped()
    {
        var original = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
            Assert.That(Field(Manifest(nameof(Signals.Mandatory)), "telemetry", "AverageDelayByRepeater").FieldValue, Is.EqualTo("0.25"));
            Assert.That(Queries(nameof(Signals.Timestamp)).Single().SparQL,
                Does.Contain("\"2026-10-17T12:30:00Z\"^^<" + Xsd + "dateTime>"));
        }
        finally { System.Globalization.CultureInfo.CurrentCulture = original; }
    }

    [Test]
    public void UnsupportedStructuredTypesFailExplicitly()
    {
        Assert.Throws<NotSupportedException>(() => Manifest(nameof(Signals.Array)));
        Assert.Throws<NotSupportedException>(() => Queries(nameof(Signals.Array)));
    }

    [Test]
    public void TypeLevelGenerationAlsoPreservesAttributes()
    {
        var type = typeof(TypeSignal);
        var manifest = GeneratorSparQLManifestFile.GetManifestFile(type.Assembly, type.FullName, "type", "tests", "")!;
        Assert.That(Field(manifest, "telemetry", "NumberOfRepeaters").FieldValue, Is.EqualTo("2"));
        Assert.That(GeneratorSparQLManifestFile.GetSparQLQueries(type.Assembly, type.FullName)!.Values.Single().SparQL,
            Does.Contain($"?telemetry ddhub:NumberOfRepeaters \"2\"^^<{Xsd}int>"));
    }

    [SemanticTypeVariable("value")]
    [SemanticFact("value", Nouns.Enum.DrillingDataPoint)]
    [SemanticFact("telemetry", Nouns.Enum.WiredPipeTelemetry, "NumberOfRepeaters", "2")]
    [SemanticFact("value", Verbs.Enum.IsTransmittedBy, "telemetry")]
    public class TypeSignal { }

    public class Signals
    {
        public const string Special = "a\"\\\n\r\t\u0001 Ω";
        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint, "IsValid", "TRUE")]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("telemetry", Nouns.Enum.WiredPipeTelemetry, "NumberOfRepeaters", "003")]
        [SemanticFact("measurement", Verbs.Enum.IsTransmittedBy, "telemetry", "AverageDelayByRepeater", "2.5E-1")]
        public ScalarDrillingProperty Mandatory { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("provider", Nouns.Enum.DataProvider, "ProviderName", Special)]
        [SemanticFact("measurement", Verbs.Enum.IsProvidedBy, "provider")]
        public ScalarDrillingProperty Escaped { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("telemetry", Nouns.Enum.WiredPipeTelemetry)]
        [SemanticFact("measurement", Verbs.Enum.IsTransmittedBy, "telemetry")]
        [OptionalFact(1, "measurement", Verbs.Enum.IsTransmittedBy, "telemetry", "NumberOfRepeaters", "4")]
        public ScalarDrillingProperty Optional { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("telemetry", Nouns.Enum.WiredPipeTelemetry)]
        [SemanticFact("measurement", Verbs.Enum.IsTransmittedBy, "telemetry")]
        [ExcludeFact("measurement", Verbs.Enum.IsTransmittedBy, "telemetry", "NumberOfRepeaters", "9")]
        public ScalarDrillingProperty Excluded { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("telemetry", Nouns.Enum.WiredPipeTelemetry)]
        [SemanticFact("measurement", Verbs.Enum.IsTransmittedBy, "telemetry")]
        [OptionalExcludeFact(1, "measurement", Verbs.Enum.IsTransmittedBy, "telemetry", "NumberOfRepeaters", "9")]
        public ScalarDrillingProperty OptionalExcluded { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("telemetry", Nouns.Enum.WiredPipeTelemetry, "NumberOfRepeaters", "three")]
        [SemanticFact("measurement", Verbs.Enum.IsTransmittedBy, "telemetry")]
        public ScalarDrillingProperty InvalidInteger { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("telemetry", Nouns.Enum.WiredPipeTelemetry, "NotAnAttribute", "x")]
        [SemanticFact("measurement", Verbs.Enum.IsTransmittedBy, "telemetry")]
        public ScalarDrillingProperty Unknown { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("telemetry", Nouns.Enum.WiredPipeTelemetry, "AverageDelayByRepeater", "NaN")]
        [SemanticFact("measurement", Verbs.Enum.IsTransmittedBy, "telemetry")]
        public ScalarDrillingProperty NonFinite { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("telemetry", Nouns.Enum.WiredPipeTelemetry, "NumberOfRepeaters", "3")]
        [SemanticFact("measurement", Verbs.Enum.IsTransmittedBy, "telemetry", "NumberOfRepeaters", "4")]
        public ScalarDrillingProperty Conflict { get; set; } = new();
        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("target", Nouns.Enum.WiredPipeTelemetry, "NumberOfRepeaters", "2147483648")]
        [SemanticFact("measurement", Verbs.Enum.IsRecordedFrom, "target")]
        public ScalarDrillingProperty Overflow { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("target", Nouns.Enum.DrillingDataPoint, "IsValid", "yes")]
        [SemanticFact("measurement", Verbs.Enum.IsRecordedFrom, "target")]
        public ScalarDrillingProperty InvalidBoolean { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("target", Nouns.Enum.WiredPipeTelemetry, "AverageDelayByRepeater", "0,25")]
        [SemanticFact("measurement", Verbs.Enum.IsRecordedFrom, "target")]
        public ScalarDrillingProperty CommaDecimal { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("target", Nouns.Enum.DynamicDrillingSignal, "TimeStampAtSource", "yesterday")]
        [SemanticFact("measurement", Verbs.Enum.IsRecordedFrom, "target")]
        public ScalarDrillingProperty InvalidDate { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("target", Nouns.Enum.DynamicDrillingSignal, "TimeStampAtSource", "2026-10-17T12:30:00Z")]
        [SemanticFact("measurement", Verbs.Enum.IsRecordedFrom, "target")]
        public ScalarDrillingProperty Timestamp { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("target", Nouns.Enum.EndPointMapping, "FixedIndices", "[1,2]")]
        [SemanticFact("measurement", Verbs.Enum.IsRecordedFrom, "target")]
        public ScalarDrillingProperty Array { get; set; } = new();

        [SemanticDiracVariable("value")]
        [SemanticFact("measurement", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("measurement", Verbs.Enum.HasDynamicValue, "value")]
        [SemanticFact("telemetry", Nouns.Enum.WiredPipeTelemetry, "NumberOfRepeaters", "003", "NumberOfRepeaters", "3")]
        public ScalarDrillingProperty Standalone { get; set; } = new();
    }
}
