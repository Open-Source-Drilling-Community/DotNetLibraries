using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Octree
{
    /// <summary>
    /// This class is used to represent the bounds of an OctreeNode
    /// </summary>
    public class Bounds
    {
        public double? MinX { get; set; } = null;
        public double? MaxX { get; set; } = null;
        public double? MinY { get; set; } = null;
        public double? MaxY { get; set; } = null;
        public double? MinZ { get; set; } = null;
        public double? MaxZ { get; set; } = null;
        public double? MiddleX { get { return (MaxX + MinX) / 2; } }
        public double? MiddleY { get { return (MaxY + MinY) / 2; } }
        public double? MiddleZ { get { return (MaxZ + MinZ) / 2; } }
        public double? IntervalX { get { return MaxX - MinX; } }
        public double? IntervalY { get { return MaxY - MinY; } }
        public double? IntervalZ { get { return MaxZ - MinZ; } }
        /// <summary>
        /// default constructor
        /// </summary>
        public Bounds()
        {

        }
        /// <summary>
        /// Initialization Constructor
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        /// <param name="minZ"></param>
        /// <param name="maxZ"></param>
        public Bounds(double? minX, double? maxX, double? minY, double? maxY, double? minZ, double? maxZ)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            MinZ = minZ;
            MaxZ = maxZ;
        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="bounds"></param>
        public Bounds(Bounds bounds)
        {
            if (bounds != null)
            {
                bounds.Copy(this);
            }
        }

        /// <summary>
        /// copy function
        /// </summary>
        /// <param name="dest"></param>
        /// <returns></returns>
        public bool Copy(Bounds dest)
        {
            if (dest != null)
            {
                dest.MinX = MinX;
                dest.MinY = MinY;
                dest.MinZ = MinZ;
                dest.MaxX = MaxX;
                dest.MaxY = MaxY;
                dest.MaxZ = MaxZ;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the bounds of the node at a given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Bounds? CalculateBounds(int index)
        {
            switch (index)
            {
                case 0:
                    return new Bounds(MinX, MiddleX, MinY, MiddleY, MinZ, MiddleZ);
                case 1:
                    return new Bounds(MiddleX, MaxX, MinY, MiddleY, MinZ, MiddleZ);
                case 2:
                    return new Bounds(MinX, MiddleX, MiddleY, MaxY, MinZ, MiddleZ);
                case 3:
                    return new Bounds(MiddleX, MaxX, MiddleY, MaxY, MinZ, MiddleZ);
                case 4:
                    return new Bounds(MinX, MiddleX, MinY, MiddleY, MiddleZ, MaxZ);
                case 5:
                    return new Bounds(MiddleX, MaxX, MinY, MiddleY, MiddleZ, MaxZ);
                case 6:
                    return new Bounds(MinX, MiddleX, MiddleY, MaxY, MiddleZ, MaxZ);
                case 7:
                    return new Bounds(MiddleX, MaxX, MiddleY, MaxY, MiddleZ, MaxZ);
                default:
                    return null;

            }
        }

        /// <summary>
        /// Convert the bounds to a couple of Point3D
        /// </summary>
        /// <returns></returns>
        public Tuple<Point3D, Point3D> ConvertToCoupleOfPoints3D()
        {
            return new Tuple<Point3D, Point3D>(new Point3D(MinX, MinY, MinZ), new Point3D(MaxX, MaxY, MaxZ));
        }

        /// <summary>
        /// Convert the bounds to an array of 8 points defining the corners
        /// </summary>
        /// <returns></returns>
        public Point3D[] CornerPoints3D()
        {
            Point3D[] results = new Point3D[8];
            results[0] = new Point3D(MinX, MinY, MinZ);
            results[1] = new Point3D(MinX, MaxY, MinZ);
            results[2] = new Point3D(MaxX, MinY, MinZ);
            results[3] = new Point3D(MaxX, MaxY, MinZ);
            results[4] = new Point3D(MinX, MinY, MaxZ);
            results[5] = new Point3D(MinX, MaxY, MaxZ);
            results[6] = new Point3D(MaxX, MinY, MaxZ);
            results[7] = new Point3D(MaxX, MaxY, MaxZ);
            return results;
        }

        /// <summary>
        /// Returns the Point3D at the center of the bounds
        /// </summary>
        public Point3D Center
        {
            get { return new Point3D(MiddleX, MiddleY, MiddleZ); }
        }

        /// <summary>
        /// Checks if all the min / max values are defined
        /// </summary>
        /// <returns></returns>
        public bool IsDefined()
        {
            return Numeric.IsDefined(MinX) && Numeric.IsDefined(MaxX) && Numeric.IsDefined(MinY) && Numeric.IsDefined(MaxY) && Numeric.IsDefined(MinZ) && Numeric.IsDefined(MaxZ);
        }

        /// <summary>
        /// Checks if the point is contained in the bounds
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Contains(Point3D p)
        {
            return Contains(p.X, p.Y, p.Z);
        }

        /// <summary>
        /// Checks if the coordinates are contained in the bounds
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool Contains(double? x, double? y, double? z)
        {
            return IncludedInInterval(x, MinX, MaxX) && IncludedInInterval(y, MinY, MaxY) && IncludedInInterval(z, MinZ, MaxZ);
        }

        /// <summary>
        /// Checks if the otherBounds intersects the bounds
        /// </summary>
        /// <param name="otherBounds"></param>
        /// <returns></returns>
        public bool Intersects(Bounds otherBounds)
        {
            if (otherBounds != null &&
                  (Contains(otherBounds.MinX, otherBounds.MinY, otherBounds.MinZ)
                || Contains(otherBounds.MaxX, otherBounds.MinY, otherBounds.MinZ)
                || Contains(otherBounds.MinX, otherBounds.MaxY, otherBounds.MinZ)
                || Contains(otherBounds.MaxX, otherBounds.MaxY, otherBounds.MinZ)
                || Contains(otherBounds.MinX, otherBounds.MinY, otherBounds.MaxZ)
                || Contains(otherBounds.MaxX, otherBounds.MinY, otherBounds.MaxZ)
                || Contains(otherBounds.MinX, otherBounds.MaxY, otherBounds.MaxZ)
                || Contains(otherBounds.MaxX, otherBounds.MaxY, otherBounds.MaxZ)
                || otherBounds.Contains(MinX, MinY, MinZ)
                || otherBounds.Contains(MaxX, MinY, MinZ)
                || otherBounds.Contains(MinX, MaxY, MinZ)
                || otherBounds.Contains(MaxX, MaxY, MinZ)
                || otherBounds.Contains(MinX, MinY, MaxZ)
                || otherBounds.Contains(MaxX, MinY, MaxZ)
                || otherBounds.Contains(MinX, MaxY, MaxZ)
                || otherBounds.Contains(MaxX, MaxY, MaxZ)))
            {
                // One of the corner points of otherBounds are contained in this bound or vice verca
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a value is included in the closed interval [min, max]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool IncludedInInterval(double? value, double? min, double? max)
        {
            if (Numeric.IsUndefined(value) || Numeric.IsUndefined(min) || Numeric.IsUndefined(max) || min > max)
            {
                return false;
            }
            else
            {
                return value >= min && value <= max;
            }
        }
    }
}
