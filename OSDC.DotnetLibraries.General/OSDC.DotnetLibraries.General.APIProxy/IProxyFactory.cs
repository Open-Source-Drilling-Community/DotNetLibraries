using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.APIProxy
{
    public interface IProxyFactory
    {
        public IProxyLight<a, b, c>? GetProxy<a, b, c>(string host, string api, string endPoint) where a : class where b: class where c: class;
        public IProxyStandard<a, c>? GetProxy<a, c>(string host, string api, string endPoint) where a : class where c : class;
        public IProxySearch<a>? GetProxy<a>(string host, string api, string endPoint) where a : class;
    }
}
