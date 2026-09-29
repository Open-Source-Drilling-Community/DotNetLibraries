# Well and WellBore vocabulary — 2026-09-29

Status: **Reviewed**, approved by Eric Cayeux on 2026-09-29. The tie-in interpretation was also explicitly confirmed: parent-wellbore measured depth with an explicit MD origin/reference, without automatic WGS84 vertical-depth conversion.

Source 0.8.0 contains the 208 published Reviewed definitions from 0.7.0 unchanged, plus 15 approved additions, all Reviewed. The additions are embedded in `catalogue.json` and exposed through stable `Concepts.cs` constants. Publication and provider integration remain separate operations; no provider code or deployed data is changed by this curation.

## Scope and source evidence

Inspected the current OSDC Well and WellBore Model projects, their service-owned OpenAPI and MCP schema builders, reference-integrity validators, external-reference validators, sidetrack compatibility mapping and WebPages editors. Both providers now use ResourceClassification 0.1.0. The Rig model was inspected only to identify the external Rig resource; this is not a full curation of Rig equipment vocabulary.

## Approved entries

| Concept | Kind | Meaning |
| --- | --- | --- |
| Well | Noun | Managed well resource associating wellbores with a well identity and surface slot/site context; distinct from an individual wellbore or trajectory. |
| Wellbore | Noun | Managed bore-path resource belonging to a well, with optional parent-branch topology and rig-job history; distinct from the well and from a particular survey or trajectory representation. |
| Sidetrack wellbore | Noun | Wellbore branching from a parent wellbore at a tie-in location expressed in the parent path coordinate system. |
| Sidetrack flag | Noun | Boolean declaration that a wellbore is a branch of a parent wellbore. |
| Rig | Noun | Managed drilling-rig resource identifying the installation or equipment system associated with drilling operations; distinct from a dated job performed by that rig. |
| Rig job | Noun | Identified historical association of a rig with work on one wellbore during a declared time interval, including the ownership of its drill-floor depth. |
| Rig-job period | Noun | Time interval of a rig job, including its start and excluding its end. |
| Rig-job start | Role | Inclusive starting instant of a rig job. |
| Rig-job end | Role | Exclusive ending instant of a rig job. |
| Spatial abscissa | Noun | Length-valued coordinate locating a position along a specified axis or path from a declared origin. |
| Curvilinear abscissa | Noun | Spatial abscissa measured by accumulated arc length along a specified oriented curve from a declared origin. |
| Measured depth | Noun | Curvilinear abscissa along a wellbore path, measured from its declared measured-depth origin in the declared increasing direction. |
| Tie-in measured depth | Noun | Measured-depth coordinate of a sidetrack tie-in on the parent wellbore path, expressed using that parent path's MD origin. |
| Drill-floor depth | Noun | Vertical depth coordinate locating the drill floor relative to the declared vertical reference. |
| Drill-floor depth source | Noun | Discriminator identifying whether the authoritative drill-floor depth belongs to the referenced rig or to the rig-job entry. |

## Inheritance and quantities

- `Well`, `Wellbore`, `Rig` and `RigJob` specialize Resource. A Wellbore is associated with a Well; it does not inherit Well. A RigJob is associated with a Rig; it does not inherit Rig. SidetrackWellbore specializes Wellbore, not the individual parent record.
- `SpatialAbscissa -> Coordinate`; `CurvilinearAbscissa -> SpatialAbscissa`; `MeasuredDepth -> CurvilinearAbscissa` and existing `Depth`; `TieInMeasuredDepth -> MeasuredDepth`. SpatialAbscissa is explicitly length-valued to avoid claiming every mathematical abscissa has length units. MD inherits DepthDrilling and SI metres. These coordinates remain distinct from additive segment lengths.
- SpatialAbscissa/CurvilinearAbscissa leave the engineering quantity specialization to their concrete child; they have SI metre representation. Giving a generic parent LengthStandard would conflict with the existing DepthDrilling specialization under current catalogue inheritance rules. We therefore do not introduce a universal Length parent in this increment.
- `DrillFloorDepth -> Depth`, with the actual reference bound by its provider. WellBore uses WGS84 ellipsoidal depth, positive downward. The noun itself does not force that convention on every future provider.
- Standard deviations reuse LinearStandardUncertainty (`LengthStandard`, metres), not DepthDrilling. Gaussian means carry TieInMeasuredDepth or DrillFloorDepth with ExpectedValue; the wrapper remains GaussianUncertainValue without a global quantity. Domain bounds reuse DistributionLowerBound/DistributionUpperBound, with the mean's path/origin or vertical reference. No truncation or confidence-interval meaning is inferred.
- RigJobPeriod is a half-open time interval `[StartDate, EndDate)`. Start uses Instant + RigJobStart, end uses Instant + RigJobEnd; both use UTC-compatible timestamp representation. Existing AssignmentValidityPeriod and ValidityStart/ValidityEnd remain inclusive and must not be reused for job ends.

**No new UnitConversion quantity is needed.** Existing DepthDrilling and LengthStandard provide the required physical quantities and meaningful display precision; display precision is not measurement accuracy.

## Provider binding map

`[]` denotes each list entry. ID fields bind ResourceIdentifier with explicit target type and owning service; a resource UUID is not the object it references.

| Provider field/type | Binding |
| --- | --- |
| Well | Well |
| WellBore | Wellbore; SidetrackWellbore applies conditionally when IsSidetrack is true |
| MetaInfo / MetaInfo.ID / Name / Description | Existing ResourceMetadata / ResourceIdentifier / ResourceName / ResourceDescription |
| CreationDate / LastModificationDate | Instant + CreationTime / LastModificationTime, UTC; not operational job timestamps |
| Well.ClusterID / SlotID | ResourceIdentifier targeting WellCluster / WellSlot owned by Cluster; slot membership is required context |
| Well.IsSingleWell | Reuse SingleWellClusterFlag as a declaration about the associated cluster/site, not about the number of wellbores |
| WellBore.WellID | ResourceIdentifier targeting Well in the Well service |
| WellBore.IsSidetrack | SidetrackFlag |
| WellBore.ParentWellBoreID | ResourceIdentifier targeting the parent Wellbore in this service; topology association, no inheritance assertion |
| WellBore.TieInPointAlongHoleDepth | GaussianUncertainValue with contextual scalar bindings |
| TieInPointAlongHoleDepth.GaussianValue.Mean | TieInMeasuredDepth + ExpectedValue; DepthDrilling, m; parent wellbore path and MD origin must be supplied |
| TieInPointAlongHoleDepth.GaussianValue.StandardDeviation | LinearStandardUncertainty; LengthStandard, m, no origin offset |
| TieInPointAlongHoleDepth.GaussianValue.MinValue / MaxValue | TieInMeasuredDepth + DistributionLowerBound / DistributionUpperBound; same parent path/MD origin as mean |
| WellBore.RigJobs[] | RigJob |
| RigJob.RigJobID | ResourceIdentifier identifying the history entry in the containing Wellbore |
| RigJob.RigID | ResourceIdentifier targeting Rig in the Rig service |
| RigJob.StartDate / EndDate | Instant + RigJobStart / RigJobEnd, UTC; collectively RigJobPeriod |
| RigJob.DrillFloorDepthSource | DrillFloorDepthSource; Rig or RigJob determines ownership, not measurement provenance |
| RigJob.DrillFloorDepth | GaussianUncertainValue when job-owned; null when Rig-owned |
| DrillFloorDepth.GaussianValue.Mean | DrillFloorDepth + ExpectedValue; DepthDrilling, m, Wgs84 |
| DrillFloorDepth.GaussianValue.StandardDeviation | LinearStandardUncertainty; LengthStandard, m, without vertical-reference offset |
| DrillFloorDepth.GaussianValue.MinValue / MaxValue | DrillFloorDepth + distribution-bound roles; m, Wgs84; not confidence limits |
| Deprecated WellBore.RigID | ResourceIdentifier targeting Rig; legacy association or latest-job projection, never authoritative over non-null RigJobs |
| Well/WellBore Identity / IdentityAssignment | Existing IdentityDefinition / IdentityAssignment |
| Assignment.IdentityID / Value | ResourceIdentifier targeting the owning identity definition / IdentityValue within that scheme |
| Well/WellBore FeatureCategory / FeatureOption / FeatureAssignment | Existing FeatureCategory / FeatureOption / FeatureAssignment |
| Category.IsExclusive / HasValidityPeriod | Existing CategoryExclusivity / CategoryValidityPeriodEnabled |
| FeatureAssignment.FromDate / ToDate | Existing Instant + ValidityStart / ValidityEnd, UTC; inclusive endpoints |
| Assignment.ID / FeatureCategoryID / FeatureOptionID | Existing ResourceIdentifier with assignment/category/option target and scope |
| WellDetailsUpdate / WellBoreDetailsUpdate | Reuse Name/Description bindings; mutation wrapper is a protocol envelope |
| WellLocationUpdate / WellBoreTopologyUpdate | Reuse corresponding resource-property bindings; no new noun for each CRUD DTO |
| Search Items / batch Wells or WellBores / catalogue dependencies | Reuse resource and classification bindings for embedded objects |
| Reference-audit WellID / WellBoreID / ClusterID / SlotID / RigID | ResourceIdentifier with explicit target; validation status is a diagnostic, not an entity |

HTTP locators, paging offsets, counts, schema versions, backup policies, error codes, concurrency tokens, search filters and audit statuses remain protocol/diagnostic metadata. UTC export/restore/check timestamps can reuse Instant with their purpose explained in the provider description. This increment does not invent drilling nouns for transport envelopes.

## Provider corrections and unresolved representation work

1. **Tie-in MD is not WGS84 vertical depth.** `WellBore/Service/Mcp/Tools/McpToolArgumentHelpers.cs: CreateTieInPointSchema` currently claims otherwise for the wrapper and mean/bounds. Correct those descriptions during integration. The model confirms along-hole depth on the parent. Do not automatically reinterpret or convert stored numbers; inspect legacy data origins first.
2. **An explicit MD origin is currently absent.** ParentWellBoreID identifies the path resource, but the inspected DTO has no MD-origin/reference identifier. A provider extension or resolvable referenced path definition is needed. Do not silently substitute the current rig floor, WGS84 zero or the child wellbore origin. Capturing this missing context is a follow-up contract decision, not a new physical quantity.
3. **Editor handling needs review.** The tie-in input uses `DrillingSignalReferenceType.Depth`. Verify and remove any automatic vertical-datum transformation for MD. The uncertainty editors use LengthSmall while the curated reusable uncertainty concept uses LengthStandard; align the selected quantity and clear standard-deviation labels during integration.
4. **Rig-job ownership is conditional.** Keep the existing Rig/RigJob discriminated union. Rig-owned depth must be resolved from the identified fixed-platform Rig; job-owned depth is required locally. Undefined is not a valid resolved source. A nullable depth is not evidence that its value is zero.
5. **Preserve history states.** RigJobs null is legacy/unmigrated, an empty list is authoritative with no known rig, and a populated list is authoritative history. The provider sorts by StartDate, permits gaps, rejects overlaps, and permits an open end only on the last job. No new glossary noun is needed for JSON null.
6. **Keep classification values as data.** Technical/Production/Appraisal/Lateral sidetrack options are provider catalogue records. The legacy SidetrackType projects the SidetrackClassification feature; do not create universal semantic subclasses from these editable option names or remove compatibility as part of vocabulary extraction.
7. **Do not overstate validation.** The inspected topology validator checks parent existence, self-parenting, cycles and same-well context when known. It does not establish that the tie-in MD is within a resolved parent trajectory domain. Missing context and finite/nonnegative uncertainty checks must be verified during provider integration.
8. **Binding infrastructure remains a separate improvement.** Nested scalar paths, reference target ownership and dynamic context links should become shared machine-readable bindings. This increment uses the existing catalogue document structure; it does not pretend those infrastructure extensions have already been implemented.

## Review and integration sequence

The definitions and inheritance are curated. Publish 0.8.0 before updating Well/WellBore dependencies and annotations. Provider work must then address the MD-reference contract, REST/MCP/UI consistency and generated schemas, with data compatibility checks. Approval of the vocabulary does not imply that provider issues are already corrected, or authorize publication or deployment.
