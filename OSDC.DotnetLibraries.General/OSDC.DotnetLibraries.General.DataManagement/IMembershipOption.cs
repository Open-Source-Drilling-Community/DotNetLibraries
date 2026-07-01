using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DataManagement
{
    public interface IMembershipOption
    {
        /// <summary>
        /// stable identifier for the option inside its category
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// user-defined name of the option
        /// </summary>
        public string? Name { get; set; }

    }
}
