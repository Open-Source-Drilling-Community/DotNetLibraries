using Newtonsoft.Json;
using System.Text;

namespace OSDC.DotnetLibraries.General.APIProxy
{
	public class ProxyStandard<metaInfo, dataClass> : Proxy, IProxyStandard<metaInfo, dataClass> where metaInfo : class where dataClass : class
	{
		/// <summary>
		/// default constructor
		/// </summary>
		public ProxyStandard() :base()
		{
		}
		/// <summary>
		/// copy constructor
		/// </summary>
		/// <param name="src"></param>
		public ProxyStandard(ProxyStandard<metaInfo, dataClass> src) :base(src)
		{
		}
		/// <summary>
		/// return an array of IDs for that API
		/// </summary>
		/// <returns></returns>
		public async Task<Guid[]?> Get()
		{
			Guid[]? ids = null;
			HttpClient? httpClient = GetHTTPClient();
			if (httpClient != null)
			{
				var a = await httpClient.GetAsync(API);
				if (a.IsSuccessStatusCode)
				{
					string str = await a.Content.ReadAsStringAsync();
					if (!string.IsNullOrEmpty(str))
					{
						ids = Newtonsoft.Json.JsonConvert.DeserializeObject<Guid[]>(str);
					}
				}
			}
			return ids;
		}

		/// <summary>
		/// return an array of metaInfo for that API
		/// </summary>
		/// <returns></returns>
		public async Task<metaInfo[]?> GetMetaInfos()
		{
			metaInfo[]? ids = null;
			HttpClient? httpClient = GetHTTPClient();
			if (httpClient != null)
			{
				var a = await httpClient.GetAsync(API + "MetaInfos/");
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

        /// <summary>
        /// return the instance of dataClass having this ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<dataClass?> Get(Guid id)
		{
			dataClass? instance = null;
			HttpClient? httpClient = GetHTTPClient();
			if (httpClient != null)
			{
				var a = await httpClient.GetAsync(API + id.ToString());
				if (a.IsSuccessStatusCode)
				{
					string str = await a.Content.ReadAsStringAsync();
					if (!string.IsNullOrEmpty(str))
					{
						instance = Newtonsoft.Json.JsonConvert.DeserializeObject<dataClass>(str);
					}
				}
			}
			return instance;
		}

		/// <summary>
		/// post (create) an instance of dataClass
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task Post(dataClass instance)
		{
			bool ok = false;
			HttpClient? httpClient = GetHTTPClient();
			if (httpClient != null && instance != null)
			{
				var content = new StringContent(JsonConvert.SerializeObject(instance), Encoding.UTF8, "application/json");
				var a = await httpClient.PostAsync(API, content);
				if (a.IsSuccessStatusCode)
				{
					ok = true;
				}
			}
		}
		/// <summary>
		/// put (modify) an instance of dataClass with this ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task Put(Guid id, dataClass instance)
		{
			bool ok = false;
			HttpClient? httpClient = GetHTTPClient();
			if (httpClient != null && instance != null)
			{
				var content = new StringContent(JsonConvert.SerializeObject(instance), Encoding.UTF8, "application/json");
				var a = await httpClient.PutAsync(API + id.ToString(), content);
				if (a.IsSuccessStatusCode)
				{
					ok = true;
				}
			}
		}

		/// <summary>
		/// delete the instance of dataClass having this ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task Delete(Guid id)
		{
			bool ok = false;
			HttpClient? httpClient = GetHTTPClient();
			if (httpClient != null)
			{
				var a = await httpClient.DeleteAsync(API + id.ToString());
				if (a.IsSuccessStatusCode)
				{
					ok = true;
				}
			}
		}

	}
}