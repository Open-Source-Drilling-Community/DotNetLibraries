using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IDotProductable<T>
    {
        double? Dot(T v);
    }
}
