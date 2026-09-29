# Canonical references and along-hole terminology — 2026-09-29

Approved by Eric Cayeux in this session. Catalogue 0.9.0 contains 232 concepts: 230 Reviewed and two Deprecated legacy names. Schema version 2 adds portable, versioned reference profiles; the loader also accepts schema version 1 documents without profiles.

## Decisions

- Along-hole depth is the preferred wellbore specialization of curvilinear abscissa. It may be planned, calculated, interpolated or measured. Acquisition provenance is independent of coordinate identity.
- New AlongHoleDepth and TieInAlongHoleDepth IDs replace the preferred use of MeasuredDepth and TieInMeasuredDepth. The old IDs remain resolvable with Deprecated status and SupersededBy links; their definitions and payload names are not rewritten. Aliases are discovery hints, not proof of identity.
- Along-hole zero is the path's intersection/extension to the WGS84 ellipsoid, as explicitly clarified by Eric. The reference requires the applicable path, its extension model and a selected intersection if ambiguous. A scalar vertical offset cannot establish that origin.
- Canonical drilling profile 1.0.0 uses WGS84 for vertical depth and latitude/longitude; the separate WGS84 path-intersection convention for along-hole depth; WGS84 equator/Greenwich arc coordinates for Riemannian North/East; true north clockwise for wellbore azimuth; the downward WGS84 ellipsoid normal for internal wellbore inclination; vacuum for absolute pressure.
- The profile is an explicit provider assertion for canonical drilling data. Generic geodesy/projection conversion inputs retain their declared source and target references. It is not a global override for arbitrary quantities, geoid depth, magnetic dip, angular separations, pressure differences or standard uncertainties.
- No new per-record tie-in reference property is introduced. JSON field names, stored numbers and physical quantity choices remain unchanged.

## Contract mechanism

A provider selects `SemanticCatalogue.OsdcCanonicalDrilling` through `SemanticAttribute.ReferenceProfile` or the shared `SemanticMetadata.Create` factory. Profile mappings inherit through noun specialization. Generated metadata includes the resolved reference, reference definition/context, profile identity/version and inherited constraints. Explicit contradictory references fail, as do conflicting inherited mappings or malformed profile bindings. The profile is embedded in catalogue.json and does not require network access.

Providers must opt in at canonical bindings. Selecting a profile does not authorize arithmetic or numerically convert data. A concept with no profile mapping retains its explicitly supplied reference. Uncertainties must be bound separately from means. Consumers must not treat a catalogue version upgrade alone as proof that a provider adopted the profile.

## Existing-provider audit

| Provider | Finding and required adoption |
| --- | --- |
| EarthGravity | Latitude/longitude and ellipsoidal depth already explicitly bind WGS84. Gravity vectors retain their NED convention. Adopt profile on canonical coordinate scalars after publication. |
| EarthMagneticField | Evaluation coordinates already explicitly bind WGS84. Keep magnetic dip and declination separate from wellbore inclination/azimuth. |
| EarthVerticalDatum | WGS84 and EGM84/geoid inputs and outputs are intentionally distinct conversion endpoints. Do not apply WGS84 to geoid depth. |
| EarthGeodesy | Arbitrary source/target datum operations are intentional. No blanket WGS84 default on general conversion coordinates. |
| EarthCartographicProjection | Base datum, projected easting/northing and true-to-grid convergence are explicitly contextual. Do not replace them with WGS84/Riemannian or wellbore-azimuth conventions. |
| Field / Cluster | Provider registries already bind Riemannian North/East to WGS84RiemannianCoordinates and vertical depth to WGS84. Their hand-built metadata factories should adopt the common factory on upgrade. |
| Well / WellBore | Replace the preferred tie-in noun with TieInAlongHoleDepth and bind its mean/bounds to Wgs84AlongHoleOrigin; uncertainty remains LengthStandard without an origin. Rig-job drill-floor depth remains vertical WGS84. |

## Internal references versus presentation references

The canonical profile applies to persisted values and REST/MCP payloads, not to the user's chosen display reference. Metadata exposes referenceScope = canonical-storage-and-api and presentationReferencesAllowed = true. Users may display and edit depth relative to MSL, mud line/ground level, rotary table or other supported references. Presentation code converts from canonical values for display and back before saving; display selection never redefines the wire contract. Standard uncertainties have no coordinate origin.

The tie-in editor retains its existing DrillingSignalReferenceType.Depth integration and reference selector. Removing this presentation conversion was an incorrect response to the canonical-reference clarification and has been reversed. Its label identifies along-hole depth on the parent path, while the reference-aware unit adornment shows the selected presentation reference.

The mathematical definition of canonical along-hole zero remains the parent path's intersection/extension to WGS84. Existing shared display helpers use reference offsets. A separate path-aware review is needed to establish where that implementation represents the requested along-hole conversion; this does not justify removing user-selectable references. No conversion algorithm or historical stored value has been changed by this correction.

## Release and verification

Publish SemanticCatalogue 0.9.0 before downstream CI/Docker restores that depend on it. Local package builds are for verification and do not publish to nuget.org. Regenerate service OpenAPI, dependency snapshots, merged contracts and clients after provider adoption. Verify both REST and MCP bindings, the absence of a new reference field, and the distinction between means and uncertainties.

Well and WellBore have been updated and verified against the locally packed 0.9.0 package, including regenerated REST/MCP contracts and clients. Other audited providers retain their published package versions until a subsequent explicit adoption. Verification: 40 catalogue tests and 155 isolated provider tests pass; one optional backup test skips; live HTTP tests excluded. Payload/validation shapes are unchanged.
