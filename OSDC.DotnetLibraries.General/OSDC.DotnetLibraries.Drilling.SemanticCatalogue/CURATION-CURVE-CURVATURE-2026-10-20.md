# Curve curvature characteristics

Approved on 2026-10-20 for reusable curve and compound-curve calculation results.

`curve-curvature-characteristics` describes curvature over a provider-declared curve domain. Its curvature components retain the `wellbore-curvature` measurand and use the generic roles `minimum-value`, `junction-value`, and `maximum-value`. These roles denote attained calculated values; they are distinct from statistical bounds, operating limits, and interval endpoints.

For trajectory double-curve calculations, the domain contains the actively steered upstream and downstream sections and excludes any lead-in continuation. Circular-arc and constant-curvature/toolface solutions have equal minimum, junction, and maximum curvature. Constant-build/turn solutions evaluate their continuous curvature profile over both sections and report its attained extrema and the continuous junction value.

The same characteristic object is intended for trajectory reconnection, target landing, geosteering, directional-control analysis, and future curve calculations. Solved-section role value concepts preserve detailed provenance without making the characteristic object operation-specific.
