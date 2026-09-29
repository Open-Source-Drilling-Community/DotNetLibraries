# ResourceClassification

`OSDC.DotnetLibraries.General.ResourceClassification` 0.1.0 is a .NET 8 NuGet
library for resource identities, features and memberships. It references
`OSDC.DotnetLibraries.General.DataManagement` 2.2.0, which remains the owner of
`MetaInfo` and the existing `IIdentity*`, `IFeature*`, and `IMembership*` interfaces.

## Model ownership

- `IdentityDefinition` describes an identification scheme; `IdentityAssignment`
  associates its UUID with a resource-specific value. These are not authentication
  identities, nor substitutes for the resource's own UUID.
- `FeatureCategory`, `FeatureOption`, and `FeatureAssignment` describe selectable
  classifications and their assignments.
- `MembershipCategory`, `MembershipOption`, and `MembershipAssignment` describe
  membership classifications. A membership option does not imply a remotely
  resolvable group or access-control permission.
- `FeatureCategory<TOption>` and `MembershipCategory<TOption>` support concrete
  service-specific option types. Their interface adapters preserve compatible
  option instances; otherwise they copy the interface's ID and Name only.
  Interface getters return list snapshots, not live collection views.

Services can inherit these models to retain their public domain-specific type
names, for example `FieldFeatureCategory : FeatureCategory<FieldFeatureOption>`.
Properties remain concrete, mutable and serializable with System.Text.Json.
Null collections, nullable references, property casing and caller-assigned IDs
are preserved. Constructors do not generate UUIDs or timestamps.

The owning microservice remains responsible for catalogue values, default UUIDs,
timestamps, persistence, transactions, REST/MCP routes, error envelopes, resource
relationships and preventing deletion of referenced definitions. This package
has no database, ASP.NET, unit-conversion or SemanticCatalogue dependency.
Shared semantic vocabulary can describe these concepts separately without
creating a dependency cycle.

## Validation

`ClassificationValidation` provides deterministic, side-effect-free helpers:

- Category/option references: both null means an unlinked draft; partial,
  empty, unknown or wrong-category references produce a position-aware issue.
- Optional references: null is permitted; empty and unknown UUIDs are invalid.
- Stable IDs: detect empty or duplicate IDs within a caller-selected collection.
- Validity: null bounds are unbounded; FromDate must not exceed ToDate. Categories
  without validity periods cannot carry bounds. This helper never erases dates.
- Exclusive-category conflicts: at most one assignment is active at any instant.
  Bounds are inclusive, so touching periods overlap, including the same instant
  represented with different UTC offsets. Undated categories permit one assignment.

For exclusivity, pass assignments for **one category on one resource**, only when
the category is exclusive. Validate references and periods first. Conflict indices
refer to the supplied list. Duplicate same-option assignments also conflict.
The helpers do not silently apply validation during deserialization.

The Field/Cluster pilot reuses the shared reference checks and editor interval
checks without changing accepted server payloads. Unique-ID, complete validity
and exclusivity helpers are available for subsequent server-side adoption;
they are not newly enforced on existing persisted records by this extraction.
Before tightening write validation, audit existing assignments, cover catalogue
edits and restore paths, and apply the same rules to REST and MCP writes.

## Incremental migration inventory

| Resource | Findings and migration status |
| --- | --- |
| Field | Identity, feature and membership models migrated; common reference validation and editor interval checks used. |
| Cluster / Slot | Cluster identity and feature models, Slot feature models migrated; common reference validation and editor interval checks used. |
| WellBore | Existing identity and feature models are candidates for the next migration. |
| WellBoreArchitecture | Existing identity and feature models are candidates; component identity remains a distinct service concern. |
| Trajectory / Survey Run | Existing trajectory identity and feature models are candidates; inspect resource-specific usage before migration. |
| Rig | Separate migration required: category/option Code, Description, IsBuiltIn and IsDeprecated; assignment Notes and EvidenceReference; non-nullable reference UUIDs. Preserve these contracts explicitly. |
| Well / SurveyInstrument | No matching standalone classification model files found in the inspected Model folders; verify their current resource contracts before introducing any new catalogue. |

This first release does not migrate the remaining services or curate new semantic
terms implicitly. Per-service migrations must check stored JSON, API schemas,
generated clients, default catalogue IDs and transactional reference integrity.

## Build, test and publish order

From `OSDC.DotnetLibraries.General`, using an SDK supported by `global.json`:

```powershell
dotnet test OSDC.DotnetLibraries.General.ResourceClassification.UnitTest -c Release
dotnet pack OSDC.DotnetLibraries.General.ResourceClassification -c Release
```

The package includes its README, MIT licence and XML API documentation. Tests
cover reference failures, typed option adapters, serialization, null handling,
validity bounds, exclusivity and duplicate IDs.

Publish ResourceClassification 0.1.0 before building Field/Cluster in a clean CI
or Docker environment. Pilot projects use unconditional package references;
there is no cross-repository project reference or local-feed fallback committed
to either service. For pre-publication verification, create a temporary NuGet
configuration outside the service repositories with the local package directory
and the normal feed:

```xml
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="C:\absolute\package-directory" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

```powershell
dotnet restore <service-solution> --configfile <absolute-path-to-temporary-NuGet.Config>
```

After publication, normal restore works without the local source. Build each
service and compare its OpenAPI schemas; regenerate shared clients only if the
authoritative contract actually changes.

## Pilot verification (2026-09-29)

- Release builds of the complete Field and Cluster solutions passed.
- 155 tests passed: 14 library, 22 Field model, 8 Field isolated service,
  27 Field MCP, 10 Cluster model and 74 Cluster service tests.
- Exported service OpenAPI documents compared equal as JSON objects to their
  pre-extraction versions, including all paths and schemas. Generated clients
  therefore require no changes.
- NuGet contents include the .NET 8 assembly, XML documentation, README and licence;
  the sole package dependency is DataManagement 2.2.0.

This verifies the first two consumers locally; it is not a deployment verification
or a migration of the remaining microservices.
