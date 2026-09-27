# EarthVerticalDatum curation review — 2026-09-27

Catalogue 0.4.0 is unpublished and contains 72 Reviewed entries. Eric Cayeux approved the 14 EarthVerticalDatum additions on 2026-09-27, with LengthStandard for geoid undulation. Provider integration and deployment are separate steps. The published 0.3.0 package remains unchanged.

## Evidence

Inspected EarthVerticalDatum's README, all engineering DTOs, evaluator and MCP tool schemas, plus calculator/model-page unit labels. The service offers two inverse stateless batch conversions, model information and ping. GeographicLib egm84-30 is loaded with cubic interpolation. Existing latitude, longitude, ellipsoidal depth, model identity/runtime, input/output roles, UTC Instant and digest concepts are reused.

[GeographicLib GeoidEval documentation](https://geographiclib.sourceforge.io/html/GeoidEval.1.html) distinguishes interpolation/quantization errors relative to the specified geoid from model accuracy against reality. The new error concept covers that representation error. Source fields MaximumInterpolationError/RMSInterpolationError come directly from GeographicLib MaxError/RMSError; they are not total vertical-position uncertainty.

## Approved curation decisions

1. **Geoid-referenced depth**, specializing Depth coordinate, is the shared noun. The provider's `MeanSeaLevelDepth` binds to it with the EGM84 model geoid reference. Retain the property initially, but explicitly describe it as EGM84 geoid-referenced depth; do not treat all meanings of MSL as equivalent. Wgs84EllipsoidalDepth reuses Ellipsoidal depth and WGS84.
2. **Geoid undulation** is signed surface separation, positive upward; use the existing LengthStandard quantity, with meaningful display precision 0.001 m (1 mm), not DepthDrilling. The forward rule is D_ellipsoid = D_geoid - N; the inverse is D_geoid = D_ellipsoid + N. These transformations need references and sign conventions, not merely matching metre units.
3. **Angular grid spacing** has canonical SI radians, but current `GridResolutionMinutes` uses arcminutes. Thirty arcminutes equals pi/360 radians. Recommendation for the provider upgrade: replace the misleadingly unit-bearing field with `AngularGridSpacing` in radians, and display arcminutes through UnitConversion. This contract change is approved but not performed here. If retaining the old field, explicitly declare its wire unit and conversion: the current SemanticAttribute API only publishes the concept's SI unit and cannot alone correctly label a non-SI wire value. Do not annotate a raw value of 30 as radians.
4. **Geoid representation error + statistic roles** covers the maximum and RMS fields using Length/metres. It is not coordinate depth, a generic confidence interval, or model accuracy. DatasetTimestamp is a role on Instant with UTC, distinct from ReleaseDate; the header timestamp's event meaning should be documented without guessing it.

## Curated vocabulary

IDs below have prefix `urn:osdc:semantic:`. No new UnitConversion quantity is needed. All entries below are Reviewed.

| ID | Label | Kind; parent | Quantity / SI | Definition |
| --- | --- | --- | --- | --- |
| `geoid-referenced-depth` | Geoid-referenced depth | Noun; depth-coordinate | inherited DepthDrilling / m | Depth coordinate positive downward from a specified model geoid; the negative of the corresponding geoid-referenced height. |
| `geoid-undulation` | Geoid undulation | Noun; — | LengthStandard / m | Signed height of a specified model geoid relative to a specified reference ellipsoid at a horizontal position, positive when the geoid is above the ellipsoid. |
| `geoid-depth-position` | Position with geoid-referenced depth | Noun; — | — / — | Position represented by geodetic latitude and longitude with a depth relative to a specified model geoid. |
| `geoid-model-provenance` | Geoid model provenance | Noun; scientific-model-provenance | — / — | Identity and reproducibility information for a geoid model, its sampled data and evaluation method. |
| `angular-grid-spacing` | Angular grid spacing | Noun; — | PlaneAngleGeodesic / rad | Angular separation between adjacent grid nodes along a declared coordinate axis. |
| `geoid-approximation-error` | Geoid representation error | Noun; — | Length / m | Magnitude statistic of errors from a sampled geoid representation and its interpolation relative to the reference geoid model. |
| `maximum-absolute-error` | Maximum absolute error | Role; — | — / — | Role identifying a reported maximum absolute error over the stated evaluation domain. |
| `root-mean-square-error` | Root mean square error | Role; — | — / — | Role identifying the square root of the mean squared errors over the stated sample population. |
| `interpolation-method` | Interpolation method | Noun; — | — / — | Named method used to estimate values between sampled data points. |
| `dataset-timestamp` | Dataset timestamp | Role; — | — / — | Role identifying an instant supplied as a timestamp of a dataset; the provider declares the event represented. |
| `vertical-datum-conversion-request` | Vertical datum conversion request | Noun; — | — / — | Request to express supplied vertical coordinates relative to another declared vertical reference. |
| `vertical-datum-conversion-response` | Vertical datum conversion response | Noun; — | — / — | Converted vertical-coordinate samples with the model provenance used for their conversion. |
| `vertical-datum-conversion-sample` | Vertical datum conversion sample | Noun; — | — / — | An input position paired with a converted vertical coordinate and the reference-surface separation used in the conversion. |
| `egm84-geoid` | EGM84 model geoid | Reference; — | — / — | The geoid represented by the EGM84 model, used by this provider as its mean-sea-level reference surface. |

## Provider binding map

| Provider record/property | Approved semantic binding |
| --- | --- |
| EarthVerticalDatumPosition | Position with geoid-referenced depth; not the existing ellipsoidal-depth position noun |
| Wgs84ToMeanSeaLevelPosition | Existing Position (ellipsoidal-depth representation) |
| Latitude / Longitude (both inputs/echoes) | Existing geodetic latitude / longitude, WGS84, PlaneAngleGeodesic/rad |
| MeanSeaLevelDepth (input or result) | Geoid-referenced depth, EGM84 geoid, inherited DepthDrilling/m |
| Wgs84EllipsoidalDepth (input or result) | Existing Ellipsoidal depth, WGS84, DepthDrilling/m |
| GeoidUndulation (both directions) | Geoid undulation, LengthStandard/m; EGM84-to-WGS84 reference pair documented by provider |
| Both request types / Positions | Vertical datum conversion request; position item concept appropriate to direction + existing InputPositions role |
| Both sample types | Vertical datum conversion sample; Position property bound to the appropriate input-position noun |
| Both response types / Samples / Model | Vertical datum conversion response; sample + OutputSamples; geoid model provenance + Provenance role |
| EarthVerticalDatumModelInfo | Geoid model provenance, specializing Scientific model provenance |
| Name / ID / GeographicLibVersion / ReferenceEllipsoid | Existing ModelName / ModelId / RuntimeVersion / ReferenceEllipsoid |
| DataDateTime | Existing Instant + DatasetTimestamp role, UTC; nullable, not a physical duration |
| GridResolutionMinutes | Angular grid spacing, after a declared arcminute-to-radian conversion or explicit wire-unit support |
| Interpolation | Interpolation method; current provider value cubic |
| MaximumInterpolationError / RMSInterpolationError | Geoid representation error + MaximumAbsoluteError / RootMeanSquareError, Length/m |
| CoefficientSHA256 | Existing SHA-256 file digest + CoefficientFile role; hashes complete PGM grid bytes |
| Description | Provider model documentation; no physical quantity |
| SupportedVerticalDatums / SupportedConversionDirections | Provider discovery declarations identifying reference surfaces and supported direction pairs; not quantities |
| DepthPositiveDirection / IsThreadSafe | Provider convention/execution declarations; no physical quantity |
| Ping Status / Service | Operational strings, no physical quantity |

For surface separation, one Reference attribute cannot express two independent reference surfaces. Do not invent an equivalence between the EGM84 geoid and WGS84. Carry the pair in provider descriptions and model provenance; richer structured reference-pair bindings remain future work. Similarly, plain HasPart relationships do not encode operation direction or nullable/cardinality rules.

## Contract and UI follow-up after curation

- Add model attributes and export matching REST/MCP annotations only after resolving the arcminute wire representation. Preserve the approved 0.3.0 concepts without changing their definitions.
- Document geoid/reference distinctions, both conversion equations and error-estimate scope in DTO XML, MCP schemas and README/Home pages.
- Replace raw model-page error values with Length-aware unit controls. Grid spacing must use angular units, never time minutes. The calculation page currently uses DepthDrilling for actual depths, which is appropriate; geoid undulation is a signed separation and should use LengthStandard when displayed.
- Share model-info definitions across tools, regenerate clients and schemas, and test both conversion directions, reference/sign consistency, unit metadata and REST/MCP parity.
- Do not infer new physical quantities from operational validation-error codes, counters or provider identity fields. These remain outside the engineering vocabulary increment.

No EarthVerticalDatum source, payload, package reference or deployment was changed while preparing this increment. The vocabulary is approved; future additions still require explicit curation.

## Approval record

Eric Cayeux: “Can you use LengthStandard for the Geoid undulation such that it has a reasonable meaningful precision? Otherwise I accept your proposed additions. Consider the new vocabulary as curated.” This approves the definitions, hierarchy, references, roles and directions above with that quantity correction. LengthStandard owns the 1 mm display precision; it is not a statement of geoid accuracy and does not round stored or transmitted values. Geoid representation error retains Length.
