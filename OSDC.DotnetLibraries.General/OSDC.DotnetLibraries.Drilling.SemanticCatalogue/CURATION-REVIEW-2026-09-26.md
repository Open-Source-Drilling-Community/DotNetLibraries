# EarthGravity semantic curation review — 2026-09-26

Prepared for Eric Cayeux. This is a review proposal, not an approval record. All 34 published concepts remain **Proposed**; no catalogue definitions, IDs, versions or bindings have been changed by this review.

## Baseline and evidence

- Published `OSDC.DotnetLibraries.Drilling.SemanticCatalogue` **0.1.0** was downloaded from NuGet and inspected directly. Its vocabulary is identical to the current local `catalogue.json`: 25 nouns, 7 roles and 2 references.
- The published package depends on `OSDC.UnitConversion.Conversion.DrillingEngineering` **3.4.2**. Total gravity potential resolves by canonical name `EarthGravityPotential`.
- Local DotNetLibraries HEAD is `e48f5b7`; the already-fetched `origin/main` has `db2816c`, changing package references only. This review did not merge branches. The local NuGet cache contains an older locally built 0.1.0 package; it must not be treated as evidence of the published package's contents.
- Source evidence: EarthGravity `Model/EarthGravityPosition.cs`, `EarthGravityVector.cs`, `EarthGravitySample.cs`, `EarthGravityEvaluationRequest.cs`, `EarthGravityEvaluationResponse.cs`, `EarthGravityModelInfo.cs`, and `EarthGravityEvaluator.cs`. Bindings are published by `Service/SemanticSchemaFilter.cs` and the MCP tools. Runtime validation and inheritance behavior are in this library's `SemanticCatalogue.cs` and `SemanticAttribute.cs`.
- This review covers the six annotated engineering DTOs. Validation errors, health, metrics and usage-statistics schemas have not been counted as curated engineering vocabulary.

## Previously agreed versus awaiting approval

The conversation explicitly established a shared, versioned catalogue, incremental curation, provisional concepts, semantic specialization distinct from physical quantities, and the `EarthGravityPotential` quantity with meaningful display precision 0.01 m²/s². The user also accepted that total potential values near 62.6 million m²/s² are plausible.

The source implements WGS84 ellipsoidal depth, NED components, and total gravitational plus centrifugal potential. These are verified provider conventions, not evidence that every current wording, URN, hierarchy or curation status has been approved. Physical quantities are resolved through UnitConversion; display precision is not measurement uncertainty or model accuracy.

## Three decisions to make first

| Decision | Issue in 0.1.0 | Recommendation | Compatibility approach |
| --- | --- | --- | --- |
| D1 — Result versus vector | `EarthGravityVector` contains North, East, Down, Magnitude **and TotalPotential**, but its class is bound to Total gravity vector. A scalar potential is not part of an acceleration vector. Gravity sample's definition also omits potential. | Introduce a **Gravity evaluation result** noun containing an acceleration vector and a scalar total potential; bind the existing record to the result noun. Keep the vector noun for the mathematical vector. | Preserve the JSON fields and DTO name initially. Add the new semantic ID and correct the class binding in the next version; update sample relations. A later DTO rename is optional, not needed for curation. |
| D2 — Generic position versus depth representation | Geodetic position currently mandates ellipsoidal **depth**, although other services can represent it with ellipsoidal height. WGS84 reference also bundles ellipsoid, meridian and depth sign. | Distinguish generic geodetic position from a **Geodetic position with ellipsoidal depth** specialization. Define depth as negative ellipsoidal height along the ellipsoid normal. Keep WGS84 as the explicit EarthGravity provider reference. | Because 0.1.0 is published, avoid silently broadening the existing ID. Prefer retaining its narrow meaning with a clearer label and adding a generic parent ID; document all label/binding changes. |
| D3 — Reusable meaning versus API policy | Shared definitions embed EarthGravity-specific ranges, batch atomicity, ordering and synchronous/stateless behavior. Generic labels such as Model name contain gravity-specific wording. | Keep reusable scientific meaning in the catalogue; keep representation and execution guarantees in provider schemas/tool contracts. Retain useful request/result/provenance nouns for now rather than redesigning all metadata at once. | Move provider-specific claims only when the provider still publishes/enforces them. Review whether any removal changes consumer assumptions before release. |

These are recommendations for your approval; no particular answer is assumed.

## Complete vocabulary review

IDs below are suffixes of `urn:osdc:semantic:`. Quantity and SI columns show **effective inherited values**, so a blank quantity in the JSON is not misreported as unresolved. “Retain” means recommended for review, not already approved.

| ID / label | Kind; parent | Current definition | Effective quantity / SI | Review recommendation |
| --- | --- | --- | --- | --- |
| `coordinate` — Coordinate | Noun; — | Position coordinate in an explicitly specified frame. | — / — | Retain reference and direction requirements; non-additivity does not prohibit differences or justified coordinate transformations. |
| `geodetic-angle` — Geodetic angular coordinate | Noun; Coordinate | Angular position on a specified geodetic reference ellipsoid. | PlaneAngleGeodesic / rad | Retain; explicitly distinguish geodetic from geocentric latitude. |
| `geodetic-latitude` — Geodetic latitude | Noun; Geodetic angular coordinate | Angle between the ellipsoid normal and the equatorial plane; north positive. | PlaneAngleGeodesic / rad | Retain definition; put the accepted numeric range in the provider schema. |
| `geodetic-longitude` — Geodetic longitude | Noun; Geodetic angular coordinate | Angular position eastward from the reference meridian. | PlaneAngleGeodesic / rad | Retain; longitude wrap/range belongs to the provider representation. |
| `depth-coordinate` — Depth coordinate | Noun; Coordinate | Depth position relative to an established reference, with a declared direction. | DepthDrilling / m | Retain as a coordinate; do not imply that measured depth and vertical depth are interchangeable. |
| `ellipsoidal-depth` — Ellipsoidal depth | Noun; Depth coordinate | Negative ellipsoidal height: positive downward from the reference ellipsoid. | DepthDrilling / m | Retain; specify distance along the ellipsoid normal, positive inward, D2. |
| `geodetic-position` — Geodetic position | Noun; — | Position specified by geodetic latitude, longitude and ellipsoidal depth. | — / — | Resolve D2: generic position versus the specific latitude/longitude/depth representation. |
| `total-gravity-vector` — Total gravity vector | Noun; — | Local acceleration vector including gravitational and centrifugal contributions. | — / — | Resolve D1: a vector comprises North/East/Down; potential is a separate scalar result. |
| `total-gravity-acceleration` — Total gravity acceleration | Noun; — | Component or magnitude of total gravity acceleration, including centrifugal acceleration. | AccelerationDrilling / m/s^2 | Retain with explicit component/magnitude role; model provenance needed through the containing result. |
| `total-gravity-potential` — Total gravity potential | Noun; — | Total gravitational plus centrifugal potential returned by the gravity model. | EarthGravityPotential / m^2/s^2 | Physical meaning agreed; add position and explicit model/sign convention context. Keep display precision owned by UnitConversion. |
| `gravity-evaluation-request` — Gravity evaluation request | Noun; — | Ordered batch of positions for a stateless synchronous gravity evaluation. | — / — | Resolve D3: separate request meaning from synchronous/stateless/atomic API behavior. |
| `gravity-evaluation-response` — Gravity evaluation response | Noun; — | Model provenance and ordered evaluated samples. | — / — | Resolve D3: retain result meaning; input/output ordering is a provider guarantee. |
| `gravity-sample` — Gravity sample | Noun; — | An evaluated position paired with its total-gravity vector. | — / — | Resolve D1: definition currently omits the potential carried by the actual record. |
| `gravity-model-provenance` — Gravity model provenance | Noun; — | Identity and reproducibility information for an installed gravity model. | — / — | Retain for this domain; propose generic Model provenance parent only when another service needs it. |
| `model-name` — Model name | Noun; — | Human-readable name used to select the gravity model. | — / — | Generic ID but gravity-specific definition: generalize to scientific-model selection name, D3. |
| `model-identifier` — Model identifier | Noun; — | Published model identifier; not necessarily a UUID. | — / — | Retain; confirm wording and identity scope. |
| `model-publisher` — Model publisher | Noun; — | Organization publishing the scientific model. | — / — | Retain; confirm wording and identity scope. |
| `model-release-date` — Model release date | Noun; — | Publication calendar date represented as YYYY-MM-DD. | — / — | Distinguish calendar-date meaning from YYYY-MM-DD representation; evaluator can emit Unknown when metadata is absent. |
| `coefficient-data-version` — Coefficient data version | Noun; — | Version of the installed coefficient dataset. | — / — | Retain; confirm wording and identity scope. |
| `spherical-harmonic-degree` — Spherical-harmonic degree | Noun; — | Maximum spherical-harmonic degree represented by a model; a nonnegative integer, not an angle. | — / — | The field is a maximum degree, not the degree of an individual term; clarify preferred label. |
| `spherical-harmonic-order` — Spherical-harmonic order | Noun; — | Maximum spherical-harmonic order represented by a model; a nonnegative integer. | — / — | The field is a maximum order; clarify label and encode 0 <= Order <= Degree at provider level. |
| `calculation-runtime-version` — Calculation runtime version | Noun; — | Version of the calculation implementation used for reproducibility. | — / — | Retain; confirm wording and identity scope. |
| `reference-ellipsoid` — Reference ellipsoid | Noun; — | Identifier of the ellipsoid used for geographic coordinates. | — / — | The value is an ellipsoid identifier; distinguish identifier from the ellipsoid entity itself. |
| `includes-centrifugal-acceleration` — Includes centrifugal acceleration | Noun; — | Boolean declaration that total gravity includes centrifugal acceleration. | — / — | A boolean declaration, not an acceleration value; retain in this first increment, D3. |
| `coefficient-sha256` — Coefficient SHA-256 | Noun; — | Hexadecimal SHA-256 digest of the installed coefficient file. | — / — | Retain; confirm wording and identity scope. |
| `north-component` — North component | Role; — | Signed vector component positive along the local north axis. | — / — | Retain; confirm wording and identity scope. |
| `east-component` — East component | Role; — | Signed vector component positive along the local east axis. | — / — | Retain; confirm wording and identity scope. |
| `down-component` — Down component | Role; — | Signed vector component positive downward along the local vertical. | — / — | Replace ambiguous local vertical with the declared frame axis; here it is opposite ellipsoid-normal up, not the gravity/plumb direction. |
| `vector-magnitude` — Vector magnitude | Role; — | Nonnegative Euclidean norm of a vector. | — / — | Retain; the Euclidean norm itself is invariant under orthonormal frame rotations. |
| `input-positions` — Input positions | Role; — | Ordered input positions; completeness means the supplied request batch. | — / — | Move ordered-batch/completeness guarantees to the provider binding, D3. |
| `output-samples` — Output samples | Role; — | Output samples in one-to-one input order. | — / — | Move one-to-one ordering guarantee to the provider binding, D3. |
| `model-provenance` — Model provenance role | Role; — | Scientific model information supporting a result. | — / — | Retain; confirm wording and identity scope. |
| `wgs84` — WGS84 ellipsoid convention | Reference; — | WGS84 geodetic latitude/longitude with Greenwich origin and ellipsoidal depth positive downward; not a fully specified terrestrial frame realization or epoch. | — / — | Keep as a provider convention initially; do not equate ellipsoid, meridian, vertical sign, and terrestrial-frame realization. |
| `local-north-east-down` — Local North-East-Down | Reference; — | Local north, east and downward axes at the evaluation position on WGS84; downward is opposite ellipsoidal up. | — / — | Retain; make ellipsoid-normal down explicit. Document the provider convention at poles if consumers need it. |

## Concrete binding review

| Provider field / record | Current binding | Curation outcome proposed |
| --- | --- | --- |
| Position.Latitude / Longitude | Geodetic latitude / longitude; WGS84 | Retain SI radians, north/east positive; provider accepts [-π/2, π/2] and [-π, π]. |
| Position.Depth | Ellipsoidal depth; WGS84 | Retain metres, positive down; explicitly state ellipsoid-normal direction. Negative values are above the ellipsoid. |
| Sample.Position | Geodetic position; WGS84 | Use the agreed depth-specific representation from D2. |
| Sample.Gravity and EarthGravityVector class | Total gravity vector; field has NED reference | Change semantic binding to result concept under D1; preserve payload. |
| Gravity.North / East / Down | Total gravity acceleration + component role + NED | Retain signed m/s². Down is opposite ellipsoidal up. |
| Gravity.Magnitude | Total gravity acceleration + magnitude role + NED | Retain nonnegative norm; frame metadata describes source components, not a directional scalar. |
| Gravity.TotalPotential | Total gravity potential → EarthGravityPotential | Retain m²/s² and geodetic sign; relate to position and model provenance through result/sample/response. |
| Request.Positions / Response.Samples | Position + input role / Sample + output role | Retain item semantics; collection cardinality and one-to-one ordering remain provider assertions. |
| Response.Model | Gravity model provenance + provenance role | Retain. Distinguish source model, coefficient dataset and calculation-library versions. |
| ModelInfo's eleven fields | Eleven metadata nouns | Keep identities explicit; refine maximum degree/order, ellipsoid identifier and missing-date semantics. |

The evaluator converts radians to degrees for GeographicLib, passes height = -Depth, maps East/North/Up to North/East/Down, and retains returned potential. The magnitude is computed from the three components. Latitude and longitude ranges, finite coordinates, nonempty batches and maximum batch size are enforced by provider validation.

## Follow-up implementation after decisions

1. Apply approved semantic definitions and additive IDs; keep a dated decision record with approver, affected IDs, rationale and release version. Record rejected/deferred proposals as well.
2. Mark only individually approved concepts `Reviewed` (the existing enum value); do not mark the whole graph reviewed automatically. A reviewed dependent concept should not rely on an unresolved meaning without an explicit recorded exception.
3. Complete structural relations for result, sample and provenance. Existing `HasPart` edges do not yet encode collection cardinality, component role or concrete schema paths. Context requirements are text and are not automatically satisfied by a runtime binding. Do not claim general composition is already machine-validated.
4. Preserve the separation of quantity hierarchy and noun hierarchy. The current validator rejects different quantity names anywhere in one noun lineage, even if UnitConversion specializes those quantities. Decide an explicit compatibility rule before introducing such a lineage; do not bypass the check.
5. Add precise source evidence per concept and test inherited quantities, required context, scalar/vector separation, and matching REST/MCP metadata. Regenerate provider schemas after binding changes.
6. Release a new package version; do not replace published 0.1.0. Align embedded catalogue version and package version, update EarthGravity's dependency and report the resulting schema-version/binding changes. Clean up obsolete conditional package references and stale dependency documentation as a separate mechanical follow-up.
7. Continue with EarthMagneticField, extracting only additions/differences and reusing approved concepts where meanings match. Proposed dip/magnetic dip concepts and magnetic-versus-geodetic angle distinctions can then be reviewed with that service's actual evidence.

## Remaining policy choices

- Keep `urn:osdc:semantic:` as the permanent identity namespace; aliases aid discovery but never establish equivalence. Recommended: yes.
- Use `Reviewed` to mean explicitly approved for catalogue use, with dated human attribution in the decision record. Recommended: yes; automated extraction alone stays Proposed.
- Keep UnitConversion's application-specific quantity choices in this first increment (`PlaneAngleGeodesic`, `DepthDrilling`, `AccelerationDrilling`, `EarthGravityPotential`). Revisit a generic-quantity versus display-profile split only if the next services provide a concrete need.

No deployment, source-code change, catalogue status transition or NuGet publication is part of preparing this review.
