# Fixed-length double-section geosteering solver

`DoubleSectionByDrilledLengthSolver` solves two commanded sections when both section lengths and the final vertical depth, inclination, and azimuth are imposed.

- `TryCalculateCircularArcs` uses one curvature magnitude and independent upstream/downstream reference toolfaces.
- `TryCalculateConstantCurvatureAndToolfaceArcs` uses one curvature magnitude and independent constant toolfaces.
- `TryCalculateBuildAndTurnArcs` uses independent build and turn rates while matching spatial curvature at the junction.

All lengths and vertical coordinates use SI metres, angles use radians, and rates use radians per metre. The caller supplies the fully defined starting station and may choose an explicit whole-turn azimuth branch for build/turn curves. A valid request can still be geometrically infeasible; the methods return `false` rather than emitting an approximate path outside their residual tolerances.
