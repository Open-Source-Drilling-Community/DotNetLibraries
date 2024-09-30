using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class GroupNameAttribute : Attribute
    {
        public string GroupName { get; }

        public GroupNameAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}
