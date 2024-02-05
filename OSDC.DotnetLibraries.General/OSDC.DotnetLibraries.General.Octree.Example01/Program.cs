using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Octree;

class Example
{
    static void Main()
    {
        Bounds boundsA = new Bounds(-1, 1, -1, 1, -1, 1);
        
        Point3D pointA = new Point3D(0,0,0);
        Console.WriteLine("Is the point: (" + pointA.X + ", " + pointA.Y + ", " + pointA.Z +
            ") contained in the bounds: (" + 
            boundsA.MinX + ", " + boundsA.MaxX + "), (" + boundsA.MinY + ", " + boundsA.MaxY + "), (" + boundsA.MinZ + ", " + boundsA.MaxZ + ") ? " +
            boundsA.Contains(pointA));
        Point3D pointB = new Point3D(2, 0, 0);
        Console.WriteLine("Is the point: (" + pointB.X + ", " + pointB.Y + ", " + pointB.Z +
            ") contained in the bounds: (" +
            boundsA.MinX + ", " + boundsA.MaxX + "), (" + boundsA.MinY + ", " + boundsA.MaxY + "), (" + boundsA.MinZ + ", " + boundsA.MaxZ + ") ? " +
            boundsA.Contains(pointB));
        Bounds boundsB = new Bounds(-2, 0, -1, 0, -0.5, 0);
        Console.WriteLine("Does the bounds (" + boundsB.MinX + ", " + boundsB.MaxX + "), (" + boundsB.MinY + ", " + boundsB.MaxY + "), (" + boundsB.MinZ + ", " + boundsB.MaxZ + 
            ") intersects the bounds (" +
            boundsA.MinX + ", " + boundsA.MaxX + "), (" + boundsA.MinY + ", " + boundsA.MaxY + "), (" + boundsA.MinZ + ", " + boundsA.MaxZ + ") ? " +
            boundsB.Intersects(boundsA));
        Bounds boundsC = new Bounds(-3, -2, -3, -2, -3, -2);
        Console.WriteLine("Does the bounds (" + boundsC.MinX + ", " + boundsC.MaxX + "), (" + boundsC.MinY + ", " + boundsC.MaxY + "), (" + boundsC.MinZ + ", " + boundsC.MaxZ +
            ") intersects the bounds (" +
            boundsA.MinX + ", " + boundsA.MaxX + "), (" + boundsA.MinY + ", " + boundsA.MaxY + "), (" + boundsA.MinZ + ", " + boundsA.MaxZ + ") ? " +
            boundsC.Intersects(boundsA));
    }
}
