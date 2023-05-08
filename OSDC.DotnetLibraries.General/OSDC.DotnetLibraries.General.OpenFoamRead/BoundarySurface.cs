using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.OpenFoamRead
{
    public class BoundarySurface
    {
        public string Name { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
        /// <summary>
        /// default constructor
        /// </summary>
        public BoundarySurface() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public BoundarySurface(BoundarySurface src)
        {
            if (src != null)
            {
                Name = src.Name;
                StartIndex = src.StartIndex;
                Count = src.Count;
            }
        }
    }
}
