# Ordered wellbore radial profile

Approved by Eric Cayeux on 2026-10-10.

The architecture evaluator returns every known circular construction boundary at one along-hole abscissa, ordered from outermost to innermost. Coincident values remain separate when they carry different meanings: a borehole wall can also be a cement outer boundary, and a cement inner boundary can also be a casing outer boundary.

Each boundary declares its diameter, denomination, adjacent known material kinds, applicable interval, and source component. Casing boundaries may additionally expose the casing body OD and ID, collar OD, grade, material density, and linear mass density already present in the architecture model. Unknown geometry or material remains unknown.

`OutermostKnownPhysicalEnvelopeDiameter` and `InnermostKnownPhysicalEnvelopeDiameter` are symmetric typed scalar projections of the largest and smallest known applicable radial boundaries. They support physical-clearance, projected-envelope and internal-clearance calculations without asserting that missing geometry has zero thickness. Each projection retains the selected boundary's provenance.

`DeepestCasingShoeEvaluation` selects the deepest valid casing-section bottom and returns its along-hole abscissa together with the same ordered radial profile evaluated at that shoe. The result therefore supports questions about shoe depth, outer or inner diameter, material and construction provenance without introducing separate evaluators for each projection.

The evaluator is a stateless keyed read. The vocabulary does not prescribe REST routes, MCP tool names, storage, or implementation-specific casing-table traversal.
