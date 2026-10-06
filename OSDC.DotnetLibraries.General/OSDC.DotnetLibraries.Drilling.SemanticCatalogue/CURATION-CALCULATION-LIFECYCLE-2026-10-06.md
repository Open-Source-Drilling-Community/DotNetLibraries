# Persisted calculation-case lifecycle vocabulary — 2026-10-06

Status: **curated at the request of Eric Cayeux on 2026-10-06**. Catalogue 0.15.0 adds 19 Reviewed definitions to the 577-entry 0.14.0 baseline and compatibly clarifies `CalculationCase`, `CalculationState` and `CalculationProgress`. Provider bindings, microservice contract descriptions, publication and deployment are separate operations.

## Scope and terminology

Several OSDC services implement a persisted calculation-case lifecycle: a caller submits or replaces a case describing what is to be calculated; the service evaluates it; and a later read returns the persisted case and its server-derived result. Heavy calculations may run in the background and expose a lightweight status record while simpler calculations can complete in the mutation response.

This vocabulary calls that behavior a **persisted calculation-case lifecycle**. “Factory design pattern” remains a useful implementation description, but is not the canonical domain concept: a calculation case is a managed resource, not merely an object factory.

## Reviewed additions

| Concept | Kind | Purpose |
| --- | --- | --- |
| Calculation specification | Noun | Caller-controlled configuration and input references |
| Calculation result | Noun | Server-derived calculated output |
| Calculation status snapshot | Noun | Lightweight identity and lifecycle projection |
| Calculation diagnostic message | Noun | Human-readable execution status, warning or failure information |
| Calculation result manifest | Noun | Lightweight description of result availability and partitioning |
| Calculation result chunk | Noun | Stable bounded partition of a result |
| Calculation input | Role | Input data or referenced resource in a specification |
| Server-derived calculation result | Role | Output computed by the service |
| Calculation status projection | Role | Light DTO retaining a concrete case's identity |
| Calculation submission | Role | Create-and-evaluate operation |
| Immediate calculation submission | Role | Submission completes in its response |
| Queued calculation submission | Role | Submission schedules background execution |
| Calculation replacement | Role | Replace specification and reevaluate operation |
| Immediate calculation replacement | Role | Replacement completes in its response |
| Queued calculation replacement | Role | Replacement schedules background execution |
| Calculation case retrieval | Role | Retrieve the persisted aggregate |
| Calculation status retrieval | Role | Retrieve lightweight execution status |
| Calculation result retrieval | Role | Retrieve the complete result independently |
| Calculation result chunk retrieval | Role | Retrieve one bounded result partition |

The catalogue also adds the typed relations `HasInput`, `Produces` and `ProjectionOf`. Existing `HasPart` continues to describe conceptual constituents.

## Lifecycle decisions

- Immediate and queued behavior are operation-role specializations, not competing calculation-case types. The same calculation family can use either execution policy.
- `CalculationState` and `CalculationProgress` are optional lifecycle facts. An immediate operation need not expose polling or intermediate progress.
- A queued operation should expose lightweight status retrieval until it reaches a terminal state.
- A light DTO should bind to its concrete calculation-case noun with the `CalculationStatusProjection` role when it retains that case's identity. `CalculationStatusSnapshot` is available for a dedicated generic status representation.
- Caller-controlled specifications are distinct from results, state, progress and diagnostics derived by the server.
- Large results may be exposed through a manifest and stable chunks or pages without changing the meaning of the complete result.
- Optimistic-concurrency tokens, HTTP status codes, routes, pagination syntax and transport envelopes remain provider contract mechanics rather than domain vocabulary.

## Physical-quantity audit

No new physical quantity is required. `CalculationProgress` remains `ProportionStandard` with SI unit `1` and the range zero through one. States, messages, manifests, chunks and operation roles are not measurements.

## Provider binding guidance

Catalogue 0.15.0 permits `[Semantic]` on methods so providers can publish the same noun/role binding for an operation through OpenAPI and MCP. A provider should bind:

- the concrete calculation family as the noun;
- one of the submission, replacement or retrieval roles on the operation;
- caller-controlled members with `CalculationInput` where that role adds useful clarity;
- calculated members with `ServerDerivedCalculationResult`;
- light records with the concrete calculation noun plus `CalculationStatusProjection`.

This review does not certify or modify any provider. The next phase is to assess and update each microservice's REST/OpenAPI and MCP descriptions against these meanings.
