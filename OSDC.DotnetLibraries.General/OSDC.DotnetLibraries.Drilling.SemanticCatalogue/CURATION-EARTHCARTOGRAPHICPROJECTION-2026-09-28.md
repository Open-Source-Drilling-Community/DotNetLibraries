# EarthCartographicProjection curated vocabulary — 2026-09-28

Status: **approved and implemented in source 0.6.0**, not yet published.
Eric Cayeux accepted these 34 additions on 2026-09-28 and authorized binding to
the published ProjectionScaleFactor quantity. All 156 entries are Reviewed; the
122 entries in published 0.5.0 remain unchanged. Provider integration is separate.

## Evidence and scope

Inspected EarthCartographicProjection's domain models, generated REST contract
(47 schema types), the 11 MCP tool definitions, projection engine, EPSG importer,
validation and WebPages quantity choices. The packaged EPSG 13.101 catalogue has
47 methods, 5,778 projected CRS definitions and 39 distinct parameter identities.
This is a horizontal 2D conversion service on one unchanged datum. A web workflow
can separately call EarthGeodesy for datum transformation; that does not turn the
projection service itself into a datum-transforming service.

Primary references:

- [PROJ projection operations](https://proj.org/en/stable/operations/projections/index.html)
- [PROJ Transverse Mercator parameters](https://proj.org/en/stable/operations/projections/tmerc.html)
- [PROJ Krovak parameters](https://proj.org/en/stable/operations/projections/krovak.html)
- [GDAL axis mapping rules](https://gdal.org/en/stable/development/rfc/rfc73_proj6_wkt2_srsbarn.html)

The first establishes projection as conversion. The method documents distinguish
full scale factors and origin offsets. GDAL's mapping rules concern axis order;
they are not a guarantee that coordinates are converted to metres or that every
authority axis direction becomes east/north.

## Reuse from 0.5.0

Reuse Latitude, Longitude, Coordinate, CoordinateReferenceSystem,
GeodeticReferenceObject, CoordinateOperation, CoordinateOperationMethod,
CoordinateOperationParameter, GeographicBoundingBox and its four boundary roles,
GeodeticUsage, AuthorityIdentifier, CatalogEntryProvenance, Instant/Utc,
InputPositions, OutputPositions, SourceReference, TargetReference, LowerBound and
UpperBound. `AreaOfUse` combines usage/scope and geographic extent; a duplicate
area noun is unnecessary for this increment.

Do **not** bind a 2D geographic position to existing `Position` or
`GenericGeodeticPosition`: both published definitions require a vertical
coordinate. Do not modify those meanings to accommodate the new provider.
Do not reuse NED vector-component roles for map coordinates, or
GeodeticTransformation for a same-datum projection conversion.

## Approved additions

IDs use `urn:osdc:semantic:` followed by the suffix in the first column. A dash
means no engineering quantity; parent quantities/context are inherited. Context
requirements in the definitions are mandatory for later provider bindings.

| ID suffix | Kind / parent | Quantity / SI | Meaning and required context |
| --- | --- | --- | --- |
| geographic-position-2d | Noun | — | Latitude/longitude pair in a specified geographic CRS; datum, prime meridian, axis/sign and angular units required. No vertical value implied. |
| projected-position-2d | Noun | — | Pair of map-plane coordinates in a specified projected CRS and serialization convention. No height/depth implied. |
| projected-coordinate | Noun / Coordinate | PositionDrilling / m | Signed map-plane coordinate relative to the declared grid axes and origin; coordinate, not an additive length. |
| easting | Noun / ProjectedCoordinate | inherited | Projected coordinate positive in the declared grid-east direction, with CRS and origin explicit. |
| northing | Noun / ProjectedCoordinate | inherited | Projected coordinate positive in the declared grid-north direction, with CRS and origin explicit. |
| geographic-coordinate-reference-system | Noun / CoordinateReferenceSystem | — | CRS using ellipsoidal latitude/longitude and optionally a vertical coordinate; provider declares dimensionality. |
| projected-coordinate-reference-system | Noun / CoordinateReferenceSystem | — | CRS derived by map projection from a base geographic CRS, including conversion and Cartesian coordinate system. |
| coordinate-conversion | Noun / CoordinateOperation | — | Coordinate operation changing representation while retaining the underlying datum/frame. |
| cartographic-projection | Noun / CoordinateConversion | — | Conversion between ellipsoidal geographic coordinates and map-plane coordinates under a specified method and parameters. |
| cartographic-projection-method | Noun / CoordinateOperationMethod | — | Mathematical projection method, distinct from a configured conversion or projected CRS instance. |
| cartographic-projection-parameter | Noun / CoordinateOperationParameter | — | Method-defined projection parameter whose identity determines meaning, representation, range and quantity. No universal physical quantity. |
| projection-parameter-definition | Noun | — | Specification of a parameter's identity, permitted representation, requiredness and bounds; distinct from an assigned value. |
| coordinate-system | Noun | — | Axes and mathematical rules defining coordinate measurement and ordering, distinct from a CRS tied to a datum. |
| projected-coordinate-system | Noun / CoordinateSystem | — | Cartesian coordinate system of a projected CRS, with explicit axis directions, order and units. |
| coordinate-system-axis | Noun | — | One axis with name, abbreviation, direction, unit and serialization order. |
| coordinate-axis-direction | Noun | — | Direction of an axis in its declared frame; an enum value is not an azimuth or a physical quantity. Polar axes require meridian/orientation context. |
| linear-unit-conversion-factor | Noun | — | Multiplicative conversion coefficient between explicitly identified source/target linear units. UnitToMetre means metres per source unit, not map scale. |
| grid-convergence | Noun | PlaneAngleGeodesic / rad | Signed angular relation between geodetic north and grid north at a specified position in a specified projection; explicit sign convention required. |
| projection-scale-factor | Noun / CartographicProjectionParameter | ProjectionScaleFactor / 1 | Full multiplicative scale factor at the method-defined origin/line/parallel; unity means no local scale change. Not Helmert's increment s or a universal pointwise distortion result. |
| projection-angular-parameter | Noun / CartographicProjectionParameter | PlaneAngleGeodesic / rad | Angular method parameter; parameter identity distinguishes latitude, longitude, azimuth, cone angle or grid rotation. |
| projection-linear-parameter | Noun / CartographicProjectionParameter | LengthSmall / m | Linear method parameter, such as a false-origin offset or projection-plane height; not automatically a projected position. |
| forward-projection-request | Noun | — | Request to convert ordered geographic positions to projected positions using one definition. |
| inverse-projection-request | Noun | — | Request to convert ordered projected positions to geographic positions using one definition. |
| projection-result | Noun | — | Projection evaluation result with definition/CRS identity, conventions, ordered corresponding positions and applicability. Shared by both directions. |
| projection-position-result | Noun | — | One corresponding geographic/projected pair with position index and nullable convergence. Input/output roles depend on direction. |
| natural-origin | Role | — | Method-defined natural origin; cannot be substituted for a false origin or projection centre. |
| false-origin | Role | — | Method-defined false origin for assigned coordinate offsets. |
| projection-centre | Role | — | Method-defined projection centre. |
| standard-parallel | Role | — | Method-defined standard parallel; distinct from an arbitrary latitude or grid spacing. |
| first-standard-parallel | Role / StandardParallel | — | First of the method's ordered standard parallels. |
| second-standard-parallel | Role / StandardParallel | — | Second of the method's ordered standard parallels. |
| pseudo-standard-parallel | Role | — | Method-defined pseudo standard parallel; not asserted to be a true standard parallel. |
| grid-convergence-true-to-grid-clockwise | Reference | — | Positive clockwise from true/geodetic north to grid north at the evaluated position. Null represents unavailable, not zero. |
| projected-easting-northing-si | Reference | — | Canonical grid easting then northing, positive grid east/north and in SI metres, independent of authority serialization and units. Must be enforced before annotation. |

Implemented relations: GeographicPosition2D HasPart Latitude/Longitude;
ProjectedPosition2D HasPart Easting/Northing; ProjectionPositionResult HasPart both
positions and GridConvergence; CartographicProjection EvaluatedUsing
CartographicProjectionMethod; ProjectedCoordinateSystem HasPart
CoordinateSystemAxis. Neither a CRS nor a method is a subtype of a position.

## Physical quantities and precision decision

- Easting/northing: reuse `PositionDrilling`, 0.01 m meaningful display precision,
  matching the current UI. This does not limit stored or calculated precision.
- Geographic and projection angles, convergence: reuse `PlaneAngleGeodesic`.
  Its display precision is not the accuracy of the finite-difference convergence
  algorithm, which currently uses a latitude increment of `1e-7` radians.
- Linear projection parameters: retain `LengthSmall`, 0.00001 m display precision,
  matching the editor. Offsets and coordinates need not use identical display
  precision. Parameter identity, not dimensionality, governs allowed arithmetic.
- **Approved general Conversion engineering quantity:
  `ProjectionScaleFactorQuantity : DimensionlessQuantity`, meaningful precision
  `1e-9`.** The existing `DimensionLessStandard` precision is `0.001`, inadequate
  for values such as UTM's `0.9996`. A `1e-9` scale step represents 1 mm over a
  1,000 km linear extent. This is a presentation choice, not an accuracy claim.
  Use the full dimensionless factor, with no automatic ppm-increment conversion.
  `HelmertScaleDifference` and `InverseFlattening` have different meanings and
  must not be reused solely for their precision. `ImageScale` is dots/metre and
  is dimensionally unsuitable. No new length or angle quantity is needed.

ProjectionScaleFactorQuantity is published in UnitConversion 3.4.5. The catalogue
references Conversion.DrillingEngineering 3.4.5, which brings general Conversion
transitively, and resolves the authoritative quantity and unit identities at
runtime. No physical-quantity UUID is duplicated in the vocabulary.

## Provider binding map

| Provider surface | Binding / handling |
| --- | --- |
| GeographicCoordinate | GeographicPosition2D; Latitude/Longitude in radians, with the enclosing base CRS/datum and prime meridian. |
| ProjectedCoordinate | ProjectedPosition2D; Easting/Northing with projected CRS and enforced canonical SI convention. |
| ProjectionDefinition, summaries/references | ProjectedCoordinateReferenceSystem, despite the DTO name. This object is not just a projection method. |
| BaseGeographicCrs / GeographicCrsInput / GeographicCrsReference | GeographicCoordinateReferenceSystem; datum reference reuses GeodeticReferenceObject. |
| MethodId / ProjectionMethod | CartographicProjectionMethod. Identifier retains authority/code context; internal UUID is not an EPSG code. |
| ConversionIdentifier / ConversionName | CartographicProjection / CoordinateConversion identity. |
| ProjectionParameterDefinition | ProjectionParameterDefinition; MinimumValue/MaximumValue use LowerBound/UpperBound on the identified parameter's domain. |
| ParameterId / ProjectionParameterInput / ProjectionParameterValue | CartographicProjectionParameter. Resolve identity from the selected method; input Value has no local Quantity discriminator. |
| Value versus OriginalValue | Value is intended to be normalized according to the parameter definition; OriginalValue uses OriginalUnit/OriginalUnitName and can be encoded DMS. Do not share a fixed SI annotation between them. |
| Parameters classified Angle/Length/Scale | Use reviewed parameter identity plus method context to specialize meaning. Quantity enum alone is insufficient and currently contains errors. Integer/Boolean are representation categories, not physical quantities. |
| CoordinateSystem, Axes, Direction, UnitToMetre | New coordinate-system/axis concepts and conversion-factor context; retain official axis metadata separately from API values. Order is a one-based serialization ordinal. |
| AreaOfUse / Bounds | Reuse GeodeticUsage and GeographicBoundingBox. Explicitly identify the bounds' reference and prime meridian. West/east may cross the antimeridian. |
| Search ContainsPosition / ContainsBounds | Spatial filters need a common reference across definitions; must not inherit an arbitrary projection datum implicitly. |
| Forward/InverseProjectionRequest | Corresponding request noun, InputPositions role, projected CRS reference. |
| Forward/InverseProjectionResponse | ProjectionResult, OutputPositions; source/target roles reverse with direction. Geographic/projected CRS properties keep their intrinsic CRS meanings. |
| Forward/InverseProjectionPosition | ProjectionPositionResult; distinguish echoed input from calculated output within each pair; GridConvergence has the explicit reference convention. |
| CreatedUtc / ModifiedUtc | Instant, UTC; pinned ExpectedModifiedUtc is also an opaque revision token copied exactly, not a coordinate epoch. |
| AuthorityIdentifier / CatalogProvenance | Reuse the reviewed nouns; identity-match status is not semantic curation or runtime availability. |
| Status enums, runtime restrictions, errors, warnings, pagination, MatchScore and counts | Provider contract information. Explain unitless ranking versus probability/accuracy; no invented physical quantity. |

All 11 tools fit these bindings. Ping/service information have no new scientific
quantity. List/get methods and definitions expose the corresponding entities;
create/update bind their simplified input DTOs; deletion uses an identifier;
forward/inverse use their request and common result meanings. JSON property
names, IDs, limits and lifecycle behavior need no vocabulary-driven renaming.

## Contract issues discovered during curation

These are provider/importer defects or unresolved conventions, not reasons to
encode incorrect meanings in the vocabulary. Resolve them before declaring the
provider semantically correct.

1. **Confirmed live: non-metre projected output is labelled metres.** A stateless
   dev REST calculation on 2026-09-28 for EPSG:2227 at latitude 37.5 degrees,
   longitude -120.5 degrees returned easting **6561666.667** while declaring SI
   metres. This is the CRS's false easting in US survey feet; multiplying by
   `1200/3937` gives **2000000.0001016003 m**. `ProjectionEngine` imports the
   authoritative target CRS and copies output coordinates directly. Normalize
   forward and inverse units explicitly. Add a non-metre regression case.
2. **Confirmed in packaged data/importer: encoded DMS loses correctness at
   floating-point boundaries.** EPSG:2227's original longitude `-120.3` in
   sexagesimal DMS represents -120 degrees 30 minutes = -120.5 degrees. The
   normalized catalogue contains -120.5111111111111 degrees. The floor-based
   `DmsToRadians` can interpret a minute fraction just below 30 as 29 minutes
   plus almost 100 seconds. Correct and regression-test the importer, then
   regenerate affected normalized metadata from authoritative originals.
   PROJ executes its own packaged EPSG definitions, so execution and exposed
   catalogue metadata can currently disagree.
3. **Confirmed classification conflicts.** EPSG:8818 (latitude of pseudo standard
   parallel) is labelled Length, although its OriginalUnit is DMS and Value is
   radians. EPSG:8617/8618 evaluation-point ordinates are labelled Scale despite
   metre OriginalUnit. Do not derive semantics from that enum until corrected.
4. **Coefficient quantities/ranges need method-specific review.** Krovak modified
   coefficients C1-C10 are all labelled Scale and assigned a positive minimum.
   Generic coefficient units do not establish dimensionlessness, positivity or
   common dimensions across polynomial terms. Retain generic parameter identity
   until the defining equations and normalization units have been reviewed.
5. **Reference mismatch in area checks needs resolution.** EPSG geographic bounds
   and the source datum's geographic coordinates are compared directly. Ferro/
   Paris prime-meridian longitudes cannot be compared to Greenwich bounds without
   normalization. Define common search/bounds context and convert consistently;
   distinguish approximate applicability checks from datum transformations.
6. **Official axis direction is not normalized by reordering alone.** The catalogue
   includes westing/southing and 108 axis records mapped to Other. Traditional GIS
   axis mapping does not establish a universal east/north convention for all of
   these. Review sign/permutation and polar orientation, and preserve authority
   orientation information that Other alone cannot describe. The MCP output enum
   currently excludes Other although the domain enum/catalogue allow it.
7. **Scale display precision** needs the new quantity above. Never use a rounded
   editor value to rewrite a stored parameter without an intentional user edit.

No service code, persisted data, deployment or public package has been changed by
this review. The live investigation used stateless calculation only.

## Suggested sequence

1. Completed: curate these nouns, roles, references and quantity choices.
2. Completed in UnitConversion 3.4.5: add/release the scale-factor quantity through the established UnitConversion
   generation workflow.
3. Completed in source 0.6.0: add approved entries, constants and
   inheritance/quantity tests; preserve the 122 existing definitions. Package
   publication remains a separate release operation.
4. Repair the importer and provider conventions above with focused regressions.
5. Bind Model attributes and publish equivalent REST/MCP metadata, including
   explicit method/parameter mappings and representation exceptions. A single
   static annotation on generic Value cannot express every parameter's quantity.
6. Regenerate upstream/downstream contracts and validate standard, non-metre,
   non-Greenwich, polar, inverse, null-convergence and outside-area cases.

The vocabulary and dedicated ProjectionScaleFactor quantity with `1e-9`
meaningful precision are accepted. Steps 4–6 remain provider integration work.

## Inventory of all 39 parameter identities

This table is an extraction of the pinned source catalogue, not an endorsement of its current Quantity labels. EPSG code plus method identity is the binding key. Integer and Boolean exist in the provider enum but do not occur in this extracted set.

| EPSG parameter | Label | Current classification | Approved semantic interpretation / remaining provider work |
| --- | --- | --- | --- |
| 1026 | C1 | Scale | CartographicProjectionParameter; coefficient dimension/range pending method-equation review |
| 1027 | C2 | Scale | CartographicProjectionParameter; coefficient dimension/range pending method-equation review |
| 1028 | C3 | Scale | CartographicProjectionParameter; coefficient dimension/range pending method-equation review |
| 1029 | C4 | Scale | CartographicProjectionParameter; coefficient dimension/range pending method-equation review |
| 1030 | C5 | Scale | CartographicProjectionParameter; coefficient dimension/range pending method-equation review |
| 1031 | C6 | Scale | CartographicProjectionParameter; coefficient dimension/range pending method-equation review |
| 1032 | C7 | Scale | CartographicProjectionParameter; coefficient dimension/range pending method-equation review |
| 1033 | C8 | Scale | CartographicProjectionParameter; coefficient dimension/range pending method-equation review |
| 1034 | C9 | Scale | CartographicProjectionParameter; coefficient dimension/range pending method-equation review |
| 1035 | C10 | Scale | CartographicProjectionParameter; coefficient dimension/range pending method-equation review |
| 1036 | Co-latitude of cone axis | Angle | ProjectionAngularParameter; retain method-specific angular meaning |
| 1038 | Ellipsoid scaling factor | Scale | CartographicProjectionParameter; ellipsoid scaling is distinct from local map scale |
| 1039 | Projection plane origin height | Length | ProjectionLinearParameter; offset/height meaning supplied by parameter identity |
| 8617 | Ordinate 1 of evaluation point | Scale | ProjectionLinearParameter; evaluation-point ordinate; FIX current Scale classification |
| 8618 | Ordinate 2 of evaluation point | Scale | ProjectionLinearParameter; evaluation-point ordinate; FIX current Scale classification |
| 8801 | Latitude of natural origin | Angle | Latitude / NaturalOrigin |
| 8802 | Longitude of natural origin | Angle | Longitude / NaturalOrigin |
| 8805 | Scale factor at natural origin | Scale | ProjectionScaleFactor; location supplied by method/parameter identity |
| 8806 | False easting | Length | ProjectionLinearParameter; offset/height meaning supplied by parameter identity |
| 8807 | False northing | Length | ProjectionLinearParameter; offset/height meaning supplied by parameter identity |
| 8811 | Latitude of projection centre | Angle | Latitude / ProjectionCentre |
| 8812 | Longitude of projection centre | Angle | Longitude / ProjectionCentre |
| 8813 | Azimuth at projection centre | Angle | ProjectionAngularParameter; retain method-specific angular meaning |
| 8814 | Angle from Rectified to Skew Grid | Angle | ProjectionAngularParameter; retain method-specific angular meaning |
| 8815 | Scale factor at projection centre | Scale | ProjectionScaleFactor; location supplied by method/parameter identity |
| 8816 | Easting at projection centre | Length | ProjectionLinearParameter; offset/height meaning supplied by parameter identity |
| 8817 | Northing at projection centre | Length | ProjectionLinearParameter; offset/height meaning supplied by parameter identity |
| 8818 | Latitude of pseudo standard parallel | Length | Latitude / PseudoStandardParallel; FIX current Length classification |
| 8819 | Scale factor on pseudo standard parallel | Scale | ProjectionScaleFactor; location supplied by method/parameter identity |
| 8821 | Latitude of false origin | Angle | Latitude / FalseOrigin |
| 8822 | Longitude of false origin | Angle | Longitude / FalseOrigin |
| 8823 | Latitude of 1st standard parallel | Angle | Latitude / FirstStandardParallel |
| 8824 | Latitude of 2nd standard parallel | Angle | Latitude / SecondStandardParallel |
| 8826 | Easting at false origin | Length | ProjectionLinearParameter; offset/height meaning supplied by parameter identity |
| 8827 | Northing at false origin | Length | ProjectionLinearParameter; offset/height meaning supplied by parameter identity |
| 8830 | Initial longitude | Angle | Longitude; method-defined initial meridian |
| 8831 | Zone width | Angle | ProjectionAngularParameter; retain method-specific angular meaning |
| 8832 | Latitude of standard parallel | Angle | Latitude / StandardParallel |
| 8833 | Longitude of origin | Angle | Longitude; method-defined origin |
