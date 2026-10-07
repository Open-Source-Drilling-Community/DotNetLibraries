# Earth Gravity curation increment

Catalogue 0.2.0 contains 36 Reviewed entries. Eric Cayeux approved structural directions D1-D3 on 2026-09-26; see [the decision record](CURATION-DECISIONS.md). Eric approved the remaining definitions on the same date, completing this increment. The [initial review](CURATION-REVIEW-2026-09-26.md) preserves the 0.1.0 baseline and recommendations.

| Label | Kind | Specializes | Declared physical quantity |
| --- | --- | --- | --- |
| Coordinate | Noun | — | — |
| Geodetic angular coordinate | Noun | Coordinate | PlaneAngleGeodesic |
| Geodetic latitude | Noun | Geodetic angular coordinate | — |
| Geodetic longitude | Noun | Geodetic angular coordinate | — |
| Depth coordinate | Noun | Coordinate | DepthDrilling |
| Ellipsoidal depth | Noun | Depth coordinate | — |
| Geodetic position with ellipsoidal depth | Noun | Geodetic position | — |
| Total gravity vector | Noun | — | — |
| Total gravity acceleration | Noun | — | AccelerationDrilling |
| Total gravity potential | Noun | — | EarthGravityPotential |
| Gravity evaluation request | Noun | — | — |
| Gravity evaluation response | Noun | — | — |
| Gravity sample | Noun | — | — |
| Gravity model provenance | Noun | — | — |
| Model name | Noun | — | — |
| Model identifier | Noun | — | — |
| Model publisher | Noun | — | — |
| Model release date | Noun | — | — |
| Coefficient data version | Noun | — | — |
| Spherical-harmonic degree | Noun | — | — |
| Spherical-harmonic order | Noun | — | — |
| Calculation runtime version | Noun | — | — |
| Reference ellipsoid | Noun | — | — |
| Includes centrifugal acceleration | Noun | — | — |
| Coefficient SHA-256 | Noun | — | — |
| North component | Role | — | — |
| East component | Role | — | — |
| Down component | Role | — | — |
| Vector magnitude | Role | — | — |
| Input positions | Role | — | — |
| Output samples | Role | — | — |
| Model provenance role | Role | — | — |
| WGS84 ellipsoid convention | Reference | — | — |
| Local North-East-Down | Reference | — | — |
| Gravity evaluation result | Noun | — | — |
| Geodetic position | Noun | — | — |

Physical quantities on parents are inherited. A dash in this table means no directly declared quantity, not necessarily an unresolved quantity.

Next: extract additions from EarthMagneticField and review them against this approved baseline. New concepts remain Proposed until separately approved. Preserve stable IDs and retain evidence; aliases do not authorize substitution.

## Digest increment — 0.3.0 (unpublished)

The 36-entry EarthGravity baseline above is retained. Eric Cayeux approved the following four additions on 2026-09-27; the source catalogue now contains 40 Reviewed entries.

| Label | Kind | Specializes | Declared physical quantity |
| --- | --- | --- | --- |
| File content digest | Noun | — | — |
| SHA-256 file digest | Noun | File content digest | — |
| Coefficient file | Role | — | — |
| Model metadata file | Role | — | — |

The existing Coefficient SHA-256 noun now specializes SHA-256 file digest while preserving its narrower meaning. See CURATION-DECISIONS.md for the migration to noun/role bindings.

## EarthMagneticField increment — 0.3.0 (unpublished)

All 18 entries in [the approved EarthMagneticField vocabulary](CURATION-EARTHMAGNETICFIELD-2026-09-26.md) were approved on 2026-09-27 and added as Reviewed. Together with the EarthGravity baseline and digest increment, the catalogue now contains **58 Reviewed entries**. Gravity model provenance now specializes Scientific model provenance; its ID and definition are retained.

## EarthVerticalDatum increment — 0.4.0 (unpublished)

See [the EarthVerticalDatum proposal and complete binding map](CURATION-EARTHVERTICALDATUM-2026-09-27.md). Fourteen EarthVerticalDatum entries were approved on 2026-09-27, extending the 58 existing Reviewed entries without changing them. All 72 entries are Reviewed. Geoid undulation uses LengthStandard with 1 mm meaningful display precision.

## EarthGeodesy increment — 0.5.0 (unpublished)

The [approved EarthGeodesy vocabulary](CURATION-EARTHGEODESY-2026-09-27.md) adds 50 Reviewed entries to the unchanged 72 Reviewed entries in published 0.4.0. Eric Cayeux approved the decisions on 2026-09-28, with InverseFlattening and HelmertScaleDifference bound to their new UnitConversion 3.4.3 quantities. All 122 entries are Reviewed. The review includes quantity choices, representation exceptions, a binding map and provider contract issues for the next integration step.

## 2026-09-28 — EarthCartographicProjection vocabulary approval (0.6.0)

Approver: Eric Cayeux. Evidence: “The proposed semantic catalogue can now refer to this new quantity and can be considered as curated. Prepare it for compilation and nugetification.” The 34 additions in CURATION-EARTHCARTOGRAPHICPROJECTION-2026-09-28.md are Reviewed, extending the unchanged 122 definitions in published 0.5.0 to 156.

ProjectionScaleFactor binds to the published UnitConversion 3.4.5 quantity, with SI 1 and meaningful display precision 1e-9. Conversion.DrillingEngineering 3.4.5 supplies general Conversion transitively. The full projection factor remains distinct from HelmertScaleDifference. Two-dimensional positions, CRS/conversion/method distinctions, origin roles, reference conventions and generic parameter exceptions are accepted. UnitConversion owns display precision; it does not imply scientific accuracy or stored-value rounding.

This increment prepares source and the NuGet package without publishing it. Provider/importer corrections and REST/MCP integration remain separate work; vocabulary approval does not certify current provider behavior. Future concepts still require curation.

## 2026-10-05 — Trajectory REST/MCP vocabulary (0.13.0)

The [Trajectory review](CURATION-TRAJECTORY-2026-10-05.md) adds 96 Reviewed definitions to the unchanged 421-entry 0.12.0 baseline. The review treats REST and MCP as two publications of the same domain meanings and covers the full Trajectory controller/model surface without turning transport mechanics into vocabulary.

The physical-quantity audit found that all unit-bearing fields resolve through existing UnitConversion quantities. No new physical quantity or package dependency is introduced. Provider annotations and publication remain separate operations.

## 2026-10-05 — Unit Conversion REST/MCP vocabulary (0.14.0)

The [Unit Conversion review](CURATION-UNITCONVERSION-2026-10-05.md) adds 60 Reviewed definitions to the unchanged 517-entry 0.13.0 baseline. It models the authoritative metrology domain—physical quantities, dimensions, units, affine definitions, hierarchy, unit systems and conversion results—without turning REST or MCP transport mechanics into vocabulary.

No new physical quantity is required. Generic converted values inherit the caller-selected quantity dynamically, and conversion coefficients and precision metadata intentionally have no fixed `quantityName`. Provider annotations and publication remain separate operations.

## 2026-10-06 — Persisted calculation-case lifecycle vocabulary (0.15.0)

The [calculation lifecycle review](CURATION-CALCULATION-LIFECYCLE-2026-10-06.md) adds 19 Reviewed definitions to the 577-entry 0.14.0 baseline and compatibly clarifies `CalculationCase`, `CalculationState` and `CalculationProgress`. It distinguishes caller-controlled specifications from server-derived results; immediate execution from queued execution; full case retrieval from lightweight status retrieval; and complete results from manifests and stable chunks.

No new physical quantity is required. `CalculationProgress` remains `ProportionStandard` in SI unit `1`. Provider operation annotations and microservice REST/OpenAPI/MCP description updates are deliberately deferred to the next phase.

## 2026-10-06 — Calculation-case deletion vocabulary (0.16.0)

The [calculation-case deletion review](CURATION-CALCULATION-CASE-DELETION-2026-10-06.md) adds one Reviewed operation role to the unchanged 596-entry 0.15.0 baseline. `CalculationCaseDeletion` describes permanent removal of a persisted case and its service-owned lifecycle information and results.

Deletion is explicitly distinct from replacement and from cancellation of queued or running work. The provider contract remains responsible for destructive-write safety annotations, authorization, optimistic concurrency, errors and any explicit cancellation guarantee. No new physical quantity or canonical reference is required.
