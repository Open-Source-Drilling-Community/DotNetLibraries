using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.APIProxy
{
    public interface IProxyLight<metaInfo, lightDataClass, dataClass> : IProxyStandard<metaInfo, dataClass> where metaInfo : class where lightDataClass : class where dataClass : class
    {
        public Task<lightDataClass[]?> GetLightDatas();
    }
}
