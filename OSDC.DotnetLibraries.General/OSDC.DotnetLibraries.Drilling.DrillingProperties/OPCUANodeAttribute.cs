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
        public virtual int? NameSpaceID { get; set; } = null;
        public virtual string? PrefixID { get; set; } = null;
        public virtual string? ID { get; set; }

        public OPCUANodeAttribute(string? ns, string? prefixID, string? id) : base()
        {
            NameSpace = ns;
            PrefixID = prefixID;
            ID = id;
        }
        public OPCUANodeAttribute(int? nsID, string? id) : base()
        {
            NameSpaceID = nsID;
            ID = id;
        }
    }
}
