namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// The settings that shape the octree a planned well is generated on.
    /// </summary>
    public class StreamlineGridOptions
    {
        /// <summary>
        /// the smallest cell the octree is allowed to make, m
        /// </summary>
        public double FinestCellSize { get; set; } = 0.5;

        /// <summary>
        /// the largest cell, used wherever nothing asks for better, m
        /// </summary>
        public double CoarsestCellSize { get; set; } = 25.0;

        /// <summary>
        /// What fraction of the width of a passage one cell may take.
        /// <para>
        /// Classifying conservatively costs up to one cell of clearance on each side of a passage, and a
        /// channel needs a few cells across it before the flow through it means anything, so a gap of
        /// width g and a cell of size h leave about g minus two h of open channel and n = g/h minus two
        /// cells across it. A quarter puts two cells across the narrowest passage that is still meant to
        /// be usable; lower it for more.
        /// </para>
        /// </summary>
        public double ClearanceCellFraction { get; set; } = 0.25;

        /// <summary>
        /// what fraction of the local semi-minor axis of an obstacle one cell may take where the cell
        /// straddles its surface, so that the shape of the solid is resolved even where there is nothing
        /// else nearby to squeeze the passage
        /// </summary>
        public double SurfaceCellFraction { get; set; } = 0.5;

        /// <summary>
        /// How much room to leave around the sources and the target, as a fraction of the distance
        /// between them.
        /// <para>
        /// The edge of the box is a wall, so it can only push the flow inward and never let it spread.
        /// Too small a margin therefore squeezes the corridor without saying so. Doubling it and checking
        /// that the streamlines do not move is the way to know it is wide enough.
        /// </para>
        /// </summary>
        public double MarginFraction { get; set; } = 0.5;

        /// <summary>
        /// the least margin to leave whatever the span, m
        /// </summary>
        public double MinimumMargin { get; set; } = 100.0;

        /// <summary>
        /// how far around a source the octree is taken to its finest, as a multiple of the finest cell
        /// </summary>
        public double SourceRefinementRadius { get; set; } = 20.0;

        /// <summary>
        /// The vertical above which every cell is shut, m, positive downward, or null to leave the box
        /// open at the top.
        /// <para>
        /// The box carries a margin on all six sides, so without this it reaches above the surface and a
        /// homogeneous medium sends a share of the flow up into it. Set it to the ground, or to the
        /// shallowest point a planned well is allowed to reach. Only cells lying wholly above it are shut,
        /// so a source sitting exactly on it still has its own cell.
        /// </para>
        /// </summary>
        public double? CeilingVertical { get; set; } = null;

        /// <summary>
        /// The vertical below which every cell is shut, m, positive downward, or null to leave the box
        /// open at the bottom.
        /// <para>
        /// The counterpart of <see cref="CeilingVertical"/>, and it matters most when the target admits
        /// only its upper face. The margin leaves hundreds of metres of open medium under the target, and
        /// flow that wanders down there can only come back by climbing to an admitted face, which turns a
        /// path round on itself. Set it at the target, or at the deepest a planned well may go.
        /// </para>
        /// </summary>
        public double? FloorVertical { get; set; } = null;

        /// <summary>
        /// checks that the settings are usable
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool IsValid(out string? reason)
        {
            reason = null;
            if (!(FinestCellSize > 0))
            {
                reason = "FinestCellSize must be strictly positive.";
            }
            else if (!(CoarsestCellSize >= FinestCellSize))
            {
                reason = "CoarsestCellSize cannot be smaller than FinestCellSize.";
            }
            else if (!(ClearanceCellFraction > 0) || ClearanceCellFraction > 1)
            {
                reason = "ClearanceCellFraction must be in (0, 1].";
            }
            else if (!(SurfaceCellFraction > 0) || SurfaceCellFraction > 1)
            {
                reason = "SurfaceCellFraction must be in (0, 1].";
            }
            else if (MarginFraction < 0)
            {
                reason = "MarginFraction cannot be negative.";
            }
            else if (MinimumMargin < 0)
            {
                reason = "MinimumMargin cannot be negative.";
            }
            else if (SourceRefinementRadius < 0)
            {
                reason = "SourceRefinementRadius cannot be negative.";
            }
            else if (CeilingVertical != null && FloorVertical != null
                     && FloorVertical.Value <= CeilingVertical.Value)
            {
                reason = "FloorVertical must lie below CeilingVertical.";
            }
            return reason == null;
        }
    }
}
