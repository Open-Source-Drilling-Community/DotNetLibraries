using System.Reflection;
using System.Text.Json.Nodes;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

/// <summary>Provider-owned binding to a shared concept. It does not change payload serialization.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Method, Inherited = true)]
public sealed class SemanticAttribute(string concept) : Attribute
{
    public string Concept { get; } = concept;
    public string? Role { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceProfile { get; set; }
}

public static class SemanticMetadata
{
    public const string ExtensionName = "x-osdc-semantic";
    public static JsonObject? For(MemberInfo member, SemanticCatalogue? catalogue = null)
    {
        SemanticAttribute? binding = member.GetCustomAttribute<SemanticAttribute>();
        if (binding is null) return null;
        return Create(binding.Concept, binding.Role, binding.Reference, binding.ReferenceProfile, catalogue);
    }

    /// <summary>Shared factory for model attributes and provider-owned bindings, including inherited canonical references.</summary>
    public static JsonObject Create(string conceptId, string? role = null, string? reference = null,
        string? referenceProfile = null, SemanticCatalogue? catalogue = null,
        string assertionSource = "provider-model-attribute")
    {
        catalogue ??= SemanticCatalogue.Default;
        SemanticDefinition concept = catalogue.Get(conceptId);
        if (concept.Kind != SemanticKind.Noun) throw new InvalidDataException("Bindings require a noun concept.");
        if (role is not null && catalogue.Get(role).Kind != SemanticKind.Role)
            throw new InvalidDataException("Role binding must refer to a role.");
        if (reference is not null && catalogue.Get(reference).Kind != SemanticKind.Reference)
            throw new InvalidDataException("Reference binding must refer to a reference convention.");
        reference = catalogue.ResolveReference(conceptId, reference, referenceProfile);
        var result = new JsonObject
        {
            ["catalogue"] = catalogue.Document.Id, ["catalogueVersion"] = catalogue.Document.Version,
            ["concept"] = concept.Id, ["curationStatus"] = concept.Status.ToString(),
            ["assertionSource"] = assertionSource
        };
        if (concept.SupersededBy is not null) result["supersededBy"] = concept.SupersededBy;
        if (referenceProfile is not null)
        {
            var profile = catalogue.GetReferenceProfile(referenceProfile);
            result["referenceProfile"] = profile.Id;
            result["referenceProfileVersion"] = profile.Version;
            result["referenceScope"] = profile.Scope;
            result["presentationReferencesAllowed"] = true;
        }
        if (role is not null) result["role"] = role;
        if (reference is not null) result["reference"] = reference;
        if (catalogue.SiUnit(concept.Id) is string unit) result["siUnit"] = unit;
        if (catalogue.Quantity(concept.Id) is QuantityIdentity quantity)
        {
            result["physicalQuantityStatus"] = "resolved";
            result["physicalQuantity"] = new JsonObject { ["catalogue"] = quantity.Catalogue,
                ["id"] = quantity.Id.ToString(), ["name"] = quantity.Name, ["siUnitName"] = quantity.SiUnitName };
        }
        else if (catalogue.SiUnit(concept.Id) is not null) result["physicalQuantityStatus"] = "unresolved";
        result["requiredContext"] = new JsonArray(catalogue.RequiredContext(concept.Id).Select(x => (JsonNode?)JsonValue.Create(x)).ToArray());
        result["constraints"] = new JsonArray(catalogue.Constraints(concept.Id).Select(x => (JsonNode?)JsonValue.Create(x)).ToArray());
        if (reference is not null)
        {
            result["referenceDefinition"] = catalogue.Get(reference).Definition;
            result["referenceRequiredContext"] = new JsonArray(catalogue.RequiredContext(reference).Select(x => (JsonNode?)JsonValue.Create(x)).ToArray());
        }
        return result;
    }

    /// <summary>Annotate one object and its directly declared properties in an existing JSON schema.</summary>
    public static void AnnotateObject(JsonObject schema, Type modelType)
    {
        if (For(modelType) is JsonObject typeMetadata) schema[ExtensionName] = typeMetadata;
        foreach (PropertyInfo property in modelType.GetProperties())
            if (schema["properties"]?[property.Name] is JsonObject target && For(property) is JsonObject metadata)
                target[ExtensionName] = metadata;
    }
}
