using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DataManagement
{
    public interface IIdentityAssignment
    {
        /// <summary>
        /// unique ID of the assignment
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// reference to the selected Identity
        /// </summary>
        public Guid? IdentityID { get; set; }

        /// <summary>
        /// field-specific identity value
        /// </summary>
        public string? Value { get; set; }

    }
}
