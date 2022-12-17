using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace OSDC.DotnetLibraries.General.DataManagement
{
    /// <summary>
    /// a class that allows to describe meta information around a given data including how to identify it and providing contextualization
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
        /// the http host base path allowing to access the data in a service oriented architecture (ex: http://my-server:80/HttpHostBasePath)
        /// </summary>
        public string HttpHostBasePath { get; set; }

        /// <summary>
        /// the end point string to append to the HttpHostBasePath to locate the data in a service oriented architecture (ex: http://my-server:80/HttpHostBasePath/HttpEndPoint)
        /// </summary>
        public string HttpEndPoint { get; set; }

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
        public MetaInfo(Guid id, string name, string descr, string typeName, string httpHostBasePath, string httpEndPoint)
        {
            ID = id;
            Name = name;
            Description = descr;
            TypeName = typeName;
            CreationDate = DateTime.UtcNow;
            LastModificationDate = CreationDate;
            HttpHostBasePath = httpHostBasePath;
            HttpEndPoint = httpEndPoint;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id) :
            this(id, "", "", typeof(object).Name, "", "")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name) :
            this(id, name, "", typeof(object).Name, "", "")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name, string descr) :
            this(id, name, descr, typeof(object).Name, "", "")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name, string descr, string httpHostBasePath, string httpEndPoint) :
            this(id, name, descr, typeof(object).Name, httpHostBasePath, httpEndPoint)
        {
        }
    }
}
