using DWIS.API.DTO;
using DWIS.Client.ReferenceImplementation;
using System.Reflection;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public interface IRDFDrillingProperty 
    {
        public Dictionary<string, QuerySpecification>? SparQLQueries { get; set; }

        public AcquiredSignals[]? SubscribedSignals { get; set; }

        public ManifestFile? ManifestFile { get; set; }

        public void CreateSparQLQueries(Assembly? assembly, string? typeName, string? propName)
        {
            if (assembly != null && !string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(propName))
            {
                SparQLQueries = GeneratorSparQLManifestFile.GetSparQLQueries(assembly, typeName, propName);
            }
        }

        public void SubscribeSparQLQueries(IOPCUADWISClient? ddhubClient)
        {
            if (ddhubClient != null && ddhubClient.Connected && SparQLQueries != null)
            {
                foreach (var kvp in SparQLQueries)
                {
                    if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null && kvp.Value.NumberOfArguments > 0 && !string.IsNullOrEmpty(kvp.Value.SparQL))
                    {
                        var result = ddhubClient.GetQueryResult(kvp.Value.SparQL);
                        if (result != null && result.Results != null && result.Results.Count > 0)
                        {
                            SubscribedSignals = new AcquiredSignals[kvp.Value.NumberOfArguments];
                            for (int i = 0; i < kvp.Value.NumberOfArguments; i++)
                            {
                                SubscribedSignals[i] = AcquiredSignals.CreateWithSubscription([kvp.Value.SparQL], [kvp.Key], i, ddhubClient);
                            }
                        }
                    }
                }
            }
        }

        public void CreateManifestFile(Assembly? assembly, string? typeName, string? propName, string manifestName, string companyName, string prefix)
        {
            if (assembly != null && !string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(propName))
            {
                ManifestFile = GeneratorSparQLManifestFile.GetManifestFile(assembly, typeName, propName, manifestName, companyName, prefix);
            }
        }

        public void PublishManifest(IOPCUADWISClient? ddhubClient)
        {
            if (ddhubClient != null && ddhubClient.Connected && ManifestFile != null && SparQLQueries != null && SparQLQueries.Count > 0)
            {
                // check if the semantic has not already been injected
                bool found = false;
                foreach (var kvp in SparQLQueries)
                {
                    if (kvp.Value != null && kvp.Value.NumberOfArguments > 0 && !string.IsNullOrEmpty(kvp.Value.SparQL))
                    {
                        var result = ddhubClient.GetQueryResult(kvp.Value.SparQL);
                        if (result != null && result.Results != null && result.Results.Count > 0)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                // if not, inject
                if (!found)
                {
                    var result = ddhubClient.Inject(ManifestFile);
                    if (result == null || !result.Success)
                    {
                        throw new Exception("Error in manifest file");
                    }
                }
            }
        }
    }
}
