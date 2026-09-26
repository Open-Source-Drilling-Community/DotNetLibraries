using System.Text.Json.Serialization;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

[JsonConverter(typeof(JsonStringEnumConverter<SemanticKind>))]
public enum SemanticKind { Noun, Role, Reference }

[JsonConverter(typeof(JsonStringEnumConverter<CurationStatus>))]
public enum CurationStatus { Proposed, Reviewed, Deprecated }

[JsonConverter(typeof(JsonStringEnumConverter<SemanticRelationKind>))]
public enum SemanticRelationKind { HasPart, LocatedAt, EvaluatedUsing }

public sealed record SemanticRelation(SemanticRelationKind Kind, string Target);

public sealed record SemanticDefinition
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Definition { get; init; }
    public SemanticKind Kind { get; init; }
    public CurationStatus Status { get; init; }
    public IReadOnlyList<string> Parents { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Aliases { get; init; } = Array.Empty<string>();
    public string? QuantityName { get; init; }
    public string? SiUnit { get; init; }
    public IReadOnlyList<string> RequiredContext { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Constraints { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Evidence { get; init; } = Array.Empty<string>();
    public IReadOnlyList<SemanticRelation> Relations { get; init; } = Array.Empty<SemanticRelation>();
    public string? SupersededBy { get; init; }
}

public sealed record CatalogueDocument
{
    public required string Id { get; init; }
    public required string Version { get; init; }
    public int SchemaVersion { get; init; } = 1;
    public required IReadOnlyList<SemanticDefinition> Concepts { get; init; }
}

public sealed record QuantityIdentity(string Catalogue, Guid Id, string Name, string SiUnitName);
