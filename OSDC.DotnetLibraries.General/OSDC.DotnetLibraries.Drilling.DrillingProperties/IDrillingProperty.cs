using DWIS.API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public interface IDrillingProperty
    {
        Dictionary<string, Tuple<int, string>>? GetSparQLQueries(Assembly? assembly, string? typeName, string? propertyName);
        ManifestFile? GetManifestFile(Assembly? assembly, string? typeName, string? propertyName);
    }
}
