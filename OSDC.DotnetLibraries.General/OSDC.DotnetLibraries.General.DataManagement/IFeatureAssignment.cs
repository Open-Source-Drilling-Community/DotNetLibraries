using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DataManagement
{
    public interface IFeatureAssignment
    {
        /// <summary>
        /// stable identifier for the assignment
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// the selected feature category
        /// </summary>
        public Guid? FeatureCategoryID { get; set; }

        /// <summary>
        /// the selected field feature option
        /// </summary>
        public Guid? FeatureOptionID { get; set; }

        /// <summary>
        /// first date for which the assignment is valid
        /// </summary>
        public DateTimeOffset? FromDate { get; set; }

        /// <summary>
        /// last date for which the assignment is valid
        /// </summary>
        public DateTimeOffset? ToDate { get; set; }

    }
}
