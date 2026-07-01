using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DataManagement
{
    public interface IFeatureCategory
    {
        /// <summary>
        /// a MetaInfo for the FeatureCategory
        /// </summary>
        public MetaInfo? MetaInfo { get; set; }

        /// <summary>
        /// user-defined name of the category
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// whether options from this category are mutually exclusive when assigned to a field
        /// </summary>
        public bool IsExclusive { get; set; }

        /// <summary>
        /// whether field assignments from this category carry a validity period
        /// </summary>
        public bool HasValidityPeriod { get; set; }

        /// <summary>
        /// the possible options for this category
        /// </summary>
        public List<IFeatureOption>? Options { get; set; }

    }
}
