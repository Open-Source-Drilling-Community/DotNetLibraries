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
