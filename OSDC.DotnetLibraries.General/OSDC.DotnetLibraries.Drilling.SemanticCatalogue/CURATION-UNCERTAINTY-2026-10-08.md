# Uncertainty result contract completion, 2026-10-08

User authorization: complete missing semantic terms and generic execution pieces for trajectory quantities, including U4 vertical uncertainty ellipse dimensions and orientation at MD/confidence.

Added major/minor axis roles (full diameters), uncertainty ellipse orientation (PlaneAngleDrilling), vertical section azimuth (PlaneAngleDrilling) and a precise vertical ellipse angular convention. Physical quantities resolve through OSDC UnitConversion, never local UUIDs. The convention follows the shared Surveying projected covariance formula: major-axis direction is (-sin(phi),cos(phi)) in (section distance, positive-down TVD). Confidence remains the established probability consumed by GetChiSquare3D; do not substitute a normal-distribution multiplier. Existing concepts and wire quantities are retained.

Four explicit path-intersection references were also added: ground/mud line, drill floor, wellhead and MSL. They distinguish along-hole origins from vertical datums and require an established unique path/surface intersection without implicit extrapolation. All 597 published definitions remain unchanged. Catalogue 0.17.0 adds nine definitions, producing 606 total / 603 Reviewed.
