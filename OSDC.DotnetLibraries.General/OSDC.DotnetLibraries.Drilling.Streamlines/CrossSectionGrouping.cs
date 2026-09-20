namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// Groups the crossings of a single cross-section into locally connected sets.
    /// <para>
    /// Every crossing is given a local scale: the distance to its <c>NeighbourCount</c>-th nearest
    /// crossing is measured first, ignoring the other crossings of its own streamline, and the local
    /// scale is then the median of that distance over its nearest neighbours. Two crossings are linked
    /// when their distance does not exceed <c>ContrastRatio</c> times the larger of their two local
    /// scales, and a group is a connected component of that relation.
    /// </para>
    /// <para>
    /// Taking the median over the neighbours rather than a crossing's own measurement matters. A
    /// crossing that sits on its own, far from everything, measures a large distance to its k-th
    /// neighbour, and that distance is a measure of its isolation, not of any local spacing. Comparing
    /// its gap against it would be circular and would link it to whatever it is isolated from. The
    /// neighbours of an isolated crossing, on the other hand, report the spacing of the group it is
    /// being held away from, which is the scale the gap has to be judged against.
    /// </para>
    /// <para>
    /// Because the comparison is with a local spacing and not with a fixed length, a region where the
    /// streamlines happen to be denser, as around a locally refined grid or a tight cluster of sources,
    /// does not by itself separate anything.
    /// </para>
    /// <para>
    /// One instance is not thread safe: it owns the scratch storage reused from one cross-section to the
    /// next. Use one instance per thread.
    /// </para>
    /// </summary>
    internal sealed class CrossSectionGrouping
    {
        private readonly int neighbourCount_;
        private readonly double contrastRatio_;
        private readonly int maximumLinkCandidates_;

        private int[] cellStart_ = Array.Empty<int>();
        private int[] cellFill_ = Array.Empty<int>();
        private int[] cellItem_ = Array.Empty<int>();
        private int[] cellOf_ = Array.Empty<int>();
        private double[] core_ = Array.Empty<double>();
        private double[] scale_ = Array.Empty<double>();
        private int[] neighbour_ = Array.Empty<int>();
        private int[] neighbourCountOf_ = Array.Empty<int>();
        private readonly double[] nearest_;
        private readonly int[] nearestOf_;
        private readonly double[] ranked_;
        private UnionFind? unionFind_;

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="neighbourCount"></param>
        /// <param name="contrastRatio"></param>
        /// <param name="maximumLinkCandidates"></param>
        public CrossSectionGrouping(int neighbourCount, double contrastRatio, int maximumLinkCandidates)
        {
            neighbourCount_ = neighbourCount;
            contrastRatio_ = contrastRatio;
            maximumLinkCandidates_ = maximumLinkCandidates;
            nearest_ = new double[neighbourCount];
            nearestOf_ = new int[neighbourCount];
            ranked_ = new double[neighbourCount];
        }

        /// <summary>
        /// Groups the crossings held in <paramref name="u"/> and <paramref name="v"/> over
        /// [<paramref name="start"/>, <paramref name="start"/> + <paramref name="count"/>).
        /// </summary>
        /// <param name="u">first in-plane coordinate of every crossing</param>
        /// <param name="v">second in-plane coordinate of every crossing</param>
        /// <param name="streamlineOf">the streamline every crossing belongs to</param>
        /// <param name="start">index of the first crossing of the cross-section</param>
        /// <param name="count">number of crossings in the cross-section</param>
        /// <param name="labels">receives the group of every crossing, as a local index in [0, count)</param>
        /// <param name="edges">receives a spanning forest of the groups, as pairs of local indices</param>
        /// <param name="edgeStart">where to start writing in <paramref name="edges"/></param>
        /// <param name="edgeCount">how many edges were written</param>
        /// <returns>the number of groups</returns>
        public int Group(float[] u, float[] v, int[] streamlineOf, int start, int count,
                         int[] labels, long[] edges, int edgeStart, out int edgeCount)
        {
            edgeCount = 0;
            if (count <= 0)
            {
                return 0;
            }
            if (count == 1)
            {
                labels[0] = 0;
                return 1;
            }

            double minU = double.MaxValue;
            double maxU = double.MinValue;
            double minV = double.MaxValue;
            double maxV = double.MinValue;
            for (int i = 0; i < count; i++)
            {
                double pu = u[start + i];
                double pv = v[start + i];
                if (pu < minU) { minU = pu; }
                if (pu > maxU) { maxU = pu; }
                if (pv < minV) { minV = pv; }
                if (pv > maxV) { maxV = pv; }
            }
            double width = maxU - minU;
            double height = maxV - minV;

            if (!(width > 0) && !(height > 0))
            {
                // every crossing is at the same place: a single group, linked as a star
                for (int i = 0; i < count; i++)
                {
                    labels[i] = 0;
                }
                for (int i = 1; i < count; i++)
                {
                    edges[edgeStart + edgeCount] = (uint)i;
                    edgeCount++;
                }
                return 1;
            }

            // a bucket grid holding about one crossing per cell
            double cell = System.Math.Sqrt(width * height / count);
            double span = System.Math.Max(width, height);
            if (!(cell > 0))
            {
                // all the crossings are aligned: lay the cells out along the occupied direction
                cell = span / count;
            }
            if (!(cell > 0))
            {
                cell = 1.0;
            }
            double smallest = span * 1e-9;
            if (cell < smallest)
            {
                cell = smallest;
            }
            int nx = (int)(width / cell) + 1;
            int ny = (int)(height / cell) + 1;
            long cellLimit = 4L * count + 64L;
            while ((long)nx * ny > cellLimit)
            {
                cell *= 1.4;
                nx = (int)(width / cell) + 1;
                ny = (int)(height / cell) + 1;
            }
            int cellCount = nx * ny;

            EnsureCapacity(count, cellCount);

            // bucket the crossings, by counting sort on the cell index
            int[] cellStart = cellStart_;
            int[] cellItem = cellItem_;
            int[] cellOf = cellOf_;
            Array.Clear(cellStart, 0, cellCount + 1);
            for (int i = 0; i < count; i++)
            {
                int cx = CellIndex(u[start + i], minU, cell, nx);
                int cy = CellIndex(v[start + i], minV, cell, ny);
                int c = cy * nx + cx;
                cellOf[i] = c;
                cellStart[c + 1]++;
            }
            for (int c = 0; c < cellCount; c++)
            {
                cellStart[c + 1] += cellStart[c];
            }
            int[] fill = cellFill_;
            Array.Clear(fill, 0, cellCount);
            for (int i = 0; i < count; i++)
            {
                int c = cellOf[i];
                cellItem[cellStart[c] + fill[c]] = i;
                fill[c]++;
            }

            ComputeLocalScales(u, v, streamlineOf, start, count, minU, minV, cell, nx, ny, cellStart, cellItem);

            // link every crossing to everything within its own reach, and keep the merges as a
            // spanning forest of the resulting groups
            UnionFind unionFind = unionFind_!;
            unionFind.Reset(count);
            double[] scale = scale_;
            for (int i = 0; i < count; i++)
            {
                double reach = contrastRatio_ * scale[i];
                double reach2 = reach * reach;
                double pu = u[start + i];
                double pv = v[start + i];
                int cxi = CellIndex(pu, minU, cell, nx);
                int cyi = CellIndex(pv, minV, cell, ny);
                int radius = (int)(reach / cell) + 1;
                int examined = 0;
                int xFrom = System.Math.Max(0, cxi - radius);
                int xTo = System.Math.Min(nx - 1, cxi + radius);
                int yFrom = System.Math.Max(0, cyi - radius);
                int yTo = System.Math.Min(ny - 1, cyi + radius);
                for (int cy = yFrom; cy <= yTo && examined <= maximumLinkCandidates_; cy++)
                {
                    int rowBase = cy * nx;
                    for (int cx = xFrom; cx <= xTo && examined <= maximumLinkCandidates_; cx++)
                    {
                        int c = rowBase + cx;
                        int from = cellStart[c];
                        int to = cellStart[c + 1];
                        for (int s = from; s < to; s++)
                        {
                            int j = cellItem[s];
                            if (j == i)
                            {
                                continue;
                            }
                            examined++;
                            double du = u[start + j] - pu;
                            double dv = v[start + j] - pv;
                            if (du * du + dv * dv <= reach2 && unionFind.Union(i, j))
                            {
                                int a = i < j ? i : j;
                                int b = i < j ? j : i;
                                edges[edgeStart + edgeCount] = ((long)a << 32) | (uint)b;
                                edgeCount++;
                            }
                        }
                    }
                }
            }

            // compact the roots into dense group indices
            for (int i = 0; i < count; i++)
            {
                labels[i] = -1;
            }
            int groupCount = 0;
            for (int i = 0; i < count; i++)
            {
                int root = unionFind.Find(i);
                if (labels[root] < 0)
                {
                    labels[root] = groupCount;
                    groupCount++;
                }
            }
            for (int i = 0; i < count; i++)
            {
                labels[i] = labels[unionFind.Find(i)];
            }
            return groupCount;
        }

        /// <summary>
        /// The local scale of every crossing: the distance to its k-th nearest neighbour, then the median
        /// of that distance over its nearest neighbours. Crossings belonging to the same streamline are
        /// not neighbours of one another: the quantity being measured is the spacing between streamlines.
        /// </summary>
        private void ComputeLocalScales(float[] u, float[] v, int[] streamlineOf, int start, int count,
                                        double minU, double minV, double cell, int nx, int ny,
                                        int[] cellStart, int[] cellItem)
        {
            int k = neighbourCount_;
            double[] nearest = nearest_;
            int[] nearestOf = nearestOf_;
            double[] core = core_;
            int[] neighbours = neighbour_;
            int[] neighbourCountOf = neighbourCountOf_;
            double guard = 0.5 * cell;
            int ringLimit = System.Math.Max(nx, ny);

            for (int i = 0; i < count; i++)
            {
                double pu = u[start + i];
                double pv = v[start + i];
                int own = streamlineOf[start + i];
                int cxi = CellIndex(pu, minU, cell, nx);
                int cyi = CellIndex(pv, minV, cell, ny);
                int found = 0;
                for (int r = 0; r <= ringLimit; r++)
                {
                    if (r > 0 && found >= k && (r - 1) * cell >= nearest[k - 1])
                    {
                        // everything beyond this ring is farther than the k-th neighbour already found
                        break;
                    }
                    int yFrom = System.Math.Max(0, cyi - r);
                    int yTo = System.Math.Min(ny - 1, cyi + r);
                    int xFrom = System.Math.Max(0, cxi - r);
                    int xTo = System.Math.Min(nx - 1, cxi + r);
                    for (int cy = yFrom; cy <= yTo; cy++)
                    {
                        bool edgeRow = cy == cyi - r || cy == cyi + r;
                        int rowBase = cy * nx;
                        for (int cx = xFrom; cx <= xTo; cx++)
                        {
                            if (r > 0 && !edgeRow && cx != cxi - r && cx != cxi + r)
                            {
                                continue;
                            }
                            int c = rowBase + cx;
                            int to = cellStart[c + 1];
                            for (int s = cellStart[c]; s < to; s++)
                            {
                                int j = cellItem[s];
                                if (j == i || streamlineOf[start + j] == own)
                                {
                                    continue;
                                }
                                double du = u[start + j] - pu;
                                double dv = v[start + j] - pv;
                                double d = System.Math.Sqrt(du * du + dv * dv);
                                if (found < k)
                                {
                                    int pos = found;
                                    while (pos > 0 && nearest[pos - 1] > d)
                                    {
                                        nearest[pos] = nearest[pos - 1];
                                        nearestOf[pos] = nearestOf[pos - 1];
                                        pos--;
                                    }
                                    nearest[pos] = d;
                                    nearestOf[pos] = j;
                                    found++;
                                }
                                else if (d < nearest[k - 1])
                                {
                                    int pos = k - 1;
                                    while (pos > 0 && nearest[pos - 1] > d)
                                    {
                                        nearest[pos] = nearest[pos - 1];
                                        nearestOf[pos] = nearestOf[pos - 1];
                                        pos--;
                                    }
                                    nearest[pos] = d;
                                    nearestOf[pos] = j;
                                }
                            }
                        }
                    }
                }
                // when the cross-section holds fewer crossings than the requested rank, the farthest one
                // found is the only measure of the spacing available
                double value = found > 0 ? nearest[found - 1] : 0.0;
                core[i] = value > guard ? value : guard;
                neighbourCountOf[i] = found;
                for (int n = 0; n < found; n++)
                {
                    neighbours[i * k + n] = nearestOf[n];
                }
            }

            double[] ranked = ranked_;
            double[] scale = scale_;
            for (int i = 0; i < count; i++)
            {
                int found = neighbourCountOf[i];
                if (found == 0)
                {
                    scale[i] = core[i];
                    continue;
                }
                for (int n = 0; n < found; n++)
                {
                    double value = core[neighbours[i * k + n]];
                    int pos = n;
                    while (pos > 0 && ranked[pos - 1] > value)
                    {
                        ranked[pos] = ranked[pos - 1];
                        pos--;
                    }
                    ranked[pos] = value;
                }
                scale[i] = ranked[found / 2];
            }
        }

        private static int CellIndex(double position, double origin, double cell, int extent)
        {
            int index = (int)((position - origin) / cell);
            if (index < 0)
            {
                return 0;
            }
            return index >= extent ? extent - 1 : index;
        }

        private void EnsureCapacity(int count, int cellCount)
        {
            if (cellStart_.Length < cellCount + 1)
            {
                cellStart_ = new int[cellCount + 1];
                cellFill_ = new int[cellCount + 1];
            }
            if (cellItem_.Length < count)
            {
                cellItem_ = new int[count];
                cellOf_ = new int[count];
                core_ = new double[count];
                scale_ = new double[count];
                neighbourCountOf_ = new int[count];
                neighbour_ = new int[count * neighbourCount_];
            }
            if (unionFind_ == null || unionFind_.Capacity < count)
            {
                unionFind_ = new UnionFind(count);
            }
        }
    }
}
