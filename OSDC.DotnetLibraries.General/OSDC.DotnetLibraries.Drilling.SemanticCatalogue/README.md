# OSDC Drilling Semantic Catalogue

A shared, locally available semantic vocabulary for OSDC providers and consumers. NuGet identity: `OSDC.DotnetLibraries.Drilling.SemanticCatalogue`, targeting .NET 8. The initial version is `0.1.0`; it starts with Earth Gravity's published models and UI quantity choices. Publication is a separate release operation.

See [the Earth Gravity curation table](CURATION.md) for the 34 initial entries and unresolved decisions.

## Ownership and curation

`catalogue.json` is the authoritative language-neutral vocabulary, embedded in the assembly and packed alongside `catalogue.schema.json`. `Concepts.cs` exposes stable URN constants; tests ensure these refer to actual entries. Definitions currently have **Proposed** status for review. Provider annotations are explicit assertions about their own fields, not a claim that the shared vocabulary has completed curation.

Each definition has an immutable semantic ID, label, definition, kind, curation status, aliases, parents, quantity name, SI representation, required context, constraints, source evidence, typed HasPart/LocatedAt/EvaluatedUsing relationships and optional supersession. Nouns, roles and reference conventions are distinct kinds. Only `parents` expresses specialization. Physical quantity and field role are not parent relationships. Aliases are discovery hints and can return multiple concepts.

Earth Gravity supplies the first examples: geographic coordinates specialize Coordinate; latitude/longitude specialize Geodetic angular coordinate; ellipsoidal depth specializes Depth coordinate. Total gravity is distinct from its component/magnitude roles. WGS84 and local NED are explicit reference conventions. Constraints and context requirements accumulate through inheritance; cycles, missing parents, incompatible kinds and conflicting inherited quantities/units are rejected. A child cannot weaken a parent by removing inherited requirements. This library does not evaluate natural-language constraints or authorize arithmetic.

The catalogue owns reusable meanings. Microservices own bindings to concrete data fields, actual reference conventions and representation. Deployment identities, tool enablements, schema fingerprints and provisional discovery inboxes belong to consumers such as DrillWeaver, not this package. Future concepts should be proposed and curated incrementally; deprecated IDs remain resolvable and may point to replacements. A semantic change requires a versioned release, not silently rewriting an existing meaning.

## Physical quantities

Both `OSDC.UnitConversion.Conversion` and `OSDC.UnitConversion.Conversion.DrillingEngineering` use version 3.3.28. Sibling UnitConversion source projects are used when available; set `UseLocalUnitConversionProjects=false` to use the NuGet dependencies instead. Quantity identities and SI unit names are resolved through those libraries, not duplicated UUID definitions. Earth Gravity's choices are PlaneAngleGeodesic, DepthDrilling and AccelerationDrilling. Quantities and units do not establish reference frames or additive meaning.

Total gravitational plus centrifugal potential binds to `EarthGravityPotential`, a specialization of the general `GravityPotential` quantity in UnitConversion. Its SI representation is m²/s² (equivalently J/kg); ft²/s² and ft·lbf/lbm are available. The Earth specialization defines a meaningful display precision of 0.01 m²/s², without asserting model accuracy or rounding stored values. Energy density is not a substitute.

## Provider usage

```csharp
[Semantic(Concepts.EllipsoidalDepth, Reference = Concepts.Wgs84)]
public double Depth { get; set; }
```

`SemanticMetadata.For(member)` returns a JSON object for the `x-osdc-semantic` extension. It includes catalogue identity/version, concept, curation status, optional role/reference, required context, SI representation and the resolved physical-quantity identity. Invalid concept/role/reference IDs fail explicitly. `AnnotateObject` applies attributes to an existing JSON schema object and its direct properties; it does not infer paths, recurse through `$ref`, change validation keywords or alter data serialization. Providers apply it to each named/inline model schema. Earth Gravity demonstrates equivalent MCP and OpenAPI publication from the same attributes.

Consumers can use `SemanticCatalogue.Default`, `Get`, `Find`, `IsA`, `Ancestors`, `Quantity`, `RequiredContext` and `Constraints`. No live catalogue server is required. `ToJson()` exposes the versioned language-neutral catalogue. URNs are stable identities, not assumed HTTP endpoints.

## Build and package

From `OSDC.DotnetLibraries.General`:

```powershell
dotnet test OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest/OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest.csproj -c Release
dotnet pack OSDC.DotnetLibraries.Drilling.SemanticCatalogue/OSDC.DotnetLibraries.Drilling.SemanticCatalogue.csproj -c Release
```

This does not publish the package. Inspect the NuGet dependency manifest and catalogue contents before a separately authorized release.
