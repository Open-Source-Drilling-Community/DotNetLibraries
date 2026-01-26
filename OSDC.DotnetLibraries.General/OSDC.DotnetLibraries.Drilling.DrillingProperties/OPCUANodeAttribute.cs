using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class OPCUANodeAttribute : Attribute
    {
        public virtual string? NameSpace { get; set; } = null;
        public virtual string? ID { get; set; }

        public OPCUANodeAttribute(string? ns, string? id) : base()
        {
            NameSpace = ns;
            ID = id;
        }
    }
}
