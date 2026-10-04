# OSDC.DotnetLibraries.Drilling.Section

`OSDC.DotnetLibraries.Drilling.Section` is the directional drilling geometry kernel used by the NORCE WellPath toolset. The .NET 8 class library defines reusable curve primitives for joining two curvilinear survey points and solving for missing directional parameters through deterministic and iterative solvers. It depends on the internal `OSDC.DotnetLibraries.General.Math`, `OSDC.DotnetLibraries.General.Common`, and `OSDC.DotnetLibraries.General.Statistics` packages.

## Highlights

- `ArcSection` base type wraps a `Start` and `End` `CurvilinearPoint3D`, exposes the underlying `NonLocalizedCurve`, and provides interpolation helpers such as `InterpolateAtMD` and `InterpolateAtTVD`.
- `CircularArcSection` implements the standard survey conversions (`LIA`, `SIA`, `DIA`, `XYZ`, `DTZ`, `DTI`, `LDT`, `SDT`) with awareness of physical dimensions so that only valid combinations of measured depth, inclination, azimuth, toolface, and dogleg severity are accepted.
- `BuildAndTurnArcSection` solves build/turn problems by combining build rate, turn rate, length, and target constraints and routes to specialized solvers such as `CalculateLBT`, `CalculateSIA`, `CalculateXYZ`, etc. The curvature-constrained Cartesian overload considers alternative roots, accepts an absolute position tolerance, and reports whether no root exists or all roots exceed the supplied SI curvature limit. `TargetAxisPath` retains the rejected geometric section in the latter case so clients need not repeat the inverse calculation merely to classify it.
- `TargetAxisPath.MaximumCurvature` applies that curvature-aware selection to constant-build-and-turn legs aimed at position-only targets. It is expressed in radians per metre; leaving it unset preserves the historical shortest-root behavior.
- Additional section types cover constant curvature with toolface, double arcs, cubic transitions, target axis steering, and axis utilities (`ComplexAxis`) for defining reference frames.
- `ComplexSection` chains multiple circular sections, automatically enforces parameter counts, and uses the statistics helpers to find deterministic solutions that honor control points.
- All section types are `[Serializable]` so they can be exchanged over JSON or binary channels, and the project ships with Apache 2.0 licensing.

## Repository layout

- `OSDC.DotnetLibraries.Drilling.Section/` — library source, license, and this README.
- `OSDC.DotnetLibraries.Drilling.Section.UnitTest/` — NUnit tests for the section solvers.
- `OSDC.DotnetLibraries.General.sln` — solution that groups this library with the other general and drilling libraries.

## Getting started

### Prerequisites

1. Install the .NET SDK 6.0 (or newer) which includes the netcoreapp3.1 targeting pack: `dotnet --info`.
2. Make sure you can restore the `OSDC.DotnetLibraries.General.*` packages from the configured NuGet feeds.

### Restore and build the library

```powershell
dotnet restore OSDC.DotnetLibraries.Drilling.Section/OSDC.DotnetLibraries.Drilling.Section.csproj
dotnet build OSDC.DotnetLibraries.Drilling.Section/OSDC.DotnetLibraries.Drilling.Section.csproj -c Release
```

### Run the section unit tests

```powershell
dotnet test NORCE.Drilling.UnitTest.Section/NORCE.Drilling.UnitTest.Section.csproj
```

## Usage

### Solve a single circular arc

```csharp
using OSDC.DotnetLibraries.Drilling.Section;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

var section = new CircularArcSection
{
    Start = new CurvilinearPoint3D
    {
        X = 0,
        Y = 0,
        Z = -2500,
        Abscissa = 1200,
        Incl = 10.0 * Numeric.PI / 180.0,
        Az = 20.0 * Numeric.PI / 180.0
    },
    End = new CurvilinearPoint3D
    {
        Incl = 60.0 * Numeric.PI / 180.0,
        Az = 110.0 * Numeric.PI / 180.0
    }
};

section.Circle.Length = 300.0;
section.Circle.ReferenceToolface = 15.0 * Numeric.PI / 180.0;

if (!section.CalculateLIA())
{
    throw new InvalidOperationException("Unable to solve LIA for the requested inputs.");
}

var md1350 = section.InterpolateAtMD(1350.0);
var tvd = section.InterpolateAtTVD(-2640.0);
```

### Chain multiple sections

```csharp
var tangent = new CircularArcSection
{
    Start = section.End,
    End = new CurvilinearPoint3D { Z = -2650 }
};

tangent.Circle.Length = 80.0;
tangent.Circle.Curvature = 2.0 / 1000.0;

var lateral = new CircularArcSection
{
    Start = tangent.End,
    End = new CurvilinearPoint3D { X = 450, Y = 35, Z = -3000 }
};

lateral.Circle.Length = 600.0;
lateral.Circle.ReferenceToolface = 0.0;

var profile = new ComplexSection { section, tangent, lateral };

if (!profile.Calculate())
{
    throw new InvalidOperationException("The trajectory is over or under determined.");
}
```

The complex section checks each segment for the correct number of constraints, automatically calls the appropriate solvers, and propagates updates downstream.

## Extending the library

Add new section types by inheriting from `ArcSection`, providing a concrete curve (`NonLocalizedCurve`) implementation, and overriding `Calculate`/`InterpolateAtMD`. The existing classes are good templates: they guard against under-defined inputs, keep the start and end points consistent, and make heavy use of `Numeric` helpers to stay numerically stable.

## License

`OSDC.DotnetLibraries.Drilling.Section` is released under the [Apache License 2.0](LICENSE).
