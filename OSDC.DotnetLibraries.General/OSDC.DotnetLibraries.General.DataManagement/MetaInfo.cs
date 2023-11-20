using System;
using System.Xml.Linq;

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
        public Guid ID { get; set; } = Guid.Empty;

        /// <summary>
        /// name of the data
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// a description of the data
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// the type name of the data
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

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
        public string HttpHostName { get; set; } = string.Empty;

        /// <summary>
        /// the http host base path of the microservice (ex: "DrillingUnitConversion/api/"), suffixed with "/"
        /// </summary>
        public string HttpHostBasePath { get; set; } = string.Empty;

        /// <summary>
        /// the http end point to append to the HttpHostBasePath to locate the data (ex: "DrillingUnitChoiceSets/"), suffixed with "/"
        /// </summary>
        public string HttpEndPoint { get; set; } = string.Empty;
        /// <summary>
        /// the http end point to append to the HttpHostBasePath to access the search API, suffixed with "/". 
        /// For example to search on a cartographic projection microservice, the cartographic projection that uses a geodetic datum. 
        /// In this case, the ID is the guid of the geodetic datum.
        /// A search API only implements the Get HTTP request.
        /// </summary>
        public string HttpEndPointSearch { get; set; } = string.Empty;
        /// <summary>
        /// the http end point to append to the HttpHostBasePath to access the calculation API, (ex: "DataUnitConversionSets/"), suffixed with "/"
        /// </summary>
        public string HttpEndPointCalculate { get; set; } = string.Empty;

        /// <summary>
        /// default constructor
        /// </summary>
        public MetaInfo() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name, string descr, string typeName, string httpHostName, string httpHostBasePath, string httpEndPoint, string httpEndPointSearch, string httpEndPointCalculate)
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
            HttpEndPointSearch = httpEndPointSearch;
            HttpEndPointCalculate = httpEndPointCalculate;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name, string descr, string typeName, string httpHostName, string httpHostBasePath, string httpEndPoint) : 
            this(id, name,descr, typeName, httpHostName, httpHostBasePath, httpEndPoint, string.Empty, string.Empty )
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id) :
            this(id, string.Empty, string.Empty, typeof(object).Name, string.Empty, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name) :
            this(id, name, string.Empty, typeof(object).Name, string.Empty, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name, string descr) :
            this(id, name, descr, typeof(object).Name, string.Empty, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string name, string descr, string httpHostName, string httpHostBasePath, string httpEndPoint) :
            this(id, name, descr, typeof(object).Name, httpHostName, httpHostBasePath, httpEndPoint)
        {
        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public MetaInfo(MetaInfo src)
        {
            if (src != null)
            {
                ID = src.ID;
                Name = src.Name;
                Description = src.Description;
                TypeName = src.TypeName;
                CreationDate = src.CreationDate;
                LastModificationDate = src.CreationDate;
                HttpHostName = src.HttpHostName;
                HttpHostBasePath = src.HttpHostBasePath;
                HttpEndPoint = src.HttpEndPoint;
                HttpEndPointSearch = src.HttpEndPointSearch;
                HttpEndPointCalculate = src.HttpEndPointCalculate;
            }
        }
    }
}
