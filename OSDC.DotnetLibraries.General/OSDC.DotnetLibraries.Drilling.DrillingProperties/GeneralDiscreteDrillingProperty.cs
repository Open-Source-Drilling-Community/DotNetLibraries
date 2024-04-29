using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class GeneralDiscreteDrillingProperty : DiscreteDrillingProperty
    {

        public override uint? NumberOfStates => throw new NotImplementedException();
        public override double[]? Probabilities { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool Equals(DrillingProperty? other)
        {
            if (other != null && other is GeneralDiscreteDrillingProperty)
            {
                return true;
            }
            return false;
        }

        public override void CopyTo(DrillingProperty? dest)
        {
            if (dest != null && dest is GeneralDiscreteDrillingProperty)
            {

            }
        }
    }
}
