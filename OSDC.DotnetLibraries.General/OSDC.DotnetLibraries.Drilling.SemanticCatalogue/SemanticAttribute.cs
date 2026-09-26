using System.Reflection;
using System.Text.Json.Nodes;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

/// <summary>Provider-owned binding to a shared concept. It does not change payload serialization.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, Inherited = true)]
public sealed class SemanticAttribute(string concept) : Attribute
{
    public string Concept { get; } = concept;
    public string? Role { get; set; }
    public string? Reference { get; set; }
}

public static class SemanticMetadata
{
    public const string ExtensionName = "x-osdc-semantic";
    public static JsonObject? For(MemberInfo member, SemanticCatalogue? catalogue = null)
    {
        SemanticAttribute? binding = member.GetCustomAttribute<SemanticAttribute>();
        if (binding is null) return null;
        catalogue ??= SemanticCatalogue.Default;
        SemanticDefinition concept = catalogue.Get(binding.Concept);
        if (concept.Kind != SemanticKind.Noun) throw new InvalidDataException("Bindings require a noun concept.");
        if (binding.Role is not null && catalogue.Get(binding.Role).Kind != SemanticKind.Role)
            throw new InvalidDataException("Role binding must refer to a role.");
        if (binding.Reference is not null && catalogue.Get(binding.Reference).Kind != SemanticKind.Reference)
            throw new InvalidDataException("Reference binding must refer to a reference convention.");
        var result = new JsonObject
        {
            ["catalogue"] = catalogue.Document.Id, ["catalogueVersion"] = catalogue.Document.Version,
            ["concept"] = concept.Id, ["curationStatus"] = concept.Status.ToString(),
            ["assertionSource"] = "provider-model-attribute"
        };
        if (binding.Role is not null) result["role"] = binding.Role;
        if (binding.Reference is not null) result["reference"] = binding.Reference;
        if (catalogue.SiUnit(concept.Id) is string unit) result["siUnit"] = unit;
        if (catalogue.Quantity(concept.Id) is QuantityIdentity quantity)
        {
            result["physicalQuantityStatus"] = "resolved";
            result["physicalQuantity"] = new JsonObject { ["catalogue"] = quantity.Catalogue,
                ["id"] = quantity.Id.ToString(), ["name"] = quantity.Name, ["siUnitName"] = quantity.SiUnitName };
        }
        else if (catalogue.SiUnit(concept.Id) is not null) result["physicalQuantityStatus"] = "unresolved";
        result["requiredContext"] = new JsonArray(catalogue.RequiredContext(concept.Id).Select(x => (JsonNode?)JsonValue.Create(x)).ToArray());
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
