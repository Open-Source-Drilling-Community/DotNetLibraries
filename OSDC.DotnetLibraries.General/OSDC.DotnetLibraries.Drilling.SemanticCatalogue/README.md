# OSDC Drilling Semantic Catalogue

Source **0.16.0** contains **597 concepts: 594 Reviewed and three Deprecated legacy names**. It adds the `CalculationCaseDeletion` operation role to the unchanged 596-entry 0.15.0 baseline. See [the deletion review and provider guidance](CURATION-CALCULATION-CASE-DELETION-2026-10-06.md) and the earlier [calculation lifecycle review](CURATION-CALCULATION-LIFECYCLE-2026-10-06.md).

A shared, locally available semantic vocabulary for OSDC providers and consumers. NuGet identity: `OSDC.DotnetLibraries.Drilling.SemanticCatalogue`, targeting .NET 8. Version 0.16.0 is published on NuGet.org. Its provider bindings are adopted by the reviewed OSDC services; each service remains responsible for binding its concrete fields and operations and publishing those bindings consistently through REST/OpenAPI and MCP.

Current direct provider integrations include Cluster, Well, WellBore, WellBore Architecture, Rig, Earth Gravity, Earth Magnetic Field, Earth Vertical Datum, Earth Geodesy, Earth Cartographic Projection, Field, Survey Instrument, Trajectory, Drilling Fluid, Geological Properties, Geothermal Properties, and Simulator4nDOF. Legacy calculation providers that cannot consume the .NET 8 package publish the same reviewed 0.16.0 URNs through local OpenAPI filters. Unit Conversion is the deliberate package-reference exception: this catalogue depends on Unit Conversion physical-quantity packages and the service cannot take a reverse dependency without forming a package cycle, so its vocabulary remains an external binding map rather than an in-process package reference.

## Canonical drilling references

```csharp
[Semantic(Concepts.TieInAlongHoleDepth,
    ReferenceProfile = SemanticCatalogue.OsdcCanonicalDrilling)]
public double TieInPointAlongHoleDepth { get; set; }
```

This resolves the WGS84 path-intersection along-hole reference automatically. Along-hole zero is the intersection of the parent path (or its defined extension) with the WGS84 ellipsoid, not a vertical elevation offset. MeasuredDepth and TieInMeasuredDepth IDs remain resolvable but are deprecated in favour of AlongHoleDepth and TieInAlongHoleDepth; acquisition provenance is separate.

`SemanticMetadata.Create` supplies the same mechanism for inherited/provider-owned bindings. Metadata emits resolved references, profile identity/version, reference definitions/context and inherited constraints. Contradictory explicit references fail. Metadata explicitly identifies its scope as canonical storage/API, with presentation references allowed. User-selected MSL, ground/mud-line, rotary-table and other supported display references are converted at the presentation boundary; they do not redefine persisted or transmitted values. The profile applies only when the provider selects it; generic geodesy conversions retain their source/target datum. A scalar uncertainty does not inherit its mean's origin. Schema version 2 carries portable referenceProfiles; schema version 1 remains readable without profiles.

## Ownership and curation

`catalogue.json` is the authoritative language-neutral vocabulary, embedded in the assembly and packed alongside `catalogue.schema.json`. `Concepts.cs` exposes stable URN constants; tests ensure these refer to actual entries. The 36 EarthGravity definitions have **Reviewed** status following Eric Cayeuxâ€™s approval on 2026-09-26. Provider annotations remain assertions about their own fields; future concepts start as Proposed.

Each definition has an immutable semantic ID, label, definition, kind, curation status, aliases, parents, quantity name, SI representation, required context, constraints, source evidence, typed HasPart/LocatedAt/EvaluatedUsing/HasInput/Produces/ProjectionOf relationships and optional supersession. Nouns, roles and reference conventions are distinct kinds. Only `parents` expresses specialization. Physical quantity and field role are not parent relationships. Aliases are discovery hints and can return multiple concepts.

Earth Gravity supplies the first examples: geographic coordinates specialize Coordinate; latitude/longitude specialize Geodetic angular coordinate; ellipsoidal depth specializes Depth coordinate. Total gravity is distinct from its component/magnitude roles. WGS84 and local NED are explicit reference conventions. Constraints and context requirements accumulate through inheritance; cycles, missing parents, incompatible kinds and conflicting inherited quantities/units are rejected. A child cannot weaken a parent by removing inherited requirements. This library does not evaluate natural-language constraints or authorize arithmetic.

The catalogue owns reusable meanings. Microservices own bindings to concrete data fields, actual reference conventions and representation. Deployment identities, tool enablements, schema fingerprints and provisional discovery inboxes belong to consumers such as DrillWeaver, not this package. Future concepts should be proposed and curated incrementally; deprecated IDs remain resolvable and may point to replacements. A semantic change requires a versioned release, not silently rewriting an existing meaning.

## Physical quantities

The library references `OSDC.UnitConversion.Conversion.DrillingEngineering` 3.4.5 and obtains the general Conversion library transitively. Quantity identities and SI unit names are resolved through those libraries, not duplicated UUID definitions. Earth Gravity's choices are PlaneAngleGeodesic, DepthDrilling and AccelerationDrilling. Quantities and units do not establish reference frames or additive meaning.

Total gravitational plus centrifugal potential binds to `EarthGravityPotential`, a specialization of the general `GravityPotential` quantity in UnitConversion. Its SI representation is mÂ²/sÂ² (equivalently J/kg); ftÂ²/sÂ² and ftÂ·lbf/lbm are available. The Earth specialization defines a meaningful display precision of 0.01 mÂ²/sÂ², without asserting model accuracy or rounding stored values. Energy density is not a substitute.

## Provider usage

```csharp
[Semantic(Concepts.EllipsoidalDepth, Reference = Concepts.Wgs84)]
public double Depth { get; set; }
```

`SemanticMetadata.For(member)` returns a JSON object for the `x-osdc-semantic` extension. It accepts attributed types, properties and methods, and includes catalogue identity/version, concept, curation status, optional role/reference, required context, SI representation and the resolved physical-quantity identity. Invalid concept/role/reference IDs fail explicitly. `AnnotateObject` applies attributes to an existing JSON schema object and its direct properties; it does not infer paths, recurse through `$ref`, change validation keywords or alter data serialization. Providers apply it to each named/inline model schema and may use method metadata for REST/MCP operations. Earth Gravity demonstrates equivalent MCP and OpenAPI publication from the same attributes.

Consumers can use `SemanticCatalogue.Default`, `Get`, `Find`, `IsA`, `Ancestors`, `Quantity`, `RequiredContext` and `Constraints`. No live catalogue server is required. `ToJson()` exposes the versioned language-neutral catalogue. URNs are stable identities, not assumed HTTP endpoints.

## Build and package

From `OSDC.DotnetLibraries.General`:

```powershell
dotnet test OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest/OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest.csproj -c Release
dotnet pack OSDC.DotnetLibraries.Drilling.SemanticCatalogue/OSDC.DotnetLibraries.Drilling.SemanticCatalogue.csproj -c Release
```

This does not publish the package. Inspect the NuGet dependency manifest and catalogue contents before a separately authorized release.

The repository-level `global.json` selects the .NET 8 SDK. On a machine with only SDK 9 installed, the following commands from `C:\OSDC` were verified for 0.6.0 (the library still targets .NET 8):

```powershell
dotnet test DotNetLibraries/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest/OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack DotNetLibraries/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.SemanticCatalogue/OSDC.DotnetLibraries.Drilling.SemanticCatalogue.csproj -c Release -p:GeneratePackageOnBuild=false
```

Building inside the repository or through Visual Studio requires an SDK compatible with its `global.json`.

## Approved structural directions

See [the decision record](CURATION-DECISIONS.md) for D1-D3 and migration from 0.1.0. A gravity evaluation result contains the vector and scalar potential. The original position ID remains the ellipsoidal-depth representation, specializing a new generic geodetic position. Provider execution and representation guarantees remain in provider contracts. The directions and all 36 individual concepts are approved and recorded as Reviewed.

The [EarthMagneticField curation review](CURATION-EARTHMAGNETICFIELD-2026-09-26.md) prepares the next increment: reused concepts, 18 approved magnetic-field additions and the four approved digest entries and a field-by-field binding map. These were published in 0.3.0; EarthMagneticField now uses published 0.4.0.

## File digests

`FileContentDigest` is specialized by `Sha256FileDigest`. Source-file purpose is expressed separately:

```csharp
[Semantic(Concepts.Sha256FileDigest, Role = Concepts.CoefficientFile)]
public string CoefficientSHA256 { get; set; } = string.Empty;

[Semantic(Concepts.Sha256FileDigest, Role = Concepts.ModelMetadataFile)]
public string MetadataSHA256 { get; set; } = string.Empty;
```

The provider declares the exact file bytes and representation (64 hexadecimal characters in these services). These values have no physical units. The published `CoefficientHash` concept remains available as a narrower SHA-256 file digest specialization. Catalogue 0.3.0 adds four Reviewed entries without changing existing provider property names.

All 58 concepts in the 0.3.0 source catalogue are Reviewed following the vocabulary approval of 2026-09-27. New concepts discovered in future increments still require curation.

## EarthVerticalDatum curation

The [EarthVerticalDatum review](CURATION-EARTHVERTICALDATUM-2026-09-27.md) adds 14 approved entries to the 58 unchanged Reviewed entries from published 0.3.0. Source 0.4.0 contains 72 Reviewed entries. Geoid undulation uses LengthStandard (SI metres, 1 mm meaningful display precision). Geoid/MSL terminology, signed separation, the move to a radians-valued grid-spacing field and interpolation-error scope are curated; provider integration and deployment with published 0.4.0 have been completed.

## EarthGeodesy curation

See [the approved EarthGeodesy vocabulary and binding map](CURATION-EARTHGEODESY-2026-09-27.md) for 50 Reviewed additions in source 0.5.0. The 72 Reviewed entries are unchanged. The approved vocabulary distinguishes ellipsoid definitions from identifiers, frames from ensembles, epochs from durations, SI Helmert fields from original-unit EPSG parameters, and depth changes from geoid undulation. Eric Cayeux approved these additions on 2026-09-28. All 122 entries are Reviewed; future discoveries still start as Proposed. Catalogue 0.5.0 has been published and EarthGeodesy provider bindings have been deployed and verified.

InverseFlattening and HelmertScaleDifference bind to the corresponding general UnitConversion 3.4.3 quantities, with meaningful display precisions 1e-9 and 1e-12 respectively. Their SI representation is dimensionless (1); precision is not scientific accuracy or stored-value rounding.

## EarthCartographicProjection curation

The 34 approved additions distinguish geographic/projected 2D positions, projected CRS, configured conversions, projection methods, parameter definitions, coordinate systems and axes. Origin and standard-parallel roles remain separate from physical quantities. Canonical SI easting/northing and clockwise true-to-grid convergence are explicit reference conventions.

ProjectionScaleFactor uses the full dimensionless factor (SI 1), with UnitConversion-owned meaningful display precision 1e-9. It is distinct from the Helmert scale increment. Projected coordinates inherit PositionDrilling; linear parameters use LengthSmall and angular parameters use PlaneAngleGeodesic. Generic parameters and linear-unit conversion factors require explicit context and do not assert a universal physical quantity.

The curation review retains the provider/importer defects that must be addressed during EarthCartographicProjection integration. Vocabulary approval does not certify the current provider calculations or contracts.

## Field and Cluster vocabulary

The 52 approved entries cover shared identities/classifications, Field/WellCluster/WellSlot,
record and validity-time roles, delineation geometry, WGS84 Riemannian coordinates,
ground/mud-line and water-surface depth, and Gaussian uncertainty. Existing latitude,
longitude, ellipsoidal depth, projected positions and geodetic conversion concepts are
reused. No additional UnitConversion quantity or package dependency is required.

The catalogue does not reference ResourceClassification; providers can reference both
packages without circular dependencies. The shared models and service-owned catalogue
values remain distinct from curated semantic definitions. Semantic annotations for
these models will be introduced during provider integration after publication of 0.7.0.

All 208 concepts in the Field/Cluster increment were Reviewed. Future additions still begin as Proposed.
Tests cover the frozen published definitions, curation status, coordinate and
uncertainty distinctions, quantity resolution and provider metadata generation.

## Well and WellBore vocabulary

The [approved Well/WellBore review](CURATION-WELL-WELLBORE-2026-09-29.md) adds 15 concepts for well/wellbore topology, parent-path measured depth and rig-job history. The confirmed tie-in coordinate is parent-wellbore MD with an explicit origin, not WGS84 vertical depth. Rig-job ends are exclusive, unlike inclusive classification validity ends. Existing DepthDrilling and LengthStandard quantities suffice; package dependencies are unchanged.

Source 0.8.0 embeds all 223 Reviewed definitions. Provider integration must still address the missing MD-origin representation and MCP/editor corrections identified in the review.


## WellBoreArchitecture vocabulary

The [approved WellBoreArchitecture review](CURATION-WELLBOREARCHITECTURE-2026-09-29.md) adds 60 reviewed concepts for construction, geometry, materials, capacities, connectivity and uncertainty. TensileStrength is stress; TensileCapacity uses ForceDrilling. Casing top and cement-top locations are along-hole coordinates; wellhead/hanger and fluid-top locations are vertical. HostComponentAbscissa uses a local host-top/downward origin.

Canonical drilling reference profile **1.1.0** adds the host-relative binding; all existing bindings are retained. Canonical reference metadata applies to storage and APIs, while user-selected presentation references remain supported. Physical extents, diameters, stress, differential pressure and uncertainties have no reference-origin offset. LengthStandard supplies engineering precision for extents; dimensional-length uncertainty is separate from the published coordinate/depth uncertainty definition.

The increment uses existing UnitConversion 3.4.5 quantities. NuGet publication and WellBoreArchitecture integration follow separately; this release adds no provider payload fields or runtime calculations.

## Rig vocabulary

The [approved Rig review](CURATION-RIG-2026-09-30.md) adds 78 reviewed concepts for Rig structures, equipment, engineering capabilities, measurement metadata and limit roles. Vertical depth remains WGS84-positive-downward where the depth concept applies. Elevation is instead positive upward from an explicit origin; standpipe elevations use the drill floor, and DistanceToBit uses the bit front face.

## SurveyInstrument vocabulary

The [approved SurveyInstrument review](CURATION-SURVEYINSTRUMENT-2026-09-30.md) adds 51 definitions for survey instruments, position uncertainty models, tool/model discriminators, error sources, ISCWSA propagation and gyro operating modes, environmental parameters and one-sigma error magnitudes. Fifty are Reviewed; legacy AMID is Deprecated in favour of AMIL. AMID remains a planar-angle uncertainty in radians and must not be interpreted as magnetic flux in webers; AMIL is a magnetic-flux-density uncertainty in tesla. The vocabulary distinguishes the physical tool family, the Position Uncertainty Model (PUM), actual gyro operating state and error-source applicability to one or both gyro modes.

The catalogue preserves the current provider's exact ISCWSA flags and finite `ErrorCode` tokens. Provider integration must not infer new flag values from code names. `MagnitudeQuantity` remains provider data but is semantically constrained to the quantity permitted by the selected error code. Publication and SurveyInstrument provider integration are separate operations.

Pressure ratings specialize AbsolutePressure and inherit the canonical vacuum reference. Equipment Weight fields are curated as mass, heave-compensator capacity as force, controller gains as dimensionless and rotational speeds as angular velocity in rad/s. Depth capacities are extents and deliberately have no datum binding. Measurement range and absolute accuracy retain a dynamic physical quantity declared by their sibling PhysicalQuantity field.

The increment uses existing UnitConversion 3.4.5 quantities and leaves canonical drilling reference profile 1.1.0 unchanged. NuGet publication and Rig provider integration follow separately; no Rig payload, persistence or runtime behavior changes in this vocabulary release.

## Trajectory REST/MCP vocabulary

The [Trajectory review and binding map](CURATION-TRAJECTORY-2026-10-05.md) adds 96 Reviewed concepts for survey runs and measurements, trajectories, interpolation, uncertainty ellipses, aggregation, realizations, extrapolation, target landing, directional-control evaluation, minimum distance, octree discovery, anti-collision policy/calculation, imports, external-reference audits and dependency-closed backup/restore.

REST and MCP are treated as two transports for the same domain contract. Transport operations, route names, chunk indices and pagination controls are not duplicated as drilling-domain concepts. Existing identity, feature, metadata, validity and concurrency meanings are reused.

The physical-quantity audit found no gap. `DepthDrilling`, `PositionDrilling`, `LengthStandard`, `PlaneAngleDrilling`, `CurvatureDrilling`, `ProportionStandard` and the already curated borehole-diameter quantity cover every unit-bearing Trajectory field. Generic residual distributions deliberately inherit their selected component quantity rather than asserting one universal dimension. Version 0.13.0 does not change UnitConversion or the canonical drilling reference profile.

## Unit Conversion REST/MCP vocabulary

The [Unit Conversion review](CURATION-UNITCONVERSION-2026-10-05.md) adds 60 Reviewed concepts for the metrology model exposed through REST and MCP: physical quantities and dimensional exponents, unit choices and affine definitions, unit systems and validated assignments, direct and persisted conversions, precision-aware formatting, hierarchy inheritance and semantic discovery.

No new engineering physical quantity is introduced. Generic conversion values inherit the selected quantity dynamically, while dimensional exponents, conversion coefficients and precision settings are metadata rather than measurements with one fixed quantity. Catalogue 0.14.0 therefore leaves the UnitConversion dependency and canonical drilling reference profile unchanged.

## Persisted calculation-case lifecycle vocabulary

The [calculation lifecycle review and provider guidance](CURATION-CALCULATION-LIFECYCLE-2026-10-06.md) adds 19 Reviewed definitions for caller-controlled specifications, server-derived results, lightweight status projections, diagnostics, result manifests/chunks and operation roles. Immediate and queued execution specialize the same submission and replacement roles; they do not create competing calculation-case resource types.

`CalculationCase`, `CalculationState` and `CalculationProgress` are compatibly clarified. Progress remains an optional `ProportionStandard` value in SI unit `1`; no new physical quantity or canonical reference is introduced. Source 0.15.0 also permits semantic attributes on methods so a provider can publish operation bindings consistently through REST/OpenAPI and MCP. Provider annotations and contract-description updates remain the next, separate phase.
