using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.APIProxy
{
    public class Proxy : IProxy
    {
        protected HttpClient? httpClient_ = null;

        public string EndPoint { get; set; } = string.Empty;
        public string API { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        /// <summary>
        /// default constructor
        /// </summary>
        public Proxy() { }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Proxy(Proxy src)
        {
            if (src != null)
            {
                EndPoint = src.EndPoint;
                API = src.API;
                Host = src.Host;
            }
        }

        protected HttpClient? GetHTTPClient()
        {
            if (httpClient_ == null && !string.IsNullOrEmpty(Host))
            {
                httpClient_ = new HttpClient();
                httpClient_.BaseAddress = new Uri(Host + EndPoint);
                httpClient_.DefaultRequestHeaders.Accept.Clear();
                httpClient_.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }
            return httpClient_;
        }

    }
}
