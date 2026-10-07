# Calculation-case deletion vocabulary — 2026-10-06

Status: **curated at the request of Eric Cayeux on 2026-10-06**. Catalogue 0.16.0 adds one Reviewed operation role to the published 596-entry 0.15.0 baseline. Provider bindings, publication and deployment remain separate operations.

## Reviewed addition

| Concept | Kind | Purpose |
| --- | --- | --- |
| Calculation case deletion | Role | Permanently remove a persisted calculation case and its service-owned stored lifecycle information and results |

## Decisions

- Deletion is distinct from calculation replacement. Replacement retains the case identity and starts reevaluation; deletion removes the persisted case.
- Deletion is also distinct from cancellation. Removing a case does not by itself guarantee that already-running work is interrupted. A provider must state such cancellation behavior explicitly if it supports it.
- The role is destructive. REST and MCP providers remain responsible for publishing accurate destructive-write annotations, authorization requirements and stable error semantics.
- Optimistic concurrency remains a provider contract concern. Where the resource supports a modification token, delete operations require the latest opaque token just as other mutations do.
- Temporary-case cleanup deletes only the exact case created for the calculation. It must not delete trajectories, surveys, catalog entries or other referenced inputs.
- No general resource-deletion role is introduced in this increment. This narrowly reviewed role expresses the persisted calculation-case lifecycle requested here without claiming semantics for unrelated resources.

## Physical-quantity audit

No new physical quantity or canonical reference is required. Deletion is an operation role, not an engineering measurement.

## Provider binding guidance

Bind a delete operation to the concrete calculation-family noun and `CalculationCaseDeletion` as its role. Publish the same binding through REST/OpenAPI and MCP. The transport contract should also declare destructive effect, concurrency requirements, not-found and conflict behavior, and whether deletion can cancel queued or running execution.
