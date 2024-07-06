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
        /// default constructor
        /// </summary>
        public MetaInfo() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id, string httpHostName, string httpHostBasePath, string httpEndPoint)
        {
            ID = id;
            HttpHostName = httpHostName;
            HttpHostBasePath = httpHostBasePath;
            HttpEndPoint = httpEndPoint;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public MetaInfo(Guid id) :
            this(id, string.Empty, string.Empty, string.Empty)
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
                HttpHostName = src.HttpHostName;
                HttpHostBasePath = src.HttpHostBasePath;
                HttpEndPoint = src.HttpEndPoint;
            }
        }
    }
}
