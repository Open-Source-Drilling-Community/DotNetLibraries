using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// A probability density over a disc, held as a histogram on a polar grid of equal-area cells, and
    /// exact to draw from.
    /// <para>
    /// The disc is divided into <see cref="RingCount"/> rings of equal radial thickness, and ring j is
    /// divided into 3(2j+1) angular cells. Every cell of the grid then has the same area, whatever its
    /// ring, and its angular extent is about equal to its radial extent, so the resolution is the same
    /// everywhere and the centre is not divided more finely than the data can support. The usual polar
    /// grid, with the same number of angular bins in every ring, has cells whose area falls off towards
    /// the centre and reports a spurious concentration there.
    /// </para>
    /// <para>
    /// A cell is drawn in proportion to how many samples fell in it, and the position within the cell is
    /// then uniform by area, so the draw is unbiased without the density ever being interpolated.
    /// </para>
    /// </summary>
    public class PolarDensity
    {
        private readonly double[] cumulative_;

        /// <summary>
        /// the number of rings the disc is divided into
        /// </summary>
        public int RingCount { get; }

        /// <summary>
        /// the radius of the disc. A sample beyond it is counted in the outermost ring.
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// how many samples fell in each cell, ring by ring
        /// </summary>
        public double[] CellCounts { get; }

        /// <summary>
        /// the total number of samples
        /// </summary>
        public double SampleCount { get; private set; }

        /// <summary>
        /// the area of one cell
        /// </summary>
        public double CellArea { get; }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="ringCount"></param>
        /// <param name="radius"></param>
        public PolarDensity(int ringCount, double radius)
        {
            if (ringCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(ringCount));
            }
            if (!(radius > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }
            RingCount = ringCount;
            Radius = radius;
            CellCounts = new double[3 * ringCount * ringCount];
            cumulative_ = new double[CellCounts.Length];
            double thickness = radius / ringCount;
            CellArea = System.Math.PI * thickness * thickness / 3.0;
        }

        /// <summary>
        /// the total number of cells
        /// </summary>
        public int CellCount
        {
            get
            {
                return CellCounts.Length;
            }
        }

        /// <summary>
        /// the number of angular cells in the given ring
        /// </summary>
        /// <param name="ring"></param>
        /// <returns></returns>
        public static int GetAngularCount(int ring)
        {
            return 3 * (2 * ring + 1);
        }

        /// <summary>
        /// the index of the first cell of the given ring
        /// </summary>
        /// <param name="ring"></param>
        /// <returns></returns>
        public static int GetRingOffset(int ring)
        {
            return 3 * ring * ring;
        }

        /// <summary>
        /// the cell a position falls in, or -1 when the position is not a number
        /// </summary>
        /// <param name="r"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        public int GetCell(double r, double theta)
        {
            if (double.IsNaN(r) || double.IsNaN(theta))
            {
                return -1;
            }
            double thickness = Radius / RingCount;
            int ring = r <= 0 ? 0 : (int)(r / thickness);
            if (ring >= RingCount)
            {
                ring = RingCount - 1;
            }
            int angular = GetAngularCount(ring);
            double turn = theta / (2.0 * System.Math.PI);
            turn -= System.Math.Floor(turn);
            int sector = (int)(turn * angular);
            if (sector >= angular)
            {
                sector = angular - 1;
            }
            return GetRingOffset(ring) + sector;
        }

        /// <summary>
        /// adds one sample
        /// </summary>
        /// <param name="r"></param>
        /// <param name="theta"></param>
        /// <param name="weight"></param>
        public void Add(double r, double theta, double weight = 1.0)
        {
            int cell = GetCell(r, theta);
            if (cell >= 0)
            {
                CellCounts[cell] += weight;
                SampleCount += weight;
            }
        }

        /// <summary>
        /// Spreads a fraction of every cell over the cells it touches, so that the density does not
        /// report a hole where the bundle simply happens to have no member. Call once, after all the
        /// samples have been added and before drawing.
        /// </summary>
        /// <param name="strength">between 0 and 1</param>
        public void Smooth(double strength)
        {
            if (!(strength > 0))
            {
                return;
            }
            if (strength > 1)
            {
                strength = 1;
            }
            double[] spread = new double[CellCounts.Length];
            for (int ring = 0; ring < RingCount; ring++)
            {
                int angular = GetAngularCount(ring);
                int offset = GetRingOffset(ring);
                for (int sector = 0; sector < angular; sector++)
                {
                    int cell = offset + sector;
                    double keep = CellCounts[cell] * (1.0 - strength);
                    double give = CellCounts[cell] * strength;
                    spread[cell] += keep;
                    // the two angular neighbours of the same ring, and the radial neighbours found by
                    // matching the angle across rings
                    int previous = offset + (sector + angular - 1) % angular;
                    int next = offset + (sector + 1) % angular;
                    double middle = (sector + 0.5) / angular;
                    int inner = ring > 0 ? NeighbourInRing(ring - 1, middle) : -1;
                    int outer = ring + 1 < RingCount ? NeighbourInRing(ring + 1, middle) : -1;
                    int count = 2 + (inner >= 0 ? 1 : 0) + (outer >= 0 ? 1 : 0);
                    double share = give / count;
                    spread[previous] += share;
                    spread[next] += share;
                    if (inner >= 0) { spread[inner] += share; }
                    if (outer >= 0) { spread[outer] += share; }
                }
            }
            Array.Copy(spread, CellCounts, CellCounts.Length);
        }

        private static int NeighbourInRing(int ring, double turn)
        {
            int angular = GetAngularCount(ring);
            int sector = (int)(turn * angular);
            if (sector >= angular)
            {
                sector = angular - 1;
            }
            return GetRingOffset(ring) + sector;
        }

        /// <summary>
        /// prepares the density for drawing. Must be called after the samples have been added.
        /// </summary>
        public void Prepare()
        {
            double running = 0;
            for (int cell = 0; cell < CellCounts.Length; cell++)
            {
                running += CellCounts[cell];
                cumulative_[cell] = running;
            }
            SampleCount = running;
        }

        /// <summary>
        /// the probability density at the given position, per unit area
        /// </summary>
        /// <param name="r"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        public double Evaluate(double r, double theta)
        {
            if (!(SampleCount > 0))
            {
                return 0;
            }
            int cell = GetCell(r, theta);
            return cell < 0 ? 0 : CellCounts[cell] / (SampleCount * CellArea);
        }

        /// <summary>
        /// draws a position from the density. <paramref name="firstUniform"/> and the two others are
        /// independent uniform deviates on [0, 1).
        /// </summary>
        /// <param name="firstUniform"></param>
        /// <param name="secondUniform"></param>
        /// <param name="thirdUniform"></param>
        /// <param name="r"></param>
        /// <param name="theta"></param>
        /// <returns>false when the density holds no sample at all</returns>
        public bool Draw(double firstUniform, double secondUniform, double thirdUniform,
                         out double r, out double theta)
        {
            r = 0;
            theta = 0;
            double total = cumulative_.Length > 0 ? cumulative_[cumulative_.Length - 1] : 0;
            if (!(total > 0))
            {
                return false;
            }
            double target = firstUniform * total;
            int low = 0;
            int high = cumulative_.Length - 1;
            while (low < high)
            {
                int middle = (low + high) / 2;
                if (cumulative_[middle] <= target)
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle;
                }
            }
            int cell = low;

            // which ring the cell belongs to: the offset of ring j is 3j squared
            int ring = (int)System.Math.Sqrt(cell / 3.0);
            while (ring + 1 < RingCount && GetRingOffset(ring + 1) <= cell)
            {
                ring++;
            }
            while (ring > 0 && GetRingOffset(ring) > cell)
            {
                ring--;
            }
            int sector = cell - GetRingOffset(ring);
            int angular = GetAngularCount(ring);

            // uniform by area inside the cell
            double thickness = Radius / RingCount;
            double inner = ring * thickness;
            double outer = (ring + 1) * thickness;
            r = System.Math.Sqrt(inner * inner + secondUniform * (outer * outer - inner * inner));
            theta = 2.0 * System.Math.PI * (sector + thirdUniform) / angular;
            return true;
        }

        /// <summary>
        /// draws a position from the density using the given generator, or the shared one when it is null
        /// </summary>
        /// <param name="random"></param>
        /// <param name="r"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        public bool Draw(Random? random, out double r, out double theta)
        {
            if (random != null)
            {
                return Draw(random.NextDouble(), random.NextDouble(), random.NextDouble(), out r, out theta);
            }
            RandomGenerator generator = RandomGenerator.Instance;
            return Draw(generator.NextDouble(), generator.NextDouble(), generator.NextDouble(), out r, out theta);
        }
    }
}
