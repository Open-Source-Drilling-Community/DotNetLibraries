using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DataManagement
{
    public interface IIdentity
    {
        /// <summary>
        /// a MetaInfo for the identity
        /// </summary>
        public MetaInfo? MetaInfo { get; set; }

        /// <summary>
        /// symbolic name of the identity
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// the date when the data was created
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTimeOffset? LastModificationDate { get; set; }

    }
}
