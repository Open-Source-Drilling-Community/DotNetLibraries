using System;
using System.Collections.Generic;

namespace OSDC.DotnetLibraries.General.DataManagement
{
    /// <summary>
    /// a class which contains extended but light information allowing to identify a data (useful for data transfer and sorting purposes)
    /// </summary>
    public class MetaID
    {
        /// <summary>
        /// an ID for the data
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// name of the data
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// a description of the data
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// the type name of the data
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// a list of flags used by the data
        /// </summary>
        public List<string> FlagList { get; set; }

        /// <summary>
        /// a list of ID for data attributes that allows to filter and sort a set of datas
        /// </summary>
        public List<Guid> FilterIDList { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaID(Guid id, string name, string descr, string typeName, List<string> flags, List<Guid> filterIDs)
        {
            ID = id;
            Name = name;
            Description = descr;
            TypeName = typeName;
            FlagList = flags;
            FilterIDList = filterIDs;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaID(Guid id, string name, string descr, string flag)
        {
            ID = id;
            Name = name;
            Description = descr;
            TypeName = typeof(object).Name;
            FlagList = new List<string>() { flag };
            FilterIDList = new List<Guid>();
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaID(Guid id, string name, string descr, string typeName, Guid filterID)
        {
            ID = id;
            Name = name;
            Description = descr;
            TypeName = typeName;
            FlagList = new List<string>();
            FilterIDList = new List<Guid>() { filterID };
        }
    }
}
