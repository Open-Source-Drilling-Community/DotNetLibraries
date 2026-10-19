# Aggregation and ordered-collection query vocabulary review — 2026-10-18

Version 0.22.0 adds four reviewed meanings needed to compose parameterized trajectory aggregation with provider-neutral collection queries.

The two tolerance nouns bind the existing scalar Trajectory aggregation inputs precisely. `EpsilonL` controls constancy classification of the smoothed osculating-circle centre coordinates and uses `LengthStandard` in metres. `EpsilonKappa` controls constancy classification of smoothed curvature, build-rate and turn-rate series and uses `CurvatureDrilling` in radians per metre. Neither tolerance claims to be a final trajectory-error bound.

`SequencePosition` identifies an ordinal within a provider-declared ordered collection. `ExactCollectionCardinality` identifies an exact count tied to one complete logical collection. These roles are independent of trajectories and support generic selection, counting, adjacency and run-length queries over any semantically exposed ordered collection.

Sweep bounds, increments, Cartesian expansion and query operators remain consumer planning concepts. Providers expose scalar inputs and complete structured results; they do not need a dedicated sweep endpoint or question-specific count operations.
