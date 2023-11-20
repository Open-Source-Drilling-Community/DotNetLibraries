using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.APIProxy
{
    public class ProxyLight<metaInfo, lightDataClass, dataClass> : ProxyStandard<metaInfo, dataClass>, IProxyLight<metaInfo, lightDataClass, dataClass> where metaInfo : class where lightDataClass : class where dataClass : class
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public ProxyLight():base() { }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public ProxyLight(ProxyLight<metaInfo, lightDataClass, dataClass> src) : base(src) { }

        /// <summary>
        /// return an array of lightData for that API
        /// </summary>
        /// <returns></returns>
        public async Task<lightDataClass[]?> GetLightDatas()
        {
            lightDataClass[]? ids = null;
            HttpClient? httpClient = GetHTTPClient();
            if (httpClient != null)
            {
                var a = await httpClient.GetAsync(API + "LightDatas/");
                if (a.IsSuccessStatusCode)
                {
                    string str = await a.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(str))
                    {
                        ids = Newtonsoft.Json.JsonConvert.DeserializeObject<lightDataClass[]>(str);
                    }
                }
            }
            return ids;
        }
    }
}
