# Cross-domain operation profiles and execution provenance — 2026-10-09

Status: **curated at the request of Eric Cayeux on 2026-10-09**. Catalogue 0.18.0 adds shared operation-role, lifecycle-state, paging and execution-provenance vocabulary. Updating provider bindings and MCP contracts is a separate step.

## Purpose

Provider contracts expose many domain types, including wells, wellbores, trajectories, architectures, drill-strings, fluids, targets and scientific models. Consumers should not require a separate orchestration workflow for every type. The contract must identify the externally observable operation pattern while the bound noun identifies the domain subject.

The increment defines four reusable operation families:

1. resource and collection management;
2. stateless evaluation;
3. persisted calculation-case lifecycle;
4. paged or chunked transfer.

These are operation roles rather than domain nouns. A provider binds an operation to its concrete subject concept and the applicable role. Transport routes, HTTP verbs, MCP tool names and natural-language descriptions do not establish the role.

## Resource operations

`ResourceOperation` specializes into collection retrieval, individual retrieval, creation, replacement, partial update and deletion. These roles apply uniformly to any managed resource. Provider contracts continue to own identity paths, relationship and cardinality metadata, filters, ordering, pagination, authorization, effects, validation, idempotency and optimistic concurrency.

Calculation submission, replacement, case retrieval and deletion also inherit the matching resource roles because calculation cases are managed resources. Their additional `CalculationCaseOperation` ancestry preserves the stronger lifecycle semantics.

## Stateless evaluation

`StatelessEvaluation` describes a read-only invocation that derives outputs from supplied inputs without creating or modifying a managed resource. It does not assert that a provider is mathematically pure or that different provider revisions produce identical results. Providers must still bind every required input and output concept, unit, reference, model condition, accuracy and uncertainty needed for safe composition.

## Calculation lifecycle

The existing calculation operation roles now inherit `CalculationCaseOperation`. The increment adds separate creation, execution-start and cancellation roles for services that do not combine these actions into submission. Cancellation remains distinct from deletion.

Reviewed children of `CalculationState` give contracts a common machine-readable lifecycle vocabulary: queued, running, completed, failed and cancelled. Providers may retain their wire enum values but must explicitly map them to these meanings. Terminal-state, retry and retention behavior remains contract metadata.

## Paging and chunking

`StreamOperation`, `PageRetrieval` and `ChunkRetrieval` describe bounded transfer of a larger logical value. `CollectionPage`, `CollectionItem` and `ContinuationToken` describe common response roles. A partial page or chunk never proves completeness. Contracts must declare ordering, continuation, completion and finalization behavior.

## Execution provenance

`ExecutionProvenance` contains recorded `OperationInvocation` evidence. `ToolInvocation` specializes it for calls through identified callable tools, including MCP. Supporting roles identify the provider, invoked operation, exact contract revision, attempt sequence, outcome, start/completion instants and whether the invocation contributed to a particular result.

This vocabulary enables a consumer to answer questions such as “which tools were called to produce the previous answer?” only when the consumer also records and retains invocation evidence. The vocabulary does not create that journal, expose private arguments or responses, or prove that a successful call was numerically correct. Planned but undispatched operations are not tool invocations, and retries remain separate attempts.

## DrillWeaver-owned vocabulary

DrillWeaver's current project, conversation, managed-user, role-assignment and project-information concepts describe its own application data and authorization boundaries. They remain in the `urn:drillweaver:context` namespace. They should move into the shared OSDC catalogue only when an OSDC-wide meaning and provider contract are curated independently of DrillWeaver.

The reusable parts needed by DrillWeaver are included here: operation profiles, lifecycle meanings, collection transfer and provenance. Deployment identities, enabled-tool state, schema fingerprints, execution journal identities and conversation-private bindings remain consumer runtime data rather than shared domain vocabulary.

## Provider binding guidance

A conforming MCP operation should publish:

- its concrete subject concept;
- one reviewed operation role;
- semantic bindings for identities, inputs and outputs;
- standard MCP effect hints plus provider-specific concurrency, authorization and lifecycle facts;
- paging or chunk roles when a response is partial;
- calculation state mappings and result association for asynchronous cases.

The catalogue identifies reusable meanings. A future transport-neutral contract schema may package additional machine-readable paths and policies, such as identity JSON pointers, state-value mappings, terminal-state sets and result-link paths. Those structural bindings should reference these URNs rather than create service-specific operation vocabularies.
