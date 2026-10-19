# Anti-collision planning vocabulary review — 2026-10-18

Version 0.21.0 adds semantic identities required to compose candidate discovery and anti-collision calculations without referring to provider tool names.

`ReferenceResource` and `ComparisonResource` are provider-neutral roles. They distinguish the single resource being investigated from the resources compared with it while leaving the concrete resource type and value cardinality in the provider schema.

The anti-collision additions describe the structured result of one trajectory comparison, its policy evaluation, the closed evaluation states, the closed safety classifications, and the alert and alarm separation-factor thresholds. `Indeterminate` remains distinct from a safe `Normal` result: unavailable evidence cannot establish compliance.

These concepts do not prescribe an octree implementation or a particular MCP endpoint. Providers bind their own inputs and outputs to the reviewed meanings, and planners compose compatible producer and consumer bindings by concept, role, resource type and cardinality.
