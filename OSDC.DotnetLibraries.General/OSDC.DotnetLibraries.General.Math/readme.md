This package is developed as part of the Society of Petroleum (SPE) Open Source Drilling Community, a sub-committee of the Drilling System Automation Technical Section.
This package contains standard math classes.

Version 1.4.1 exposes all distinct exact constant-build-and-turn roots found for a Cartesian target through `TrajectoryPoint3D.SolveBTTargetSolutions`. Each result includes length, build rate, turn rate, and exact peak curvature. `CompleteBTXYZ(TrajectoryPoint3D, maximumCurvature)` selects the shortest root satisfying a peak-curvature limit while retaining the original shortest-root overload for compatibility.
