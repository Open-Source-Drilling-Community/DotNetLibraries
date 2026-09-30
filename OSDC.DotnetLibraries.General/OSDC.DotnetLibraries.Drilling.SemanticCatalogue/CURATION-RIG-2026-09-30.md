# Rig vocabulary analysis — 2026-09-30

Status: **curated and approved by Eric Cayeux on 2026-09-30**. Catalogue 0.11.0 adds 78 Reviewed entries to the 292-entry 0.10.0 baseline. Provider integration, NuGet publication and deployment are separate operations.

## Evidence and scope

The review used the current `C:\OSDC\Rig` model, its generated schema inputs, REST/MCP helpers and WebPages unit inference, together with the existing shared drilling vocabulary. The provider currently has two independent name-based quantity maps: `WebPages/Shared/DataUtils.cs` and `Service/Mcp/Tools/McpToolArgumentHelpers.cs`. Those maps are evidence for this curation, but they are not authoritative semantics and should later be replaced by one provider-owned binding registry.

This increment defines Rig aggregates and equipment, reusable engineering measurands and limit roles. It does not change Rig payloads, persistence, calculations or enum serialization.

## Approved physical conventions

- OSDC vertical depth is referred to WGS84 and is positive downward. Existing `EllipsoidalDepth` and `DrillFloorDepth` semantics retain that convention through the canonical drilling reference profile.
- Elevation is a signed vertical coordinate relative to an explicit local origin and is positive upward. No universal elevation origin is added to the canonical profile.
- `DistanceToBit` specializes `Elevation`; its origin is the front face of the identified bit and its positive direction is upward.
- Rig pressure ratings are absolute pressures referred to vacuum. A pressure difference remains a distinct measurand.
- Equipment fields named `Weight` in the reviewed Rig model denote mass and use `MassDrilling` / kg. `DrillingMarineRiser.JointWeight` and `DrillLine.LinearWeight` denote linear mass density and retain `LinearMassDensity` / kg/m.
- Heave-compensator capacity is force (`ForceDrilling` / N).
- `StandPipe.PressureMeasurementElevation` and `StandPipe.MudHoseHangingPointElevation` use the drill-floor origin and are positive upward.
- Top-drive controller gains are dimensionless (`ProportionStandard` / 1).
- Generator and other rotational speed values are angular velocity (`AngularVelocityDrilling` / rad/s).

## Provider binding matrix

| Rig model path or pattern | Concept | Role/reference or rule |
|---|---|---|
| `Rig` | existing `Rig` | Managed resource root. |
| `Rig.Identification` | `RigIdentification` | Identity and ownership block. |
| `Rig.RigType` | `RigType` | Preserve exact closed enum tokens. |
| `Rig.OperatingEnvironment` | `RigOperatingEnvironment` | Preserve exact closed enum tokens. |
| `Rig.MobilityType` | `RigMobilityType` | Preserve exact closed enum tokens. |
| `RigComponentBase` | `RigComponent` | Base containment concept. |
| `RigEquipmentBase` | `RigEquipment` | Specializes `RigComponent`. |
| `RigMast`, hoisting, envelope and specialized profile blocks | corresponding structural nouns | Bind by declaring type and property, not display label alone. |
| `MudPump`, `CementPump`, `MudTank`, `ShaleShaker`, `Generator`, `TopDrive`, `RotaryTable`, `Drawworks`, BOP, MPD, marine-riser, heave-compensator and flow-routing equipment | corresponding equipment nouns | Each specializes `RigEquipment`. |
| `StandPipe.PressureMeasurementElevation`, `StandPipe.MudHoseHangingPointElevation` | `Elevation` | Explicit `DrillFloorUpward` reference. |
| `FixedPlatformProperties.DrillFloorDepth` | existing `DrillFloorDepth` | Canonical WGS84 reference; Gaussian mean carries the coordinate, standard deviation carries same-dimensional uncertainty without an origin. |
| `*.MaxLimitDesignPressure` and equivalent design-pressure fields | `AbsolutePressureRating` | `MaximumDesignLimit`; vacuum reference inherited from `AbsolutePressure`. |
| `*.MaxLimitOperatingPressure` | `AbsolutePressureRating` | `MaximumOperatingLimit`. |
| `*.MinLimitOperatingPressure` | `AbsolutePressureRating` | `MinimumOperatingLimit`. |
| `*.MaxLimitTestPressure` | `AbsolutePressureRating` | `MaximumTestLimit`. |
| activation, discharge, static and dynamic pressure ratings | `AbsolutePressureRating` | Use the corresponding pressure-limit role. |
| `AutoDriller.*DifferentialPressure` | existing `PressureDifference` | Never bind to absolute pressure or vacuum. |
| absolute pressure measurement accuracy | `AbsoluteMeasurementAccuracy` | Dynamic measurand quantity; no vacuum/reference offset on accuracy. |
| equipment `Weight` fields | `EquipmentMass` | kg; despite the legacy property name, these are not force. |
| `DrillingMarineRiser.JointWeight`, `DrillLine.LinearWeight` | existing `LinearMassDensity` | kg/m. |
| heave-compensator capacity | `ForceCapacity` | N. |
| `TopDrive.ProportionalGain`, `TopDrive.IntegralGain` | `ControllerGain` | Dimensionless. |
| `Generator.Speed` and rotational-speed properties | `RotationalAngularVelocity` | rad/s. |
| maximum drilling depth and maximum water depth | `DrillingDepthCapacity`, `WaterDepthCapacity` | Capability/extent; no coordinate origin or datum binding. |
| `EquipmentMeasurementCapability.Minimum`, `.Maximum` | `MeasurementRangeValue` | `MeasurementRangeMinimum` / `MeasurementRangeMaximum`; quantity is declared by sibling `PhysicalQuantity`. |
| `EquipmentMeasurementCapability.AbsoluteAccuracy` | `AbsoluteMeasurementAccuracy` | Same dynamic quantity as the measurand. |
| `EquipmentMeasurementCapability.RelativeAccuracy` | `RelativeMeasurementAccuracy` | Dimensionless. |
| `EquipmentMeasurementCapability.UpdateFrequency` | `SignalFrequency` | Hz. |
| `MaxLimitDesign*`, `MaxLimitOperating*`, `MinLimitOperating*`, `MaxLimitTest*` | measurand-specific concept | Add `MaximumDesignLimit`, `MaximumOperatingLimit`, `MinimumOperatingLimit` or `MaximumTestLimit`. |
| continuous/intermittent, make-up/breakout, cold/warm-start fields | measurand-specific concept | Add the corresponding reviewed role. |
| physical extents and diameters | existing `PhysicalLengthExtent` / `PipeDiameter` specializations | Reuse existing concepts and applicable roles. |

Classification enum literals remain the provider's exact closed-schema tokens. This release gives each classification property a stable semantic dimension; cross-provider identities for individual literals should be introduced only when a concrete interoperability requirement exists.

## Integration boundary

Catalogue 0.11.0 and this binding matrix are the source for the next Rig integration step. After the package is published, Rig should reference that exact version and introduce a single `ProviderSemantics` registry keyed by declaring type and property identity. REST/OpenAPI/MCP descriptions and WebPages unit presentation should consume that registry so the UI and MCP cannot infer different quantities from the same property name.

No Rig repository file is changed by this vocabulary-only release. No NuGet package is published and no service is deployed as part of this curation.
