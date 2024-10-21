using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class QuerySpecification
    {
        public int NumberOfArguments { get; set; } = 0;
        public List<byte>? Options { get; set; } = null;
        public List<string>? Variables { get; set; } = null;
        public string? SparQL { get; set; } = null;
    }
}
