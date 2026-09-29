using System.Text.Json;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

/// <summary>A locally loaded, validated catalogue. Aliases are discovery hints, never identity.</summary>
public sealed class SemanticCatalogue
{
    private readonly Dictionary<string, SemanticDefinition> concepts;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Lazy<SemanticCatalogue> DefaultCatalogue = new(() =>
    {
        using Stream stream = typeof(SemanticCatalogue).Assembly.GetManifestResourceStream(
            "OSDC.DotnetLibraries.Drilling.SemanticCatalogue.catalogue.json")!;
        return Load(new StreamReader(stream).ReadToEnd());
    });
    public static SemanticCatalogue Default => DefaultCatalogue.Value;
    public CatalogueDocument Document { get; }
    public static SemanticCatalogue Load(string json) => new(JsonSerializer.Deserialize<CatalogueDocument>(json, Json)
        ?? throw new InvalidDataException("Missing catalogue document."));

    public SemanticCatalogue(CatalogueDocument document)
    {
        if (document.SchemaVersion is not (1 or 2) || string.IsNullOrWhiteSpace(document.Id) || !Version.TryParse(document.Version, out _))
            throw new InvalidDataException("Invalid catalogue identity or version.");
        // Own a detached snapshot so mutation of caller-owned arrays cannot alter validation results.
        Document = document with {
            ReferenceProfiles = Array.AsReadOnly(document.ReferenceProfiles.Select(p => p with
            { Bindings = Array.AsReadOnly(p.Bindings.ToArray()) }).ToArray()), Concepts = Array.AsReadOnly(document.Concepts.Select(c => c with
        {
            Parents = Array.AsReadOnly(c.Parents.ToArray()), Aliases = Array.AsReadOnly(c.Aliases.ToArray()),
            RequiredContext = Array.AsReadOnly(c.RequiredContext.ToArray()), Constraints = Array.AsReadOnly(c.Constraints.ToArray()),
            Evidence = Array.AsReadOnly(c.Evidence.ToArray()), Relations = Array.AsReadOnly(c.Relations.ToArray())
        }).ToArray()) };
        concepts = new(StringComparer.Ordinal);
        foreach (SemanticDefinition concept in Document.Concepts)
        {
            if (!Uri.TryCreate(concept.Id, UriKind.Absolute, out _) || string.IsNullOrWhiteSpace(concept.Label) ||
                string.IsNullOrWhiteSpace(concept.Definition) || !Enum.IsDefined(concept.Kind) || !Enum.IsDefined(concept.Status) ||
                !concepts.TryAdd(concept.Id, concept))
                throw new InvalidDataException($"Invalid or duplicate concept: {concept.Id}");
        }
        foreach (SemanticDefinition concept in Document.Concepts)
        {
            foreach (SemanticRelation relation in concept.Relations)
                if (!Enum.IsDefined(relation.Kind) || concept.Kind != SemanticKind.Noun ||
                    !concepts.TryGetValue(relation.Target, out var target) || target.Kind != SemanticKind.Noun)
                    throw new InvalidDataException($"Invalid semantic relationship: {concept.Id}");
            foreach (string parent in concept.Parents)
                if (!concepts.TryGetValue(parent, out var definition) || definition.Kind != concept.Kind)
                    throw new InvalidDataException($"Missing or incompatible parent: {parent}");
            if (concept.SupersededBy is not null && (!concepts.ContainsKey(concept.SupersededBy) || concept.SupersededBy == concept.Id))
                throw new InvalidDataException($"Invalid supersession: {concept.Id}");
            _ = Ancestors(concept.Id); // Detect cycles before serving any queries.
            string[] quantities = Lineage(concept.Id).Select(c => c.QuantityName).OfType<string>().Distinct().ToArray();
            string[] units = Lineage(concept.Id).Select(c => c.SiUnit).OfType<string>().Distinct().ToArray();
            if (quantities.Length > 1 || units.Length > 1)
                throw new InvalidDataException($"Conflicting inherited quantity/unit: {concept.Id}");
            foreach (string quantity in quantities) _ = ResolveQuantity(quantity);
        }
        if (Document.SchemaVersion == 1 && Document.ReferenceProfiles.Count != 0)
            throw new InvalidDataException("Reference profiles require schema version 2.");
        var profileIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var profile in Document.ReferenceProfiles)
        {
            if (profile.Scope != "canonical-storage-and-api" || !Uri.TryCreate(profile.Id, UriKind.Absolute, out _) || !Version.TryParse(profile.Version, out _) || !profileIds.Add(profile.Id))
                throw new InvalidDataException("Invalid or duplicate reference profile.");
            var bound = new HashSet<string>(StringComparer.Ordinal);
            foreach (var binding in profile.Bindings)
                if (Get(binding.Concept).Kind != SemanticKind.Noun || Get(binding.Reference).Kind != SemanticKind.Reference || !bound.Add(binding.Concept))
                    throw new InvalidDataException("Invalid or duplicate canonical reference binding.");
            foreach (var concept in Document.Concepts.Where(c => c.Kind == SemanticKind.Noun))
                _ = CanonicalReference(concept.Id, profile.Id);
        }
    }

    public const string OsdcCanonicalDrilling = "urn:osdc:reference-profile:canonical-drilling";

    public ReferenceProfile GetReferenceProfile(string id) => Document.ReferenceProfiles.SingleOrDefault(p => p.Id == id)
        ?? throw new KeyNotFoundException(id);

    /// <summary>Resolve a profile convention through concept inheritance; conflicting ancestors fail closed.</summary>
    public string? CanonicalReference(string concept, string profile = OsdcCanonicalDrilling)
    {
        _ = Get(concept);
        string[] references = GetReferenceProfile(profile).Bindings.Where(b => IsA(concept, b.Concept))
            .Select(b => b.Reference).Distinct(StringComparer.Ordinal).ToArray();
        if (references.Length > 1) throw new InvalidDataException($"Conflicting canonical references for {concept}.");
        return references.SingleOrDefault();
    }

    public string? ResolveReference(string concept, string? declaredReference, string? profile = null)
    {
        _ = Get(concept);
        if (declaredReference is not null && Get(declaredReference).Kind != SemanticKind.Reference)
            throw new InvalidDataException("Reference binding must refer to a reference convention.");
        string? canonical = profile is null ? null : CanonicalReference(concept, profile);
        if (canonical is not null && declaredReference is not null && canonical != declaredReference)
            throw new InvalidDataException($"Reference {declaredReference} contradicts {profile} for {concept}; expected {canonical}.");
        return canonical ?? declaredReference;
    }

    public SemanticDefinition Get(string id) => concepts.TryGetValue(id, out var concept) ? concept : throw new KeyNotFoundException(id);
    public IReadOnlyList<SemanticDefinition> Find(string label) => Document.Concepts.Where(c =>
        c.Label.Equals(label, StringComparison.OrdinalIgnoreCase) || c.Aliases.Contains(label, StringComparer.OrdinalIgnoreCase)).ToArray();
    public bool IsA(string child, string parent)
    {
        _ = Get(parent);
        return Get(child).Id == parent || Ancestors(child).Contains(parent);
    }
    public IReadOnlyList<string> Ancestors(string id)
    {
        HashSet<string> found = new(StringComparer.Ordinal), active = new(StringComparer.Ordinal);
        void Visit(string current)
        {
            if (!active.Add(current)) throw new InvalidDataException($"Inheritance cycle at {current}");
            foreach (string parent in Get(current).Parents)
            {
                if (active.Contains(parent)) throw new InvalidDataException($"Inheritance cycle at {parent}");
                if (found.Add(parent)) Visit(parent);
            }
            active.Remove(current);
        }
        Visit(id);
        return found.Order(StringComparer.Ordinal).ToArray();
    }
    private IEnumerable<SemanticDefinition> Lineage(string id) => new[] { Get(id) }.Concat(Ancestors(id).Select(Get));
    public IReadOnlyList<string> RequiredContext(string id) => Lineage(id).SelectMany(c => c.RequiredContext).Distinct().ToArray();
    public IReadOnlyList<string> Constraints(string id) => Lineage(id).SelectMany(c => c.Constraints).Distinct().ToArray();
    public QuantityIdentity? Quantity(string id)
    {
        string? name = Lineage(id).Select(c => c.QuantityName).FirstOrDefault(n => n is not null);
        return name is null ? null : ResolveQuantity(name);
    }
    public string? SiUnit(string id) => Lineage(id).Select(c => c.SiUnit).FirstOrDefault(u => u is not null);
    private static QuantityIdentity ResolveQuantity(string name)
    {
        // Explicit lookup through the authoritative libraries; no local quantity UUID catalogue.
        BasePhysicalQuantity quantity = DrillingPhysicalQuantity.GetQuantity(name)
            ?? throw new InvalidDataException($"Unknown physical quantity: {name}");
        if (quantity.Name != name) throw new InvalidDataException($"Noncanonical physical quantity: {name}");
        return new(quantity.GetType().Assembly.GetName().Name!, quantity.ID, quantity.Name, quantity.SIUnitName);
    }
    public string ToJson() => JsonSerializer.Serialize(Document, new JsonSerializerOptions(Json) { WriteIndented = true });
}
