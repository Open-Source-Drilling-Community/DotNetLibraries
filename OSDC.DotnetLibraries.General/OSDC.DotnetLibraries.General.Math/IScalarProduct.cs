using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IScalarProduct<T>
    {
        T Time(double x);
        void TimeAssign(double x);
    }
}
