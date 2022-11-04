using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace OSDC.DotnetLibraries.General.DataManagement
{
    /// <summary>
    /// a class which contains extended but light information allowing to identify a data AND store light output information to conduct simple data analytics
    /// </summary>
    public class MetaInfo
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
        /// the date when the data was created
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTime LastModificationDate { get; set; }

        /// <summary>
        /// a dictionary of flags used by the data, the key being an name identifier of the flag and the value being the value of the flag itself
        /// </summary>
        public Dictionary<string, bool> Flags { get; set; }

        /// <summary>
        /// a dictionary of IDs associated to the data (for filtering and sorting purpose), the key being an identifier of the filterID and the value being the Guid itself
        /// </summary>
        public Dictionary<string, Guid> FilterIDs { get; set; }

        /// <summary>
        /// a dictionary of labels associated to the data (for filtering and sorting purpose), the key being an identifier of the label and the value being the label itself
        /// </summary>
        public Dictionary<string, string> Labels { get; set; }

        /// <summary>
        /// a dictionary of values associated to the data (for filtering and sorting purpose), the key being an identifier of the double value and the value being the double value itself
        /// </summary>
        public Dictionary<string, double> Values { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public MetaInfo() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        [JsonConstructor]
        public MetaInfo(Guid id, string name, string descr, string typeName,
                Dictionary<string, bool> flags,
                Dictionary<string, Guid> filterIDs,
                Dictionary<string, string> labels,
                Dictionary<string, double> values)
        {
            ID = id;
            Name = name;
            Description = descr;
            TypeName = typeName;
            CreationDate = DateTime.UtcNow;
            LastModificationDate = CreationDate;
            Flags = flags;
            FilterIDs = filterIDs;
            Labels = labels;
            Values = values;

        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id) :
            this(id, "", "", typeof(object).Name,
                new Dictionary<string, bool>(),
                new Dictionary<string, Guid>(),
                new Dictionary<string, string>(),
                new Dictionary<string, double>())
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name) :
            this(id, name, "", typeof(object).Name,
                new Dictionary<string, bool>(),
                new Dictionary<string, Guid>(),
                new Dictionary<string, string>(),
                new Dictionary<string, double>())
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name, string descr, Dictionary<string, bool> flags) :
            this(id, name, descr, typeof(object).Name,
                flags,
                new Dictionary<string, Guid>(),
                new Dictionary<string, string>(),
                new Dictionary<string, double>())
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name, string descr, string typeName, Dictionary<string, Guid> filterIDs) :
            this(id, name, descr, typeName,
                new Dictionary<string, bool>(),
                filterIDs,
                new Dictionary<string, string>(),
                new Dictionary<string, double>())
        {
        }
    }
}
