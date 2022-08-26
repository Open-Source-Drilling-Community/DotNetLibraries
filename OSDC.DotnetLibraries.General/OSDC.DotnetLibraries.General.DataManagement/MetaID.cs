using Newtonsoft.Json;
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
        /// a dictionary of flags used by the data, the key being an name identifier of the flag and the value being the value of the flag itself
        /// </summary>
        public Dictionary<string, bool> Flags { get; set; }

        /// <summary>
        /// a dictionary of IDs associated to the data (for filtering and sorting purpose), the key being an identifier of the filterID and the value being the Guid itself
        /// </summary>
        public Dictionary<string, Guid> FilterIDs{ get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        [JsonConstructor]
        public MetaID(Guid id, string name, string descr, string typeName, Dictionary<string, bool> flags, Dictionary<string, Guid> filterIDs)
        {
            ID = id;
            Name = name;
            Description = descr;
            TypeName = typeName;
            Flags = flags;
            FilterIDs = filterIDs;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaID(Guid id) :
            this(id, "", "", typeof(object).Name, new Dictionary<string, bool>(), new Dictionary<string, Guid>())
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaID(Guid id, string name) :
            this(id, name, "", typeof(object).Name, new Dictionary<string, bool>(), new Dictionary<string, Guid>())
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaID(Guid id, string name, string descr, Dictionary<string, bool> flags) :
            this(id, name, descr, typeof(object).Name, flags, new Dictionary<string, Guid>())
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaID(Guid id, string name, string descr, string typeName, Dictionary<string, Guid> filterIDs) :
            this(id, name, descr, typeName, new Dictionary<string, bool>(), filterIDs)
        {
        }
    }
}
