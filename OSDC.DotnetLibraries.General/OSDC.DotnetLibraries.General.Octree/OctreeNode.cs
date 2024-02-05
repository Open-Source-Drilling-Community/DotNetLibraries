using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Octree
{
    public class OctreeNode<c> where c : IOctreeCode, new()
    {
        /// <summary>
        /// An OctreeNode can be parent for 8 OctreeNode children (Nodes). To mark an OctreeNode as a leaf, we set all Nodes[0-7] elements to null. 
        /// If Nodes == null we have reached an empty node in the Octree, but this node is not a leaf
        /// </summary>
        public OctreeNode<c>[]? Nodes { get; set; } = null;
        /// <summary>
        /// Check if the current OctreeNode has Nodes. Note that HasNodes will be true also for a leaf
        /// </summary>
        public bool HasNodes { get { return Nodes != null; } }
        /// <summary>
        /// A leaf has all Nodes[0-7] = null, but it should be enough to check the first
        /// </summary>
        public bool Leaf { get { return Nodes != null && Nodes.Length > 0 && Nodes[0] == null; } }

        /// <summary>
        /// Create  the 8 Nodes
        /// </summary>
        private void CreateNodes()
        {
            if (Nodes == null)
            {
                Nodes = new OctreeNode<c>[8];
            }
            for (int i = 0; i < Nodes.Length; i++)
            {
                Nodes[i] = new OctreeNode<c>();
            }
        }

        #region Add methods
        /// <summary>
        /// Add the node pointed to by the code. This includes creating new nodes, setting the added node to leaf and merging nodes if necessary
        /// </summary>
        /// <param name="code"></param>
        public void Add(List<byte> code)
        {
            if (code == null || code.Count == 0 || code[0] < 0 || code[0] > 7)
            {
                // If code.Count == 0, or indexes are messed up, we cannot proceed
                return;
            }

            if (Nodes == null)
            {
                CreateNodes();
            }
            if (Nodes == null || Nodes.Length < 8)
            {
                return;
            }
            if (Leaf)
            {
                // Prepare for adding sub-nodes to a leaf
                //CreateNodes();
                // Avoid splitting a leaf since it may already have been merged from sub-leaves
                return;
            }
            if (code.Count > 1)
            {
                Nodes[code[0]].Add(code.GetRange(1, code.Count - 1));
                // If the sub-node has been set to leaf and all other sub-nodes are already leafs, we should manage leaves also at this level
                if (FilledWithLeaves())
                {
                    // Delete all nodes and set current OctreeNode to leaf
                    Nodes = null;
                    SetToLeaf();
                }
            }
            else
            {
                if (!Nodes[code[0]].Leaf)
                {
                    // Set to leaf 
                    Nodes[code[0]].SetToLeaf();
                    if (FilledWithLeaves())
                    {
                        // Delete all nodes and set current OctreeNode to leaf
                        Nodes = null;
                        SetToLeaf();
                    }
                }
                return;
            }
        }
        #endregion

        /// <summary>
        /// Returns the bounds of the lowest OctreeNode that exists along the "path" determined by the list of indexes. It does not distinguish between a leaf and an empty node
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Bounds? GetActualBounds(Bounds? bounds, List<byte>? list)
        {
            if (bounds == null || !bounds.IsDefined() || list == null || list.Count == 0 || list[0] < 0 || list[0] > 7 || Nodes == null || Nodes.Length < 8)
            {
                // If list.Count == 0, or indexes are messed up, we cannot proceed
                return null;
            }

            Bounds? newBounds = bounds?.CalculateBounds(list[0]);
            if (newBounds == null) return null;
            if (Nodes[list[0]].Leaf || !Nodes[list[0]].HasNodes || list.Count == 1)
            {
                return newBounds;
            }
            else
            {
                return Nodes[list[0]].GetActualBounds(newBounds, list.GetRange(1, list.Count - 1));
            }
        }

        /// <summary>
        /// Checks if the current node contains leaves on every OctreeNode in Nodes
        /// </summary>
        /// <returns></returns>
        private bool FilledWithLeaves()
        {
            if (Nodes == null)
                return false;

            foreach (OctreeNode<c> node in Nodes)
            {
                if (!node.Leaf)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Set the current node to leaf. A leaf has all Nodes[0-7] = null
        /// </summary>
        private void SetToLeaf()
        {
            Nodes = new OctreeNode<c>[8];
        }

        /// <summary>
        /// Check if the list of indexes points to a leaf or not
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool IsLeaf(List<byte> code)
        {
            if (code == null || code.Count == 0 || code[0] < 0 || code[0] > 7)
            {
                // If code.Count == 0, or indexes are messed up, we cannot proceed
                return false;
            }

            if (Nodes == null)
                return false;

            if (code.Count > 1)
            {
                if (Nodes[code[0]].Leaf)
                {
                    // In this case we hit a leaf before the end of the code list
                    return false;
                }
                return Nodes[code[0]].IsLeaf(code.GetRange(1, code.Count - 1));
            }
            else
            {
                return Nodes[code[0]].Leaf;
            }
        }

        /// <summary>
        /// Get all leaves down to a given depth
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public List<c> GetLeaves(int depth)
        {
            List<c> results = new List<c>();
            List<byte> list = new List<byte>();
            GetLeaves(depth, ref list, ref results);
            return results;
        }

        /// <summary>
        /// Get all leaves at a given depth
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="list"></param>
        /// <param name="results"></param>
        private void GetLeaves(int depth, ref List<byte> list, ref List<c> results)
        {
            if (Nodes != null && list.Count < depth)
            {
                for (int i = 0; i < 8; i++)
                {
                    list.Add((byte)i);
                    if (Nodes[i].Leaf)
                    {
                        c oc = new c();
                        oc.TryParse(list);
                        results.Add(oc);
                    }
                    else
                    {
                        Nodes[i].GetLeaves(depth, ref list, ref results);
                    }
                    list.RemoveAt(list.Count - 1);
                }
            }
        }

        /// <summary>
        /// Returns true if the Octree contains the node pointed to by the code. Note that this method does not check if the node is a leaf or not
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool Contains(List<byte>? code)
        {
            if (code == null || code.Count == 0 || code[0] < 0 || code[0] > 7)
            {
                // If code.Count == 0, or indexes are messed up, we cannot proceed
                return false;
            }
            if (Nodes == null)
            {
                // The list still contains indexes, but there are no Nodes to access
                return false;
            }

            if (code.Count > 1)
            {
                if (Nodes[code[0]].Leaf)
                {
                    // In this case we hit a leaf before the end of the code list
                    return false;
                }
                return Nodes[code[0]].Contains(code.GetRange(1, code.Count - 1));
            }
            else
            {
                return Nodes[code[0]] != null;
            }
        }

        /// <summary>
        /// Delete the sub-nodes of the node pointed to by code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool DeleteSubNodes(List<byte> code)
        {
            if (code == null || code.Count == 0 || code[0] < 0 || code[0] > 7)
            {
                // If code.Count == 0, or indexes are messed up, we cannot proceed
                return false;
            }
            if (Nodes == null)
            {
                // The list still contains indexes, but there are no Nodes to access
                return false;
            }

            if (code.Count > 1)
            {
                if (Nodes[code[0]].Leaf)
                {
                    // In this case we hit a leaf before the end of the code list
                    return false;
                }
                return Nodes[code[0]].DeleteSubNodes(code.GetRange(1, code.Count - 1));
            }
            else
            {
                if (Nodes[code[0]] != null)
                {
                    Nodes[code[0]].Nodes = null;
                }
                // We return true also if the sub-nodes have already been deleted
                return true;
            }
        }
    }
}
