namespace OSDC.DotnetLibraries.General.Math
{
    public class BoundingBox2D
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }

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
