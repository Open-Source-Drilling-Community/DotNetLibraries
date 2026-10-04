This package is developed as part of the Society of Petroleum (SPE) Open Source Drilling Community, a sub-committee of the Drilling System Automation Technical Section.
This package contains standard math classes.

Version 1.4.2 exposes all distinct constant-build-and-turn roots found for a Cartesian target through `TrajectoryPoint3D.SolveBTTargetSolutions`. Each result includes length, build rate, turn rate, and peak curvature. Tolerance-aware overloads stop the inverse solve at the requested Cartesian accuracy, and curvature-constrained completion distinguishes no geometric solution from roots rejected by curvature while retaining a rejected root for inspection. The exact-default overloads remain available for compatibility.
