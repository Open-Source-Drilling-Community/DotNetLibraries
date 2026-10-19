# Circular physical envelopes for positional uncertainty, 2026-10-10

Status: **reviewed at the request of Eric Cayeux on 2026-10-10**.

## Purpose

A user may request the dimensions of a positional-uncertainty ellipse and then ask to include a circular physical extent such as a borehole diameter. This is a geometric envelope operation. It is distinct from changing statistical confidence and from adding arbitrary quantities that happen to share a length unit.

`BoreholeDiameterAtAbscissaEvaluation` is the stateless architecture lookup. It considers declared casing and open-hole intervals, selects the outermost applicable diameter at the supplied along-hole coordinate and returns the contributing component and interval provenance. Boundary handling is part of the provider contract; missing or invalid geometry must not be guessed.

`CircularUncertaintyEnvelopeDilation` is a stateless geometric operation. Its contract supplies a source planar uncertainty ellipse, its projection plane and axis convention, local inclination and azimuth, a circular-envelope diameter, and vertical-section azimuth when that plane is requested. The circular cross-section normal to the borehole tangent is orthogonally projected into the requested plane. The projected circle is generally an ellipse: its semi-major axis is the borehole radius and its semi-minor axis is the radius multiplied by the absolute dot product of the tangent and plane normal.

The exact Minkowski sum of two rotated ellipses is generally not an ellipse. The returned envelope therefore uses the declared `MinimumDeterminantEllipsoidalOuterBoundConvention`: among the standard family

`Q(beta) = (1 + 1/beta) Q1 + (1 + beta) Q2`, for `beta > 0`,

the provider selects the member having minimum determinant. This is a conservative enclosing ellipse within that family. It does not claim to be the exact Minkowski sum or a globally minimum-area enclosing ellipse.

`CircularlyDilatedUncertaintyEnvelope` identifies that result. Its axes and orientation may differ from the source ellipse. The provider must retain provenance for the source statistical result, projected physical cross-section, physical diameter and approximation convention. A borehole provider can bind its existing `BoreholeDiameter` value as the circular extent; the shared operation is not restricted to boreholes.

The perpendicular projection is circular. Horizontal and vertical-section projections use the local station attitude; the vertical case additionally uses the section azimuth. All angles and axes retain the provider-declared projection conventions.

`UncertaintyProjectionPlane` carries the `Horizontal`, `Vertical` or `Perpendicular` selector as a typed value. Evaluators echo it so a consumer can compose a retained uncertainty result with a later geometric evaluator without inferring the plane from property names or conversational wording.

`IntervalStartCoordinate` and `IntervalEndCoordinate` distinguish the half-open interval boundaries returned by a keyed lookup from the evaluated coordinate itself. They are generic coordinate roles and do not imply measured depth, a datum or a unit.
