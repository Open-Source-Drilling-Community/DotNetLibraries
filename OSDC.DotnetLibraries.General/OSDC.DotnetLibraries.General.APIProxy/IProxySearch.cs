using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.APIProxy
{
    public interface IProxySearch<metaInfo> : IProxy where metaInfo : class
    {
        public Task<metaInfo[]?> Get(params Guid[] id);
    }
}
