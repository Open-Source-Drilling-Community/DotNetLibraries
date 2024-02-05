using OSDC.DotnetLibraries.General.Octree;
using OSDC.DotnetLibraries.General.Math;

class Example
{
    static void Main()
    {
        Bounds bounds = new Bounds(-1, 1, -1, 1, -1, 1);
        Octree<OctreeCodeLong> octree = new Octree<OctreeCodeLong>(bounds);

        Random rnd = new Random();
        for (int i = 0; i < 10; i++)
        {
            Point3D pt = new Point3D(2*rnd.NextDouble()-1, 2 * rnd.NextDouble() - 1, 2 * rnd.NextDouble() - 1);
            Console.WriteLine("Point : (" +  pt.X + ", " + pt.Y + ", " + pt.Z + ")");
            octree.Add(pt);
        }

        List<OctreeCodeLong>? vol = octree.GetLeaves();
        if (vol != null)
        {
            Console.WriteLine("The current leaves of the volumne");
            foreach (var code in vol)
            {
                Bounds? b = octree.GetBounds(code);
                if (b != null)
                {
                    Console.WriteLine("Bound min: (" + b.MinX + ", " + b.MinY + ", " + b.MinZ + ") + max: (" + b.MaxX + ", " + b.MaxY + ", " + b.MaxZ + ")");
                }
            }
        }
    }
}