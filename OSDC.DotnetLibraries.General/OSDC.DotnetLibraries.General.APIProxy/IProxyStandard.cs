using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.APIProxy
{
    public interface IProxyStandard<metaInfo, dataClass> : IProxy where metaInfo : class where dataClass : class
    {
        public Task<Guid[]?> Get();
        public Task<metaInfo[]?> GetMetaInfos();
        public Task<dataClass?> Get(Guid id);
        public Task Post(dataClass instance);
        public Task Put(Guid id, dataClass instance);
        public Task Delete(Guid id);
    }
}
