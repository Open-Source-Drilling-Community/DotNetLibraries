using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Common
{
    public interface ITimeSpanValue
    {
        public TimeSpan? Value { get; set; }
    }
}
