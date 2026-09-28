# EarthGeodesy curation — proposed 2026-09-27, approved 2026-09-28

Source version **0.5.0**, unpublished. The 72 Reviewed definitions from 0.4.0 are unchanged. This increment adds **50 Reviewed definitions**, approved by Eric Cayeux on 2026-09-28. No provider integration or publication is performed.

## Approved vocabulary

| Label | Kind | Parent | Quantity / SI | Definition |
| --- | --- | --- | --- | --- |
| Authority identifier | Noun |  | — | Identifier consisting of an authority namespace and an object code, optionally qualified by registry version and URI. |
| Catalogue entry provenance | Noun |  | — | Source, source version and identity-matching evidence for a catalogue definition. |
| Ellipsoid axis length | Noun |  | LengthStandard / m | Length of a principal semi-axis of a reference ellipsoid. |
| Semi-major axis | Role |  | — | Role identifying the equatorial semi-major axis a of an oblate ellipsoid of revolution. |
| Semi-minor axis | Role |  | — | Role identifying the polar semi-minor axis b of an oblate ellipsoid of revolution. |
| Inverse flattening | Noun |  | InverseFlattening / 1 | Reciprocal of ellipsoidal flattening f=(a-b)/a for a non-spherical ellipsoid. |
| Reference ellipsoid definition | Noun |  | — | Definition of an ellipsoid of revolution used as a geometric reference surface, including its dimensions and shape. |
| Prime meridian | Noun |  | — | Meridian defining the origin from which geodetic longitudes are measured. |
| Prime-meridian longitude | Noun | Geodetic longitude | — | Angular position of a selected prime meridian measured east from a declared comparison meridian. |
| Geodetic reference object | Noun |  | — | A terrestrial geodetic reference frame or datum ensemble used to reference coordinates. |
| Geodetic reference frame | Noun | Geodetic reference object | — | Realization of a terrestrial reference system that establishes the origin, scale and orientation of coordinates. |
| Static geodetic reference frame | Noun | Geodetic reference frame | — | Geodetic reference frame in which defining station coordinates are treated as invariant with time. |
| Dynamic geodetic reference frame | Noun | Geodetic reference frame | — | Geodetic reference frame whose defining coordinates evolve with time, referenced to a stated frame epoch. |
| Geodetic datum ensemble | Noun | Geodetic reference object | — | Collection of reference-frame realizations considered equivalent for applications within a stated ensemble accuracy. |
| Datum ensemble accuracy | Noun |  | LengthStandard / m | Stated positional difference tolerance associated with treating ensemble members as equivalent. |
| Coordinate reference system | Noun |  | — | Coordinate system related to the Earth through a datum, reference frame or datum ensemble. |
| Coordinate operation domain | Noun |  | — | Coordinate representation and dimensionality expected by an operation, such as geographic 2D, geographic 3D or geocentric 3D. |
| Geodetic epoch | Noun | Instant | — | Time position at which a geodetic frame, coordinates or realization is defined or evaluated. |
| Coordinate epoch | Noun | Geodetic epoch | — | Epoch to which the coordinates of a position refer. |
| Frame reference epoch | Noun | Geodetic epoch | — | Epoch at which the defining coordinates of a dynamic reference frame are specified. |
| Anchor epoch | Noun | Geodetic epoch | — | Epoch associated with the anchor definition of a reference frame. |
| Realization epoch | Noun | Geodetic epoch | — | Epoch associated with realization of a geodetic reference frame as documented by the defining authority. |
| Geographic bounding box | Noun |  | — | Geographic extent bounded by southern and northern latitudes and western and eastern longitudes. |
| Southern boundary | Role |  | — | Southern latitude boundary of a geographic extent. |
| Northern boundary | Role |  | — | Northern latitude boundary of a geographic extent. |
| Western boundary | Role |  | — | Western longitude boundary of a geographic extent, respecting its wrap convention. |
| Eastern boundary | Role |  | — | Eastern longitude boundary of a geographic extent, respecting its wrap convention. |
| Geodetic usage | Noun |  | — | Purpose and spatial extent for which a geodetic definition is intended to be used. |
| Coordinate operation | Noun |  | — | Specified operation that maps coordinates from a source CRS to a target CRS. |
| Geodetic transformation | Noun | Coordinate operation | — | Coordinate operation relating coordinates referenced to different geodetic frames or ensembles. |
| Coordinate operation method | Noun |  | — | Identified mathematical method specifying how coordinate-operation parameters are applied. |
| Coordinate operation parameter | Noun |  | — | Method-defined numeric or file-valued parameter of a coordinate operation. |
| Geocentric translation | Noun |  | LengthStandard / m | Signed translation component between geocentric Cartesian coordinate frames. |
| Helmert rotation | Noun |  | PlaneAngleGeodesic / rad | Signed rotation parameter about a geocentric Cartesian axis in a Helmert transformation. |
| Cartesian X component | Role |  | — | Role identifying the X axis component in a declared Cartesian frame. |
| Cartesian Y component | Role |  | — | Role identifying the Y axis component in a declared Cartesian frame. |
| Cartesian Z component | Role |  | — | Role identifying the Z axis component in a declared Cartesian frame. |
| Helmert scale difference | Noun |  | HelmertScaleDifference / 1 | Dimensionless increment s of the multiplicative scale factor 1+s in a Helmert transformation. |
| Coordinate operation accuracy | Noun |  | Length / m | Stated estimate of positional accuracy attributable to a coordinate operation. |
| Transformation path | Noun |  | — | Ordered sequence of directed coordinate operations connecting a source CRS or datum to a target. |
| Transformation path accuracy | Noun |  | Length / m | Provider-derived accuracy estimate for a selected sequence of coordinate operations. |
| Ellipsoidal depth change | Noun |  | LengthStandard / m | Signed difference of output and input ellipsoidal depths for corresponding transformed positions. |
| Maximum absolute value | Role |  | — | Role identifying the largest absolute value over a stated collection or domain. |
| Source reference | Role |  | — | Role identifying the reference object or CRS in which operation input coordinates are expressed. |
| Target reference | Role |  | — | Role identifying the reference object or CRS in which operation output coordinates are expressed. |
| Output positions | Role |  | — | Role identifying the positions returned by an operation. |
| Coordinate transformation request | Noun |  | — | Request to transform positions between specified coordinate references. |
| Coordinate transformation response | Noun |  | — | Result of transforming positions, including the applied path and interpretation of returned depths. |
| Transformation path candidate | Noun |  | — | Proposed transformation path with applicability, executability and selection evidence. |
| Transformation selection token | Noun |  | — | Opaque provider-issued token identifying a checked transformation selection in its bound request context. |

## Approved decisions

1. **Ellipsoid definition versus identifier.** Published `ReferenceEllipsoid` means an identifier, as used by the earlier services. Keep it unchanged and use `ReferenceEllipsoidDefinition` for the EarthGeodesy object containing axis lengths and shape. Do not make the definition a subclass of an identifier.
2. **Frames and ensembles.** `GeodeticReferenceObject` covers both frames and ensembles. Static and dynamic frames specialize `GeodeticReferenceFrame`; an ensemble is a sibling of that frame concept. A CRS adds coordinate-system axes/units and is not a synonym for a datum. Bind the provider's `GeodeticDatum` to the broad noun; refine by `ReferenceObjectType` when known. A catalogue-defined datum is a Noun; the actual provider datum UUID/authority code supplies dynamic reference context, not a newly minted catalogue Reference for every EPSG entry.
3. **Precision choices.** Axis lengths, ensemble accuracy, translations and depth changes use LengthStandard. Geodetic rotations use PlaneAngleGeodesic. Inverse flattening now uses InverseFlattening (1e-9 meaningful precision); Helmert scale difference uses HelmertScaleDifference (1e-12). Both specializations are supplied by general Conversion 3.4.3, transitively through Conversion.DrillingEngineering 3.4.3. They retain dimensionless SI values and leave numeric conversions unrounded. EarthGeodesy's inverse-flattening editor should adopt the new quantity during provider integration. Operation and path accuracy retain Length. Precision remains a presentation convention, not an accuracy claim.
4. **Epochs.** Coordinate, frame-reference, anchor and realization epochs are distinct specializations of GeodeticEpoch, itself an Instant. They have no duration quantity. CoordinateEpochUtc is explicitly UTC. Numeric frame/anchor epochs are retained from EPSG and require their decimal-year/calendar convention to be documented; do not label them seconds or assume their time scale is UTC. RealizationEpoch is an authority-supplied string and cannot be treated as a parsed instant without qualification.
5. **Mixed parameter representations.** Explicit TranslationX/Y/Z, RotationX/Y/Z and ScaleDifference are SI. In contrast, imported Parameters[].Value retains the EPSG unit identified by Parameters[].Unit/UnitName. Its meaning is method/parameter-dependent: lengths, angles, scale, rates, epochs and file-valued parameters cannot share one physical quantity. The generic parameter concept deliberately has none. Detailed parameter-specific binding must resolve authority identity and unit first.
6. **Depth change and accuracy.** Depth change is output minus input in the declared depth policy, not a geoid undulation or an error. MaximumAbsoluteDepthChange uses MaximumAbsoluteValue, not MaximumAbsoluteError. CombinedAccuracy follows the provider's root-sum-square convention; it does not establish independent uncertainties or a guaranteed bound.
7. **Geographic bounds.** Bounds use WGS84/Greenwich while position longitude uses the selected source/target prime meridian. Use explicit western/eastern roles rather than numeric lower/upper roles, because a bounding box may cross the antimeridian.

## Binding map for later provider integration

Bindings below apply to corresponding summary, reference and create/update representations where present. Their semantic meanings are approved; provider attribute integration remains outstanding.

| Provider type or fields | Approved semantic binding / interpretation |
| --- | --- |
| ReferenceEllipsoid, ReferenceEllipsoidSummary, EllipsoidReference, Create/UpdateReferenceEllipsoidRequest | ReferenceEllipsoidDefinition; request wrapper remains a provider operation contract |
| SemiMajorAxis / SemiMinorAxis | EllipsoidAxisLength + SemiMajorAxis / SemiMinorAxis role; SI m |
| InverseFlattening | InverseFlattening quantity, dimensionless SI 1; provider zero sentinel means sphere |
| ReferenceEllipsoidId, DatumReference.ReferenceEllipsoid | Existing ReferenceEllipsoid identifier noun for ID; ReferenceEllipsoidDefinition for expanded object |
| GeodeticDatum, GeodeticDatumSummary, DatumReference | GeodeticReferenceObject; specialize through ReferenceObjectType |
| MemberDatumIds | References to GeodeticReferenceFrame members, not coordinates or distances |
| PrimeMeridianName / PrimeMeridianIdentifier | Name/authority identity of PrimeMeridian |
| PrimeMeridianLongitude | PrimeMeridianLongitude, PlaneAngleGeodesic, rad east of Greenwich |
| FrameReferenceEpoch / AnchorEpoch / RealizationEpoch | Corresponding epoch noun; explicit provider representation required |
| EnsembleAccuracy | DatumEnsembleAccuracy, LengthStandard, m |
| GeodeticUsage; Usage[].Scope / Extent | GeodeticUsage, with purpose and geographic extent descriptors |
| GeodeticPosition | Existing Position (geodetic position with ellipsoidal depth) |
| Latitude / Longitude / Depth | Existing Latitude / Longitude / EllipsoidalDepth; quantities PlaneAngleGeodesic / PlaneAngleGeodesic / DepthDrilling; context from source or target datum |
| CoordinateEpochUtc | CoordinateEpoch + Utc reference; ISO 8601 UTC, no duration quantity |
| GeographicBoundingBox / AreaOfUseBounds | GeographicBoundingBox, WGS84/Greenwich radians |
| SouthLatitude / NorthLatitude | Latitude + SouthernBoundary / NorthernBoundary |
| WestLongitude / EastLongitude | Longitude + WesternBoundary / EasternBoundary; wrapped interval permitted |
| GeodeticTransformation, summaries, TransformationReference, DatumTransformationConnection | GeodeticTransformation (connection is a reference/projection) |
| SourceDatumId / SourceDatum; TargetDatumId / TargetDatum | GeodeticReferenceObject + SourceReference / TargetReference |
| Method, MethodIdentifier, MethodName | CoordinateOperationMethod, preserving authority and rotation convention |
| CoordinateReferenceSystemReference; SourceCrs / TargetCrs | CoordinateReferenceSystem + SourceReference / TargetReference |
| Domain | CoordinateOperationDomain; dimensionality is not inferred from a length unit |
| ComponentOperationIdentifiers | Ordered CoordinateOperation references; concatenate with defined directions |
| Parameters[] | CoordinateOperationParameter; numeric and file-valued alternatives remain provider-owned |
| Parameters[].Value, Unit, UnitName, FileReference | Parameter-dependent value, declared unit and resource reference; no blanket SI binding |
| TranslationX/Y/Z | GeocentricTranslation + CartesianX/Y/Z; LengthStandard, m |
| RotationX/Y/Z | HelmertRotation + CartesianX/Y/Z; PlaneAngleGeodesic, rad; explicit position-vector convention for custom operations |
| ScaleDifference | HelmertScaleDifference quantity, dimensionless SI 1; multiplicative scale is 1+s |
| Accuracy | CoordinateOperationAccuracy, Length, m; null means unknown |
| AreaOfUse | Human-readable GeodeticUsage extent; does not guarantee executable applicability |
| TransformCoordinatesRequest / Response | CoordinateTransformationRequest / Response |
| Input Positions / output Positions | Existing Position + InputPositions / OutputPositions role, respectively |
| TransformationPath; candidate Operations | TransformationPath with ordered directed CoordinateOperation members |
| DepthChanges[] | EllipsoidalDepthChange, LengthStandard, m; matches ordered positions |
| MaximumAbsoluteDepthChange | EllipsoidalDepthChange + MaximumAbsoluteValue, LengthStandard, m |
| TransformationPathCandidate | TransformationPathCandidate; keep applicability and runtime executability separate |
| CombinedAccuracy | TransformationPathAccuracy, Length, m; unknown if any constituent is unknown |
| SelectionToken | TransformationSelectionToken; opaque, request-bound and expiring |
| ResolveTransformationPathsRequest / Response | Provider discovery wrappers over positions, reference objects and TransformationPathCandidate; not coordinate transformation results |
| AuthorityIdentifier; Identifier, MethodIdentifier, PrimeMeridianIdentifier, SourceUnit, Unit | AuthorityIdentifier with object category in binding context; authority codes are not provider UUIDs |
| CatalogProvenance | CatalogEntryProvenance; distinct from scientific-model provenance and semantic curation status |
| CreatedUtc / ModifiedUtc | Existing Instant + Utc; creation/modification purpose remains provider-owned |
| RevisionDate / PublicationDate | Authority revision/publication metadata; do not infer an exact UTC instant from a calendar date or unparsed text |

## Provider facts retained outside reusable scientific vocabulary

The following remain documented provider attributes rather than new physical concepts: names/descriptions/aliases, UUIDs and textual authority/version fields, source and matching notes, LegacyId/CanonicalId, IsLegacyCombinedDefinition, catalog match/lifecycle statuses, IsBuiltIn/IsDefault/IsDeprecated/IsSuperseded, Origin, ConventionalReferenceSystem, RealizationMethod, remarks/information sources, and publication/revision representation details.

Search wrappers retain Query, exact authority/code filters, IncludeLegacy/IncludeDeprecated, Offset/Limit, TotalCount/ReturnedCount, MatchScore and MatchReason. These are search controls and ranking evidence, not physical quantities or confidence probabilities. SelectionPolicy, ApplicabilityPolicy, DepthPolicy, TransformationPathIds, MaximumCandidates, Rank/IsRecommended/RecommendationReason, IsAmbiguous/SelectionGuidance, Applicability/ApplicabilityMessage, IsExecutable/ExecutionIssues, IsReversible/AppliedInReverse, RequiresExternalResource/RequiresCoordinateEpoch, DepthReferenceChanged, warnings, service limits, counters and operational metadata remain provider guarantees. Neither catalogue inheritance nor curation enforces those guarantees at execution time.

## Contract issues to address when integrating EarthGeodesy

- Expand documentation for FrameReferenceEpoch, AnchorEpoch and RealizationEpoch, including their representation and source convention.
- State the original-unit exception for imported Parameters[].Value explicitly in REST/MCP. The current broad SI prose can otherwise mislead consumers. Include typed authority-identifier schemas in currently underspecified nested objects.
- Adopt InverseFlattening and HelmertScaleDifference quantities in provider annotations and relevant unit-aware controls; display rounding must not silently change stored values.
- Keep `ReferenceEllipsoid`'s published identifier meaning; bind the new definition concept to ellipsoid objects.
- Attach source/target datum context to coordinates. Do not hardcode the Wgs84 catalogue reference on all EarthGeodesy positions.
- Preserve the zero sphere sentinel, antimeridian wrapping, unknown accuracy and explicit 2D depth policy in provider validation and tests.

These issues are recorded for the next provider-integration step; this increment edits the shared catalogue only.

## Evidence

- Local model: EarthGeodesy/Model/DomainModels.cs; model transformations: GeodeticTransformer.cs.
- MCP schemas: EarthGeodesy/Service/Mcp/Tools/CatalogMcpTools.cs.
- Imported representations: EarthGeodesy/tools/EpsgCatalogGenerator/Program.cs, especially Parameters, datum epochs and Bounds.
- Accuracy aggregation: EarthGeodesy/Service/CatalogViewFactory.cs; depth policy and wrapped bounds: TransformationSafety.cs; UTC to decimal-year conversion: AuthoritativeCoordinateOperationEngine.cs.
- UI quantities: ReferenceEllipsoidEdit.razor, GeodeticDatumEdit.razor, EarthGeodesyCalculation.razor.
- [OGC referencing by coordinates](https://docs.ogc.org/as/18-005r4/18-005r4.html): frame, ensemble, CRS and coordinate-epoch distinctions.
- [OGC CRS WKT](https://docs.ogc.org/is/18-010r7/18-010r7.html): frame epoch and ensemble accuracy representation.
- [PROJ Helmert transformation](https://proj.org/en/stable/operations/transformations/helmert.html): rotation conventions and parameter units; provider SI fields must be distinguished from PROJ command-line units.
