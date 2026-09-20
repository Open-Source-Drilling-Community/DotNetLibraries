using System;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    /// <summary>
    /// The settings a trajectory reduction runs under. Every one of them has a default which is what the
    /// method was characterised with, so a caller who does not care can leave the whole thing alone.
    /// Specification section 1.
    /// </summary>
    public class ReductionOptions
    {
        /// <summary>
        /// How close to vertical a station has to be before the two azimuth carrying families are refused
        /// any interval containing it, expressed as the sine of the inclination. The default is the sine
        /// of three degrees. Specification section 3.5.
        /// </summary>
        public double MinimumSineInclination { get; set; } = ReducedOrderTrajectory.DefaultMinimumSineInclination;

        /// <summary>
        /// How many times the refinement may perturb its best answer and solve again. Each restart costs
        /// about as much as the first solve. Specification section 8.2.
        /// </summary>
        public int MaximumRestarts { get; set; } = 7;

        /// <summary>
        /// How long the refinement may spend on one trajectory before it stops restarting and returns the
        /// best it has. It is checked between solves, not inside one, so a single solve can overrun it.
        /// </summary>
        public TimeSpan RefinementBudget { get; set; } = TimeSpan.FromSeconds(20.0);

        /// <summary>
        /// The step the reported profile is sampled at when the deviation from the original is measured,
        /// m. The metrics were checked at four, two, one and half a metre and agree to five significant
        /// figures, so the default of two metres is not a compromise. Specification section 9.
        /// </summary>
        public double MeasurementStep { get; set; } = 2.0;

        /// <summary>
        /// Whether to run the refinement at all. Without it the answer is the chained initialisation,
        /// which is continuous and correctly segmented but drifts, by hundreds of metres on a long well.
        /// Turning it off is for inspecting the initialisation, not for production use.
        /// </summary>
        public bool Refine { get; set; } = true;

        /// <summary>
        /// Whether to reproduce a defect in the reference implementation's segmentation, rather than
        /// correcting it. False by default, so the correction is what a caller gets.
        ///
        /// The search for the longest interval a family can fit doubles its reach until a fit fails and
        /// then bisects. Where the doubling runs off the end of the survey instead of failing, the upper
        /// end of the bisection is the last station, which is feasible but which the bisection never
        /// evaluates, because it stops as soon as the two ends are adjacent. The last section of the well
        /// is then cut short and one more section than necessary comes out. Over the three wells of the
        /// reference vector set, correcting it saves a section in four of the nine segmentation cases.
        ///
        /// Set this true only to reproduce the reference bit for bit, which is what the golden vector
        /// tests do so that they keep checking the rest of the method against it.
        /// </summary>
        public bool ReproduceReferenceSegmentation { get; set; } = false;

        /// <summary>
        /// The seed of the perturbations the refinement restarts from. The same input and the same seed
        /// give the same answer, every time.
        /// </summary>
        public int RandomSeed { get; set; } = 0;

        /// <summary>
        /// How many Levenberg Marquardt iterations one solve may take. The solve usually converges in far
        /// fewer; this is the guard against a stalled one eating the budget.
        /// </summary>
        public int MaximumIterationsPerSolve { get; set; } = 100;

        /// <summary>
        /// A copy of these options, so that a caller's instance is never written to.
        /// </summary>
        public ReductionOptions Clone()
        {
            return new ReductionOptions()
            {
                MinimumSineInclination = MinimumSineInclination,
                MaximumRestarts = MaximumRestarts,
                RefinementBudget = RefinementBudget,
                MeasurementStep = MeasurementStep,
                Refine = Refine,
                ReproduceReferenceSegmentation = ReproduceReferenceSegmentation,
                RandomSeed = RandomSeed,
                MaximumIterationsPerSolve = MaximumIterationsPerSolve
            };
        }
    }
}
