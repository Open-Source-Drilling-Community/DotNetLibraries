# OSDC.DotnetLibraries.Drilling.WebAppUtils

Shared .NET utilities and small configuration abstractions used by OSDC drilling web applications.

The package targets `net8.0` and is intended for Blazor/web-page projects that need common host URL configuration, API client setup, and reusable unit/reference source objects for `OSDC.UnitConversion.DrillingRazorMudComponents`.

## Features

- Host URL interfaces for drilling-related microservices.
- `APIUtils.SetHttpClient(...)` helper for JSON HTTP clients.
- `DataUtils.UnitAndReferenceParameters` for shared unit/reference selector state.
- `DataUtils` reference-source classes compatible with `MudUnitAndReferenceChoiceTag`.
- Support for depth, position, and geodetic datum reference sources.

## Installation

```powershell
dotnet add package OSDC.DotnetLibraries.Drilling.WebAppUtils
```

The package depends on:

- `OSDC.UnitConversion.DrillingRazorMudComponents`

## Host URL Interfaces

The package provides small interfaces for projects that need to pass service host URLs through configuration or dependency injection.

Available interfaces:

- `ICartographicProjectionHostURL`
- `IClusterHostURL`
- `IDrillingFluidHostURL`
- `IDrillStringHostURL`
- `IFieldHostURL`
- `IGeodeticDatumHostURL`
- `IGeologicalPropertiesHostURL`
- `IGeothermalPropertiesHostURL`
- `IRigHostURL`
- `ISurveyInstrumentHostURL`
- `ITrajectoryHostURL`
- `IUnitConversionHostURL`
- `IWellBoreArchitectureHostURL`
- `IWellBoreHostURL`
- `IWellHostURL`

Each interface exposes a nullable string property named after the service. For example:

```csharp
using OSDC.DotnetLibraries.Drilling.WebAppUtils;

public sealed class WebPagesHostConfiguration : IWellHostURL, ITrajectoryHostURL
{
    public string? WellHostURL { get; set; }
    public string? TrajectoryHostURL { get; set; }
}
```

Example dependency injection registration:

```csharp
builder.Services.AddSingleton<IWellHostURL>(configuration);
builder.Services.AddSingleton<ITrajectoryHostURL>(configuration);
```

## APIUtils

`APIUtils.SetHttpClient` creates an `HttpClient` with:

- `BaseAddress = new Uri(host + microServiceUri)`
- `Accept: application/json`
- a handler that accepts the server certificate presented by the service

```csharp
HttpClient httpClient = APIUtils.SetHttpClient(
    host: "https://localhost:5001",
    microServiceUri: "/api/Trajectory/");
```

## DataUtils

`DataUtils` contains lightweight classes used by drilling web pages to share unit and reference settings with `MudUnitAndReferenceChoiceTag`.

### UnitAndReferenceParameters

```csharp
DataUtils.UnitAndReferenceParameters.UnitSystemName
DataUtils.UnitAndReferenceParameters.DepthReferenceName
DataUtils.UnitAndReferenceParameters.PositionReferenceName
DataUtils.UnitAndReferenceParameters.GeodeticReferenceName
DataUtils.UnitAndReferenceParameters.AzimuthReferenceName
DataUtils.UnitAndReferenceParameters.PressureReferenceName
DataUtils.UnitAndReferenceParameters.DateReferenceName
```

Defaults:

- `UnitSystemName = "Metric"`
- `DepthReferenceName = "Rotary table"`
- `PositionReferenceName = "Well-head"`

The remaining reference names default to `null`.

### Reference Source Classes

The nested `DataUtils` classes implement the reference-source interfaces from `OSDC.UnitConversion.DrillingRazorMudComponents`.

Depth references:

```csharp
DataUtils.GroundMudLineDepthReferenceSource
DataUtils.RotaryTableDepthReferenceSource
DataUtils.SeaWaterLevelDepthReferenceSource
```

Position references:

```csharp
DataUtils.WellHeadPositionReferenceSource
DataUtils.CartographicGridPositionReferenceSource
DataUtils.FieldPositionReferenceSource
DataUtils.ClusterPositionReferenceSource
```

Geodetic datum reference:

```csharp
DataUtils.CartographicProjectionDatumGeodeticReferenceSource
```

`FieldPositionReferenceSource` is the current name for the former lease-line reference concept.

## Reference Source Properties

### Depth

```csharp
public class GroundMudLineDepthReferenceSource : IGroundMudLineDepthReferenceSource
{
    public double? GroundMudLineDepthReference { get; set; }
}

public class RotaryTableDepthReferenceSource : IRotaryTableDepthReferenceSource
{
    public double? RotaryTableDepthReference { get; set; }
}

public class SeaWaterLevelDepthReferenceSource : ISeaWaterLevelDepthReferenceSource
{
    public double? SeaWaterLevelDepthReference { get; set; }
}
```

### Position

```csharp
public class WellHeadPositionReferenceSource : IWellHeadPositionReferenceSource
{
    public double? WellHeadNorthPositionReference { get; set; }
    public double? WellHeadEastPositionReference { get; set; }
}

public class CartographicGridPositionReferenceSource : ICartographicGridPositionReferenceSource
{
    public double? CartographicGridNorthPositionReference { get; set; }
    public double? CartographicGridEastPositionReference { get; set; }
}

public class FieldPositionReferenceSource : IFieldPositionReferenceSource
{
    public double? FieldNorthPositionReference { get; set; }
    public double? FieldEastPositionReference { get; set; }
}

public class ClusterPositionReferenceSource : IClusterPositionReferenceSource
{
    public double? ClusterNorthPositionReference { get; set; }
    public double? ClusterEastPositionReference { get; set; }
}
```

### Geodetic Datum

```csharp
public class CartographicProjectionDatumGeodeticReferenceSource
    : ICartographicProjectionDatumGeodeticReferenceSource
{
    public double? CartographicProjectionDatumLatitudeReference { get; set; }
    public double? CartographicProjectionDatumLongitudeReference { get; set; }
}
```

Latitude and longitude references are expected in SI angle units.

## Example: Wiring MudUnitAndReferenceChoiceTag

```razor
@using OSDC.DotnetLibraries.Drilling.WebAppUtils
@using OSDC.UnitConversion.DrillingRazorMudComponents

<MudUnitAndReferenceChoiceTag UnitSystemName="@DataUtils.UnitAndReferenceParameters.UnitSystemName"
                              DepthReferenceName="@DataUtils.UnitAndReferenceParameters.DepthReferenceName"
                              PositionReferenceName="@DataUtils.UnitAndReferenceParameters.PositionReferenceName"
                              GeodeticReferenceName="@DataUtils.UnitAndReferenceParameters.GeodeticReferenceName"
                              RotaryTableDepthReferenceSource="@rotaryTable"
                              WellHeadPositionReferenceSource="@wellHead"
                              FieldPositionReferenceSource="@field"
                              CartographicProjectionDatumGeodeticReferenceSource="@cartographicProjectionDatum">
    @ChildContent
</MudUnitAndReferenceChoiceTag>

@code {
    private readonly DataUtils.RotaryTableDepthReferenceSource rotaryTable = new();
    private readonly DataUtils.WellHeadPositionReferenceSource wellHead = new();
    private readonly DataUtils.FieldPositionReferenceSource field = new();
    private readonly DataUtils.CartographicProjectionDatumGeodeticReferenceSource cartographicProjectionDatum = new();
}
```

## Project Structure

```text
OSDC.DotnetLibraries.Drilling.WebAppUtils/
├── APIUtils.cs
├── DataUtils.cs
├── I*HostURL.cs
├── OSDC.DotnetLibraries.Drilling.WebAppUtils.csproj
└── Readme.md
```

## Notes

- Reference source values are expected in SI units.
- The helper source classes are intentionally small data carriers.
- `FieldPositionReferenceSource` replaces the older `LeaseLinePositionReferenceSource` naming.
- Use `CartographicProjectionDatumGeodeticReferenceSource` when enabling the geodetic reference selector in `MudUnitAndReferenceChoiceTag`.

## License

This project is licensed under the terms specified in the `LICENSE` file.
