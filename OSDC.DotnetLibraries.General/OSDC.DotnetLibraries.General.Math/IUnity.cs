using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IUnity
    {
        void SetUnity();
        bool IsUnity();
        bool IsUnity(double precision);
    }
}
