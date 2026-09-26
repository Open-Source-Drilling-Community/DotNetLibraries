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
