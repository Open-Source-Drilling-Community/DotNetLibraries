using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true)]
    public class SemanticExclusiveOrAttribute : Attribute
    {
        /// <summary>
        /// an array of the exclusive options.
        /// </summary>
        public byte[]? ExclusiveOr { get; } = null;

        /// <summary>
        /// The constructor takes at least two arguments.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public SemanticExclusiveOrAttribute(byte arg1, byte arg2, params byte[]? arg3)
        {
            int c = 2;
            if (arg3 != null)
            {
                c += arg3.Length;
            }
            ExclusiveOr = new byte[c];
            ExclusiveOr[0] = arg1;
            ExclusiveOr[1] = arg2;
            if (arg3 != null)
            {
                for (int i = 0; i < arg3.Length; i++)
                {
                    ExclusiveOr[i+2] = arg3[i];
                }
            }
        }
    }
}
