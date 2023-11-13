using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class ReferenceAttribute : Attribute
    {
    }
}
