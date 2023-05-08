using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.OpenFoamRead
{
    public class Face
    {
        public int[] Vertices { get; set; }
        /// <summary>
        /// default constructor
        /// </summary>
        public Face()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Face(Face src)
        {
            if (src != null)
            {
                if (src.Vertices != null)
                {
                    Vertices = new int[src.Vertices.Length];
                    src.Vertices.CopyTo(Vertices, 0);
                }
            }
        }
    }
}
