using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.APIProxy
{
    public class ProxyFactory : IProxyFactory
    {
        private List<object> _proxies = new List<object>();
        /// <summary>
        /// default constructor
        /// </summary>
        public ProxyFactory() { }

        public IProxySearch<a>? GetProxy<a>(string host, string api, string endPoint) where a : class
        {
            IProxySearch<a>? proxy = null;
            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(api) && !string.IsNullOrEmpty(endPoint))
            {
                foreach (var prox in _proxies)
                {
                    if (prox is not null and IProxySearch<a>)
                    {
                        IProxySearch<a>? iprox = prox as IProxySearch<a>;
                        if (iprox != null && host.Equals(iprox.Host) && api.Equals(iprox.API) && endPoint.Equals(iprox.EndPoint))
                        {
                            proxy = (IProxySearch<a>)prox;
                            break;
                        }
                    }
                }
                if (proxy == null)
                {
                    proxy = new ProxySearch<a>() { Host = host, API = api, EndPoint = endPoint };
                    _proxies.Add(proxy);
                }
            }
            return proxy;
        }
        public IProxyStandard<a, c>? GetProxy<a, c>(string host, string api, string endPoint) where a : class where c : class
        {
            IProxyStandard<a, c>? proxy = null;
            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(api) && !string.IsNullOrEmpty(endPoint))
            {
                foreach (var prox in _proxies)
                {
                    if (prox is not null and IProxyStandard<a, c>)
                    {
                        IProxyStandard<a, c>? iprox = prox as IProxyStandard<a, c>;
                        if (iprox != null && host.Equals(iprox.Host) && api.Equals(iprox.API) && endPoint.Equals(iprox.EndPoint))
                        {
                            proxy = (IProxyStandard<a, c>)prox;
                            break;
                        }
                    }
                }
                if (proxy == null)
                {
                    proxy = new ProxyStandard<a, c>() { Host = host, API = api, EndPoint = endPoint };
                    _proxies.Add(proxy);
                }
            }
            return proxy;
        }
        public IProxyLight<a, b, c>? GetProxy<a, b, c>(string host, string api, string endPoint) where a : class where b : class where c : class
        {
            IProxyLight<a, b, c>? proxy = null;
            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(api) && !string.IsNullOrEmpty(endPoint))
            {
                foreach (var prox in _proxies)
                {
                    if (prox is not null and IProxyLight<a, b, c>)
                    {
                        IProxyLight<a, b, c>? iprox = prox as IProxyLight<a, b, c>;
                        if (iprox != null && host.Equals(iprox.Host) && api.Equals(iprox.API) && endPoint.Equals(iprox.EndPoint))
                        {
                            proxy = (IProxyLight<a, b, c>)prox;
                            break;
                        }
                    }
                }
                if (proxy == null)
                {
                    proxy = new ProxyLight<a, b, c>() { Host = host, API = api, EndPoint = endPoint };
                    _proxies.Add(proxy);
                }
            }
            return proxy;
        }
    }
}
