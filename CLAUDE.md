# CLAUDE.md — CTC survey-calculation update

## What this is

This folder is a handoff bundle. It contains a validated reference implementation and golden test vectors for a new, closed-form method of constructing a CTC (Constant Toolface / Constant Curvature) curve from a survey station. The task is to port it into the existing C# codebase.

The method is described in `SPEC_CTC_from_survey.md`, which is **normative** — where this file and the spec disagree, the spec wins. `CTC_from_survey_note.md` is the background note explaining why the change is being made; read it for context but implement from the spec.

## The task

Replace the existing calculation of CTC curve-defining parameters from two survey stations with the closed-form solution in the spec.

The change in one line: given two survey attitudes and the along-hole distance between them, the curvature and toolface angle are

```
beta = dTheta/L,  t = G*dAlpha/L,  kappa = hypot(beta, t),  phi = atan2(t, beta)
```

where `G` is the effective sine defined in spec §4. This is closed form. The previous approach — a curvature formula with a `beta = 0` branch, `arccos` for the toolface magnitude, and a trial-and-error test for its sign — is replaced entirely.

## Start by finding the existing code

Do not assume a file layout. Before writing anything, locate:

1. The class that computes curvature, build-up rate, toolface angle and turn rate from two survey stations. Likely named around `Ctc`, `ConstantToolface`, `Survey`, `Trajectory`, `CurveParameters`, or similar. Grep for `Acos`, `Atan`, `Math.Log`, `Tan(` near an inclination variable.
2. The forward CTC method — the one that takes curvature, toolface angle and curve length and produces the next station. This is the "standard method" and should already exist.
3. The composite Simpson integration used for the North and East coordinates, probably with a hard-coded 16 subintervals.
4. The Levenberg–Marquardt target-point solver, so you can leave it alone (see "Out of scope").
5. Existing unit tests covering any of the above.

Report what you find and how you propose to map it onto the new code **before** making changes.

## Constraints

- **Pure SI internally.** Radians, metres, rad/m, vertical positive downward. If the existing code carries degrees or °/30 m, convert at the public boundary only and keep the core in SI. Do not introduce a degrees path into the algorithm.
- **Match the existing naming and style conventions of the repository**, not the ones in `CtcSurvey.cs`. That file is a reference implementation, not a drop-in — its names were chosen to be self-explanatory in isolation.
- **Preserve the public API** where existing callers depend on it. If a signature must change, add an overload rather than breaking callers, and list every call site you touched.
- **`branch` is a new optional parameter, defaulting to 0.** See spec §6. Never silently select a nonzero branch.
- **No new NuGet dependencies.** The Gauss–Legendre rule is ~40 lines and is included in the reference file.
- **Target framework:** match whatever the existing projects target. `CtcSurvey.cs` uses `readonly struct` (C# 7.2+) and has an `#if` fallback for `Math.Atanh` on .NET Framework / .NET Standard 2.0. Adjust if the repo targets something older.

## Acceptance

The port is done when `ctc_reference_vectors.json` passes within the declared tolerances. `CtcSurveyVectorTests.cs` is a ready xUnit harness for this; adapt it to the repo's test framework if it uses NUnit or MSTest.

Also confirm:

- The forward and inverse methods round-trip: `Forward(kappa, phi, L)` then `FromSurvey` returns the original `kappa` and `phi` to ~1e-13.
- Existing tests still pass. Where an existing test asserts a value that the new method changes, **stop and report it** rather than updating the expectation — a changed number is either a bug being fixed or a regression, and which one matters.

## Out of scope — do not touch

- The **Levenberg–Marquardt target-point solver** (thesis §3.3). It solves a different problem — find `(kappa, phi, L)` reaching a given target coordinate — which is genuinely nonlinear and has no closed form. Nothing in this bundle replaces it. It benefits indirectly because its residual calls the forward method, but its own logic stays as it is.
- Anything to do with well/wellbore data models, geodesy, magnetic declination, or unit-system plumbing.
- Do not add a root-finder or optimiser anywhere in the survey path. If the port seems to need one, the port is wrong.

## Notes and things to verify rather than assume

- The reference implementation is verified against the Python source of truth on Mono: max relative error 9.5e-14 in `kappa`, 6.0e-14 rad in `phi`, 1.4e-9 m in position, across all 256 vectors.
- The existing Simpson rule with 16 subintervals is *not* accurate enough for long, strongly turning sections — measured worst case 0.2 m. Spec §5.3 gives the replacement and, if Simpson must be retained somewhere, the corrected node count.
- Check whether anything else in the codebase calls the `ln(tan/tan)` expression directly — the torsion calculation with a `1e-3` m finite-difference step is a likely second site, and it has the same cancellation problem. Report it; fixing it is a reasonable follow-up but is not part of this task.
- Check whether azimuth is stored wrapped or continuous. The `unwrapAzimuth` parameter exists for callers that already carry a continuous azimuth; using the wrong one silently changes results near north.

## Files in this bundle

| File | Role |
|---|---|
| `SPEC_CTC_from_survey.md` | Normative specification. Implement from this. |
| `CtcSurvey.cs` | Reference C# implementation, compiles and passes all vectors. |
| `ctc_reference_vectors.json` | 220 inverse + 36 forward golden cases with tolerances. |
| `ctc_reference_vectors_inverse.tsv`, `..._forward.tsv` | Same data, flat, for harnesses without a JSON dependency. |
| `CtcSurveyVectorTests.cs` | xUnit harness reading the JSON. |
| `VerifyVectors.cs` | Dependency-free console checker reading the TSV. Compile-verified. |
| `CTC_from_survey_note.md` | Background: derivation, error analysis, why the old method fails. |
| `ctc_survey.py`, `validate_ctc.py`, `make_vectors.py` | Python source of truth and vector generator. |
