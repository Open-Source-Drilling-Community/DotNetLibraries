# EarthMagneticField semantic curation review — 2026-09-26

Prepared for Eric Cayeux. This is the second microservice increment. The published 0.2.0 catalogue remains unchanged, with 36 Reviewed entries. The additions below were approved by Eric Cayeux on 2026-09-27 and are implemented as Reviewed in unpublished catalogue 0.3.0.

## Evidence and scope

Inspected the deployed `earth_magnetic_field_evaluate` and `earth_magnetic_field_get_model_info` MCP schemas on app.digiwells.no, the REST discovery response, and the corresponding local Model DTOs, evaluator, MCP schemas and WebPages calculator. The current service exposes three MCP tools, including ping; it does not yet publish `x-osdc-semantic` annotations.

The engineering scope is evaluation points, samples, requests, responses, installed-model provenance and service discovery. Operational statistics and error envelopes remain outside this vocabulary increment. Discovery declarations such as concurrency support are provider facts, not physical nouns.

Primary sources supporting terminology:

- [NOAA World Magnetic Model](https://www.ncei.noaa.gov/products/world-magnetic-model): the seven geomagnetic elements X, Y, Z, H, F, D and I; I is also called dip.
- [USGS dip diagram](https://www.usgs.gov/media/images/dipgif): geological dip concerns a planar surface relative to horizontal. A shared parent must not make geological plane dip and magnetic vector dip interchangeable.

Concrete source evidence: `EarthMagneticField/Model/EarthMagneticFieldEvaluationPoint.cs`, `EarthMagneticFieldSample.cs`, `EarthMagneticModelInfo.cs`, `EarthMagneticFieldServiceInfo.cs`, `EvaluateEarthMagneticFieldRequest.cs`, `EvaluateEarthMagneticFieldResponse.cs`, `EarthMagneticFieldEvaluator.cs`; `Service/Mcp/Tools/EarthMagneticFieldMcpSchemas.cs`; `WebPages/EarthMagneticFieldCalculation.razor`. Quantity evidence: `UnitConversion/Conversion/EarthMagneticFluxDensityQuantity.cs` and `Conversion.DrillingEngineering/PlaneAngleDrillingQuantity.cs`.

## Curation decisions

| Decision | Recommendation | Compatibility and limits |
| --- | --- | --- |
| M1 — Dip hierarchy | Use **Dip → Magnetic dip**, as previously suggested. Dip is an angular departure from a specified horizontal plane, with the geometric object, measurement section and sign convention supplied by its specialization/context. Magnetic dip concerns the directed magnetic vector, positive down. | Use `MagneticDip` in C# and JSON and “Magnetic dip” in the UI; the user explicitly rejected retaining the former `Inclination` property. Do not make bare “inclination” a global equivalence. Geological dip can later specialize Dip with its own planar geometry and range. |
| M2 — Temporal meaning and bounds | Introduce **Instant**, **UTC** reference, and generic **Lower bound / Upper bound** roles. Bind DateTimeUtc to Instant and model time limits to the same noun plus a bound role. Apply the same bound roles to ellipsoidal depth limits. | An instant is not a duration and should not use UnitConversion.Time. ReleaseDate remains the existing calendar-date concept, even though this provider serializes a nullable DateTime at midnight. Inclusive endpoints and accepted timestamp syntax remain provider rules. |
| M3 — Model provenance reuse | Introduce **Scientific model provenance** as a generic noun and **Geomagnetic model provenance** as its specialization. Add the generic parent to the existing gravity provenance noun in the next catalogue release. | Preserve the existing gravity ID and definition. The existing `model-provenance` ID is a Role, so it must not be reused as a noun parent. Model selection token, scientific model ID, coefficient hash and runtime version retain distinct meanings. |

For this increment, retain the UI's existing application-specific quantity choices: EarthMagneticFluxDensity for magnetic values and PlaneAngleDrilling for dip/declination. Applying PlaneAngleDrilling to Dip is a catalogue display-profile choice, not a claim that dip is drilling-specific. Use that same quantity throughout the Dip lineage: the current validator does not support overriding a parent's PlaneAngle with PlaneAngleDrilling. A later generic-quantity/display-profile separation should be explicit and tested.

## Approved additions

IDs are suffixes of `urn:osdc:semantic:`. All entries below are now **Reviewed** following explicit vocabulary approval. Blank physical quantity means a compound, categorical or temporal concept rather than an unresolved engineering scalar.

| ID / preferred label | Kind; parent | Approved definition and context | Quantity / SI |
| --- | --- | --- | --- |
| `dip` — Dip | Noun; none | Angular departure of a specified geometric object from a declared horizontal plane, measured in a specified vertical section. The specialization supplies object type, section, sign and range. No universal sign/range is imposed here. | PlaneAngleDrilling / rad |
| `magnetic-dip` — Magnetic dip | Noun; dip | Signed angle of a magnetic flux-density vector from its horizontal projection, positive toward the declared downward axis. Requires position, instant, model and local frame. Undefined for the zero vector. | Inherited / rad |
| `magnetic-declination` — Magnetic declination | Noun; none | Signed horizontal angle from geodetic north to the magnetic vector's horizontal projection, positive east. Requires position, instant, model and frame. Undefined when the horizontal projection is zero. | PlaneAngleDrilling / rad |
| `earth-magnetic-flux-density` — Earth magnetic flux density | Noun; none | Component or magnitude of Earth's magnetic flux-density vector. A component or magnitude role specifies which scalar is represented. Requires position, instant and geomagnetic model; components require a frame. | EarthMagneticFluxDensity / T |
| `geomagnetic-vector` — Geomagnetic vector | Noun; none | Earth's magnetic flux-density vector at a position and instant, expressed in a declared frame and evaluated with a declared model. | — |
| `instant` — Instant | Noun; none | A position on a time axis identified using a declared time scale and representation. Neither elapsed duration nor a calendar date alone. | — |
| `geodetic-evaluation-point` — Geodetic evaluation point | Noun; none | A geodetic position paired with an evaluation instant. HasPart position and instant; is not a subtype of position merely because its JSON flattens coordinates and timestamp. | — |
| `geomagnetic-sample` — Geomagnetic sample | Noun; none | An evaluation point paired with a geomagnetic vector and derived magnitudes and orientation angles. HasPart evaluation point, vector, magnetic dip and declination; EvaluatedUsing geomagnetic model provenance. Derived undefined angles are optional values. | — |
| `geomagnetic-evaluation-request` — Geomagnetic evaluation request | Noun; none | A request to evaluate a selected geomagnetic model at supplied evaluation points. Execution and batching policies belong to the provider. | — |
| `geomagnetic-evaluation-response` — Geomagnetic evaluation response | Noun; none | Evaluated geomagnetic samples with the model provenance supporting them. Ordering belongs to the provider. | — |
| `scientific-model-provenance` — Scientific model provenance | Noun; none | Identity and reproducibility information for a scientific model and the implementation/data used to evaluate it. | — |
| `geomagnetic-model-provenance` — Geomagnetic model provenance | Noun; scientific-model-provenance | Identity, applicability and reproducibility information for a geomagnetic reference model. | — |
| `model-selection-token` — Model selection token | Noun; none | Provider-defined token selecting an installed model; its value set and mapping to model identity belong to that provider. | — |
| `horizontal-magnitude` — Horizontal magnitude | Role; none | Nonnegative Euclidean norm of a vector's projection onto the declared horizontal plane. Requires the projection plane. Not equivalent to the full-vector magnitude role. | — |
| `lower-bound` — Lower bound | Role; none | Lower endpoint of a declared ordered domain or interval. The binding identifies the bounded domain and whether the endpoint is included. | — |
| `upper-bound` — Upper bound | Role; none | Upper endpoint of a declared ordered domain or interval. The binding identifies the bounded domain and whether the endpoint is included. | — |
| `input-evaluation-points` — Input evaluation points | Role; none | Evaluation points supplied to an operation. Does not imply ordering, completeness or atomicity. | — |
| `utc` — Coordinated Universal Time | Reference; none | UTC time-scale reference for a timestamp. Accepted serialization and representable precision are provider concerns. | — |

The eighteen entries below and four reusable digest entries are now approved. Catalogue 0.3.0 contains 58 Reviewed entries. No new physical quantity or conversion factor is required. EarthMagneticFluxDensity already supplies a meaningful display precision of 1 nT, which is not a model-accuracy guarantee.

## Complete engineering binding map

| Provider record / field | Approved binding design | Contract clarification |
| --- | --- | --- |
| EvaluationPoint class; Sample.Input | Geodetic evaluation point | Position plus instant, represented by four flattened fields. |
| Latitude / Longitude | Existing geodetic latitude / longitude, WGS84 | SI radians, geodetic rather than geocentric latitude. |
| Depth | Existing ellipsoidal depth, WGS84 | SI metres positive inward along ellipsoid normal. |
| DateTimeUtc | Instant, UTC | Evaluation instant; explicit Z or +00:00 required. |
| Sample class | Geomagnetic sample | Contains input, vector components and derived scalars. It is not solely a vector. |
| North / East / Down | Earth magnetic flux density + existing component roles, NED | SI teslas; ellipsoid-normal Down. |
| HorizontalIntensity | Earth magnetic flux density + horizontal-magnitude, NED | sqrt(North² + East²), nonnegative. Label “Horizontal magnetic flux density”. |
| TotalIntensity | Earth magnetic flux density + existing vector-magnitude | sqrt(North² + East² + Down²), nonnegative. Label “Total magnetic flux density”. |
| Declination | Magnetic declination, NED | atan2(East, North), radians in [-pi, pi]; null at zero horizontal magnitude. This is a circular angle: ordinary arithmetic mean is generally inappropriate. |
| MagneticDip | Magnetic dip, NED | atan2(Down, horizontal magnitude), radians in [-pi/2, pi/2]; null at zero total magnitude. Rename the former Inclination property without a compatibility alias. |
| Request class / Samples | Geomagnetic evaluation request / evaluation point + input-evaluation-points | Batch validation, maximum size and ordering remain provider assertions. |
| Request.Model / ModelInfo.Model | Model selection token | WMM2025 and IGRF14 are enum tokens; do not equate them with the model ID. |
| Response class / Samples | Geomagnetic evaluation response / geomagnetic sample + existing output-samples | Response retains the existing wire shape. |
| Response.Model / ModelInfo class | Geomagnetic model provenance + existing provenance role where applicable | Parent is the new scientific-model-provenance noun. |
| ModelInfo.Name / ID | Existing model-name / model-identifier | e.g. wmm2025 versus WMM2025A, distinct from WMM2025 selection token. |
| ReleaseDate | Existing model-release-date | Calendar date; nullable date-time serialization does not imply a UTC evaluation instant. |
| MinimumUtc / MaximumUtc | Instant + lower-bound / upper-bound, UTC | Bounds of the selected model's supported evaluation-time domain; currently inclusive. |
| MinimumDepth / MaximumDepth | Ellipsoidal depth + lower-bound / upper-bound, WGS84 | Bounds of the supported depth domain, not object start/end coordinates or a thickness. |
| Degree / Order | Existing spherical-harmonic-degree / order | Maximum expansion indices, not angles. |
| GeographicLibVersion | Existing calculation-runtime-version | Distinct from model/coefficient version. |
| ReferenceEllipsoid | Existing reference-ellipsoid | Current provider value WGS84. |
| MetadataSHA256 / CoefficientSHA256 | SHA-256 file digest + model-metadata-file / coefficient-file role | Same algorithm, different source-file purposes. Each value is the digest of the complete file bytes encoded as 64 hexadecimal characters. |
| ModelInfo.Description | Provider documentation | Human description of this model, not a new engineering noun. |
| CoordinateFrame, MagneticFluxDensityUnit, AngleUnit, DepthPositiveDirection | Provider declarations supporting bindings | NED, tesla, radian and down. Keep constants explicit in schemas. |
| ConcurrentEvaluationEnabled | Provider execution capability | Not a physical property or proof that every tool invocation is safe to parallelize. |
| ServiceInfo.Models | Geomagnetic model provenance items | Discovery container remains provider-specific in this increment. |
| ServiceInfo.Name / Description / TimeConvention / DepthReference / CoordinateFrame / DepthPositiveDirection | Provider identity and convention declarations | Service name is not model-name. UTC/WGS84/NED declarations supply context for engineering values. |

Magnetic “intensity” here is B, measured in teslas. It must not be bound to magnetic field strength H in A/m. In geomagnetic notation H also names horizontal B magnitude; that overloaded symbol must not drive automated quantity inference.

## Provider findings and implementation work

1. The evaluator agrees with the above formulas. It converts GeographicLib's east/north/up nanoteslas to north/east/down teslas, maps height = -Depth, and computes decimal year from the UTC instant using that calendar year's length. These are implementation boundary conversions, not public units.
2. MCP requires Model, while the C# request defaults it to WMM2025. Decide whether Model should remain explicitly required everywhere or omission should be a documented default in both interfaces. Recommendation: retain explicit MCP selection and document the REST default as a provider difference initially; do not claim identical requiredness. No behavior change is needed for semantic annotation.
3. UTC/depth validity endpoints are inclusive in source. WMM2025 currently accepts exactly 2030-01-01T00:00:00Z. Preserve current behavior unless a separate model-validity decision changes it. Do not replace source ranges using an external calculator's presentation range.
4. The current manual MCP schemas duplicate modelInfo in evaluation and discovery. When implementing semantic annotations, derive both from the same annotated DTO and verify equivalent annotations across REST and MCP, as in EarthGravity.
5. Add XML descriptions to the presently sparsely documented provenance fields. Label the UI “Magnetic dip” and use magnetic-flux-density labels. Explain nullable angles, time/depth applicability, and release-date versus evaluation-instant meaning in REST/MCP descriptions and README/Home documentation.
6. Use a new catalogue/package version after curation. Do not modify the already-published 0.2.0 vocabulary. Keep new entries Proposed until explicit approval; do not inherit approval from EarthGravity. Then apply attributes, regenerate OpenAPI and shared clients, test units/references/null semantics and REST/MCP parity, and publish/deploy through the normal release workflow.
7. The deployed EarthGravity check identified a shared Swagger middleware defect: Map appended the schema endpoint to the advertised API base. The local middleware and regression test were corrected in both repositories as part of this verification, independently of catalogue curation.

## Composition implications

This increment reuses coordinates, component/magnitude roles, references and provenance fields rather than creating a capability for every magnetic question. It also exposes necessary constraints: component addition requires a compatible basis; horizontal and total magnitudes differ; angular means may need circular statistics; times must lie inside model applicability; a missing orientation angle is not zero. These are suitable preconditions for future generic binding capabilities, not proof that the current catalogue already implements a planner or validates all context automatically.

## Direction approval and naming correction

Eric Cayeux approved M1–M3 with the explicit correction: “do not keep Inclination as it is really confusing and I expect no special compatibility issues. So replace with Magnetic dip.” The resulting C#/JSON name is `MagneticDip`, and the display label is “Magnetic dip”, without a compatibility property. The provider rename is implemented across model, evaluator, REST/MCP contracts, generated client, UI and tests. This records approval of the three structural directions; it does not automatically mark every proposed catalogue definition Reviewed.

## Digest decision — 2026-09-27

The separate model-metadata-sha256 proposal is withdrawn. Use the approved File content digest → SHA-256 file digest hierarchy with coefficient-file/model-metadata-file roles, as recorded in CURATION-DECISIONS.md. This approval applies to the four digest entries; other proposed definitions retain their review status.

## Complete vocabulary approval — 2026-09-27

Eric Cayeux subsequently approved the vocabulary as curated. This supersedes the pending-review status in the historical notes above. All 18 entries in this table are implemented as Reviewed; the binding map is approved design, with provider integration still to implement. See CURATION-DECISIONS.md.
