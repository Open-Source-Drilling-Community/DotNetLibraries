# Trajectory REST/MCP vocabulary analysis — 2026-10-05

Status: **curated from the deployed Trajectory domain and requested by Eric Cayeux on 2026-10-05**. Catalogue 0.13.0 adds 96 Reviewed definitions to the unchanged 421-entry 0.12.0 baseline. Packaging, publication and provider annotation are separate operations.

## Evidence and scope

The review used the authoritative Trajectory model, controllers, generated OpenAPI model and MCP registration/schema generator. REST and MCP expose the same controller operations and domain payloads, so the catalogue defines one meaning for each domain concept rather than transport-specific duplicates. It covers:

- survey runs, measurements, corrections, references and calculated stations;
- actual, planned and definitive trajectories assembled from survey-run sections;
- interpolation, uncertainty ellipses, aggregation and stochastic realizations;
- fixed-length, reconnect, geosteering and explicit well-path extrapolation;
- target landing, target-plane boundaries and curve-specific steering controls;
- directional-control samples, linked residuals, bundles and distributions;
- survey-run and trajectory minimum-distance calculations;
- octree indexing/search, global anti-collision and immutable policy revisions;
- survey batch import, dependency validation and dependency-closed backup/restore.

Identity/feature catalogs, resource metadata, optimistic-concurrency timestamps, validity periods and common external resources reuse existing catalogue concepts. HTTP verbs, route templates, pagination mechanics, chunk indices and MCP tool names are transport mechanics and are intentionally not modeled as drilling-domain meanings.

## Core distinctions

- A `SurveyMeasurement` is an observed/imported/calculated directional input. A `SurveyStation` is the calculated directional and Cartesian state of a path. They are not synonyms.
- A `SurveyRun` owns measurements and calculated stations for one acquisition/planning run. A `Trajectory` combines ordered sections from one or more runs for one wellbore.
- Along-hole depth, true vertical depth, local north/east coordinates and length extents remain distinct. Canonical storage/reference rules continue to come from the drilling reference profile.
- Circular arc, constant curvature and toolface, and constant build and turn are distinct section semantics. In particular, turn rate is not assumed to equal a spatial-curvature component near vertical inclination.
- Geological, driller and reachable target boundaries are different meanings. Reachability may legitimately yield multiple contours or holes; the vocabulary does not assert that every displayed boundary is one simply connected polygon.
- Directional-control residual means actual minus expected. Curvature/toolface and build/turn are linked component pairs. Toolface residual statistics are circular.
- Centerline distance, borehole clearance and anti-collision separation factor are different quantities. A negative clearance denotes overlap; separation factor is dimensionless and policy/confidence dependent.

## Physical-quantity review

No new UnitConversion physical quantity is required. Every numeric engineering concept introduced here resolves to a published quantity already used by the working Trajectory Web UI:

| Meaning | Existing quantity | SI |
|---|---|---|
| Along-hole intervals and true vertical depth | `DepthDrilling` | m |
| Local position and centerline separation | `PositionDrilling` | m |
| Geometric extents, tolerances and clearance | `LengthStandard` | m |
| Inclination, azimuth, toolface and bearing | `PlaneAngleDrilling` | rad |
| Curvature, build rate and turn rate | `CurvatureDrilling` | rad/m |
| Confidence, progress and separation factor | `ProportionStandard` | 1 |
| Borehole diameter | existing `BoreholeDiameter` concept / `DiameterPipeDrilling` | m |

Residual distribution statistics inherit the quantity of the residual component being summarized; the generic distribution object therefore does not assert one universal physical quantity. Instants, identifiers, states, counts, hashes and enum discriminators are not physical quantities.

The tests resolve every quantity family through the authoritative UnitConversion libraries. This guards against introducing a semantic name that silently depends on an absent or noncanonical quantity.

## REST and MCP binding map

| Contract family | Primary concepts |
|---|---|
| `SurveyRun*`, measurement chunks and correction fields | SurveyRun, SurveyMeasurement, SurveyStation, SurveyMeasurementCorrection and reference conventions |
| `Trajectory*`, search and station chunks | Trajectory, TrajectorySurveyRunSection, TrajectoryCalculationMethod and survey-station geometry |
| `InterpolatedTrajectory*` | InterpolatedTrajectory, InterpolationInterval, MaximumChordArcDistance |
| `SurveyStationEllipseCalculation*` | SurveyStationUncertaintyEllipse and its horizontal, vertical and perpendicular specializations |
| `TrajectoryAggregationCase*` | TrajectoryAggregationCase and the three section types |
| `TrajectoryRealizationCase*` | TrajectoryRealizationCase and TrajectoryRealization |
| `TrajectoryExtrapolationCase*` | TrajectoryExtrapolationCase, its four enforced variants and SolvedTrajectorySection |
| `TargetLandingCase*` | TargetLandingCase, TargetPlane, target-boundary specializations and LandingControlPoint |
| `DirectionalControlEvaluationCase*` | DirectionalControlEvaluationCase, sample, residual specializations, bundle and distribution summary |
| survey-run/trajectory minimum-distance endpoints | MinimumDistanceCalculation, CenterToCenterDistance, ClearanceDistance and ClosestApproachToolface |
| octree endpoints and search jobs | OctreeTrajectoryIndex and OctreeCandidateSearch |
| global anti-collision and policy endpoints | GlobalAntiCollisionCalculation, SeparationFactor, AntiCollisionPolicyRevision and FieldAntiCollisionPolicyAssignment |
| batch import/export/restore and reference audits | SurveyRunBatchImport, DependencyClosedTrajectoryBackup and ExternalReferenceValidation |

## Provider integration boundary

This increment expands only the shared vocabulary. The Trajectory microservice should subsequently bind its model properties to these stable IDs from one provider-owned registry, then publish the same metadata into OpenAPI and MCP schemas. That integration must not change payload serialization, persisted SI values, routes, calculation behavior or optimistic-concurrency semantics.

Large result arrays remain addressed through the existing light objects and chunk endpoints. Semantic annotation does not replace payload-conscious contract design.
