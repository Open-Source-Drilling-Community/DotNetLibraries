using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    [TestFixture]
    public class OctreeTests
    {
        // ---- the key packing -----------------------------------------------------------------------

        [Test]
        public void CoordinatesSurviveTheRoundTrip()
        {
            Random random = new Random(1234);
            for (int attempt = 0; attempt < 20000; attempt++)
            {
                int depth = random.Next(0, OctreeKey.MaximumDepth + 1);
                int extent = 1 << depth;
                int i = random.Next(extent), j = random.Next(extent), k = random.Next(extent);
                OctreeKey key = OctreeKey.Create(i, j, k, depth);
                key.GetCoordinates(out int bi, out int bj, out int bk);
                Assert.That((bi, bj, bk, key.Depth), Is.EqualTo((i, j, k, depth)));
            }
        }

        [Test]
        public void TheDeepestAddressableCellIsSupported()
        {
            int depth = OctreeKey.MaximumDepth;
            int last = (1 << depth) - 1;
            OctreeKey key = OctreeKey.Create(last, last, last, depth);
            key.GetCoordinates(out int i, out int j, out int k);
            Assert.That((i, j, k), Is.EqualTo((last, last, last)));
            Assert.That(key.Depth, Is.EqualTo(depth));
            Assert.That(key.CodeSpan, Is.EqualTo(1UL));
        }

        [Test]
        public void AChildIsInsideItsParentAndKnowsTheWayBack()
        {
            Random random = new Random(99);
            for (int attempt = 0; attempt < 5000; attempt++)
            {
                int depth = random.Next(0, OctreeKey.MaximumDepth);
                int extent = 1 << depth;
                OctreeKey parent = OctreeKey.Create(random.Next(extent), random.Next(extent),
                                                    random.Next(extent), depth);
                for (int child = 0; child < 8; child++)
                {
                    OctreeKey son = parent.GetChild(child);
                    Assert.That(son.Depth, Is.EqualTo(depth + 1));
                    Assert.That(parent.Contains(son), Is.True);
                    Assert.That(son.Parent, Is.EqualTo(parent));
                }
            }
        }

        [Test]
        public void TheEightChildrenExactlyFillTheParentCodeRange()
        {
            OctreeKey parent = OctreeKey.Create(3, 5, 2, 4);
            ulong span = parent.CodeSpan;
            List<ulong> codes = new List<ulong>();
            for (int child = 0; child < 8; child++)
            {
                codes.Add(parent.GetChild(child).NormalizedCode);
            }
            codes.Sort();
            Assert.That(codes[0], Is.EqualTo(parent.NormalizedCode));
            for (int c = 1; c < 8; c++)
            {
                Assert.That(codes[c] - codes[c - 1], Is.EqualTo(span / 8));
            }
            Assert.That(codes[7] + span / 8, Is.EqualTo(parent.NormalizedCode + span));
        }

        [Test]
        public void SortingPutsACellBeforeItsDescendants()
        {
            OctreeKey parent = OctreeKey.Create(1, 2, 3, 5);
            for (int child = 0; child < 8; child++)
            {
                Assert.That(parent.CompareTo(parent.GetChild(child)), Is.LessThan(0));
            }
        }

        [Test]
        public void NeighboursAreAdjacentAndStopAtTheEdge()
        {
            OctreeKey key = OctreeKey.Create(0, 4, 4, 3);
            Assert.That(key.GetNeighbour(0, -1), Is.Null, "there is nothing below index zero");
            OctreeKey? up = key.GetNeighbour(0, 1);
            Assert.That(up, Is.Not.Null);
            up!.Value.GetCoordinates(out int i, out int j, out int k);
            Assert.That((i, j, k), Is.EqualTo((1, 4, 4)));

            OctreeKey last = OctreeKey.Create(7, 7, 7, 3);
            Assert.That(last.GetNeighbour(0, 1), Is.Null);
            Assert.That(last.GetNeighbour(1, 1), Is.Null);
            Assert.That(last.GetNeighbour(2, 1), Is.Null);
        }

        // ---- the frame -----------------------------------------------------------------------------

        [Test]
        public void TheFrameCoversTheBoxWithExactBinaryCells()
        {
            double[] minimum = { -100.0, 20.0, 0.0 };
            double[] maximum = { 400.0, 120.0, 1300.0 };
            OctreeFrame frame = OctreeFrame.Covering(minimum, maximum, 0.5, out int depth);
            Assert.That(frame.GetCellSize(depth), Is.EqualTo(0.5).Within(1e-12));
            Assert.That(frame.RootSize, Is.GreaterThanOrEqualTo(1300.0));
            // the box must be inside the root cube
            Assert.That(frame.OriginNorth, Is.LessThanOrEqualTo(minimum[0]));
            Assert.That(frame.OriginEast, Is.LessThanOrEqualTo(minimum[1]));
            Assert.That(frame.OriginVertical, Is.LessThanOrEqualTo(minimum[2]));
            Assert.That(frame.OriginNorth + frame.RootSize, Is.GreaterThanOrEqualTo(maximum[0]));
            Assert.That(frame.OriginEast + frame.RootSize, Is.GreaterThanOrEqualTo(maximum[1]));
            Assert.That(frame.OriginVertical + frame.RootSize, Is.GreaterThanOrEqualTo(maximum[2]));
        }

        [Test]
        public void ACellHoldsThePositionsItWasFoundFrom()
        {
            OctreeFrame frame = new OctreeFrame(0, 0, 0, 1024.0);
            Random random = new Random(7);
            for (int attempt = 0; attempt < 5000; attempt++)
            {
                double n = random.NextDouble() * 1024.0;
                double e = random.NextDouble() * 1024.0;
                double v = random.NextDouble() * 1024.0;
                int depth = random.Next(0, 11);
                OctreeKey? key = frame.GetKey(n, e, v, depth);
                Assert.That(key, Is.Not.Null);
                Assert.That(frame.GetCell(key!.Value).Contains(n, e, v), Is.True);
            }
        }

        // ---- the tree ------------------------------------------------------------------------------

        private static LinearOctree BuildUniform(int baseDepth)
        {
            OctreeFrame frame = new OctreeFrame(0, 0, 0, 1024.0);
            double[] min = { 0, 0, 0 };
            double[] max = { 1024.0, 1024.0, 1024.0 };
            return LinearOctree.Build(frame, min, max, baseDepth, baseDepth, (in OctreeCell c) => false);
        }

        [Test]
        public void AUniformTreeHasTheExpectedNumberOfLeaves()
        {
            LinearOctree tree = BuildUniform(4);
            Assert.That(tree.LeafCount, Is.EqualTo(16 * 16 * 16));
        }

        [Test]
        public void EveryPositionLandsInExactlyOneLeaf()
        {
            OctreeFrame frame = new OctreeFrame(0, 0, 0, 1024.0);
            double[] min = { 0, 0, 0 };
            double[] max = { 1024.0, 1024.0, 1024.0 };
            // refine hard around one corner so the tree is genuinely graded
            LinearOctree tree = LinearOctree.Build(frame, min, max, 3, 7,
                (in OctreeCell c) => c.CentreNorth < 200 && c.CentreEast < 200 && c.CentreVertical < 200);

            Random random = new Random(21);
            for (int attempt = 0; attempt < 20000; attempt++)
            {
                double n = random.NextDouble() * 1024.0;
                double e = random.NextDouble() * 1024.0;
                double v = random.NextDouble() * 1024.0;
                int leaf = tree.FindLeaf(n, e, v);
                Assert.That(leaf, Is.GreaterThanOrEqualTo(0), $"({n:F2},{e:F2},{v:F2}) fell in no leaf");
                Assert.That(tree.GetCell(leaf).Contains(n, e, v), Is.True);
            }
        }

        [Test]
        public void TheLeavesTileTheDomainWithoutOverlapOrHole()
        {
            OctreeFrame frame = new OctreeFrame(0, 0, 0, 64.0);
            double[] min = { 0, 0, 0 };
            double[] max = { 64.0, 64.0, 64.0 };
            LinearOctree tree = LinearOctree.Build(frame, min, max, 2, 5,
                (in OctreeCell c) => c.CentreNorth + c.CentreEast + c.CentreVertical < 40.0);

            // the volumes must add up exactly, which they can only do if nothing overlaps and
            // nothing is missing
            double volume = 0;
            for (int leaf = 0; leaf < tree.LeafCount; leaf++)
            {
                double size = tree.GetCell(leaf).Size;
                volume += size * size * size;
            }
            Assert.That(volume, Is.EqualTo(64.0 * 64.0 * 64.0).Within(1e-6));
        }

        [Test]
        public void ABalancedTreeHasNoNeighbourMoreThanOneLevelApart()
        {
            OctreeFrame frame = new OctreeFrame(0, 0, 0, 256.0);
            double[] min = { 0, 0, 0 };
            double[] max = { 256.0, 256.0, 256.0 };
            LinearOctree tree = LinearOctree.Build(frame, min, max, 2, 7,
                (in OctreeCell c) =>
                {
                    double dn = c.CentreNorth - 30.0, de = c.CentreEast - 30.0, dv = c.CentreVertical - 30.0;
                    return System.Math.Sqrt(dn * dn + de * de + dv * dv) < 20.0;
                });

            List<int> neighbours = new List<int>();
            int worst = 0;
            for (int leaf = 0; leaf < tree.LeafCount; leaf++)
            {
                int depth = tree.GetKey(leaf).Depth;
                for (int axis = 0; axis < 3; axis++)
                {
                    for (int step = -1; step <= 1; step += 2)
                    {
                        neighbours.Clear();
                        tree.GetFaceNeighbours(leaf, axis, step, neighbours);
                        foreach (int other in neighbours)
                        {
                            int difference = System.Math.Abs(tree.GetKey(other).Depth - depth);
                            if (difference > worst)
                            {
                                worst = difference;
                            }
                        }
                    }
                }
            }
            Assert.That(worst, Is.LessThanOrEqualTo(1), $"found neighbours {worst} levels apart");
        }

        [Test]
        public void FaceNeighboursAreReciprocalAndActuallyTouch()
        {
            OctreeFrame frame = new OctreeFrame(0, 0, 0, 128.0);
            double[] min = { 0, 0, 0 };
            double[] max = { 128.0, 128.0, 128.0 };
            LinearOctree tree = LinearOctree.Build(frame, min, max, 2, 6,
                (in OctreeCell c) => c.CentreNorth < 40.0 && c.CentreEast < 40.0);

            List<int> neighbours = new List<int>();
            List<int> back = new List<int>();
            int pairs = 0;
            for (int leaf = 0; leaf < tree.LeafCount; leaf++)
            {
                OctreeCell cell = tree.GetCell(leaf);
                for (int axis = 0; axis < 3; axis++)
                {
                    for (int step = -1; step <= 1; step += 2)
                    {
                        neighbours.Clear();
                        tree.GetFaceNeighbours(leaf, axis, step, neighbours);
                        foreach (int other in neighbours)
                        {
                            pairs++;
                            OctreeCell far = tree.GetCell(other);
                            // the two cells must share the face, so they touch along the axis and
                            // overlap across it
                            double nearFace = step > 0 ? Low(cell, axis) + cell.Size : Low(cell, axis);
                            double farFace = step > 0 ? Low(far, axis) : Low(far, axis) + far.Size;
                            Assert.That(farFace, Is.EqualTo(nearFace).Within(1e-9),
                                        $"{cell} and {far} do not touch along axis {axis}");
                            for (int other2 = 0; other2 < 3; other2++)
                            {
                                if (other2 == axis) { continue; }
                                Assert.That(Low(cell, other2) < Low(far, other2) + far.Size + 1e-9, Is.True);
                                Assert.That(Low(far, other2) < Low(cell, other2) + cell.Size + 1e-9, Is.True);
                            }
                            // and the relation must be symmetric
                            back.Clear();
                            tree.GetFaceNeighbours(other, axis, -step, back);
                            Assert.That(back, Does.Contain(leaf),
                                        $"{far} does not name {cell} back across axis {axis}");
                        }
                    }
                }
            }
            Assert.That(pairs, Is.GreaterThan(1000), "the fixture did not exercise enough faces");
        }

        private static double Low(OctreeCell cell, int axis)
        {
            return axis == 0 ? cell.MinimumNorth : axis == 1 ? cell.MinimumEast : cell.MinimumVertical;
        }

        [Test]
        public void RefiningOnlyWhereAskedLeavesTheRestCoarse()
        {
            OctreeFrame frame = new OctreeFrame(0, 0, 0, 256.0);
            double[] min = { 0, 0, 0 };
            double[] max = { 256.0, 256.0, 256.0 };
            // overlapping the corner box, so that the coarsest cell holding it refines all the way down
            LinearOctree tree = LinearOctree.Build(frame, min, max, 2, 6,
                (in OctreeCell c) => c.MinimumNorth < 16.0 && c.MinimumEast < 16.0 && c.MinimumVertical < 16.0,
                balance: false);
            int[] histogram = tree.GetDepthHistogram();
            Assert.That(histogram[2], Is.GreaterThan(0), "nothing stayed at the base depth");
            Assert.That(histogram[6], Is.GreaterThan(0), "nothing reached the deepest depth");
            // a uniform tree at the deepest depth would be 64 cubed
            Assert.That(tree.LeafCount, Is.LessThan(64 * 64 * 64 / 100));
        }
    }
}
