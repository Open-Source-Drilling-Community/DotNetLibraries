# EarthGravity curation decisions

## 2026-09-26 — D1, D2 and D3

Approver: Eric Cayeux. Evidence: explicit reply “Yes I agree” to the three directions in the EarthGravity curation review. Target catalogue/package version: **0.2.0** (not yet published).

Approved directions:

- **D1:** distinguish a gravity evaluation result from its total acceleration vector and scalar potential. Added `urn:osdc:semantic:gravity-evaluation-result`. `gravity-sample` now points to that result. EarthGravity's existing `EarthGravityVector` DTO and `Sample.Gravity` field bind to the result concept; their names and serialized fields remain unchanged. The vector concept remains available for the vector itself.
- **D2:** distinguish generic geodetic position from the ellipsoidal-depth representation. Added `urn:osdc:semantic:generic-geodetic-position` as parent. The published `geodetic-position` ID retains its narrow latitude/longitude/depth meaning and has the clearer label “Geodetic position with ellipsoidal depth”. Ellipsoidal depth wording explicitly names the ellipsoid normal. No height representation is invented in this increment.
- **D3:** separate shared meanings from provider policy. Removed EarthGravity's input ranges, WGS84-specific choices, batch atomicity and ordering guarantees from shared concept constraints where they were provider-specific. Request, response and input/output roles no longer imply synchronous/stateless/ordered execution. EarthGravity still declares and enforces those guarantees in its schemas, tool description, evaluator and API documentation. Model name is scientific-model-neutral; model release date no longer embeds its serialization format.

At this stage only structural directions were approved; individual entries remained Proposed. The subsequent approval below completes that review.

## Compatibility and migration from 0.1.0

- Existing IDs remain resolvable. The old position ID has not been silently broadened. Two new IDs increase the vocabulary from 34 to 36 entries.
- Provider `x-osdc-semantic.catalogueVersion` changes to 0.2.0; consumers must refresh cached bindings. The gravity record's concept changes from `total-gravity-vector` to `gravity-evaluation-result`.
- Consumers of old request/response/role definitions must take ordering, validation and execution guarantees from the versioned provider contract. Existing 0.1.0 users keep their original embedded vocabulary.
- UnitConversion remains authoritative for quantities and display precision. Package dependencies follow the published 0.1.0 dependency baseline, Conversion.DrillingEngineering 3.4.2, without stale conditional 3.3.28 references.
- The package version and embedded catalogue version advance together. Publish 0.2.0 before building standalone EarthGravity/CI/Docker against its package reference. Sibling-source development remains supported.

## Subsequent approval — 2026-09-26

Approver: Eric Cayeux. Evidence: “I have looked at the other concepts and I find them satisfactory.” Together with approval of D1-D3, this approves all 36 concepts in the EarthGravity increment, including their current definitions, roles, references, hierarchy and quantity bindings. Their status changes from Proposed to Reviewed in the still-unpublished 0.2.0 release. No definitions or IDs change in this approval step.

The complete approved ID list is the 36-entry table in CURATION.md, corresponding to catalogue.json version 0.2.0. New concepts discovered in later microservices still start as Proposed; this approval does not extend to future additions. The original review is retained as historical evidence. Suggestions for richer reference modeling, quantity-inheritance rules and machine-readable context remain possible future enhancements, not unresolved objections to the approved vocabulary or implemented capabilities.

## 2026-09-27 — Digest hierarchy and source-file roles

Approver: Eric Cayeux. Evidence: “I agree” in response to the proposed File content digest → SHA-256 file digest hierarchy and coefficient-file/model-metadata-file roles. Target catalogue/package: **0.3.0**, unpublished.

Added four Reviewed entries: `file-content-digest`, `sha256-file-digest` (specializes the former), `coefficient-file` (Role), and `model-metadata-file` (Role). Digests have no physical quantity or SI unit. The algorithm is fixed by the SHA-256 specialization; encoding and the exact source file remain provider facts. Existing EarthGravity/EarthMagneticField SHA-256 properties use 64 hexadecimal characters; no normalized-JSON hashing is implied.

The published `coefficient-sha256` noun keeps its ID, label, definition and Reviewed status, and now specializes SHA-256 file digest. It remains usable by existing EarthGravity attributes. It is not superseded by the generic noun alone: doing so would lose the coefficient-file meaning. New bindings use `Sha256FileDigest` plus `CoefficientFile` or `ModelMetadataFile`. The current model has no machine-readable equivalence between a specialized noun and a noun/role pair; the migration is explicitly documented here.

The proposed separate `model-metadata-sha256` noun is withdrawn before publication in favor of the reusable noun/role binding. Property names `CoefficientSHA256` and `MetadataSHA256` do not change. No provider package references are upgraded in this step. No package is published; the 0.2.0 release remains immutable. The source catalogue now contains 40 Reviewed entries; other EarthMagneticField additions still await individual curation.

## 2026-09-27 — Complete EarthMagneticField vocabulary approval

Approver: Eric Cayeux. Evidence: “You can consider the vocabulary as currated.” This approves the remaining 18 definitions in CURATION-EARTHMAGNETICFIELD-2026-09-26.md, together with the previously approved MagneticDip naming, M1–M3 directions and digest structure. All 58 entries in the unpublished 0.3.0 catalogue now have Reviewed status: 42 nouns, 13 roles and 3 references.

Implemented Dip → Magnetic dip and Scientific model provenance → Gravity/Geomagnetic model provenance, compound sample/evaluation-point relationships, UTC instants, bound roles and authoritative quantity bindings. Geodetic evaluation point contains a generic geodetic position and instant; it does not impose Earth's provider-specific ellipsoidal-depth representation on every future provider. HasPart relations describe conceptual constituents, not mandatory non-null JSON properties; undefined orientation angles remain nullable under the provider contract.

Approval is limited to this vocabulary increment. Future discoveries still require curation. EarthMagneticField provider semantic attributes and REST/MCP annotation integration remain subsequent implementation work; publication and deployment have not occurred.

## 2026-09-27 — Complete EarthVerticalDatum vocabulary approval

Approver: Eric Cayeux. Evidence: “Otherwise I accept your proposed additions. Consider the new vocabulary as curated,” with the explicit correction to use LengthStandard for Geoid undulation. All 14 entries in CURATION-EARTHVERTICALDATUM-2026-09-27.md change from Proposed to Reviewed in unpublished 0.4.0. The 58 published 0.3.0 entries remain unchanged.

Geoid undulation resolves to UnitConversion LengthStandard with SI metres and meaningful display precision 0.001 m (1 mm). This precision is owned by UnitConversion, not duplicated as model accuracy or a wire-rounding rule. Geoid representation error retains Length. The approved direction replaces provider GridResolutionMinutes with AngularGridSpacing in radians. The provider and its generated downstream contracts implemented that replacement on 2026-10-05; deployment remains a separate operation.

The catalogue now contains 72 Reviewed entries. New vocabulary discovered later still starts as Proposed.

## 2026-09-28 — Complete EarthGeodesy vocabulary approval

Approver: Eric Cayeux. Evidence: “You can upgrade the Semantic Catalogue to use the new unit definitions. Otherwise I agree with your proposed decisions.” This approves the 50 EarthGeodesy entries and decisions in CURATION-EARTHGEODESY-2026-09-27.md with the requested specialized quantity bindings. All 122 entries in source 0.5.0 are now Reviewed; the 72 published 0.4.0 definitions remain unchanged.

The package references published Conversion.DrillingEngineering 3.4.3, which brings Conversion 3.4.3 transitively. InverseFlattening binds to InverseFlattening (meaningful precision 1e-9); HelmertScaleDifference binds to HelmertScaleDifference (1e-12). Both remain dimensionless. UnitConversion owns precision, not the semantic schema. The accepted frame/ensemble, epoch, original-unit parameter, depth-change and geographic-boundary distinctions remain in place.

Provider integration is subsequent work. This approval neither publishes a NuGet nor deploys a service. Future concepts still require separate curation.

## 2026-09-28 — EarthCartographicProjection vocabulary approval (0.6.0)

Approver: Eric Cayeux. Evidence: “The proposed semantic catalogue can now refer to this new quantity and can be considered as curated. Prepare it for compilation and nugetification.” The 34 additions in CURATION-EARTHCARTOGRAPHICPROJECTION-2026-09-28.md are Reviewed, extending the unchanged 122 definitions in published 0.5.0 to 156.

ProjectionScaleFactor binds to the published UnitConversion 3.4.5 quantity, with SI 1 and meaningful display precision 1e-9. Conversion.DrillingEngineering 3.4.5 supplies general Conversion transitively. The full projection factor remains distinct from HelmertScaleDifference. Two-dimensional positions, CRS/conversion/method distinctions, origin roles, reference conventions and generic parameter exceptions are accepted. UnitConversion owns display precision; it does not imply scientific accuracy or stored-value rounding.

This increment prepares source and the NuGet package without publishing it. Provider/importer corrections and REST/MCP integration remain separate work; vocabulary approval does not certify current provider behavior. Future concepts still require curation.

## 2026-10-06 — Persisted calculation-case lifecycle vocabulary

Approver: Eric Cayeux. Evidence: “Go ahead and extend the vocabulary first. Then we will update the description of each microservice according the new vocabulary.” Target catalogue/package: **0.15.0**, unpublished.

Added 19 Reviewed definitions covering calculation specifications and results, lightweight status projections, diagnostics, result manifests/chunks, immediate and queued submission/replacement, and focused retrieval roles. Added `HasInput`, `Produces` and `ProjectionOf` relations. Compatibly clarified `CalculationCase`, `CalculationState` and `CalculationProgress`; no identifier was replaced or deprecated.

No new physical quantity is introduced. Calculation progress remains a dimensionless proportion. REST/OpenAPI and MCP provider bindings are explicitly subsequent work.

## 2026-10-06 — Calculation-case deletion vocabulary

Approver: Eric Cayeux. Evidence: “Can you revise the vocabulary to enable the description of case deletion?” Target catalogue/package: **0.16.0**, unpublished.

Added the Reviewed `CalculationCaseDeletion` operation role. It permanently removes a persisted calculation case and its service-owned stored lifecycle information and results. It does not imply cancellation of already-running execution, and it is not a specialization of replacement or retrieval. Provider contracts retain responsibility for destructive-write annotations, authorization, optimistic concurrency and stable error behavior. No physical quantity or canonical reference is added.
