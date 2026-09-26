# Earth Gravity curation increment

All entries are Proposed. Provider-owned bindings are implemented; shared definitions remain available for review before a catalogue release is approved.

| Label | Kind | Specializes | Physical quantity |
| --- | --- | --- | --- |
| Coordinate | Noun | — | — |
| Geodetic angular coordinate | Noun | Coordinate | PlaneAngleGeodesic |
| Geodetic latitude | Noun | Geodetic angular coordinate | — |
| Geodetic longitude | Noun | Geodetic angular coordinate | — |
| Depth coordinate | Noun | Coordinate | DepthDrilling |
| Ellipsoidal depth | Noun | Depth coordinate | — |
| Geodetic position | Noun | — | — |
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

## Decisions to review

- Confirm the initial URN namespace and the distinction between Noun, Role and Reference.
- Confirm that DepthDrilling is a quantity assignment, not the parent noun of every depth position. Coordinate specialization inherits reference requirements and non-additivity.
- Confirm the Earth Gravity UI choices PlaneAngleGeodesic, DepthDrilling and AccelerationDrilling. Their UUIDs are resolved from the conversion NuGets.
- TotalPotential now binds to the authoritative EarthGravityPotential quantity in UnitConversion, with dimensions m²/s² and display precision 0.01 m²/s². Its geodetic sign convention and gravitational plus centrifugal meaning remain explicit semantic constraints; vocabulary curation status remains Proposed.
- WGS84 here identifies the existing ellipsoid convention, not a fully specified frame realization/epoch. The actual provider conventions remain explicit.
- Harmonic Degree and Order are integers rather than angle values. Model ID is not assumed to be a UUID.
- Input/output sample order and atomic evaluation are provider constraints; they do not establish that arbitrary collections are complete.

## Extension rules

New services should reuse IDs only when definitions and requirements apply. Propose new specializations or separate concepts for different meanings, retain source evidence, and add regression tests. Schema bindings are provider-owned. Change a shared meaning only through an explicit versioned migration; aliases do not authorize substitution.
