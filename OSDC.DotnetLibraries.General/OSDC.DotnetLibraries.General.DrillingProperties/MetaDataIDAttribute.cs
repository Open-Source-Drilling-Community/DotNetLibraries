using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class MetaDataIDAttribute : ReferenceAttribute
    {
        public Guid MetaDataID { get; }

        public MetaDataIDAttribute(string metaDataID)
        {
            Guid id;
            if (Guid.TryParse(metaDataID, out id) && id != Guid.Empty)
            {
                MetaDataID = id;
            }

        }
    }
}
