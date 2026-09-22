# Handoff prompt — streamline anti-collision planner

Paste the block below into a new thread. Everything else it needs is in the memory file, which the
new thread loads automatically through `MEMORY.md`.

---

## Prompt to paste

Continue work on the streamline-based anti-collision well path planner in
`C:\OSDC\DotNetLibraries\OSDC.DotnetLibraries.General` — project
`OSDC.DotnetLibraries.Drilling.Streamlines`, tests in the parallel `.UnitTest` project.

**Read the memory file `streamline-generator.md` first, and read its final section, "STATE AS OF
2026-09-21", before anything above it.** The file is append-only and several earlier entries were
superseded later the same day; that section says which, and it wins where they disagree.

### What it is

A Darcy/current flow solve used as an anti-collision path planner. Existing wells' 99 % uncertainty
volumes are zero-permeability obstacles on a linear octree; a slot is the source, a polygonal target
the sink; the traced streamlines are candidate well-path corridors. They are then bundled into
corridors, and each corridor is replaced by a *factory*: a median path, a convex tolerance region per
cross-section, and one probability density to draw realizations from.

Test case: Ullrigg, 13 real trajectories, a new slot at U1's wellhead, target 400 m out and 900 m
down. Two cases now exist, at entrance inclinations 0° and 85°.

### The chain

    generate (octree + TPFA + conduit at the slot + scalar channel + outlet guide)
      -> StreamlineBundler, PartitionRule.Separated
      -> StreamlineFactorySetBuilder.Build(source, bundles, zones)
      -> set.GetRoutes()
      -> route.GetRealizations(count, RealizationFilter, Random)

106 tests pass. Run the scenes with
`dotnet test -c Release --filter "FullyQualifiedName~WriteFactoryScenes"` (about 10 minutes, writes
`%TEMP%\ullrigg-factory-scene.json`). The 3D viewer is built from that by
`build_factory_view.py`, which lives in the previous session's scratchpad — ask me for it or rebuild
it; it takes a `cases` array and gives a case switcher plus two filter sliders (room, curvature).

### Where I would pick up

1. `TargetIncidence.Through` quantises the imposed arrival direction to the octree axes, so 85°
   lands on 90°. Right for axis-aligned targets, wrong for arbitrary ones.
2. The median is not smooth enough to be a plan — 6–15 °/30 m over a 30 m station against 3.7–6.6
   over 120 m, so mostly corner. The shared-head join has been tested and ruled out as the cause.
3. `DoubleArcs` from `OSDC.DotnetLibraries.Drilling.Section` as the reference spine instead of the
   Hermite. I was about to try this. The hook is `StreamlineGeneratorOptions.ChannelSpines`, so no
   new package dependency is needed, but a spine drawn between the two ends is obstacle-blind and
   must still go through `SpineRelaxer`.
4. More robustness cases: `UllriggFactoryScene.Inclinations` is a one-line edit per case.

### How I want you to work

Measure, do not predict. My mechanism intuitions have held up on this project; "which way should
this knob go" intuitions have been about 50/50, and every one that mattered was settled by a cheap
experiment. Report the median and a high quantile, not the worst; report clearance separately.

**Always measure curvature at two station lengths (30 m and 120 m, both normalised to 30 m).** Real
curvature reads the same at any window; a corner is a fixed angle and falls as one over the window.
That single check has caught several wrong conclusions.

Where an existing test asserts a value your change moves, stop and report it rather than updating
the expectation — a changed number is either a bug being fixed or a regression, and which one
matters.

### Uncommitted work

Three modified files and one new file are outstanding:
`Generation/StreamlineGenerator.cs`, `Generation/TargetPolygon.cs`,
`UnitTest/UllriggFactoryScene.cs`, and the new `UnitTest/ThroughConduitDiagnosis.cs`.
Everything else from the session is already committed.

---

## Why these files

- `streamline-generator.md` (memory, ~600 lines) — the full history with every measurement. The new
  thread loads it automatically. Its last section is the authoritative current state.
- `streamline-bundling.md` (memory) — the agreed Stage 1 bundling scope and the bundle-factory
  decisions that predate this session.
- This file — the prompt itself, so it survives the thread.
