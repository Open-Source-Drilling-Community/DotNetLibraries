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
