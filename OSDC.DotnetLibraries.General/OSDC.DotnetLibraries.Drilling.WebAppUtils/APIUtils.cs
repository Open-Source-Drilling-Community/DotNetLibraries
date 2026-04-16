using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.WebAppUtils
{
    public class APIUtils
    {
        public static HttpClient SetHttpClient(string host, string microServiceUri)
        {
            HttpClientHandler handler = new();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            HttpClient httpClient = new(handler)
            {
                BaseAddress = new Uri(host + microServiceUri)
            };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
