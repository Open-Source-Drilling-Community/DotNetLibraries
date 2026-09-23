# Handoff prompt — streamline anti-collision planner, fault management

Paste the block below into a new thread. Everything else it needs is in the memory files, which the
new thread loads automatically through `MEMORY.md`.

---

## Prompt to paste

Continue work on the streamline-based anti-collision well path planner in
`C:\OSDC\DotNetLibraries\OSDC.DotnetLibraries.General` — project
`OSDC.DotnetLibraries.Drilling.Streamlines`, tests in the parallel `.UnitTest` project.

**Read the memory file `streamline-generator.md` first, and read its final section, "STATE AS OF
2026-09-23", before anything above it.** The file is append-only and earlier entries were superseded;
that section says which, and it wins where they disagree. Then read `streamline-faults.md`, which
covers the fault work specifically.

### What it is

A Darcy/current flow solve used as an anti-collision path planner. Existing wells' 99 % uncertainty
volumes are zero-permeability obstacles on a linear octree; a slot or a sidetrack window is the
source, a polygonal target the sink; the traced streamlines are bundled into corridors, and each
corridor is replaced by a *factory*: a median path, a convex tolerance region per cross-section, and
one probability density to draw realizations from. Faults are now carried through the chain too.

Test case: Ullrigg, 13 real trajectories, **four cases**:

| case | start | target |
|---|---|---|
| 0° entrance | new slot beside U1's wellhead | flat, entered vertically |
| 85° entrance | same slot | standing on end, entered at 85° |
| U3 sidetrack | window on U3 at **MD 275 m** | flat, entered vertically |
| U3 sidetrack, 85° | same window | standing on end, entered at 85° |

Cases sharing a target can be superimposed in the 3D view.

### The chain

    generate (octree + TPFA + conduit at the source + scalar channel + outlet guide
              + optional fault mobility)
      -> StreamlineBundler, PartitionRule.Separated
      -> StreamlineFactorySetBuilder.Build(source, bundles, zones)
      -> FaultTrim.Apply per corridor          (post-process, pulls boundaries back from faults)
      -> set.GetRoutes()
      -> route.GetRealizations(count, RealizationFilter, Random)

106 tests pass. Run the scenes with
`dotnet test -c Release --filter "FullyQualifiedName~WriteFactoryScenes"` (about 8 minutes for all
four cases, writes `%TEMP%\ullrigg-factory-scene.json`). Build the 3D view with
`python build_factory_view.py` in the UnitTest directory, then serve it
(`python -m http.server 8080 --bind 127.0.0.1`) and open `ullrigg-factory-view.html`.

There is also a cheap diagnostic, `ReportTheCaseStarts` (~30 s, no flow solved), which checks every
case's window, clearance, start cell and conduit before committing to a full run. **Use it.** It has
caught a blocked window and a sealed conduit that would each have cost a full run to discover.

### Uncommitted work

Commit `59e3548` holds the sidetrack cases, four bug fixes and the speed work. Everything since is
uncommitted and is the fault management:

- new: `UllriggFaultSet.cs`, `FaultTrim.cs`, `FaultMobility.cs`, `UllriggFaults/` (247 data files)
- modified: `UllriggFactoryScene.cs`, `build_factory_view.py`, `factory-view-source.html`,
  `ullrigg-factory-view.html`, the `.csproj` (copy rule for the fault data), and
  `StreamlineGenerator.cs` (16 lines: the `SuppliedMobility` hook)
- `temp/FaultExamples` is the raw input the fault files were generated from; it is untracked and can
  be deleted once the generated data is committed.

### Current settings, all in `UllriggFactoryScene.cs` unless noted

| setting | value | what it does |
|---|---|---|
| `ReportedFaultExpansion` | 200 m | box around start→target that selects faults for a case |
| `FaultOverlapLimit` | 0.50 | most of a cross-section a corridor may give up to avoid one fault |
| `FaultTrim.Standoff` | 0.5 m | how far short of a fault the boundary stops |
| `FaultCrossings.CrossingTolerance` | 0.25 m | how deep a fault must reach to count as crossed |
| `UseFaultMobility` | true | whether the flow feels the faults |
| `FaultBandWidth` / `FaultContrast` | 50 m / 20 | the fault mobility field |

### Where I would pick up

1. **Commit the fault work.** It is a coherent increment and the tests pass.
2. **Sweep `FaultContrast`.** 20 is a first guess never swept. The mobility field bought a large
   improvement in crossing obliquity and cost 2–3× the corner at 30 m — but the 120 m window barely
   moved, so the cost is corner, not turn. A gentler contrast may buy most of the benefit for much
   less. Measure at both station lengths.
3. **The 85° sidetrack trims nothing** — all 36 of its fault encounters are median crossings, so no
   trimming rule can help it. If that case matters, splitting the corridor at the fault is the route,
   the way `StreamlineFactorySetBuilder` already splits a bundle whose median sits in a forbidden
   zone.
4. **A cumulative trim budget**, if corridors grazing several faults turn out to be over-trimmed. One
   case reached 94 % worst loss. If you add it, make it a **separate named knob**, not the same
   number as the overlap limit — conflating them was a bug once already.
5. Older open items that remain: the median is not smooth enough to be a plan (corner, not turn — the
   two-window check says so); `TargetIncidence.Through` quantises the imposed direction to the octree
   axes; `MaximumSplitRounds` (3) is exhausted on the slot cases.

### How I want you to work

Measure, do not predict. My mechanism intuitions have held up on this project; "which way should
this knob go" intuitions have been about 50/50, and every one that mattered was settled by a cheap
experiment.

**Always measure curvature at two station lengths (30 m and 120 m, both normalised to 30 m).** Real
curvature reads the same at any window; a corner is a fixed angle and falls as one over the window.
That single check has caught several wrong conclusions.

Report the median and a high quantile, not the worst; report clearance separately.

Where an existing test asserts a value your change moves, stop and report it rather than updating the
expectation — a changed number is either a bug being fixed or a regression, and which one matters.

**When a geometric result looks wrong, distrust the criterion before the arithmetic.** Every one of
the five defects found in the fault work was in what counted as a hit, never in the intersection
mathematics. Twice a fix was applied that was itself unsound, and only measurement caught it.

---

## Why these files

- `streamline-generator.md` (memory) — the full history with every measurement. Its last section is
  the authoritative current state.
- `streamline-faults.md` (memory) — the fault work: the data, the selection, the crossing rule, the
  trim, the mobility field, and the five defects with their measurements.
- `streamline-bundling.md` (memory) — the Stage 1 bundling scope and the bundle-factory decisions
  that predate all of this.
- This file — the prompt itself, so it survives the thread.
