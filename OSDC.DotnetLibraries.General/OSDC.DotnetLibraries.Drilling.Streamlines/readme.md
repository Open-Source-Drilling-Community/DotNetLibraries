# OSDC.DotnetLibraries.Drilling.Streamlines

`OSDC.DotnetLibraries.Drilling.Streamlines` groups non-crossing streamlines into bundles. A streamline is an ordered suite of positions; a bundle is a set of streamlines that travel together over their whole common extent. The streamlines are expected to resemble the flow lines of an anisotropic porous medium, or the current lines of an anisotropic conductor: they leave a few common positions, they end inside a common region, they never cross one another, and they part company where something obstructs their passage.

The .NET 8 class library depends only on `OSDC.DotnetLibraries.General.Math` and `OSDC.DotnetLibraries.General.Common`.

## No separation threshold

The caller does not supply a separation distance, and there is no length anywhere in the decision. Whether a gap counts as a parting is decided by comparing it with the local spacing of the streamlines themselves, through the dimensionless `ContrastRatio`. Multiplying every input coordinate by a constant leaves the bundles unchanged, and so does refining the sampling of the streamlines or the grid they were traced on. All three invariances are asserted by the unit tests.

The only quantity with a dimension is `SweepPlaneSpacing`, which is the resolution of the analysis rather than a criterion, and which is derived from the extent of the data when it is left null.

## How it works

1. Three families of parallel sweep planes, one per axis, cut the streamlines. A plane family that a streamline runs nearly along is not used for it: such a slice is not a cross-section of the flow, and two streamlines travelling side by side but offset along the flow would appear to have parted in it.
2. The crossings of a slab of consecutive planes form a cross-section. Merging several planes is what obliges a parting to persist: a gap present on one plane only is filled in by the neighbouring planes of the slab.
3. In each cross-section, every crossing is given a local scale — the median, over its nearest neighbours, of their distance to their own k-th nearest crossing — and two crossings are linked when their distance does not exceed `ContrastRatio` times the larger of the two scales. A group is a connected component of that relation. Measuring the scale from the neighbours rather than from the crossing itself is what keeps an isolated streamline isolated: its own distance to its k-th neighbour measures its isolation, not any spacing, and comparing the gap against it would be circular.
4. Two streamlines that are found adjacent in at least one cross-section, and that are never afterwards found in different groups of a cross-section where both are present, stay in the same bundle. The bundles are the connected components of what survives.

Because the state carried along the sweep is one integer per streamline, the cost is linear in the number of positions and the memory does not depend on how long the streamlines are.

## Getting started

```csharp
using OSDC.DotnetLibraries.Drilling.Streamlines;
using OSDC.DotnetLibraries.General.Math;

List<Streamline> streamlines = LoadStreamlines();

StreamlineBundler bundler = new StreamlineBundler();
StreamlineBundlingResult result = bundler.Bundle(streamlines);

foreach (StreamlineBundle bundle in result.Bundles)
{
    Console.WriteLine($"bundle {bundle.Index}: {bundle.Count} streamlines");
}

bool together = result.AreBundledTogether(12, 37);
```

A streamline that travels with no other one is a bundle of one.

### Large inputs

`Bundle(IReadOnlyList<Streamline>)` holds every position as a `Point3D`, which costs of the order of seventy bytes each. Past a few tens of millions of positions, implement `IStreamlineSource` instead and let the bundler stream: it makes three sequential passes and never holds more than the sweep plane crossings.

```csharp
public sealed class FileStreamlineSource : IStreamlineSource
{
    public int Count => ...;
    public Guid? GetID(int index) => ...;
    public string? GetName(int index) => ...;

    public IEnumerable<(double X, double Y, double Z)> GetPositions(int index)
    {
        // read the streamline from disk and yield its positions; do not cache
    }
}

StreamlineBundlingResult result = new StreamlineBundler().Bundle(new FileStreamlineSource(...));
```

Measured on one desktop machine, with the streamlines produced procedurally so that nothing is stored:

| streamlines | positions | time |
|---|---|---|
| 10 000 | 2.0 · 10⁷ | 2.1 s |
| 50 000 | 1.0 · 10⁸ | 7.6 s |
| 100 000 | 2.0 · 10⁸ | 17.9 s |

## Replacing a bundle by a factory

A `StreamlineBundleFactory` is a surrogate for one bundle: it produces as many streamlines of that bundle as are wanted, without holding any of the originals. It is built from a median curve, a series of cross-sections perpendicular to it at regular intervals, a transform per cross-section, and one probability density. Drawing a streamline means drawing a polar position in a normalized cross-section and carrying it through every cross-section, so a produced streamline is two numbers and everything else follows from them.

```csharp
StreamlineBundleFactoryBuilder builder = new StreamlineBundleFactoryBuilder();
List<StreamlineBundleFactory?> factories = builder.BuildAll(source, result, out var reasons);

StreamlineBundleFactory? factory = factories[0];
if (factory != null)
{
    Streamline median = factory.GetMedianCurve();
    List<Streamline> produced = factory.Draw(1000, new Random(1));   // seeded, so repeatable
    Streamline exact = factory.Generate(r, theta);                   // the same r and theta always
}
```

Four things are worth knowing about it.

**The median curve is asked for two things that do not both hold.** It should lie in the middle of the bundle, and it should have the shape, meaning the tangent, of the streamlines around it. The geometric median of each cross-section gives the first; it also wanders sideways wherever the cross-section is lopsided, and that wandering is tangent the bundle does not have. Smoothing buys the tangent back at the price of the position, and `MedianDisplacementBudget` says how much position may be paid. Position is favoured by default. `TangentDeviation` and `BundleTangentSpread` report what was achieved: the first is how far the median's tangent is from the bundle's, the second how far the streamlines are from one another, and the median is doing as well as it can be asked to when the first is no larger than the second.

**The angle is measured in a rotation minimising frame.** A Frenet frame would be undefined wherever the median curve is straight and would flip through an inflection, and these curves have long straight stretches. The angle zero is therefore a convention fixed by the frame at the first cross-section and carried from there; it is not tied to anything outside the factory.

**Production is deterministic in the drawn position**, so produced streamlines are smoother than real ones. What is left out is measured rather than discarded: `ResidualDeviation` is how far the streamlines of the bundle sit from where the factory puts them, and `ResidualCorrelationLength` is the distance along the curve over which that departure stays correlated — what an added roughness would need if one were wanted.

**A produced streamline can only be placed to the size of a density cell.** A draw picks a cell and then a position uniformly inside it, and the number of cells comes from the number of streamlines there were to estimate the density from, roughly one cell per `DensityCellPopulation` of them. Lower that setting for finer draws at the price of a noisier density.

Two limits are checked rather than assumed. `MaximumTubeRatio` is the largest product of the radius of the bundle and the curvature of the median curve; past one, the planes of neighbouring cross-sections cut into one another on the inside of the bend and produced streamlines can cross, so `IsTubeValid` says whether the description holds. And cross-sections are kept only where as many streamlines reach them as reach the best populated one, which trims the ends where a plane has begun to leave the bundle; the factory therefore covers a little less than the full length of the bundle.

A bundle too small to measure any of this from does not get a factory: `Build` returns null and a `StreamlineFactoryFailureReason` saying why. The default floor is eight streamlines, three being the algebraic minimum for the transform of a cross-section.

Measured on one desktop machine, building a factory from a bundle produced procedurally:

| streamlines in the bundle | positions | time | smaller by |
|---|---|---|---|
| 256 | 5.1 · 10⁵ | 0.19 s | 309 x |
| 2 500 | 5.0 · 10⁶ | 1.7 s | 3 098 x |
| 6 320 | 1.3 · 10⁷ | 4.4 s | 6 896 x |

## Generating streamlines: the grid

The `Generation` namespace builds the octree a planned well path is generated on. This is the first
increment of the generator: the grid, the obstacles and the question of whether a path exists at all.
There is no flow solver in it yet.

The problem it serves is anti-collision. Existing wells are given as their ellipse of uncertainty at every
survey station, swept along the trajectory into a solid; cells touching a solid are closed, so a path
through open cells clears the true volumes. A planned well starts at a slot, or at a tie-in on an existing
trajectory it is about to sidetrack, and has to reach a planar target region.

```csharp
ObstacleField field = new ObstacleField();
foreach (string file in wellFiles)
{
    field.Add(new WellboreUncertainty(SurveyFileReader.ReadWithUncertainty(file),
                                      wellheadNorth, wellheadEast, 0));
}

StreamlineSource tieIn = new StreamlineSource(position, direction)
{
    ParentWell = 3, ParentMeasuredDepth = 1240.0, ParentMutedLength = 30.0
};
StreamlineGrid grid = StreamlineGrid.Build(field, new[] { tieIn }, target);

if (grid.Status != StreamlineGridStatus.Connected)
{
    // SourceBlocked, TargetBlocked or NotConnected, before any flow has been computed
}
```

Four things are worth knowing.

**The cells follow the passages, not a schedule.** A cell is divided until it is at most
`ClearanceCellFraction` of the width of the passage it sits in, the passage being the room between the two
nearest distinct obstacles. So the grid is finest exactly where two wells run close. On cluster data that
turns out to be the *top* of the hole, not the bottom: near surface the wells are metres apart while the
uncertainty is still small, and by the time the uncertainty is tens of metres wide the wells have fanned
out much further. Measured on the thirteen Ullrigg trajectories, the median passage grows from about two
metres in the first two hundred metres to hundreds of metres below a kilometre, a range of more than three
hundred in required cell size. No fixed grid spans that.

**The classification is one sided.** A cell is closed as soon as it *touches* a volume, not only when its
centre is inside one, so the obstacle is effectively grown by up to one cell. A path that is found is
therefore certainly clear; what resolution buys is not safety but the avoidance of a false refusal.
`FinestCellSize` is that margin, and near surface, where the obstacle is a half-metre conductor, it is
comparable to the obstacle itself.

**A tie-in starts inside its parent.** Several of the Ullrigg wells are sidetracks and come within 0.35 m
of their parents, which is not a near miss but the same hole. `ParentMutedLength` stops the parent being an
obstacle over a short interval around the window; without it every sidetrack returns `SourceBlocked`. It
exists only so the start is not inside a solid, and it is deliberately not the separation a sidetrack needs
from its parent, which is an outcome to be measured rather than a parameter to be set.

**"No path" is an answer.** A flood fill over open cells runs before any solve. It costs milliseconds and
it separates two things that otherwise look identical: a geometry with no way through, and a solver that
has gone wrong.

Measured on Ullrigg, thirteen trajectories, cells from 0.5 m to 32 m, on one desktop machine:

| case | leaves | closed | build |
|---|---|---|---|
| new slot in the cluster to a target at 900 m | 1.19 M | 299 k | 2.2 s |
| sidetrack off the deepest well | 0.35 M | 116 k | 0.6 s |

The uncertainty model is not a detail. Growing the volumes from 0.05 % to 0.5 % of measured depth takes the
closed cells from 167 k to 534 k, and on a tight cluster it is what decides whether the shallow section is
passable at all: with volumes taken well by well rather than relative to a shared survey reference, wells
from one platform lie inside one another near surface and nothing can be threaded between them.

## What the result does not say

Bundles are global: two streamlines that start apart and converge are found in different groups on the early cross-sections and are therefore kept in different bundles, even though they share the rest of their path. A set of streamlines that travel together only over part of their length, and the depths at which an obstruction parts a bundle, need the interval form of the result, which this version does not produce.

Expect the partition to be fine grained on data where everything leaves a tight cluster and fans out towards separate targets. Two paths that share a corridor for a long stretch and then part for their own targets are, correctly under this definition, two bundles.

## Settings

| setting | default | what it does |
|---|---|---|
| `SweepPlaneSpacing` | null | resolution of the analysis; largest extent divided by `MaximumPlanesPerAxis` when null |
| `MaximumPlanesPerAxis` | 512 | how many planes span the largest extent |
| `SlabThickness` | 3 | planes merged into one cross-section; how long a parting must persist |
| `ContrastRatio` | 3.0 | how much wider than the local spacing a gap must be to count as a parting |
| `NeighbourCount` | 6 | rank of the neighbour measuring the local spacing |
| `MinimumObliquity` | 0.5 | how squarely a streamline must cross a plane for that crossing to be used |
| `SeparationTolerance` | 0 | cross-sections on which two neighbours may be apart and still bundled |
| `UseParallelism` | true | affects speed only; the result is identical either way |

## Repository layout

- `OSDC.DotnetLibraries.Drilling.Streamlines/` — library source, license, and this README.
- `OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest/` — NUnit project, with the synthetic fixtures and the invariance tests.

## License

`OSDC.DotnetLibraries.Drilling.Streamlines` is released under the [Apache License 2.0](LICENSE).
