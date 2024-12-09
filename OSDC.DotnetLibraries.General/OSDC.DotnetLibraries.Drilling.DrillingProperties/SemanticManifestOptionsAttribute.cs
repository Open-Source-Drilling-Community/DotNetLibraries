using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]

    public class SemanticManifestOptionsAttribute : Attribute
    {
        public byte[]? Options { get; } = null;

        public SemanticManifestOptionsAttribute(byte option, params byte[]? options)
        {
            int count = 1;
            if (options != null)
            {
                count += options.Length;
            }
            Options = new byte[count];
            Options[0] = option;
            if (options != null)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    Options[i + 1] = options[i];
                }
            }
        }
    }
}
