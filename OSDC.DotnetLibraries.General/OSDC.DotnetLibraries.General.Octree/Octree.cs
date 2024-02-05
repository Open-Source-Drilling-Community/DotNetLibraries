using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Octree
{
    public class Octree<c> where c : IOctreeCode, new()
    {
        /// <summary>
        /// This is the MaxDepth for the OctreeCodeLong
        /// </summary>
        public const int MaxDepthOctreeCodeLong = 31;
        /// <summary>
        /// This is the MaxDepth for the OctreeCode
        /// </summary>
        public const int MaxDepthOctreeCode = 19;
        /// <summary>
        /// This is the MaxDepth for the Octree - Should find a way to select this based on the type of c (OctreeCode or OctreeCodeLong)
        /// </summary>
        public const int MaxDepth = MaxDepthOctreeCodeLong;

        /// <summary>
        /// This is the Root node of the Octree
        /// </summary>
        protected OctreeNode<c>? Root { get; } = null;

        /// <summary>
        /// This is the Root bounds
        /// </summary>
        public Bounds? GlobalBounds { get; } = null;

        #region Constructors
        /// <summary>
        /// Octree from Bounds
        /// </summary>
        /// <param name="globalBounds"></param>
        public Octree(Bounds globalBounds)
        {
            GlobalBounds = globalBounds;
            Root = new OctreeNode<c>();

            //#region MaxDepth
            //c oc = new c();
            //if (oc.GetType()  == typeof(OctreeCode))
            //{
            //    MaxDepth = MaxDepthOctreeCode;
            //}
            //#endregion
        }

        /// <summary>
        /// Octree from Min/Max
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        /// <param name="minZ"></param>
        /// <param name="maxZ"></param>
        public Octree(double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
        {
            GlobalBounds = new Bounds(minX, maxX, minY, maxY, minZ, maxZ);
            Root = new OctreeNode<c>();

            //#region MaxDepth
            //c oc = new c();
            //if (oc.GetType() == typeof(OctreeCode))
            //{
            //    MaxDepth = MaxDepthOctreeCode;
            //}
            //#endregion
        }
        #endregion

        #region Get methods
        /// <summary>
        /// This method returns a list of octree codes - Leaves - to reconstruct the volume
        /// </summary>
        /// <returns></returns>
        public List<c>? GetLeaves(int depth = MaxDepth)
        {
            if (Root != null)
            {
                return Root.GetLeaves(ValidateDepth(depth));
            }
            return null;
        }
        #endregion

        #region Get bounds methods
        /// <summary>
        /// This method returns the bounds.Middle point of the node the code points to, which corresponds to the center of the node.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Point3D? GetCenter(c code)
        {
            return GetBounds(code)?.Center;
        }

        /// <summary>
        /// This method returns the bounds.Middle point of the node the list points to, which corresponds to the center of the node.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public Point3D? GetCenter(List<byte> list)
        {
            return GetBounds(list)?.Center;
        }

        /// <summary>
        /// This method returns the bounds of the node the code points to as a couple of points
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Tuple<Point3D, Point3D>? GetBoundsAsCouple(c code)
        {
            return GetBounds(code)?.ConvertToCoupleOfPoints3D();
        }

        /// <summary>
        /// This method returns the bounds of the node the list points to
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public Bounds? GetBounds(List<byte> list)
        {
            if (GlobalBounds == null)
                return null;

            if (list == null || list.Count == 0)
            {
                return null;
            }
            else
            {
                return GetBounds(list, GlobalBounds);
            }
        }

        /// <summary>
        /// This method returns the bounds of the node the code points to
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Bounds? GetBounds(c code)
        {
            if (GlobalBounds == null)
                return null;

            List<byte> list = code.DecodeToListOfByte();
            if (list == null || list.Count == 0)
            {
                return null;
            }
            else
            {
                return GetBounds(list, GlobalBounds);
            }
        }

        /// <summary>
        /// This method returns the bounds of the node the list points to by keeping track of the bounds of the current node
        /// </summary>
        /// <param name="list"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        private Bounds? GetBounds(List<byte> list, Bounds? bounds)
        {
            if (bounds == null || list == null || list.Count == 0) return null;

            Bounds? newBounds = new Bounds(bounds);
            byte index = list[0];
            if (index < 8 && index >= 0)
            {
                newBounds = bounds.CalculateBounds(index);
                if (list.Count > 1)
                {
                    return GetBounds(list.GetRange(1, list.Count - 1), newBounds);
                }
                else
                {
                    return newBounds;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region GetCode methods
        /// <summary>
        /// This method returns the OctreeCode for a point at a given depth - independent of OctreeNodes
        /// </summary>
        /// <param name="point"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public c GetOctreeCode(Point3D p, int depth = MaxDepth)
        {
            c oc = new();
            List<byte>? list = GetCode(p, ValidateDepth(depth));
            if (list != null) {
                string code = string.Empty;
                foreach (int index in list)
                {
                    code += index.ToString();
                }
                oc.TryParse(code);
            }
            return oc;
        }

        /// <summary>
        /// This method returns a list of byte indexes pointing to the node which contains the point at a given depth - independent of OctreeNodes
        /// </summary>
        /// <param name="p"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public List<byte>? GetCode(Point3D p, int depth = MaxDepth)
        {
            List<byte> list = new List<byte>();
            GetCode(p, GlobalBounds, ref list, ValidateDepth(depth));
            return list;
        }

        /// <summary>
        /// This method returns the bounds of the node containing the point
        /// </summary>
        /// <param name="p"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public Bounds? GetBoundsFromOctree(Point3D p, int depth = MaxDepth)
        {
            List<byte>? list = GetCode(p, ValidateDepth(depth));
            if (Root != null)
            {
                return Root.GetActualBounds(GlobalBounds, list);
            }
            return null;
        }

        /// <summary>
        /// This method returns the Bounds together with the list pointing to the node containing the provided point at a given depth
        /// </summary>
        /// <param name="p"></param>
        /// <param name="bounds"></param>
        /// <param name="list"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private Bounds? GetCode(Point3D p, Bounds? bounds, ref List<byte> list, int depth = MaxDepth)
        {
            if (bounds == null || !bounds.IsDefined() || p.IsUndefined())
                return null;

            for (int i = 0; i < 8; i++)
            {
                var newBounds = bounds?.CalculateBounds(i);
                if (newBounds != null && newBounds.Contains(p))
                {
                    list.Add(Convert.ToByte(i));
                    if (list.Count == ValidateDepth(depth))
                    {
                        return newBounds;
                    }
                    else
                    {
                        return GetCode(p, newBounds, ref list, ValidateDepth(depth));
                    }
                }
            }
            return null;
        }
        #endregion

        #region Add
        /// <summary>
        /// This method adds the node pointed to by the octreeCode
        /// </summary>
        /// <param name="octreeCode"></param>
        public void Add(c octreeCode)
        {
            if (Root != null)
            {
                Root.Add(octreeCode.DecodeToListOfByte());
            }
        }

        /// <summary>
        /// This method adds the node pointed to by the list of byte's
        /// </summary>
        /// <param name="code"></param>
        public void Add(List<byte> code)
        {
            if (Root != null)
            {
                Root.Add(code);
            }
        }

        /// <summary>
        /// This method adds the node pointed to by the codeString
        /// </summary>
        /// <param name="codeString"></param>
        public void Add(string codeString)
        {
            c code = new c();
            if (code.TryParse(codeString) && Root != null)
            {
                Add(code);
            }
        }

        /// <summary>
        /// This method adds the node containing the point at the specified depth
        /// </summary>
        public void Add(Point3D point, int depth = MaxDepth)
        {
            if (Root != null)
            {
                var list = GetCode(point, ValidateDepth(depth));
                if (list != null)
                {
                    Root.Add(list);
                }
            }
        }

        /// <summary>
        /// This method adds the node containing the coordinates at the specified depth
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="depth"></param>
        public void Add(double x, double y, double z, int depth = MaxDepth)
        {
            Add(new Point3D(x, y, z), ValidateDepth(depth));
        }
        #endregion

        #region Delete
        /// <summary>
        /// This method will delete the nodes of Root
        /// </summary>
        public void DeleteRootNodes()
        {
            if (Root != null)
            {
                Root.Nodes = null;
            }
        }

        /// <summary>
        /// This method will delete the sub-nodes of the node that the code points to
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool DeleteSubNodes(c code)
        {
            if (Root != null)
            {
                return Root.DeleteSubNodes(code.DecodeToListOfByte());
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This method will delete the node that the code points to together with its siblings
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool DeleteNode(c code)
        {
            List<byte> list = code.DecodeToListOfByte();
            if (list != null && Root != null)
            {
                if (list.Count > 1)
                {
                    // Delete the sub-nodes of the parent node
                    return Root.DeleteSubNodes(code.DecodeToListOfByte().GetRange(0, list.Count - 1));
                }
                else
                {
                    if (list[0] >= 0 && list[0] < 8)
                    {
                        DeleteRootNodes();
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region IsLeaf / Contains
        /// <summary>
        /// This method will determine if the code points to a leaf
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool IsLeaf(c code)
        {
            if (Root != null)
            {
                return Root.IsLeaf(code.DecodeToListOfByte());
            }
            return false;
        }

        /// <summary>
        /// This method will determine if the code points to a leaf
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool IsLeaf(List<byte> code)
        {
            if (Root != null)
            {
                return Root.IsLeaf(code);
            }
            return false;
        }

        /// <summary>
        /// This method will determine if the Octree contains the node that the code points to
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool Contains(c code)
        {
            if (Root != null)
            {
                return Root.Contains(code.DecodeToListOfByte());
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This method will determine if the Octree contains a node at the given depth which contains the point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public bool Contains(Point3D point, int depth = MaxDepth)
        {
            if (Root != null)
            {
                return Root.Contains(GetCode(point, ValidateDepth(depth)));
            }
            else
            {
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Returns a value between 0 and MaxDepth
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        private int ValidateDepth(int depth)
        {
            return System.Math.Max(System.Math.Min(depth, MaxDepth), 1);
        }
    }
}
