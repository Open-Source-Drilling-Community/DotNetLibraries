namespace OSDC.DotnetLibraries.General.Math
{
    public class BoundingBox2D
    {
        public double MinX { get; }
        public double MinY { get; }
        public double MaxX { get; }
        public double MaxY { get; }

        public BoundingBox2D(double minX, double minY, double maxX, double maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public double LengthX => MaxX - MinX;
        public double LengthY => MaxY - MinY;

        public bool Contains(double x, double y) =>
            x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
    }
}
