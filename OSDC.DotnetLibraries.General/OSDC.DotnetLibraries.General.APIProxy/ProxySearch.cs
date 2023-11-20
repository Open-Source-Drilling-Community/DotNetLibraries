using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.APIProxy
{
    public class ProxySearch<metaInfo> : Proxy, IProxySearch<metaInfo> where metaInfo : class
    {
        public ProxySearch() :base() 
        { 
        }
        public ProxySearch(ProxySearch<metaInfo> src) : base(src)
        {

        }
        /// <summary>
        /// return an array of IDs for that API
        /// </summary>
        /// <returns></returns>
        public async Task<metaInfo[]?> Get(params Guid[] guids)
        {
            metaInfo[]? ids = null;
            HttpClient? httpClient = GetHTTPClient();
            if (httpClient != null)
            {
                string url = API;
                if (guids != null)
                {
                    foreach (var guid in guids)
                    {
                        url += guid.ToString() + "/";
                    }
                }
                var a = await httpClient.GetAsync(url);
                if (a.IsSuccessStatusCode)
                {
                    string str = await a.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(str))
                    {
                        ids = Newtonsoft.Json.JsonConvert.DeserializeObject<metaInfo[]>(str);
                    }
                }
            }
            return ids;
        }

    }
}
