# WellBoreArchitecture vocabulary analysis — 2026-09-29

Status: **curated and approved by Eric Cayeux on 2026-09-29**. Catalogue 0.10.0 adds 60 Reviewed entries to the unchanged 232-entry 0.9.0 baseline. All physical-quantity choices and mechanical interpretations below are accepted. Provider integration, publication and deployment are separate steps.

## Scope and source evidence

The service-owned `ModelSharedOut/json-schemas/WellBoreArchitectureFullName.json` has 40 local model schemas and 18 path entries. Eleven core domain types describe the construction; the other schemas cover classifications, enums, search, backup and diagnostics. The service has no SemanticCatalogue dependency yet. Identity and feature classes implement DataManagement interfaces locally rather than inheriting ResourceClassification 0.1.0.

Read alongside `Model/*.cs`, `Model/DerivedTypes/*.cs`, `Service/Mcp/Tools/McpToolArgumentHelpers.cs` (Definitions), `Service/Mcp/Tools/WellBoreArchitectureRestMcpToolRegistrations.cs`, `WebPages/Pages/DetailedEdit.razor`, `WellHeadEditor.razor`, `SideConnectorPages`, `FluidPropertiesPages` and the Model/Service READMEs.

`Calculate()` currently returns true; it is not a geometric solver or proof of complete/consistent construction. `Realize()` samples the distributions; it does not calculate an authoritative new architecture or survey. The Realization types are not exposed by the current service-owned REST schema. Do not add a factory/evaluation capability merely because methods named Realize exist internally.

## Reuse from 0.9.0

Reuse managed resources, resource identifiers, names/descriptions, metadata, creation/modification times, WellBore, identity definitions/assignments, feature categories/options/assignments and validity endpoints. A ComponentID is a ResourceIdentifier scoped to the containing architecture, not a new generic UUID concept. Reuse GaussianUncertainValue, ExpectedValue, StandardUncertainty, distribution bounds, AlongHoleDepth, EllipsoidalDepth, SpatialAbscissa/CurvilinearAbscissa, WGS84 and its path-intersection origin, and canonical/presentation scope metadata.

Local domain containment uses HasPart; binding evidence identifies concrete collection paths and ownership. A WellBoreArchitecture belongs to/references a WellBore: it is not a subtype of WellBore. Embedded physical components need not be independently managed resources. `SideElement.ID` is an inner diameter, while `ComponentID` is its UUID; binding by the property name ID would be incorrect.

## Approved structural nouns

| Approved concept | Meaning / binding |
| --- | --- |
| WellBoreArchitecture | Managed construction description/revision linked to a WellBore; root, light view and backup entries describe the same resource. |
| ArchitectureComponent | Embedded construction component; not automatically a separately managed resource. |
| Wellhead | ArchitectureComponent for wellhead dimensions and hanger locations. |
| SurfaceConstructionSection | ArchitectureComponent above the wellhead, with optional side circuitry; not the downhole term surface casing. |
| CasingSection | ArchitectureComponent containing casing specifications, placement and hole geometry. |
| CasingElementSpecification | A tubular specification applying over an interval; not necessarily one physical casing joint. |
| OpenHoleSection | Uncased interval represented by ordered borehole-diameter/length rows. |
| BoreholeSizeInterval | Diameter applicable over an interval length; use for both casing size tables and open-hole rows. The JSON class BoreHoleSize is not just a diameter scalar. |
| SideConnector | Connection from a host surface section into a side-circuit network. |
| SideCircuitElement | Pipe, hose, valve, choke or pump in that network. |
| SideCircuitConnectivity | Directed connection between upstream and downstream elements; the current payload embeds endpoint objects. |
| EnvironmentalFluidLayer | Air/water layer above ground or mud line; neither drilling-fluid composition nor a fluid-property evaluation. |
| SurfaceSectionKind / SideCircuitElementKind / EnvironmentalFluidKind | Finite classifications matching existing enums. Unknown means unspecified; it is not a new physical equipment type. |

Keep the existing enum values exactly during integration. Equipment-specific nouns such as BOP, MarineRiser and Choke can be added later where cross-service reasoning needs their independent meaning; a closed classification does not require one new top-level noun per enum value in this first increment.

## Approved geometry and local-coordinate concepts

| Approved concept or role | Binding | UnitConversion choice / SI | Reference |
| --- | --- | --- | --- |
| PhysicalLengthExtent | Additive geometric extent, distinct from a coordinate | LengthStandard / m (curated engineering display precision) | None |
| SectionExtent (role) | SurfaceSection.SectionLength, CasingSection.Length, CasingSectionElement.SectionLength, SideElement.Length | PhysicalLengthExtent | None |
| JointExtent (role) | CasingSectionElement.JointLength | PhysicalLengthExtent | None |
| BoreholeIntervalExtent (role) | BoreHoleSize.Length | PhysicalLengthExtent | None |
| PipeDiameter | General pipe/body diameter | DiameterPipeDrilling / m | None |
| InnerDiameter / OuterDiameter / CollarOuterDiameter (roles) | BodyID, BodyOD, SideElement.ID/OD, CollarOD | PipeDiameter | None |
| BoreholeDiameter | BoreHoleSize.HoleSize | DiameterPipeDrilling / m, matching current model/UI choice | None |
| MinimumWellheadOutsideDiameter / MaximumWellheadOutsideDiameter (roles) | WellHead.MinOD / MaxOD | PipeDiameter | None; geometric envelope, not statistical bounds |
| HostComponentAbscissa | SideConnector.Position, position along host section | LengthStandard / m; specializes CurvilinearAbscissa | Confirmed: zero at the host top, increasing downward along the host |
| WellheadLocation / CasingHangerLocation / TubingHangerLocation (roles) | WellHead.Depth / CasingHangerDepth / TubingHangerDepth | DepthDrilling / m | Confirmed: EllipsoidalDepth / vertical WGS84 |
| CasingTopLocation / TopOfCementLocation (roles) | CasingSection.TopDepth / TopCementDepth | DepthDrilling / m | Confirmed: AlongHoleDepth / WGS84 path-intersection origin |
| ConnectorLocation (role) | SideConnector.VerticalDepth | EllipsoidalDepth, DepthDrilling / m | WGS84, positive downward |
| TopDepthBoundary (existing role) | SideElement.TopVerticalDepth | EllipsoidalDepth, DepthDrilling / m | WGS84, positive downward |
| FluidLayerTopBoundary (role; specializes TopDepthBoundary) | WellBoreArchitectureFluid.Depth | EllipsoidalDepth, DepthDrilling / m | WGS84; confirmed boundary is the top of the layer |

PhysicalLengthExtent should not inherit Coordinate or AlongHoleDepth. A scalar distance and a depth can both use metres but permit different operations. HostComponentAbscissa is local to a component, not automatically a global along-hole depth or a WGS84 coordinate. The applicable host and origin must be carried in contextual bindings. The owner confirms zero at the host top and increasing downward; this local origin must not inherit the global WGS84 path-intersection origin. No new per-record reference field is required: the containing surface section provides host context, and the semantic reference definition supplies origin and direction.

The existing UI uses the general Length quantity for many extents; adopting LengthStandard would improve display precision only, with no rounding or change to SI payload values. DiameterPipeDrilling already exists and has a 0.1 mm meaningful precision. The quantity choices are approved.

## Approved mechanical/material concepts

| Approved concept | Fields | Existing physical quantity / SI | Important distinction |
| --- | --- | --- | --- |
| MaterialDensity | MaterialDensity | MassDensityDrilling / kg/m³ | Solid/component material, not a fluid-layer density inferred from Air/Water. |
| YoungModulus | YoungModulus | ElasticModulus / Pa | Elastic stiffness, not absolute pressure. |
| LinearMassDensity | LinearWeight | MassGradientPerLengthDrilling / kg/m | Includes collar for casing elements according to model/MCP; not force per length. |
| MaterialGrade | Grade | None | Grade designation needs its material/standard context; no strength value is inferred solely from text. |
| ConnectionThreadDescription | ConnectionType | None | Textual connection specification; not the side-network connectivity graph. |
| TensileStrength | TensileStrength | DrillStemMaterialStrengthDrilling / Pa | Owner confirms this is material stress, not axial load capacity. Keep existing fields and SI dimensions. |
| TensileCapacity | Approved reusable concept; no existing provider field | ForceDrilling / N | Axial tensile force limit of the specified component or connection under stated conditions and a stated limit criterion. |
| TorsionalCapacity | CasingSectionElement.TorsionalStrength | TorqueDrilling / N·m | Torque limit, not shear stress. |
| PressureDifference | Reusable parent for differential-pressure capacities | PressureDrilling / Pa | No vacuum or atmospheric origin. |
| BurstPressureCapacity | BurstPressure | PressureDrilling / Pa | Internal-minus-external differential capacity; loading/failure criterion belongs to context. |
| CollapsePressureCapacity | CollapsePressure | PressureDrilling / Pa | External-minus-internal differential capacity; sign/loading criterion explicit. |
| YieldStress | YieldStress | DrillStemMaterialStrengthDrilling / Pa; general MaterialStrength also available | Material stress, not a pressure referenced to vacuum. |
| RecommendedMakeUpTorque | MakeUpTorqueRecommended | TorqueDrilling / N·m | Recommended assembly value, not torsional failure capacity. |
| MaximumPermittedDoglegSeverity | MaxDLS | CurvatureDrilling / rad/m | Allowable curvature, not current survey curvature or an angle. |

No missing physical quantity has been identified. Existing UnitConversion source provides all proposed choices. Eric Cayeux confirmed TensileStrength is a stress and suggested a separate TensileCapacity using ForceDrilling. New semantic concepts can share units without sharing meaning or reference requirements. Never inherit AbsolutePressure merely because a property is named BurstPressure or is expressed in pascals.

## Tensile strength versus tensile capacity

Owner clarification: TensileStrength remains a material stress. The separate approved TensileCapacity describes a component or connection's axial force limit and binds to ForceDrilling. The concepts are not aliases or subtypes of each other. Capacity requires the identified component/connection, load conditions and limit criterion (for example yield, ultimate failure or allowable working load). A stated safety factor belongs to that criterion; do not apply one implicitly.

A capacity calculation requires the applicable resisting geometry and mechanical assumptions; stress and force are not related by unit conversion. A connection can limit capacity independently of the tubular body. Keep YieldStress distinct from TensileStrength and do not infer an ultimate/yield/allowable capacity criterion from the word capacity alone.

Add the reusable vocabulary concept without silently adding a new service property or deriving a force from existing stress values. A future TensileCapacity field or calculation would need its own provider contract and context. ForceDrilling already exists, so no UnitConversion change is necessary.

## Representation and uncertainty

Add ScalarValueRepresentation (Dirac-wrapped scalar) alongside GaussianUncertainValue. This describes representation, not a new domain quantity. Bind the quantity/reference to `/DiracDistributionValue/Value` or `/GaussianValue/Mean`, not to the globally shared wrapper type.

Standard deviations are nonnegative, same-dimensional dispersions, with no datum/pressure origin. LinearStandardUncertainty currently explicitly describes coordinate/depth uncertainty; dimensions such as diameter/extent use the separate DimensionalLengthStandardUncertainty concept, preserving the published definition. The increment also adds uncertainty specializations for density, linear mass density, elastic modulus/material stress, differential pressure, torque and force. Their physical quantities are selected explicitly; do not infer them from a bare StandardDeviation property name. Do not use AbsolutePressure for uncertainty. Null deviation remains unspecified, not zero.

For shared Gaussian/Dirac `$ref` schemas, use property-context JSON Pointer bindings as in WellBore, with equivalent MCP annotations. Bounds are provider domain-limit metadata; they do not assert confidence limits or a truncated distribution. A deterministic realization is one sampled value and must not be confused with the distribution mean.

## Composition implications for DrillWeaver

For the question 'length of the open-hole section', retrieve the architecture, select OpenHoleSection.HoleSizes and sum the interval Length means (or return the appropriate uncertain aggregate). No dedicated question-specific capability is necessary once the graph records containment, element type and additive extent.

This operation requires non-overlapping coverage of the same interval; ordering alone does not establish it. Do not sum casing-section lengths, material-specification interval lengths and borehole-size-table lengths together: they are different descriptions and may overlap. Do not sum depth positions or diameters. If open-hole coverage is missing, return unknown rather than silently interpreting an absent section as zero without an explicit domain rule.

For uncertain totals, summing means is distinct from propagating variance. The service publishes marginal distributions without cross-component covariances; root-sum-square standard deviations requires an explicit independence assumption. Counting nested graph elements must use ComponentID/ownership scope, not repeated embedded endpoint appearances.

The initial binding map can use HasPart for collections, reusable Upstream/Downstream endpoint roles and containment context for host-relative coordinates. Preserve the existing flow orientation; do not infer that upstream/downstream follows increasing vertical or along-hole depth.

## Approved owner decisions

1. Confirmed: CasingSection.TopDepth and TopCementDepth are along-hole coordinates. WellHead.Depth, CasingHangerDepth and TubingHangerDepth are vertical coordinates. Use the distinct canonical reference bindings already defined in 0.9.0; stored values remain SI metres and presentation references remain selectable.
2. Resolved: TensileStrength is a stress (Pa). Add TensileCapacity to the catalogue as a distinct force concept using ForceDrilling (N). Differential-pressure capacities, torsional capacity and mass-per-length interpretations are also accepted.
3. Confirmed: SideConnector.Position is relative to the host top, increasing downward. WellBoreArchitectureFluid.Depth marks the top of its layer. Define a host-top/downward local reference for the former and a fluid-layer top-boundary role for the latter. These are domain definitions, not claims about new runtime validation.

Additional contract detail to document during provider integration: whether casing collections represent nested/overlapping strings versus adjacent intervals; how each size-table partition is anchored; what a missing OpenHoleSection means; whether fluid layer ordering and terminal ground boundary are validated. Current code/comments do not establish all of these as enforced guarantees.

## Integration work after curation

- Catalogue 0.10.0 includes the approved concepts, roles and uncertainty specializations with Reviewed status; all 0.9.0 identifiers and meanings are preserved.
- Bind scalar properties explicitly, including canonical-storage-and-api scope and presentationReferencesAllowed. Preserve MSL, ground/mud-line and other user-selected presentation references in the web editors.
- Adopt ResourceClassification 0.1.0 as a separate implementation migration with stored JSON compatibility checks; no new classification vocabulary is necessary.
- Correct ambiguous REST/MCP prose in the same change as the binding; do not silently reinterpret or convert existing Pa/N values.
- Regenerate own-service OpenAPI, merge/client outputs and tests. Check core/root, nested components, granular mutations, search, backup/restore and catalogue endpoints.
- Verify alternative presentation references without changing canonical stored values; test SI dimensions, UUID-versus-inner-diameter distinction, scalar/uncertainty reference separation and REST/MCP parity.

The present analysis does not claim that inherited semantic constraints are runtime validation. It does not publish a new package, migrate data or deploy the service.

## Release implementation decisions

Canonical drilling reference profile 1.1.0 adds HostComponentAbscissa -> HostTopDownward without changing the earlier bindings. This remains canonical-storage-and-api metadata with presentation references allowed.

DimensionalLengthStandardUncertainty is added for extents and diameters; LinearStandardUncertainty retains its published coordinate/depth meaning. MassDensityStandardUncertainty, LinearMassDensityStandardUncertainty, ElasticModulusStandardUncertainty, MaterialStrengthStandardUncertainty, PressureDifferenceStandardUncertainty, TorqueStandardUncertainty and ForceStandardUncertainty bind explicitly to the quantities of their measurands. None inherits an absolute reference origin. ScalarValueRepresentation carries representation only.

The source contains 292 entries: 290 Reviewed and two Deprecated legacy names. No new TensileCapacity provider field, geometry solver, covariance assumption or runtime validation is introduced by this vocabulary release.
