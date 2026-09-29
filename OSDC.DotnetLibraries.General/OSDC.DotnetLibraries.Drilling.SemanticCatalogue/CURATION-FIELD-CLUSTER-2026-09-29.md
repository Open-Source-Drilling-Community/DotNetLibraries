# Field and Cluster vocabulary - 2026-09-29

Status: **Reviewed**, approved by Eric Cayeux on 2026-09-29. Source version
0.7.0 contains 208 Reviewed concepts: 52 approved Field/Cluster additions and
all 156 unchanged definitions from published 0.6.0.
This increment changes the catalogue only. It does not annotate, publish or
redeploy Field, Cluster or ResourceClassification.

## Approved decisions

1. Reuse one classification vocabulary across services. Identity definitions,
   assignments and values remain distinct; features and memberships specialize
   classification concepts. Service-owned catalogue options remain data, not
   new globally curated semantic nouns. Membership does not imply access rights.
2. A Field is a managed drilling-context resource, not necessarily one reservoir
   or a legal polygon. A WellCluster groups slots and common site context; its
   single-well flag does not make it a Well entity. A slot is distinct from a well
   and wellbore. The optional Field/Rig references are associations, not inheritance.
3. Preserve the established inclusive assignment intervals: null endpoints are
   unbounded and touching intervals overlap. Created/modified instants describe
   records, not physical observation times. Server enforcement remains a provider
   responsibility; the catalogue does not imply stricter validation has been deployed.
4. Riemannian north/east use PositionDrilling in metres and explicit WGS84 arc-length
   conventions. They are neither projected Northing/Easting nor local NED offsets.
   Delineation margin uses LengthStandard; top/bottom limits and ground/mud-line
   and water-surface positions use DepthDrilling via EllipsoidalDepth, WGS84,
   positive downward. No new physical quantity is required.
5. Gaussian wrappers do not have one universal quantity. Means retain the
   underlying latitude/longitude/depth concept; standard deviations use angular
   or linear uncertainty concepts, respectively PlaneAngleGeodesic and
   LengthStandard. Linear uncertainty is a dispersion, not a depth position.
   GroundMudLineDepth retains its combined surface meaning, requiring site
   context to distinguish ground from mud line. TopWaterDepth means the position
   of the water surface, **not water-column thickness**.

## Source inspection

Inspected Field and Cluster Model files, their REST/OpenAPI artifacts and MCP
schema builders; FieldDelineationCalculator and FieldCoordinateConversionService;
ResourceClassification 0.1.0 models and validation; General.Math's
Point3DGlobalCoordinates; DrillingProperties' GaussianDrillingProperty and
General.Statistics' GaussianDistribution. Each new catalogue entry records its
source paths. Source paths describe evidence, not permanent type ownership.

## Binding map for subsequent provider integration

The entries below are the approved vocabulary binding plan, not installed attributes. `[]` means
each item and `{id}` means each dictionary entry. References must identify their
target type and owner in schema descriptions even when they share ResourceIdentifier.

| Provider field/type | Concept and context |
| --- | --- |
| Field / FieldLight | Field; Light is a partial representation, not a new kind of field |
| Cluster / ClusterLight | WellCluster; Light remains the same resource identity |
| Slot | WellSlot; context is the containing cluster |
| MetaInfo | ResourceMetadata |
| MetaInfo.ID; Slot.ID; assignment, option and line IDs | ResourceIdentifier; identify the concrete object type and scope |
| MetaInfo.HttpHostName / HttpHostBasePath / HttpEndPoint | Transport locator metadata; no physical quantity or separate domain noun required |
| Name / Description | ResourceName / ResourceDescription |
| CreationDate / LastModificationDate | Instant with CreationTime / LastModificationTime role; UTC-compatible offset timestamps |
| Field.ProjectionDefinitionID | ResourceIdentifier referring to an EarthCartographicProjection definition; UUID is not an EPSG code or the definition object |
| Cluster.FieldID / ClusterLight.FieldID | ResourceIdentifier referring to the parent Field service resource |
| Cluster.RigID / ClusterLight.RigID | ResourceIdentifier referring to an associated Rig service resource |
| Cluster.IsSingleWell / IsFixedPlatform | SingleWellClusterFlag / FixedPlatformFlag |
| Field/Cluster.ReferencePoint | Existing Position, role ReferenceLocation, reference Wgs84; nullable components do not assert a complete position |
| Cluster.Slots.{id} | WellSlot; dictionary key must match Slot.ID under the provider contract |
| Field/Cluster Identity definitions | IdentityDefinition, retaining service-specific ownership |
| IdentityAssignments[] | IdentityAssignment |
| IdentityAssignment.IdentityID / Value | ResourceIdentifier targeting IdentityDefinition / IdentityValue in that definition's scheme |
| Field/Cluster/Slot feature categories and options | FeatureCategory / FeatureOption |
| Field membership categories and options | MembershipCategory / MembershipOption |
| Category.Options[] | Concrete option concept; category owns its option IDs |
| Category.IsExclusive / HasValidityPeriod | CategoryExclusivity / CategoryValidityPeriodEnabled |
| FeatureAssignments[] / MembershipAssignments[] | FeatureAssignment / MembershipAssignment |
| Assignment.CategoryID / OptionID (with actual Feature/Membership prefix) | ResourceIdentifier with selected category / selected option target context |
| Assignment.FromDate / ToDate | Instant, roles ValidityStart / ValidityEnd; collectively AssignmentValidityPeriod |
| Field.DelineationLines[] | FieldDelineationLine |
| DelineationLine.DelineationLineTypeID | ResourceIdentifier targeting DelineationLineType |
| FieldDelineationLineType | DelineationLineType |
| DelineationLine.Margin | DelineationMargin; LengthStandard, metres |
| DelineationLine.TopDepth / BottomDepth | EllipsoidalDepth, roles TopDepthBoundary / BottomDepthBoundary, Wgs84; metres |
| DelineationLine.Points[] / Boundary.Points[] | Existing Position in Wgs84, ordered vertices; preserve duplicated closing vertex as provider representation |
| CalculatedBoundaryLines[] | CalculatedDelineationBoundary; provider-derived, not caller-authoritative |
| Boundary.IsClosed / IsInteriorBoundary | ClosedLineFlag / InteriorBoundaryFlag |
| Point3DGlobalCoordinates.X / RiemannianNorth | RiemannianNorth; PositionDrilling, metres, Wgs84RiemannianCoordinates |
| Point3DGlobalCoordinates.Y / RiemannianEast | RiemannianEast; PositionDrilling, metres, Wgs84RiemannianCoordinates; latitude is required context |
| Point3DGlobalCoordinates.Z / TVD | EllipsoidalDepth, DepthDrilling, metres, Wgs84 in these providers; not a universal meaning for every class named Point3D |
| Point3DGlobalCoordinates.Latitude / Longitude | Existing Latitude / Longitude, PlaneAngleGeodesic, radians, Wgs84 |
| Cluster.GroundMudLineDepth and ClusterLight equivalent | GaussianUncertainValue whose measurand is GroundMudLineDepth; identify ground versus mud line in site context |
| Cluster.TopWaterDepth and ClusterLight equivalent | GaussianUncertainValue whose measurand is WaterSurfaceDepth |
| Slot.Latitude / Longitude | GaussianUncertainValue whose measurand is existing Latitude / Longitude in Wgs84 |
| Angular wrapper.GaussianValue.Mean | Latitude / Longitude with ExpectedValue role; PlaneAngleGeodesic, radians |
| Depth wrapper.GaussianValue.Mean | GroundMudLineDepth / WaterSurfaceDepth with ExpectedValue role; DepthDrilling, metres, Wgs84 |
| Angular wrapper.GaussianValue.StandardDeviation | AngularStandardUncertainty, PlaneAngleGeodesic, radians; no coordinate origin offset |
| Depth wrapper.GaussianValue.StandardDeviation | LinearStandardUncertainty, LengthStandard, metres; no vertical-datum offset |
| GaussianValue.MinValue / MaxValue | Underlying scalar concept with DistributionLowerBound / DistributionUpperBound role, subject to the bound interpretation below |
| FieldForwardConversionRequest / FieldInverseConversionRequest | FieldCoordinateConversionRequest, with explicit direction and field-selected definition |
| Geographic coordinate Latitude / Longitude | Existing Latitude / Longitude; reference selected by the request/result, not always Wgs84 |
| Projected coordinate Easting / Northing | Existing Easting / Northing; PositionDrilling, metres, ProjectedEastingNorthingSI and the selected projected CRS |
| Conversion VerticalDepth / ProjectionDatumVerticalDepth / Wgs84VerticalDepth | EllipsoidalDepth, DepthDrilling, metres; bind the actual datum and depth policy separately |
| Conversion CoordinateEpochUtc | Existing CoordinateEpoch with UTC representation; not CreationTime |
| TransformationPathIDs / SelectionToken | Referenced transformation identifiers / existing TransformationSelectionToken |
| Conversion GridConvergence | Existing GridConvergence with GridConvergenceTrueToGridClockwise; verify the provider operation before publishing the convention |
| FieldCoordinateConversionResponse / its position result | FieldCoordinateConversionResult; the position item binds corresponding geographic/projected representations; source/output roles depend on direction |
| Conversion catalogue reference ID / Name / Authority+Code | ResourceIdentifier / ResourceName / existing AuthorityIdentifier; UUID and authority code are distinct |

Backup, restore, mutation-error and statistics envelopes reuse these domain
concepts for embedded records. Schema versions, selection policies, array indices,
error codes, HTTP endpoints, counts and concurrency tokens are protocol/control
metadata; this increment does not create a semantic noun for every transport field.
The entity relationships in backups must retain their original owner and UUID
scope. A count is not a length and an index is not an identifier.

## Provider corrections and verification gates

- **Slot.Longitude has a wrong DWIS physical-quantity annotation:** source currently
  uses LengthStandard, while its default standard deviation and REST/MCP use
  radians. Change it to PlaneAngleGeodesic in the provider integration. The old
  `relative_north_position_cluster` / `relative_east_position_cluster` variable
  names also misdescribe absolute latitude/longitude; review their external usage
  before renaming. No service code is changed in this vocabulary increment.
- Do not annotate the shared GaussianDistribution schema globally as depth:
  it is reused for angular and linear values. Use property-context-specific
  schemas or explicit field bindings for Mean, StandardDeviation and bounds in
  both REST and MCP. The current attribute helper only annotates direct members;
  nested schema publication needs explicit provider work.
- GaussianDistribution inherits MinValue/MaxValue. In the inspected source,
  ContinuousDistribution describes these as domain limits that do not influence
  the mean or realizations; Gaussian probability methods do not implement a
  truncated-normal model. Do not label them confidence bounds or claim they
  constrain sampling. Verify the deployed package retains that behavior during
  integration. DefaultStandardDeviation attributes likewise do not assert that
  an omitted sigma was stored or that model accuracy is known.
- Do not describe Riemannian line buffering as an exact geodesic margin. Field
  currently applies planar offset operations to arc coordinates. The catalogue
  identifies the representation without certifying its distance accuracy.
- ReferencePoint, arc-coordinate synonyms and latitude/longitude may coexist in
  JSON. Validate their consistency and define conflicting-input behavior at the
  provider boundary; an annotation alone cannot reconcile contradictory values.
- Check vertical limits and Gaussian standard deviations for finite values,
  ordering and nonnegative dispersion during integration. Catalogue constraints
  express intended meaning, not a claim that every existing route enforces them.
- Calculated boundary IDs are currently generated on recalculation; do not imply
  durable identity across recalculation merely because they are UUIDs.

## Approved entries

The following table lists the 52 appended Reviewed definitions.

| Constant | Kind | Definition |
| --- | --- | --- |
| `Resource` | Noun | An individually managed domain object or catalogue definition with identity in an owning service; distinct from its serialized representation. |
| `ResourceIdentifier` | Noun | A UUID identifying a resource, nested object, catalogue definition, option or assignment in a declared ownership and type scope. |
| `ResourceMetadata` | Noun | Identity and service-location metadata accompanying a managed resource; a locator is not the semantic identity. |
| `ResourceName` | Noun | Human-readable name of a resource, definition or option; not necessarily unique or stable. |
| `ResourceDescription` | Noun | Human-readable descriptive text about a resource. |
| `IdentityDefinition` | Noun | Definition of an identification scheme whose values may be assigned to resources, for example an operator naming scheme. |
| `IdentityValue` | Noun | Value identifying a resource within a selected identification scheme; its interpretation and uniqueness depend on that scheme. |
| `IdentityAssignment` | Noun | Association of one resource with an identification scheme and a value in that scheme. |
| `ClassificationCategory` | Noun | A service-owned catalogue definition grouping selectable options and their assignment rules for resources. |
| `ClassificationOption` | Noun | A named selectable alternative owned by a classification category. |
| `ClassificationAssignment` | Noun | Association of a resource with one option of a selected category, optionally qualified by validity bounds. |
| `FeatureCategory` | Noun | Classification category describing a descriptive characteristic of an assigned resource. |
| `FeatureOption` | Noun | Selectable option representing a descriptive characteristic. |
| `FeatureAssignment` | Noun | Assignment of a descriptive characteristic option to a resource. |
| `MembershipCategory` | Noun | Classification category describing a membership in a category-defined grouping of an assigned resource. |
| `MembershipOption` | Noun | Selectable option representing a membership in a category-defined grouping. |
| `MembershipAssignment` | Noun | Assignment of a membership in a category-defined grouping option to a resource. |
| `CategoryExclusivity` | Noun | Boolean rule declaring that at most one category assignment may be active on a resource at the same instant. |
| `CategoryValidityPeriodEnabled` | Noun | Boolean rule indicating whether assignments in a category may carry validity bounds. |
| `AssignmentValidityPeriod` | Noun | Time interval during which an assignment is effective, including the declared start and end instants. |
| `CreationTime` | Role | Instant at which a resource or definition was created. Not the time a physical quantity was measured. |
| `LastModificationTime` | Role | Instant of the last resource or definition update. A provider may additionally use its serialized value as an opaque concurrency token. |
| `ValidityStart` | Role | Inclusive lower time bound of assignment validity. |
| `ValidityEnd` | Role | Inclusive upper time bound of assignment validity. |
| `Field` | Noun | Named field resource organizing drilling context, classification and user-defined spatial delineations. Its catalogue identity does not assert a particular reservoir or legal boundary. |
| `WellCluster` | Noun | Managed grouping of well slots and their shared drilling-site context, optionally linked to a field and rig. May represent a single-well site. |
| `WellSlot` | Noun | Identified position or provision for a well within a cluster; distinct from the well, wellbore and trajectory that may use it. |
| `SingleWellClusterFlag` | Noun | Boolean declaration that a cluster represents a single-well site rather than a multi-well grouping. |
| `FixedPlatformFlag` | Noun | Boolean declaration that a cluster is associated with a fixed platform rather than a floating or moveable rig. |
| `ReferenceLocation` | Role | Role of a position used as the contextual reference point of a field or cluster; not necessarily a centroid, slot or local coordinate origin. |
| `DelineationLineType` | Noun | User-maintained definition classifying the purpose of a field delineation line. |
| `FieldDelineationLine` | Noun | Ordered spatial line associated with a field and a user-defined line type, optionally carrying a margin and vertical limits. |
| `CalculatedDelineationBoundary` | Noun | Ordered boundary line derived by the provider from a source delineation line and margin. |
| `DelineationMargin` | Noun | Horizontal offset distance used to derive a boundary from a delineation line under a declared geometric algorithm. |
| `ClosedLineFlag` | Noun | Boolean declaration that a line is geometrically closed according to the provider closure rule. |
| `InteriorBoundaryFlag` | Noun | Boolean declaration that a derived boundary represents the interior side of a closed source delineation line. |
| `TopDepthBoundary` | Role | Role of the shallower vertical depth limit of a delineated domain; distinct from measured depth along a borehole. |
| `BottomDepthBoundary` | Role | Role of the deeper vertical depth limit of a delineated domain; distinct from measured depth along a borehole. |
| `RiemannianNorth` | Noun | Signed meridian arc length from the equator to a point latitude on the reference ellipsoid; north positive. |
| `RiemannianEast` | Noun | Signed arc length along the latitude parallel from the reference meridian to a point longitude; east positive. |
| `Wgs84RiemannianCoordinates` | Reference | WGS84 meridian distance north from the equator and parallel arc distance east from Greenwich, expressed in metres; distinct from local NED and projected CRS grids. |
| `GroundMudLineDepth` | Noun | Ellipsoidal depth of the ground surface or mud line at the cluster site, with the actual surface identified by site context. |
| `WaterSurfaceDepth` | Noun | Ellipsoidal depth of the upper water surface at a site; distinct from water-column thickness and the depth of the seabed. |
| `GaussianUncertainValue` | Noun | Representation of a scalar quantity by a Gaussian distribution with an expected value and standard deviation, optionally carrying provider-defined domain-limit metadata. |
| `ExpectedValue` | Role | Role of a scalar reported as the expectation or mean of its declared distribution. |
| `StandardUncertainty` | Noun | Nonnegative uncertainty expressed as one standard deviation of the stated scalar distribution; not variance, confidence interval width or an absolute coordinate. |
| `AngularStandardUncertainty` | Noun | Standard uncertainty of a geodetic angular coordinate, expressed as an angular dispersion rather than a location. |
| `LinearStandardUncertainty` | Noun | Standard uncertainty of a linear position or depth, expressed as a length dispersion rather than a position. |
| `DistributionLowerBound` | Role | Role of a declared lower domain limit associated with a distribution; does not imply truncation of the probability density or generated samples. |
| `DistributionUpperBound` | Role | Role of a declared upper domain limit associated with a distribution; does not imply truncation of the probability density or generated samples. |
| `FieldCoordinateConversionRequest` | Noun | Request to convert positions using a field-selected projection definition and, when requested, geodetic transformations; reference and depth policies are explicit. |
| `FieldCoordinateConversionResult` | Noun | Result of a field-context coordinate conversion, including matched coordinate representations, selected reference definitions and warnings. |
