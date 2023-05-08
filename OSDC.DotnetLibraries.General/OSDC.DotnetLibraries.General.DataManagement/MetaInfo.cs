using System;

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
        public DateTimeOffset CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTimeOffset LastModificationDate { get; set; }

        /// <summary>
        /// the http host name to access the data in a service oriented architecture (ex: "http://my-server:80/"), suffixed with "/"
        /// </summary>
        public string HttpHostName { get; set; }

        /// <summary>
        /// the http host base path of the microservice (ex: "DrillingUnitConversion/api/"), suffixed with "/"
        /// </summary>
        public string HttpHostBasePath { get; set; }

        /// <summary>
        /// the http end point to append to the HttpHostBasePath to locate the data (ex: "DrillingUnitChoiceSets/"), suffixed with "/"
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
        public MetaInfo(Guid id, string name, string descr, string typeName, string httpHostName, string httpHostBasePath, string httpEndPoint)
        {
            ID = id;
            Name = name;
            Description = descr;
            TypeName = typeName;
            CreationDate = DateTime.UtcNow;
            LastModificationDate = CreationDate;
            HttpHostName = httpHostName;
            HttpHostBasePath = httpHostBasePath;
            HttpEndPoint = httpEndPoint;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id) :
            this(id, "", "", typeof(object).Name, "", "", "")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name) :
            this(id, name, "", typeof(object).Name, "", "", "")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name, string descr) :
            this(id, name, descr, typeof(object).Name, "", "", "")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name, string descr, string httpHostName, string httpHostBasePath, string httpEndPoint) :
            this(id, name, descr, typeof(object).Name, httpHostName, httpHostBasePath, httpEndPoint)
        {
        }
    }
}
