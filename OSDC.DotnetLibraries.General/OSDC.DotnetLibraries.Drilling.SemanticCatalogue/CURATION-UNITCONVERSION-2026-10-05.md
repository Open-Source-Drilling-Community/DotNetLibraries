# Unit Conversion REST/MCP semantic-vocabulary review

Status: **curated from the Unit Conversion domain and requested by Eric Cayeux on 2026-10-05**. Catalogue 0.14.0 adds 60 Reviewed definitions to the unchanged 517-entry 0.13.0 baseline. Packaging, publication and provider annotation are separate operations.

## Sources reviewed

- `C:\OSDC\UnitConversion\Conversion`, especially `BasePhysicalQuantity`, `UnitChoice` and their quantity hierarchy
- `C:\OSDC\UnitConversion\ConversionUnitSystem\BaseUnitSystem.cs`
- `C:\OSDC\UnitConversion\Model` conversion and persisted-set models
- PhysicalQuantity, UnitSystem, UnitConversionSet and UnitSystemConversionSet REST controllers
- MCP quantity discovery, direct conversion, unit-system conversion, unit-system management and documentation-search contracts

REST and MCP are treated as transports over one metrology domain. HTTP routes, paging mechanics, usage counters and MCP transport envelopes are not duplicated as semantic concepts.

## Added scope

The increment covers:

- physical-quantity identity, canonical names, synonyms, descriptions and typical symbols;
- the nine dimensional exponents exposed by the service;
- SI units, unit choices, names, labels, LaTeX labels, synonyms and default/SI designations;
- affine conversion scale, bias, formulas and descriptions;
- meaningful SI display precision, quantity specialization and inherited compatible units;
- managed unit systems, validated quantity-to-unit assignments and the built-in SI, Metric, Imperial and US systems;
- direct conversions, unit-system conversions, persisted conversion sets and quantity batches;
- numeric versus formatted results, semantic examples and quantity/document discovery.

## Physical-quantity audit

No new engineering physical quantity is required. This service defines and applies the physical-quantity catalogue; it does not introduce a missing measurement kind merely by exposing conversion metadata.

Generic conversion input and output values acquire their physical quantity dynamically from the selected `PhysicalQuantity`. The generic vocabulary concepts therefore deliberately have no fixed `quantityName`. The same applies to dimensional exponents, affine coefficients and meaningful-precision metadata: binding these generic fields to one engineering quantity would be false. Numeric results retain full precision; `MeaningfulPrecisionInSI` affects formatting only.

No UnitConversion package source, package dependency or canonical drilling reference-profile binding changes in 0.14.0.

## Contract constraints retained

- Quantity and unit UUIDs remain stable identities; names and synonyms are discovery aids.
- Unit resolution is scoped to a quantity, including compatible units inherited from an ancestor quantity.
- Unit-system mappings are validated, and `IsSI` is derived by the service rather than trusted from callers.
- Conversion changes representation, never the underlying physical quantity.
- Persisted conversion-set outputs are calculated by the service.
- Documentation semantic search returns discovery resources, not quantity definitions or conversion results.
