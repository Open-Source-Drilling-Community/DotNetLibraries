namespace OSDC.DotnetLibraries.General.Math
{
    public class BoundingBox3D
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MinZ { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double MaxZ { get; set; }

        public BoundingBox3D(double minX, double minY, double minZ,
                             double maxX, double maxY, double maxZ)
        {
            MinX = minX;
            MinY = minY;
            MinZ = minZ;
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;
        }

        public double LengthX => MaxX - MinX;
        public double LengthY => MaxY - MinY;
        public double LengthZ => MaxZ - MinZ;

        public bool Contains(double x, double y, double z) =>
            x >= MinX && x <= MaxX &&
            y >= MinY && y <= MaxY &&
            z >= MinZ && z <= MaxZ;
    }
}
