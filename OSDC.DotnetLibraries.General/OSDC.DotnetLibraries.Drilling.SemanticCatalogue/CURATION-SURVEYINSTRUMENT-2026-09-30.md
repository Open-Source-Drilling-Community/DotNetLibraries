# SurveyInstrument vocabulary analysis — 2026-09-30

Status: **curated and approved by Eric Cayeux on 2026-09-30 and reconciled with ISCWSA Revision 5.13**. Catalogue 0.12.0 adds 51 definitions to the 370-entry 0.11.0 baseline: 50 Reviewed meanings and one Deprecated legacy representation. Provider integration is implemented in the shared Surveying source; NuGet publication and deployment remain separate operations.

## Evidence and ownership

The review used the shared `OSDC.DotnetLibraries.Drilling.Surveying` `SurveyInstrument`, `ErrorSource`, `ErrorCode`, `ErrorSourceFactory` and covariance implementations together with the SurveyInstrument REST/MCP schemas and WebPages editors. The physical model is owned by the shared Surveying package; the microservice adds persistence, identity/feature assignments, reusable error-source templates and frozen embedded snapshots.

This increment does not change payloads, enum tokens, stored records, calculations or provider behavior. The current provider's 81 `ErrorCode` literals remain its exact finite vocabulary. Catalogue concepts describe the code dimension and reusable error families without renaming those tokens.

## Approved physical conventions

- `Gravity` is the magnitude of total local gravity including centrifugal acceleration.
- `BField` is the magnitude of the total local geomagnetic flux-density vector.
- `Convergence` is positive clockwise from true/geodetic north toward grid north, matching the existing `GridConvergenceTrueToGridClockwise` reference.
- `EarthRotRate` is Earth angular velocity in rad/s.
- Cant angle is measured in the orthogonal tool body frame about the along-hole z axis. The declared positive convention applies while tool inclination is less than or equal to 90 degrees; behavior above 90 degrees remains dependent on the legacy K-operator treatment.
- `GyroRunningSpeed` is along-hole running speed in m/s. `GyroMinDist` is an origin-free along-hole interval in metres. `GyroNoiseRed` and `GyroSwitching` are dimensionless.
- All error magnitudes are nonnegative standard uncertainties expressed as one sigma, not signed corrections. Coordinate origins do not offset uncertainty magnitudes.

## Error classification

ISCWSA propagation mode is a distinct closed dimension with Random, Systematic, Global and Well-by-Well values. Global means fully correlated across all survey stations, all legs and all wells in the applicable project or field. It is not an additional flag layered independently over Random or Systematic.

The current shared model represents Random, Systematic and Global using booleans and has no Well-by-Well property. Catalogue 0.12.0 documents the complete domain vocabulary while provider integration must preserve existing stored flags exactly. Converting the payload to an enforced propagation-mode enum would be a separate compatibility and migration project.

Stationary and Continuous are mutually exclusive gyro operating modes. Stationary tools acquire at a distinct fixed survey station and evaluate weighting functions from that position and its local environment. Continuous tools acquire while moving; drift and random-walk weighting is recursively accumulated from earlier stations as a function of elapsed running distance/time.

`Index` is provider-owned implementation ordering, not an ISCWSA identity. `KOperatorImposed` remains an opaque legacy implementation flag until its behavior is mathematically finalized. `SingularIssues` states only that a weighting-function evaluation may encounter a singular condition.

## Magnitude quantities

The error-code definition determines `Magnitude`'s physical quantity. Catalogue 0.12.0 provides one-sigma concepts for:

- planar angle (`PlaneAngleDrilling`, rad)
- along-hole depth (`DepthDrilling`, m), explicitly origin-free
- acceleration (`AccelerationDrilling`, m/s²)
- magnetic flux density (`EarthMagneticFluxDensity`, T)
- scale factor (`ProportionSmall`, 1)
- angular velocity (`AngularVelocitySurveyInstrumentDrilling`, rad/s)
- angular random walk (`RandomWalkDrilling`, rad/√s)
- reciprocal length (`ReciprocalLengthSurveyInstrumentDrilling`, 1/m)
- angle times magnetic-flux density (`AngleMagneticFluxDensitySurveyInstrumentDrilling`, rad·T); DBH weighting divides this coefficient by magnetic-field magnitude to produce an angle

`MagnitudeQuantity` should eventually be validated against `ErrorCode` or derived from the selected standard definition. Catalogue approval does not itself change the current unrestricted string property.

## AMID and AMIL

`AMID` is the legacy axial magnetic-interference representation and is a planar angular standard uncertainty in radians. Its current factory assignment to `MagneticFlux` in webers is incorrect. The catalogue retains AMID as Deprecated so existing records remain intelligible and points to AMIL as its replacement.

`AMIL` is the current axial magnetic-interference representation and is a magnetic-flux-density standard uncertainty in tesla. The current Surveying `ErrorCode` enum does not contain AMIL. Adding it to SurveyInstrument requires a later shared-Surveying upgrade, regeneration of every affected service contract/client and an explicit compatibility decision; it must not be introduced only in generated or microservice-local code.

## Provider binding matrix

| Provider path | Concept / rule |
|---|---|
| `SurveyInstrument` | `SurveyInstrument` managed resource. |
| `ModelType` | `SurveyInstrumentModel`; the provider discriminator combines MWD/Gyro tool family with a distinct `PositionUncertaintyModel` formulation. |
| `ErrorSourceList` | Embedded frozen error-source snapshots, not live template references. |
| `Dip`, `Declination`, `Gravity`, `BField`, `Convergence`, `Latitude` | Reuse MagneticDip, MagneticDeclination, TotalGravityAcceleration magnitude, EarthMagneticFluxDensity magnitude, GridConvergence with true-to-grid clockwise reference, and GeodeticLatitude. |
| `EarthRotRate` | `EarthAngularVelocity`. |
| `CantAngle` | `SurveyInstrumentCantAngle` with `OrthogonalBodyFrameCantConvention`. |
| `GyroRunningSpeed`, `GyroMinDist`, `GyroNoiseRed`, `GyroSwitching` | Running speed, reinitialization interval, noise-reduction factor and switching parameter respectively. |
| Wolff-de Wård error fields | Corresponding one-sigma concepts; boolean `Use*` fields state model participation. |
| `ErrorSource.ErrorCode` | `SurveyErrorSourceCode`; preserve exact enum token. |
| `Magnitude` | `SurveyErrorMagnitude`, narrowed by error code. |
| `MagnitudeQuantity` | `ErrorMagnitudeQuantityIdentifier`, validated against the error code when provider integration permits. |
| `StartInclination`, `EndInclination`, `InitInclination` | Existing WellboreInclination with applicability-start, applicability-end and initialization roles. |
| propagation and gyro-mode booleans | Preserve existing ISCWSA implementation values; publish their reviewed dimensions without inferring replacements. |
| `Index`, `KOperatorImposed`, `SingularIssues` | Ordering metadata, opaque legacy flag and singularity-risk indicator. |

## Integration boundary

After 0.12.0 is published, SurveyInstrument should reference that exact package version and introduce one provider-owned binding registry keyed by declaring type and property identity. OpenAPI, MCP descriptions and WebPages quantity presentation should consume the same registry. Contract tests should verify the four model branches, exact error-code enum, one-sigma semantics, code-to-quantity compatibility, convergence/cant references, mutually exclusive gyro modes and frozen template/snapshot behavior.

No SurveyInstrument repository file is changed by this vocabulary-only release. No package is published or deployed as part of this curation.
